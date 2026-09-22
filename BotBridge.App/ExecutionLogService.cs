using System.Text;
using BotBridge.Core.Interfaces;
using BotBridge.Core.Models;

namespace BotBridge.Infrastructure.Logging;

/// <summary>
/// Tails every .txt file in the UiPath log folder.
///
/// Each file is tracked by the byte offset of the last complete line
/// that was read, so:
///  - new files are picked up automatically,
///  - appended data is read exactly once (no duplicated lines),
///  - a partially written last line is shown but re-read on the next
///    cycle once it is complete,
///  - truncated, rotated or replaced files are detected (shrunk length,
///    changed creation time or changed file header) and re-read from
///    the start instead of being appended to stale content,
///  - deleted files disappear from the list,
///  - I/O errors on one file never affect the others.
/// </summary>
public sealed class ExecutionLogService : IExecutionLogService
{
    private const int MaxCachedChars = 512 * 1024;

    private const int InitialTailBytes = 512 * 1024;

    private const int MaxReadBytes = 8 * 1024 * 1024;

    private const int FingerprintLength = 64;

    private readonly string _folder;

    private readonly SemaphoreSlim _refreshGate = new(1, 1);

    private readonly object _stateLock = new();

    private readonly Dictionary<string, TrackedLog> _tracked =
        new(StringComparer.OrdinalIgnoreCase);

    private IReadOnlyList<ExecutionLogFile> _lastFiles =
        Array.Empty<ExecutionLogFile>();

    public ExecutionLogService(string folderPath)
    {
        _folder = folderPath;
    }

    public string FolderPath => _folder;

    public async Task<ExecutionLogSnapshot> RefreshAsync(
        CancellationToken cancellationToken = default)
    {
        await _refreshGate
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            return await Task
                .Run(() => RefreshCore(), cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            _refreshGate.Release();
        }
    }

    public async Task<string> GetContentAsync(
        string fullPath,
        CancellationToken cancellationToken = default)
    {
        var tracked = FindTracked(fullPath);

        if (tracked is null)
        {
            await RefreshAsync(cancellationToken)
                .ConfigureAwait(false);

            tracked = FindTracked(fullPath);
        }

        if (tracked is null)
        {
            return File.Exists(fullPath)
                ? "Log file is not available yet."
                : "Log file no longer exists.";
        }

        return tracked.GetText();
    }

    private TrackedLog? FindTracked(string fullPath)
    {
        lock (_stateLock)
        {
            return _tracked.TryGetValue(
                fullPath,
                out var tracked)
                ? tracked
                : null;
        }
    }

    private ExecutionLogSnapshot RefreshCore()
    {
        List<FileInfo> files;

        try
        {
            if (!Directory.Exists(_folder))
            {
                lock (_stateLock)
                {
                    _tracked.Clear();
                }

                _lastFiles =
                    Array.Empty<ExecutionLogFile>();

                return new ExecutionLogSnapshot
                {
                    Files = _lastFiles,
                    FolderAvailable = false,
                    Error =
                        $"Folder not found or not accessible: {_folder}"
                };
            }

            files =
                new DirectoryInfo(_folder)
                    .EnumerateFiles(
                        "*.log",
                        SearchOption.TopDirectoryOnly)
                    .Where(x =>
                        string.Equals(
                            x.Extension,
                            ".log",
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
        }
        catch (Exception ex) when (
            ex is IOException or
            UnauthorizedAccessException or
            global::System.Security.SecurityException)
        {
            // Keep whatever was known; the next cycle retries.
            return new ExecutionLogSnapshot
            {
                Files = _lastFiles,
                FolderAvailable = false,
                Error = ex.Message
            };
        }

        var present =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        var result = new List<ExecutionLogFile>();

        foreach (var file in files.OrderByDescending(
                     x => x.LastWriteTimeUtc))
        {
            present.Add(file.FullName);

            TrackedLog? tracked;

            lock (_stateLock)
            {
                if (!_tracked.TryGetValue(
                        file.FullName,
                        out tracked))
                {
                    tracked = new TrackedLog();

                    _tracked[file.FullName] = tracked;
                }
            }

            try
            {
                tracked.Update(file);
            }
            catch (Exception ex) when (
                ex is IOException or
                UnauthorizedAccessException)
            {
                // The file may be locked or mid-rotation.
                // Keep the previous content and retry next cycle.
            }

            result.Add(
                new ExecutionLogFile
                {
                    FileName = file.Name,
                    FullPath = file.FullName,
                    LastModified = file.LastWriteTime,
                    SizeInBytes = file.Length
                });
        }

        lock (_stateLock)
        {
            var removed =
                _tracked.Keys
                    .Where(x => !present.Contains(x))
                    .ToList();

            foreach (var key in removed)
            {
                _tracked.Remove(key);
            }
        }

        _lastFiles = result;

        return new ExecutionLogSnapshot
        {
            Files = result,
            FolderAvailable = true,
            Error = null
        };
    }

    private enum TextKind
    {
        Utf8,
        Utf16LittleEndian,
        Utf16BigEndian
    }

    private sealed class TrackedLog
    {
        private readonly object _lock = new();

        private readonly StringBuilder _committed = new();

        private string _tail = string.Empty;

        private bool _truncated;

        private bool _initialised;

        private long _committedOffset;

        private long _lastLength;

        private DateTime _lastWriteUtc;

        private DateTime _creationUtc;

        private byte[] _fingerprint = Array.Empty<byte>();

        private TextKind _kind = TextKind.Utf8;

        private int _bomLength;

        public string GetText()
        {
            lock (_lock)
            {
                var text =
                    _committed.ToString() + _tail;

                return _truncated
                    ? "... earlier content omitted (showing the most recent part of this file) ..." +
                      Environment.NewLine +
                      text
                    : text;
            }
        }

        public void Update(FileInfo file)
        {
            var length = file.Length;
            var creationUtc = file.CreationTimeUtc;
            var lastWriteUtc = file.LastWriteTimeUtc;

            lock (_lock)
            {
                if (_initialised &&
                    length == _lastLength &&
                    lastWriteUtc == _lastWriteUtc &&
                    creationUtc == _creationUtc)
                {
                    return;
                }

                using var stream =
                    new FileStream(
                        file.FullName,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite | FileShare.Delete,
                        4096,
                        FileOptions.SequentialScan);

                var actualLength = stream.Length;

                // Truncated, rotated (recreated) or replaced file:
                // discard the cache and read it again from the start.
                if (_initialised &&
                    (actualLength < _committedOffset ||
                     creationUtc != _creationUtc ||
                     !HeadMatches(stream)))
                {
                    ResetState();
                }

                if (!_initialised)
                {
                    CaptureFingerprint(
                        stream,
                        actualLength);
                }
                else if (_fingerprint.Length < FingerprintLength &&
                         actualLength > _fingerprint.Length)
                {
                    CaptureFingerprint(
                        stream,
                        actualLength);
                }

                if (_committedOffset == 0)
                {
                    DetectKind();
                }

                ReadNewData(
                    stream,
                    actualLength);

                _lastLength = length;
                _lastWriteUtc = lastWriteUtc;
                _creationUtc = creationUtc;
                _initialised = true;
            }
        }

        private void ReadNewData(
            FileStream stream,
            long actualLength)
        {
            var startOffset = _committedOffset;

            var available = actualLength - startOffset;

            if (available <= 0)
            {
                _tail = string.Empty;

                return;
            }

            var skippedAhead = false;

            var limit =
                startOffset == 0
                    ? InitialTailBytes
                    : MaxReadBytes;

            if (available > limit)
            {
                // Huge file (or huge gap): only read the most
                // recent part instead of exhausting memory.
                var tailStart =
                    actualLength - InitialTailBytes;

                if (_kind != TextKind.Utf8)
                {
                    tailStart -= tailStart % 2;
                }

                startOffset =
                    Math.Max(tailStart, startOffset);

                available =
                    actualLength - startOffset;

                skippedAhead = true;
            }

            var buffer = new byte[available];

            stream.Position = startOffset;

            var read =
                ReadFully(
                    stream,
                    buffer,
                    buffer.Length);

            if (read <= 0)
            {
                return;
            }

            var index = 0;

            if (startOffset == 0)
            {
                index = Math.Min(_bomLength, read);
            }

            if (skippedAhead)
            {
                _committed.Clear();

                _tail = string.Empty;

                _truncated = true;

                index = FindFirstLineStart(
                    buffer,
                    read);
            }

            var end =
                FindLastLineEnd(
                    buffer,
                    index,
                    read);

            var encoding = GetEncoding();

            if (end > index)
            {
                AppendCommitted(
                    encoding.GetString(
                        buffer,
                        index,
                        end - index));
            }

            _tail =
                end < read
                    ? encoding.GetString(
                        buffer,
                        end,
                        read - end)
                    : string.Empty;

            _committedOffset = startOffset + end;
        }

        private void AppendCommitted(string text)
        {
            _committed.Append(text);

            if (_committed.Length <= MaxCachedChars)
            {
                return;
            }

            var all = _committed.ToString();

            var cut = all.Length - MaxCachedChars;

            var newline = all.IndexOf('\n', cut);

            var start =
                newline >= 0
                    ? newline + 1
                    : cut;

            _committed.Clear();

            _committed.Append(
                all,
                start,
                all.Length - start);

            _truncated = true;
        }

        private void ResetState()
        {
            _committed.Clear();

            _tail = string.Empty;

            _truncated = false;

            _initialised = false;

            _committedOffset = 0;

            _lastLength = 0;

            _lastWriteUtc = default;

            _creationUtc = default;

            _fingerprint = Array.Empty<byte>();

            _kind = TextKind.Utf8;

            _bomLength = 0;
        }

        private bool HeadMatches(FileStream stream)
        {
            if (_fingerprint.Length == 0)
            {
                return true;
            }

            var buffer = new byte[_fingerprint.Length];

            stream.Position = 0;

            var read =
                ReadFully(
                    stream,
                    buffer,
                    buffer.Length);

            if (read < _fingerprint.Length)
            {
                return false;
            }

            for (var i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] != _fingerprint[i])
                {
                    return false;
                }
            }

            return true;
        }

        private void CaptureFingerprint(
            FileStream stream,
            long actualLength)
        {
            var wanted =
                (int)Math.Min(
                    FingerprintLength,
                    actualLength);

            var buffer = new byte[wanted];

            stream.Position = 0;

            var read =
                ReadFully(
                    stream,
                    buffer,
                    wanted);

            _fingerprint =
                read == wanted
                    ? buffer
                    : buffer[..read];
        }

        private void DetectKind()
        {
            var head = _fingerprint;

            if (head.Length >= 3 &&
                head[0] == 0xEF &&
                head[1] == 0xBB &&
                head[2] == 0xBF)
            {
                _kind = TextKind.Utf8;
                _bomLength = 3;
            }
            else if (head.Length >= 2 &&
                     head[0] == 0xFF &&
                     head[1] == 0xFE)
            {
                _kind = TextKind.Utf16LittleEndian;
                _bomLength = 2;
            }
            else if (head.Length >= 2 &&
                     head[0] == 0xFE &&
                     head[1] == 0xFF)
            {
                _kind = TextKind.Utf16BigEndian;
                _bomLength = 2;
            }
            else
            {
                _kind = TextKind.Utf8;
                _bomLength = 0;
            }
        }

        private Encoding GetEncoding()
        {
            return _kind switch
            {
                TextKind.Utf16LittleEndian =>
                    Encoding.Unicode,

                TextKind.Utf16BigEndian =>
                    Encoding.BigEndianUnicode,

                _ =>
                    Encoding.UTF8
            };
        }

        /// <summary>
        /// Returns the exclusive end index of the last complete
        /// line inside buffer[from..count), or <paramref name="from"/>
        /// when no complete line is present.
        /// </summary>
        private int FindLastLineEnd(
            byte[] buffer,
            int from,
            int count)
        {
            if (_kind == TextKind.Utf8)
            {
                for (var i = count - 1; i >= from; i--)
                {
                    if (buffer[i] == 0x0A)
                    {
                        return i + 1;
                    }
                }

                return from;
            }

            var evenCount = count - (count % 2);

            for (var i = evenCount - 2; i >= from; i -= 2)
            {
                if (IsUtf16Newline(buffer, i))
                {
                    return i + 2;
                }
            }

            return from;
        }

        /// <summary>
        /// Returns the index just after the first newline in
        /// buffer[0..count), or count when there is none.
        /// </summary>
        private int FindFirstLineStart(
            byte[] buffer,
            int count)
        {
            if (_kind == TextKind.Utf8)
            {
                for (var i = 0; i < count; i++)
                {
                    if (buffer[i] == 0x0A)
                    {
                        return i + 1;
                    }
                }

                return count;
            }

            for (var i = 0; i + 1 < count; i += 2)
            {
                if (IsUtf16Newline(buffer, i))
                {
                    return i + 2;
                }
            }

            return count;
        }

        private bool IsUtf16Newline(
            byte[] buffer,
            int index)
        {
            return _kind == TextKind.Utf16LittleEndian
                ? buffer[index] == 0x0A &&
                  buffer[index + 1] == 0x00
                : buffer[index] == 0x00 &&
                  buffer[index + 1] == 0x0A;
        }

        private static int ReadFully(
            Stream stream,
            byte[] buffer,
            int count)
        {
            var total = 0;

            while (total < count)
            {
                var read =
                    stream.Read(
                        buffer,
                        total,
                        count - total);

                if (read <= 0)
                {
                    break;
                }

                total += read;
            }

            return total;
        }
    }
}
