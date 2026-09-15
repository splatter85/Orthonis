using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Xml;
using Microsoft.Win32.SafeHandles;
using Orthonis.Core;

namespace Orthonis.Windows;

// Only local Application reads are exposed. No session, arbitrary path/query, write, clear, export or subscription API.
[SupportedOSPlatform("windows")]
public sealed class WindowsApplicationEventLog : IApplicationEventLog
{
    public SourceContext Context(string caseId)
    {
        var identity = new WindowsRunRegistry().Context(caseId); // Read-only existing machine/user binding, not Run enumeration.
        return identity with { Mode = SourceMode.WindowsReliability, Scope = SourceScope.ApplicationEventLog };
    }
    public ApplicationRead Read(DateTimeOffset from, DateTimeOffset to, CancellationToken token)
    {
        Contract.Require(WindowsHost.Available && from.Offset == TimeSpan.Zero && to.Offset == TimeSpan.Zero &&
            to - from == TimeSpan.FromDays(7), "Unsupported Application interval or host.");
        token.ThrowIfCancellationRequested();
        var records = ImmutableArray.CreateBuilder<ApplicationEvent>();
        var examined = 0;
        var invalid = 0;
        var status = CollectionStatus.Empty;
        LogBoundary? before = null;
        LogBoundary? after = null;
        var watch = Stopwatch.StartNew();
        void Check()
        {
            token.ThrowIfCancellationRequested();
            if (watch.Elapsed >= TimeSpan.FromSeconds(3)) throw new Win32Exception(1460);
        }
        try
        {
            Check();
            before = Boundary(Check);
            var start = from.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", CultureInfo.InvariantCulture);
            var end = to.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", CultureInfo.InvariantCulture);
            var xpath = $"*[System[TimeCreated[@SystemTime >= '{start}' and @SystemTime <= '{end}'] and (Level=1 or Level=2 or Level=3 or Provider[@Name='Windows Error Reporting'])]]";
            using var query = OpenQuery(xpath, reverse: true);
            while (true)
            {
                Check();
                using var item = Next(query);
                if (item is null) break;
                examined++;
                if (examined > ReliabilityData.Limit) { status = CollectionStatus.Limited; break; }
                try
                {
                    var record = ReliabilityData.Parse(Render(item));
                    if (!ReliabilityData.Selected(record) || record.OccurredAt < from || record.OccurredAt > to)
                    { invalid++; status = CollectionStatus.Unavailable; continue; }
                    records.Add(record);
                    if (status != CollectionStatus.Unavailable) status = CollectionStatus.Observed;
                }
                catch (Exception e) when (e is RefusalException or XmlException or FormatException)
                { invalid++; status = CollectionStatus.Unavailable; }
                catch (Win32Exception e) when (e.NativeErrorCode == 122)
                { invalid++; status = CollectionStatus.Unavailable; } // Oversized render; never allocate beyond the cap.
            }
            Check();
            after = Boundary(Check);
            Check();
        }
        catch (Win32Exception e) { status = Outcome(e.NativeErrorCode); }
        catch (UnauthorizedAccessException) { status = CollectionStatus.PermissionDenied; }
        catch (System.Security.SecurityException) { status = CollectionStatus.PermissionDenied; }
        // Caller cancellation escapes; the coordinator never saves a cancelled revision.
        return new(status, examined, invalid, records.ToImmutable(), before, after);
    }
    public static CollectionStatus Outcome(int code) => code switch
    {
        5 or 1314 => CollectionStatus.PermissionDenied,
        1460 => CollectionStatus.TimedOut,
        15011 => CollectionStatus.Stale,
        2 or 3 or 15007 => CollectionStatus.Unavailable,
        50 or 120 => CollectionStatus.Unsupported,
        _ => CollectionStatus.Failed
    };
    private static LogBoundary? Boundary(Action check)
    {
        check();
        try
        {
            using var log = EvtOpenLog(nint.Zero, ReliabilityData.Channel, 1);
            if (log.IsInvalid) throw Error();
            var created = Scalar(log, 0, 17); // EvtLogCreationTime: FILETIME.
            var count = Scalar(log, 5, 10);   // EvtLogNumberOfLogRecords: UInt64.
            var oldest = Scalar(log, 6, 10);  // EvtLogOldestRecordNumber: UInt64.
            DateTimeOffset? occurred = null;
            string? hash = null;
            if (count > 0)
            {
                check();
                using var query = OpenQuery("*", reverse: false);
                using var item = Next(query);
                if (item is not null)
                {
                    var record = ReliabilityData.Parse(Render(item));
                    if (record.RecordId == oldest) { occurred = record.OccurredAt; hash = record.ContentHash; }
                }
            }
            return new(DateTimeOffset.FromFileTime(checked((long)created)), oldest, count, occurred, hash);
        }
        catch (Exception e) when (e is Win32Exception or RefusalException or XmlException or ArgumentOutOfRangeException or OverflowException)
        { return null; } // Metadata denial/loss does not become a claim of continuous history.
    }
    private static ulong Scalar(EventHandle log, int property, uint type)
    {
        if (!EvtGetLogInfo(log, property, 16, out var value, out var used)) throw Error();
        if (used != 16 || value.Type != type) throw new Win32Exception(13);
        return value.Value;
    }
    private static EventHandle OpenQuery(string query, bool reverse)
    {
        var handle = EvtQuery(nint.Zero, ReliabilityData.Channel, query, reverse ? 0x201 : 0x101);
        if (!handle.IsInvalid) return handle;
        var error = Error();
        handle.Dispose();
        throw error;
    }
    private static EventHandle? Next(EventHandle query)
    {
        var handles = new nint[1];
        if (!EvtNext(query, 1, handles, 250, 0, out var returned))
        {
            var error = Marshal.GetLastWin32Error();
            foreach (var h in handles) if (h != nint.Zero) _ = EvtClose(h);
            if (error == 259) return null;
            throw new Win32Exception(error);
        }
        if (returned != 1 || handles[0] == nint.Zero)
        {
            foreach (var h in handles) if (h != nint.Zero) _ = EvtClose(h);
            throw new Win32Exception(13);
        }
        return new EventHandle(handles[0]);
    }
    private static string Render(EventHandle handle)
    {
        var succeeded = EvtRender(nint.Zero, handle, 1, 0, nint.Zero, out var required, out _);
        var error = Marshal.GetLastWin32Error();
        ReliabilityData.ValidateRenderProbe(succeeded, error, required);
        var buffer = Marshal.AllocHGlobal(required);
        try
        {
            if (!EvtRender(nint.Zero, handle, 1, required, buffer, out var used, out _)) throw Error();
            if (used < 2 || used > required || used % 2 != 0 || Marshal.ReadInt16(buffer, used - 2) != 0)
                throw new Win32Exception(13);
            return Marshal.PtrToStringUni(buffer, used / 2 - 1) ?? throw new Win32Exception(13);
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }
    private static Win32Exception Error() => new(Marshal.GetLastWin32Error());
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    private struct Variant
    {
        [FieldOffset(0)] public ulong Value;
        [FieldOffset(8)] public uint Count;
        [FieldOffset(12)] public uint Type;
    }
    private sealed class EventHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public EventHandle() : base(true) { }
        public EventHandle(nint value) : base(true) { SetHandle(value); }
        protected override bool ReleaseHandle() => EvtClose(handle);
    }
    [DllImport("wevtapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    private static extern EventHandle EvtQuery(nint session, string path, string query, int flags);
    [DllImport("wevtapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    private static extern EventHandle EvtOpenLog(nint session, string path, int flags);
    [DllImport("wevtapi.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EvtNext(EventHandle query, int size, [Out] nint[] events, int timeout, int flags, out int returned);
    [DllImport("wevtapi.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EvtRender(nint context, EventHandle fragment, int flags, int size, nint buffer, out int used, out int count);
    [DllImport("wevtapi.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EvtGetLogInfo(EventHandle log, int property, int size, out Variant value, out int used);
    [DllImport("wevtapi.dll", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EvtClose(nint handle);
}
