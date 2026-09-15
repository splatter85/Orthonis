using System.Text;
using Orthonis.Core;

namespace Orthonis.Windows;

// Deliberately not a Windows shell parser. Unsupported input is never guessed or executed.
public static class RunCommand
{
    private static readonly HashSet<string> Launchers = new(StringComparer.OrdinalIgnoreCase)
    {
        "cmd.exe", "powershell.exe", "pwsh.exe", "wscript.exe", "cscript.exe", "mshta.exe", "rundll32.exe",
        "regsvr32.exe", "explorer.exe", "msiexec.exe", "control.exe", "conhost.exe", "wt.exe", "wsl.exe",
        "bash.exe", "python.exe", "pythonw.exe", "py.exe", "node.exe", "java.exe", "javaw.exe", "hh.exe"
    };

    public static string? Resolve(RunValue value)
    {
        RunIdentity.ValidateValue(value);
        if (value.Kind is not (1 or 2) || value.Data.Length < 2 || value.Data.Length % 2 != 0) return null;
        string command;
        try { command = new UnicodeEncoding(false, false, true).GetString(value.Data.AsSpan()); }
        catch (DecoderFallbackException) { return null; }
        // REG_SZ/REG_EXPAND_SZ may be unterminated. Refuse instead of reading past the buffer or inventing termination.
        if (!command.EndsWith('\0')) return null;
        command = command[..^1];
        if (command.Length is 0 or > 260 || command.Any(char.IsControl) || command.Contains('%')) return null;
        string path;
        string arguments;
        if (command[0] == '"')
        {
            var end = command.IndexOf('"', 1);
            if (end < 2 || end + 1 < command.Length && command[end + 1] != ' ') return null;
            path = command[1..end];
            arguments = command[(end + 1)..];
        }
        else
        {
            var end = command.IndexOf(' ');
            path = end < 0 ? command : command[..end];
            arguments = end < 0 ? "" : command[end..];
        }
        // Quoted/escaped launcher arguments are outside this small subset. Arguments are never inspected as targets.
        if (arguments.Contains('"') || arguments.IndexOfAny(['&', '|', '<', '>']) >= 0 || !LocalExe(path)) return null;
        var name = path[(path.LastIndexOf('\\') + 1)..];
        return Launchers.Contains(name) ? null : path;
    }

    public static bool LocalExe(string? path)
    {
        if (path is null || path.Length is < 7 or > 260 || !char.IsAsciiLetter(path[0]) || path[1] != ':' || path[2] != '\\' ||
            !path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || path.Any(char.IsControl) ||
            path[2..].IndexOfAny([':', '/', '"', '<', '>', '|', '?', '*', '%']) >= 0) return false;
        var parts = path[3..].Split('\\');
        if (parts.Length is 0 or > 32) return false;
        foreach (var part in parts)
        {
            if (part.Length == 0 || part is "." or ".." || part.EndsWith('.') || part.EndsWith(' ')) return false;
            var stem = part.Split('.')[0].ToUpperInvariant();
            if (stem is "CON" or "PRN" or "AUX" or "NUL" or "CONIN$" or "CONOUT$" ||
                (stem.Length == 4 && (stem.StartsWith("COM", StringComparison.Ordinal) || stem.StartsWith("LPT", StringComparison.Ordinal)) &&
                "123456789¹²³".Contains(stem[3]))) return false;
        }
        return true;
    }
}
