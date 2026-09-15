using Orthonis.Core;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Orthonis.Tests")]

namespace Orthonis.Cli;

// Single-cooperating-writer local file store. Not protection against a hostile local administrator.
public sealed class CaseStore : IDisposable
{
    private readonly string directory;
    private readonly FileStream lease;
    private string CasePath => Path.Combine(directory, "case.json");

    public CaseStore(string path, bool create = false)
    {
        directory = Path.GetFullPath(path);
        CheckParents(directory);
        if (create) Directory.CreateDirectory(directory);
        Contract.Require(Directory.Exists(directory), "Case directory does not exist.");
        CheckParents(directory);
        CheckParents(Path.Combine(directory, ".case.lock"));
        lease = new FileStream(Path.Combine(directory, ".case.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
    }

    public CaseSnapshot Load()
    {
        var value = JsonCodec.Decode<CaseSnapshot>(ReadBounded(CasePath, JsonCodec.MaxCaseBytes));
        Contract.Validate(value);
        return value;
    }

    public void Save(CaseSnapshot value, int? expectedRevision) => Save(value, expectedRevision, null);

    // Fault injection is internal to the regression assembly; never exposed through a plan or CLI.
    internal void Save(CaseSnapshot value, int? expectedRevision, Action? beforeReplace)
    {
        Contract.Validate(value);
        CheckParents(CasePath);
        if (expectedRevision is null)
            Contract.Require(!File.Exists(CasePath), "Case already exists; initialization never overwrites it.");
        else
        {
            var current = Load();
            Contract.Require(current.Revision == expectedRevision && current.CaseId == value.CaseId &&
                value.Revision == expectedRevision + 1, "Case changed or revision is not sequential.");
        }
        var bytes = JsonCodec.Encode(value);
        Contract.Require(bytes.Length <= JsonCodec.MaxCaseBytes, "Case storage limit exceeded.");
        var temp = Path.Combine(directory, $".case-{Guid.NewGuid():N}.tmp");
        try
        {
            using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes);
                stream.Flush(flushToDisk: true);
            }
            beforeReplace?.Invoke();
            File.Move(temp, CasePath, overwrite: expectedRevision is not null);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }

    public static byte[] ReadBounded(string path, int maximum)
    {
        CheckParents(Path.GetFullPath(path));
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var buffer = new MemoryStream();
        byte[] chunk = new byte[8192];
        int read;
        while ((read = stream.Read(chunk, 0, Math.Min(chunk.Length, maximum + 1 - (int)buffer.Length))) > 0)
        {
            buffer.Write(chunk, 0, read);
            Contract.Require(buffer.Length <= maximum, "Input exceeds size limit.");
        }
        return buffer.ToArray();
    }

    private static void CheckParents(string path)
    {
        for (var current = path; current is not null; current = Path.GetDirectoryName(current))
            if (Path.Exists(current))
                Contract.Require((File.GetAttributes(current) & FileAttributes.ReparsePoint) == 0, "Linked case paths are unsupported.");
    }

    public void Dispose() => lease.Dispose();
}
