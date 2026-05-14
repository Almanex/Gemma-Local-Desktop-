using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace GemmaChatWindows.Services;

public class LlamaService
{
    private Process? _serverProcess;
    private readonly int _port = 11435;
    private readonly HttpClient _httpClient = new();
    private const string ReleaseTag = "b9145";

    private static readonly Dictionary<string, string> ModelUrls = new()
    {
        ["gemma-4-e2b-it"] = "https://huggingface.co/bartowski/google_gemma-4-E2B-it-GGUF/resolve/main/google_gemma-4-E2B-it-Q4_K_M.gguf",
        ["gemma-4-e4b-it"] = "https://huggingface.co/bartowski/google_gemma-4-E4B-it-GGUF/resolve/main/google_gemma-4-E4B-it-Q4_K_M.gguf",
        ["gemma-4-26b-a4b-it"] = "https://huggingface.co/bartowski/google_gemma-4-26B-A4B-it-GGUF/resolve/main/google_gemma-4-26B-A4B-it-Q4_K_M.gguf",
        ["gemma-4-31b-it"] = "https://huggingface.co/bartowski/google_gemma-4-31B-it-GGUF/resolve/main/google_gemma-4-31B-it-Q4_K_M.gguf"
    };

    public static string DataDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "gemma-chat");

    public static string BinDir => Path.Combine(DataDir, "bin");
    public static string ModelsDir => Path.Combine(DataDir, "models");
    public static string ServerPath => Path.Combine(BinDir, "llama-server.exe");

    public static string ModelPath(string modelName) =>
        Path.Combine(ModelsDir, $"{modelName}-Q4_K_M.gguf".ToLowerInvariant());

    public bool HasRuntime(string modelName) => File.Exists(ServerPath) && File.Exists(ModelPath(modelName));

    public async Task InstallAsync(string modelName, IProgress<(string Message, double? Progress)>? progress = null, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(DataDir);
        Directory.CreateDirectory(BinDir);
        Directory.CreateDirectory(ModelsDir);

        if (!File.Exists(ServerPath))
        {
            var gpu = DetectNvidiaGpu();
            var serverZip = Path.Combine(DataDir, "tmp_llama.zip");
            var dllZip = Path.Combine(DataDir, "tmp_cudart.zip");
            var (serverUrl, dllUrl) = GetServerDownloadUrls(gpu);

            progress?.Report((gpu.Available ? $"Downloading llama.cpp CUDA {gpu.CudaVersion} server..." : "Downloading llama.cpp CPU server...", 0.05));
            await DownloadFileAsync(serverUrl, serverZip, progress, cancellationToken, 0.05, 0.2);

            if (dllUrl != null)
            {
                progress?.Report(("Downloading CUDA runtime libraries...", 0.2));
                await DownloadFileAsync(dllUrl, dllZip, progress, cancellationToken, 0.2, 0.28);
            }

            progress?.Report(("Extracting llama.cpp server...", 0.28));
            if (Directory.Exists(BinDir))
                Directory.CreateDirectory(BinDir);
            ZipFile.ExtractToDirectory(serverZip, BinDir, true);
            if (File.Exists(dllZip))
                ZipFile.ExtractToDirectory(dllZip, BinDir, true);
            TryDelete(serverZip);
            TryDelete(dllZip);

            if (!File.Exists(ServerPath))
                throw new FileNotFoundException("llama-server.exe was not found after extraction.", ServerPath);
        }

        var modelPath = ModelPath(modelName);
        if (!File.Exists(modelPath))
        {
            if (!ModelUrls.TryGetValue(modelName, out var modelUrl))
                throw new InvalidOperationException($"Unknown model: {modelName}");

            progress?.Report(("Downloading Gemma model...", 0.35));
            await DownloadFileAsync(modelUrl, modelPath, progress, cancellationToken, 0.35, 0.85);
        }
    }

    public async Task StartServerAsync(string modelPath, int gpuLayers = 0, IProgress<(string Message, double? Progress)>? progress = null, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(ServerPath) || !File.Exists(modelPath))
        {
            throw new FileNotFoundException("Server or model not found. Run setup first.");
        }

        StopServer();
        progress?.Report(("Starting llama.cpp server...", 0.9));
        if (gpuLayers <= 0)
            gpuLayers = RecommendedGpuLayers(Path.GetFileName(modelPath));

        var startInfo = new ProcessStartInfo
        {
            FileName = ServerPath,
            Arguments = $"--model \"{modelPath}\" --n-gpu-layers {gpuLayers} --ctx-size 8192 --host 127.0.0.1 --port {_port}",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        _serverProcess = Process.Start(startInfo);

        _ = Task.Run(() => 
        {
            while (_serverProcess != null && !_serverProcess.StandardOutput.EndOfStream)
            {
                var line = _serverProcess.StandardOutput.ReadLine();
                Debug.WriteLine($"[llama] {line}");
            }
        });

        await WaitForHealthAsync(TimeSpan.FromMinutes(2), cancellationToken);
        progress?.Report(("Server ready", 1));
    }

    public void StopServer()
    {
        if (_serverProcess != null && !_serverProcess.HasExited)
        {
            _serverProcess.Kill();
            _serverProcess.Dispose();
            _serverProcess = null;
        }
    }

    public async IAsyncEnumerable<string> ChatStreamAsync(string systemPrompt, string userMessage, double temperature = 0.7, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            model = "gemma:2b",
            messages = new[]
            {
                new { role = "user", content = systemPrompt + "\n\n" + userMessage }
            },
            stream = true,
            temperature = temperature,
            max_tokens = 8192
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        
        HttpRequestMessage? request = null;
        string? connectionError = null;
        try 
        {
            request = new HttpRequestMessage(HttpMethod.Post, $"http://127.0.0.1:{_port}/v1/chat/completions")
            {
                Content = content
            };
        }
        catch { yield break; }
        
        HttpResponseMessage? response = null;
        try 
        {
            response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            connectionError = $"\n\n[Ошибка подключения к серверу Llama: {ex.Message}]";
        }

        if (connectionError != null)
        {
            yield return connectionError;
            yield break;
        }

        if (response == null) yield break;

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line == null) break;
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ")) continue;
            
            var data = line.Substring(6);
            if (data == "[DONE]") break;

            using var doc = JsonDocument.Parse(data);
            var root = doc.RootElement;
            if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var delta = choices[0].GetProperty("delta");
                if (delta.TryGetProperty("content", out var textToken))
                {
                    yield return textToken.GetString() ?? "";
                }
            }
        }
    }

    private async Task DownloadFileAsync(
        string url,
        string outputPath,
        IProgress<(string Message, double? Progress)>? progress,
        CancellationToken cancellationToken,
        double start = 0,
        double end = 1)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var total = response.Content.Headers.ContentLength;
        await using var input = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var output = File.Create(outputPath);
        var buffer = new byte[1024 * 128];
        long done = 0;

        while (true)
        {
            var read = await input.ReadAsync(buffer, cancellationToken);
            if (read == 0) break;
            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            done += read;

            if (total is > 0)
            {
                var pct = done / (double)total.Value;
                progress?.Report(($"Downloading... {FormatBytes(done)} / {FormatBytes(total.Value)}", start + (end - start) * pct));
            }
        }
    }

    private async Task WaitForHealthAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        var deadline = DateTimeOffset.UtcNow + timeout;
        while (DateTimeOffset.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using var res = await _httpClient.GetAsync($"http://127.0.0.1:{_port}/health", cancellationToken);
                if (res.IsSuccessStatusCode) return;
            }
            catch
            {
                // Server is still warming up.
            }

            await Task.Delay(1000, cancellationToken);
        }

        throw new TimeoutException("llama.cpp server did not become ready in time.");
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        double value = bytes;
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }
        return $"{value:0.#} {units[unit]}";
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch { }
    }

    private static (bool Available, int VramGb, string CudaVersion) DetectNvidiaGpu()
    {
        try
        {
            var name = RunNvidiaSmi("--query-gpu=name", "--format=csv,noheader");
            var memory = RunNvidiaSmi("--query-gpu=memory.total", "--format=csv,noheader,numeric");
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(memory))
                return (false, 0, "12.4");

            var firstMemory = memory.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
            var vramMb = int.TryParse(firstMemory, out var parsed) ? parsed : 0;
            var cuda = name.Contains("RTX 40", StringComparison.OrdinalIgnoreCase) ? "13.1" : "12.4";
            return (true, Math.Max(0, vramMb / 1024), cuda);
        }
        catch
        {
            return (false, 0, "12.4");
        }
    }

    private static string RunNvidiaSmi(params string[] args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "nvidia-smi",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        foreach (var arg in args)
            psi.ArgumentList.Add(arg);

        using var proc = Process.Start(psi);
        if (proc == null) return "";
        if (!proc.WaitForExit(5000)) return "";
        return proc.ExitCode == 0 ? proc.StandardOutput.ReadToEnd().Trim() : "";
    }

    private static (string Main, string? Dlls) GetServerDownloadUrls((bool Available, int VramGb, string CudaVersion) gpu)
    {
        var baseUrl = $"https://github.com/ggml-org/llama.cpp/releases/download/{ReleaseTag}";
        if (!gpu.Available)
            return ($"{baseUrl}/llama-b9145-bin-win-cpu-x64.zip", null);

        if (gpu.CudaVersion == "13.1")
            return ($"{baseUrl}/llama-b9145-bin-win-cuda-13.1-x64.zip", $"{baseUrl}/cudart-llama-bin-win-cuda-13.1-x64.zip");

        return ($"{baseUrl}/llama-b9145-bin-win-cuda-12.4-x64.zip", $"{baseUrl}/cudart-llama-bin-win-cuda-12.4-x64.zip");
    }

    private static int RecommendedGpuLayers(string modelPath)
    {
        var gpu = DetectNvidiaGpu();
        if (!gpu.Available) return 0;

        var lower = modelPath.ToLowerInvariant();
        if (lower.Contains("e2b")) return 35;
        if (lower.Contains("e4b")) return 45;
        if (lower.Contains("26b")) return 40;
        if (lower.Contains("31b")) return 30;
        return 35;
    }
}
