using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GemmaChatWindows.Services;

public sealed class WorkspaceService : IDisposable
{
    private HttpListener? _listener;
    private CancellationTokenSource? _serverCts;
    private int _port;

    public static string WorkspacesRoot =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "gemma-chat", "workspaces-csharp");

    public int Port => _port;

    public string WorkspaceDir(string conversationId)
    {
        var safe = new string((conversationId ?? "default")
            .Select(ch => char.IsLetterOrDigit(ch) || ch is '_' or '-' ? ch : '_')
            .ToArray());
        if (string.IsNullOrWhiteSpace(safe)) safe = "default";
        return Path.Combine(WorkspacesRoot, safe);
    }

    public string PreviewUrl(string conversationId) =>
        $"http://127.0.0.1:{_port}/{Uri.EscapeDataString(conversationId)}/?v={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

    public void EnsureWorkspace(string conversationId)
    {
        Directory.CreateDirectory(WorkspaceDir(conversationId));
    }

    public async Task StartPreviewServerAsync(CancellationToken cancellationToken = default)
    {
        if (_listener != null) return;
        Directory.CreateDirectory(WorkspacesRoot);

        for (var port = 48731; port < 48850; port++)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add($"http://127.0.0.1:{port}/");
            try
            {
                listener.Start();
                _listener = listener;
                _port = port;
                break;
            }
            catch
            {
                listener.Close();
            }
        }

        if (_listener == null)
            throw new InvalidOperationException("Could not start workspace preview server.");

        _serverCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = Task.Run(() => ServeLoopAsync(_serverCts.Token), _serverCts.Token);
        await Task.CompletedTask;
    }

    public List<WorkspaceFileInfo> ListFiles(string conversationId)
    {
        EnsureWorkspace(conversationId);
        var root = WorkspaceDir(conversationId);
        return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Select(path => new FileInfo(path))
            .OrderBy(f => f.FullName, StringComparer.OrdinalIgnoreCase)
            .Select(f => new WorkspaceFileInfo
            {
                Name = Path.GetRelativePath(root, f.FullName).Replace('\\', '/'),
                Size = f.Length
            })
            .ToList();
    }

    public string ReadFile(string conversationId, string relativePath)
    {
        var path = ResolveInWorkspace(conversationId, relativePath);
        return File.Exists(path) ? File.ReadAllText(path) : "";
    }

    public void WriteFile(string conversationId, string relativePath, string content)
    {
        var path = ResolveInWorkspace(conversationId, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, CleanFileContent(content, relativePath), Encoding.UTF8);
    }

    public int EditFile(string conversationId, string relativePath, string oldString, string newString, bool replaceAll)
    {
        var path = ResolveInWorkspace(conversationId, relativePath);
        if (!File.Exists(path)) throw new FileNotFoundException($"File not found: {relativePath}");
        var content = File.ReadAllText(path);
        var count = CountOccurrences(content, oldString);
        if (count == 0) throw new InvalidOperationException($"Could not find text in {relativePath}.");
        if (!replaceAll && count > 1) throw new InvalidOperationException($"Text appears {count} times in {relativePath}; use replace_all.");
        content = replaceAll ? content.Replace(oldString, newString) : ReplaceFirst(content, oldString, newString);
        File.WriteAllText(path, content, Encoding.UTF8);
        return replaceAll ? count : 1;
    }

    public void DeleteFile(string conversationId, string relativePath)
    {
        var path = ResolveInWorkspace(conversationId, relativePath);
        if (File.Exists(path)) File.Delete(path);
        if (Directory.Exists(path)) Directory.Delete(path, true);
    }

    public string ExportZip(string conversationId)
    {
        EnsureWorkspace(conversationId);
        var source = WorkspaceDir(conversationId);
        var zip = Path.Combine(Path.GetTempPath(), $"gemma-chat-{conversationId}-{DateTime.Now:yyyyMMddHHmmss}.zip");
        if (File.Exists(zip)) File.Delete(zip);
        ZipFile.CreateFromDirectory(source, zip);
        return zip;
    }

    public List<GemmaChatWindows.ViewModels.ProjectFile> LoadProjectFiles(string conversationId)
    {
        EnsureWorkspace(conversationId);
        var root = WorkspaceDir(conversationId);
        return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(path => new FileInfo(path).Length <= 512_000)
            .Select(path => new GemmaChatWindows.ViewModels.ProjectFile
            {
                Name = Path.GetRelativePath(root, path).Replace('\\', '/'),
                Content = File.ReadAllText(path),
                Language = Path.GetExtension(path).TrimStart('.')
            })
            .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task ServeLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _listener != null)
        {
            HttpListenerContext ctx;
            try
            {
                ctx = await _listener.GetContextAsync();
            }
            catch
            {
                if (cancellationToken.IsCancellationRequested) break;
                continue;
            }

            _ = Task.Run(() => HandleRequestAsync(ctx), cancellationToken);
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext ctx)
    {
        try
        {
            var parts = ctx.Request.Url?.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries) ?? [];
            if (parts.Length == 0)
            {
                await WriteTextAsync(ctx, "gemma-chat preview server", "text/plain");
                return;
            }

            var conversationId = Uri.UnescapeDataString(parts[0]);
            var rel = parts.Length == 1 ? "index.html" : string.Join('/', parts.Skip(1).Select(Uri.UnescapeDataString));
            var path = ResolveInWorkspace(conversationId, rel);
            if (Directory.Exists(path)) path = Path.Combine(path, "index.html");
            if (!File.Exists(path))
            {
                ctx.Response.StatusCode = 404;
                await WriteTextAsync(ctx, "Not found", "text/plain");
                return;
            }

            var bytes = await File.ReadAllBytesAsync(path);
            ctx.Response.ContentType = ContentTypeFor(path);
            ctx.Response.ContentLength64 = bytes.LongLength;
            await ctx.Response.OutputStream.WriteAsync(bytes);
        }
        catch (Exception ex)
        {
            ctx.Response.StatusCode = 500;
            await WriteTextAsync(ctx, ex.Message, "text/plain");
        }
        finally
        {
            try { ctx.Response.Close(); } catch { }
        }
    }

    private static async Task WriteTextAsync(HttpListenerContext ctx, string text, string contentType)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        ctx.Response.ContentType = contentType;
        ctx.Response.ContentLength64 = bytes.Length;
        await ctx.Response.OutputStream.WriteAsync(bytes);
    }

    private string ResolveInWorkspace(string conversationId, string relativePath)
    {
        EnsureWorkspace(conversationId);
        var root = Path.GetFullPath(WorkspaceDir(conversationId));
        var target = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Path escapes workspace: {relativePath}");
        return target;
    }

    private static string ContentTypeFor(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".html" or ".htm" => "text/html; charset=utf-8",
        ".css" => "text/css; charset=utf-8",
        ".js" => "text/javascript; charset=utf-8",
        ".json" => "application/json; charset=utf-8",
        ".svg" => "image/svg+xml",
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        _ => "application/octet-stream"
    };

    private static string CleanFileContent(string raw, string path)
    {
        var s = raw.Trim();
        var full = Regex.Match(s, "^```[a-zA-Z0-9_-]*\\s*\\r?\\n([\\s\\S]*?)\\r?\\n```");
        if (full.Success) s = full.Groups[1].Value;
        if (path.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            var end = s.LastIndexOf("</html>", StringComparison.OrdinalIgnoreCase);
            if (end >= 0) s = s[..(end + "</html>".Length)] + "\n";
        }
        return s;
    }

    private static int CountOccurrences(string text, string needle)
    {
        if (string.IsNullOrEmpty(needle)) return 0;
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(needle, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += needle.Length;
        }
        return count;
    }

    private static string ReplaceFirst(string text, string oldString, string newString)
    {
        var index = text.IndexOf(oldString, StringComparison.Ordinal);
        return index < 0 ? text : text[..index] + newString + text[(index + oldString.Length)..];
    }

    public void Dispose()
    {
        _serverCts?.Cancel();
        _listener?.Close();
        _serverCts?.Dispose();
    }
}

public sealed class WorkspaceFileInfo
{
    public string Name { get; set; } = "";
    public long Size { get; set; }
}
