using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using Orthonis.Core;

namespace Orthonis.Windows;

public static class WindowsHost
{
    // Native-process-only avoids WOW64 filesystem redirection. ARM64 is not admitted in this slice.
    public static bool ArchitectureSupported => RuntimeInformation.ProcessArchitecture == RuntimeInformation.OSArchitecture &&
        RuntimeInformation.OSArchitecture is Architecture.X64 or Architecture.X86;
    public static bool Available => OperatingSystem.IsWindows() && ArchitectureSupported;
}

[SupportedOSPlatform("windows")]
public sealed class WindowsRunRegistry : IRunRegistry
{
    public SourceScope Scope => Environment.Is64BitOperatingSystem ? SourceScope.RunNative64 : SourceScope.RunNative32;
    private RegistryView View => Scope == SourceScope.RunNative64 ? RegistryView.Registry64 : RegistryView.Registry32;

    public SourceContext Context(string caseId)
    {
        Contract.Require(WindowsHost.Available, "Unsupported Windows architecture or process view.");
        using var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, View);
        using var key = root.OpenSubKey(@"Software\Microsoft\Cryptography", writable: false);
        Contract.Require(key is not null, "Machine context is unavailable.");
        var read = ReadValue(key.Handle, "MachineGuid");
        Contract.Require(read.Status == CollectionStatus.Observed && read.Value is not null && read.Value.Kind == 1,
            "Machine context is unavailable.");
        string machine;
        try { machine = new UnicodeEncoding(false, false, true).GetString(read.Value.Data.AsSpan()).TrimEnd('\0'); }
        catch (DecoderFallbackException) { throw new RefusalException("Machine context is unsupported."); }
        Contract.Require(Guid.TryParse(machine, out var guid), "Machine context is unsupported.");
        using var identity = WindowsIdentity.GetCurrent();
        var user = identity.User?.Value;
        Contract.Require(user is { Length: > 0 and <= 256 }, "User context is unavailable.");
        return RunIdentity.Context(caseId, Scope, guid.ToString("D"), user);
    }

    public RunListing Enumerate(RunHive hive, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var root = RegistryKey.OpenBaseKey(Hive(hive), View);
        using var key = root.OpenSubKey(RunIdentity.Key, writable: false);
        if (key is null) return new(CollectionStatus.Empty, [], 0);
        var values = ImmutableArray.CreateBuilder<RunValue>();
        var status = CollectionStatus.Observed;
        var examined = 0;
        if (!Stamp(key.Handle, out var before)) return new(CollectionStatus.Failed, [], 0);
        for (uint index = 0; index <= RunIdentity.PerKeyLimit; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var name = new StringBuilder(257);
            uint nameLength = 257;
            byte[] data = new byte[RunIdentity.MaxValueBytes];
            uint size = (uint)data.Length;
            var code = RegEnumValueW(key.Handle, index, name, ref nameLength, nint.Zero, out var kind, data, ref size);
            if (code == 259) break; // ERROR_NO_MORE_ITEMS
            examined++;
            if (index == RunIdentity.PerKeyLimit || code == 234) { status = CollectionStatus.Limited; break; }
            if (code != 0)
            {
                status = values.Count > 0 ? CollectionStatus.Limited : Outcome(code);
                break;
            }
            if (size > data.Length || nameLength > 256 || kind > int.MaxValue || !RunIdentity.Name(name.ToString()))
            { status = CollectionStatus.Limited; break; }
            values.Add(new(name.ToString(), (int)kind, data.AsSpan(0, (int)size).ToArray().ToImmutableArray()));
        }
        cancellationToken.ThrowIfCancellationRequested();
        if (!Stamp(key.Handle, out var after) || before != after) status = CollectionStatus.Limited;
        if (status == CollectionStatus.Observed && examined == 0) status = CollectionStatus.Empty;
        return new(status, values.ToImmutable(), examined);
    }

    public RunRead Read(RunHive hive, string valueName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Contract.Require(RunIdentity.Name(valueName), "Unsupported value name.");
        try
        {
            using var root = RegistryKey.OpenBaseKey(Hive(hive), View);
            using var key = root.OpenSubKey(RunIdentity.Key, writable: false);
            var result = key is null ? new RunRead(CollectionStatus.Unavailable, null) : ReadValue(key.Handle, valueName);
            cancellationToken.ThrowIfCancellationRequested();
            return result;
        }
        catch (UnauthorizedAccessException) { return new(CollectionStatus.PermissionDenied, null); }
        catch (System.Security.SecurityException) { return new(CollectionStatus.PermissionDenied, null); }
        catch (IOException) { return new(CollectionStatus.Failed, null); }
    }

    private static RegistryHive Hive(RunHive hive) => hive switch
    {
        RunHive.CurrentUser => RegistryHive.CurrentUser,
        RunHive.LocalMachine => RegistryHive.LocalMachine,
        _ => throw new RefusalException("Unsupported registry hive.")
    };
    private static RunRead ReadValue(SafeRegistryHandle key, string name)
    {
        byte[] bytes = new byte[RunIdentity.MaxValueBytes];
        uint size = (uint)bytes.Length;
        var code = RegQueryValueExW(key, name, nint.Zero, out var kind, bytes, ref size);
        if (code != 0) return new(Outcome(code), null);
        if (size > bytes.Length || kind > int.MaxValue) return new(CollectionStatus.Unsupported, null);
        return new(CollectionStatus.Observed, new(name, (int)kind, bytes.AsSpan(0, (int)size).ToArray().ToImmutableArray()));
    }
    private static CollectionStatus Outcome(int error) => error switch
    {
        2 or 3 => CollectionStatus.Unavailable,
        5 => CollectionStatus.PermissionDenied,
        234 => CollectionStatus.Limited,
        _ => CollectionStatus.Failed
    };
    private static bool Stamp(SafeRegistryHandle key, out long stamp) =>
        RegQueryInfoKeyW(key, nint.Zero, nint.Zero, nint.Zero, nint.Zero, nint.Zero, nint.Zero,
            nint.Zero, nint.Zero, nint.Zero, nint.Zero, out stamp) == 0;

    // Fixed-size queries only. No registry write API is imported.
    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int RegEnumValueW(SafeRegistryHandle key, uint index, StringBuilder name, ref uint nameLength,
        nint reserved, out uint type, [Out] byte[] data, ref uint dataLength);
    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int RegQueryValueExW(SafeRegistryHandle key, string name, nint reserved, out uint type,
        [Out] byte[] data, ref uint dataLength);
    [DllImport("advapi32.dll", ExactSpelling = true)]
    private static extern int RegQueryInfoKeyW(SafeRegistryHandle key, nint cls, nint clsLength, nint reserved,
        nint subKeys, nint maxSubKey, nint maxClass, nint values, nint maxName, nint maxValue,
        nint securityDescriptor, out long lastWriteTime);
}
