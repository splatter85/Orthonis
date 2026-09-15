using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32.SafeHandles;

namespace Orthonis.Windows;

// Read attributes of existing objects only. Never load, execute, read file contents or resolve associations.
[SupportedOSPlatform("windows")]
public sealed class LocalExecutableProbe : IExecutableProbe
{
    public TargetPresence Inspect(string executablePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!WindowsHost.Available || !RunCommand.LocalExe(executablePath)) return TargetPresence.Unsupported;
        var mapping = new StringBuilder(1024);
        if (QueryDosDeviceW(executablePath[..2], mapping, mapping.Capacity) == 0) return TargetPresence.Unknown;
        // Reject mapped shares, SUBST, device aliases and unfamiliar namespaces before any filesystem open.
        var device = mapping.ToString();
        if (!Regex.IsMatch(device, @"\A\\Device\\HarddiskVolume[0-9]+\z", RegexOptions.CultureInvariant))
            return TargetPresence.Unsupported;
        SafeFileHandle? parent = null;
        try
        {
            var rootStatus = Open(device + "\\", null, directory: true, out var root);
            parent = root;
            if (rootStatus != 0) return Failure(rootStatus, allowMissing: false);
            if (!Plain(parent, directory: true)) return TargetPresence.Unsupported;
            var components = executablePath[3..].Split('\\');
            for (var i = 0; i < components.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var directory = i < components.Length - 1;
                // Each component is opened relative to the already-held parent, with reparse traversal forbidden.
                var status = Open(components[i], parent, directory, out var next);
                if (status != 0) { next.Dispose(); return Failure(status, allowMissing: true); }
                if (!Plain(next, directory)) { next.Dispose(); return TargetPresence.Unsupported; }
                parent.Dispose();
                parent = next;
            }
            cancellationToken.ThrowIfCancellationRequested();
            return TargetPresence.Present;
        }
        finally { parent?.Dispose(); }
    }

    private static TargetPresence Failure(int status, bool allowMissing)
    {
        // Only explicit local file/path-not-found statuses qualify as missing. Permissions/errors do not.
        var error = RtlNtStatusToDosError(status);
        return error switch
        {
            2 or 3 when allowMissing => TargetPresence.Missing,
            5 or 1314 => TargetPresence.Inaccessible,
            267 or 4390 or 4392 or 4393 or 4394 or 4395 or 681 => TargetPresence.Unsupported,
            _ => TargetPresence.Unknown
        };
    }
    private static bool Plain(SafeFileHandle handle, bool directory)
    {
        if (!GetFileInformationByHandleEx(handle, 9, out var info, (uint)Marshal.SizeOf<AttributeTag>())) return false;
        return (info.Attributes & 0x400) == 0 && ((info.Attributes & 0x10) != 0) == directory;
    }
    private static int Open(string name, SafeFileHandle? parent, bool directory, out SafeFileHandle handle)
    {
        var buffer = Marshal.StringToHGlobalUni(name);
        var unicodePointer = Marshal.AllocHGlobal(Marshal.SizeOf<UnicodeString>());
        var parentReference = false;
        try
        {
            parent?.DangerousAddRef(ref parentReference);
            var unicode = new UnicodeString { Length = checked((ushort)(name.Length * 2)), MaximumLength = checked((ushort)((name.Length + 1) * 2)), Buffer = buffer };
            Marshal.StructureToPtr(unicode, unicodePointer, false);
            var attributes = new ObjectAttributes
            {
                Length = Marshal.SizeOf<ObjectAttributes>(), RootDirectory = parent?.DangerousGetHandle() ?? nint.Zero,
                ObjectName = unicodePointer, Attributes = 0x40 | 0x1000 // OBJ_CASE_INSENSITIVE | OBJ_DONT_REPARSE
            };
            // FILE_READ_ATTRIBUTES | SYNCHRONIZE (+ FILE_TRAVERSE on directories), share read/write/delete.
            // NtOpenFile cannot create a target. Synchronous open, reparse point itself, correct object kind.
            return NtOpenFile(out handle, 0x80u | 0x100000u | (directory ? 0x20u : 0), ref attributes, out _, 7,
                0x20u | 0x200000u | (directory ? 1u : 0x40u));
        }
        finally
        {
            if (parentReference) parent!.DangerousRelease();
            Marshal.FreeHGlobal(unicodePointer);
            Marshal.FreeHGlobal(buffer);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct UnicodeString { public ushort Length; public ushort MaximumLength; public nint Buffer; }
    [StructLayout(LayoutKind.Sequential)]
    private struct ObjectAttributes
    {
        public int Length; public nint RootDirectory; public nint ObjectName; public uint Attributes;
        public nint SecurityDescriptor; public nint SecurityQualityOfService;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct IoStatus { public nint Status; public nuint Information; }
    [StructLayout(LayoutKind.Sequential)]
    private struct AttributeTag { public uint Attributes; public uint ReparseTag; }
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    private static extern uint QueryDosDeviceW(string device, StringBuilder target, int length);
    [DllImport("ntdll.dll", ExactSpelling = true)]
    private static extern int NtOpenFile(out SafeFileHandle handle, uint desiredAccess, ref ObjectAttributes attributes,
        out IoStatus status, uint shareAccess, uint options);
    [DllImport("ntdll.dll", ExactSpelling = true)]
    private static extern uint RtlNtStatusToDosError(int status);
    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleEx(SafeFileHandle handle, int informationClass,
        out AttributeTag information, uint size);
}
