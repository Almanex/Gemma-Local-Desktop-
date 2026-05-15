using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GemmaChatWindows.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GemmaChatWindows.ViewModels;

public partial class ChatMessage : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string _role = "user";

    [ObservableProperty]
    private string _content = "";

    [ObservableProperty]
    private bool _isStreaming = false;

    [ObservableProperty]
    private ObservableCollection<AssistantToolCard> _toolCards = new();

    public bool IsUser => Role == "user";
}

public partial class AssistantToolCard : ObservableObject
{
    [ObservableProperty]
    private string _title = "";

    [ObservableProperty]
    private string _target = "";

    [ObservableProperty]
    private string _content = "";

    [ObservableProperty]
    private string _result = "";

    [ObservableProperty]
    private bool _isOpen = true;

    [ObservableProperty]
    private bool _isRunning = true;
}

public partial class ProjectFile : ObservableObject
{
    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private string _content = "";

    [ObservableProperty]
    private string _language = "";
}

public class AIModelInfo
{
    public string Name { get; set; } = "";
    public string Label { get; set; } = "";
    public string Size { get; set; } = "";
    public string Description { get; set; } = "";
}

public partial class Conversation : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShortTitleIcon))]
    private string _title = "New chat";

    [ObservableProperty]
    private ObservableCollection<ChatMessage> _messages = new();

    [ObservableProperty]
    private ObservableCollection<ProjectFile> _workspaceFiles = new();

    [ObservableProperty]
    private string _activeMode = "code";

    [ObservableProperty]
    private bool _canvasOpen = true;

    [ObservableProperty]
    private string _activeCanvasTab = "preview";

    [ObservableProperty]
    private string _currentFileName = "index.html";

    public bool IsEmpty => Messages.Count == 0;
    public string ShortTitleIcon
    {
        get
        {
            var text = string.IsNullOrWhiteSpace(Title) ? "N" : Title.Trim();
            return text.Length == 0 ? "N" : text[..1].ToUpperInvariant();
        }
    }
    public void UpdateEmptyState() => OnPropertyChanged(nameof(IsEmpty));
}

public partial class ChatViewModel : ObservableObject
{
    private readonly LlamaService _llamaService;
    private readonly WorkspaceService _workspaceService;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isRestoring;
    private static readonly string StorePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "gemma-chat",
        "csharp-conversations.json");

    [ObservableProperty]
    private ObservableCollection<Conversation> _conversations = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Messages))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private Conversation _activeConversation = null!;

    public ObservableCollection<ChatMessage> Messages => ActiveConversation?.Messages ?? new();
    public bool IsEmpty => ActiveConversation?.IsEmpty ?? true;

    [ObservableProperty]
    private string _inputText = "";

    [ObservableProperty]
    private string _currentCode = "";

    [ObservableProperty]
    private string _currentFileName = "index.html";

    [ObservableProperty]
    private int _currentLineCount = 0;

    [ObservableProperty]
    private int _currentCharCount = 0;

    [ObservableProperty]
    private string _lineNumbers = "1";

    [ObservableProperty]
    private string _previewHtml = "";

    [ObservableProperty]
    private string _previewSource = "";

    [ObservableProperty]
    private ObservableCollection<ProjectFile> _workspaceFiles = new();

    [ObservableProperty]
    private ProjectFile? _selectedFile;

    [ObservableProperty]
    private bool _isGenerating = false;

    [ObservableProperty]
    private string _setupMessage = "Welcome";

    [ObservableProperty]
    private double _setupProgress = 0;

    [ObservableProperty]
    private string _setupError = "";

    [ObservableProperty]
    private string _activeCanvasTab = "preview";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCanvasOpen))]
    [NotifyPropertyChangedFor(nameof(EmptyTitle))]
    [NotifyPropertyChangedFor(nameof(EmptySubtitle))]
    private string _activeMode = "code";

    public string EmptyTitle => ActiveMode == "code" ? "What should we build?" : "How can I help?";

    public string EmptySubtitle => ActiveMode == "code"
        ? "Gemma Local Desktop will write files into a workspace and show a live preview on the right."
        : "Everything runs locally on your PC. Privacy by design.";

    public bool IsCanvasOpen => _canvasOpen;
    private bool _canvasOpen = true;

    [ObservableProperty]
    private System.Windows.GridLength _canvasWidth = new(720);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSidebarExpanded))]
    private bool _isSidebarCollapsed = false;

    public bool IsSidebarExpanded => !IsSidebarCollapsed;

    public System.Windows.GridLength SidebarWidth => IsSidebarCollapsed
        ? new System.Windows.GridLength(56)
        : new System.Windows.GridLength(240);

    public string WorkspaceFileCountText => WorkspaceFiles.Count > 0 ? $"Files · {WorkspaceFiles.Count}" : "Files";

    [ObservableProperty]
    private bool _isSetupVisible = true;

    [ObservableProperty]
    private bool _isChatVisible = false;

    [ObservableProperty]
    private bool _isSettingsOpen = false;

    [ObservableProperty]
    private double _temperature = 0.7;

    [ObservableProperty]
    private int _gpuLayers = 0;

    [ObservableProperty]
    private string _customSystemPrompt = "";

    [ObservableProperty]
    private bool _autoOpenPreview = true;

    [ObservableProperty]
    private ObservableCollection<AIModelInfo> _availableModels = new()
    {
        new() { Name = "gemma-4-e2b-it", Label = "Gemma 4 E2B", Size = "1.5 GB", Description = "Edge-sized. Optimized for speed and efficiency. Text + Image + Audio." },
        new() { Name = "gemma-4-e4b-it", Label = "Gemma 4 E4B", Size = "3 GB", Description = "Best all-rounder. High quality multimodal capabilities. Recommended." },
        new() { Name = "gemma-4-26b-a4b-it", Label = "Gemma 4 26B (MoE)", Size = "15 GB", Description = "Mixture-of-Experts. Faster than 31B, near same quality. Requires 24GB+ RAM/VRAM." },
        new() { Name = "gemma-4-31b-it", Label = "Gemma 4 31B", Size = "18 GB", Description = "Frontier dense model. Best intelligence. Requires 32GB+ RAM/VRAM." }
    };

    [ObservableProperty]
    private AIModelInfo _selectedModel;

    public ChatViewModel(LlamaService llamaService, WorkspaceService workspaceService)
    {
        _llamaService = llamaService;
        _workspaceService = workspaceService;
        _selectedModel = AvailableModels.FirstOrDefault(m => m.Name == "gemma-4-e4b-it") ?? AvailableModels[0];
        _ = _workspaceService.StartPreviewServerAsync();
        if (!LoadState())
            NewChat();
    }

    partial void OnActiveConversationChanged(Conversation value)
    {
        if (value == null) return;
        RestoreConversation(value);
        SaveState();
    }

    partial void OnSelectedFileChanged(ProjectFile? value)
    {
        if (value == null) return;
        UpdateCurrentCode(value.Name, value.Content);
        CurrentFileName = value.Name;
        UpdateConversationShellState();
        SaveState();
    }

    [RelayCommand]
    private async Task StartSetupAsync()
    {
        SetupError = "";
        SetupProgress = 0;
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var progress = new Progress<(string Message, double? Progress)>(p =>
            {
                SetupMessage = p.Message;
                if (p.Progress.HasValue) SetupProgress = p.Progress.Value;
            });

            await _llamaService.InstallAsync(SelectedModel.Name, progress, _cancellationTokenSource.Token);
            await _llamaService.StartServerAsync(LlamaService.ModelPath(SelectedModel.Name), GpuLayers, progress, _cancellationTokenSource.Token);

            IsSetupVisible = false;
            IsChatVisible = true;
        }
        catch (OperationCanceledException)
        {
            SetupMessage = "Setup canceled";
        }
        catch (Exception ex)
        {
            SetupMessage = "Setup failed";
            SetupError = ex.Message;
        }
    }

    [RelayCommand]
    private void NewChat()
    {
        var conv = new Conversation
        {
            ActiveMode = ActiveMode,
            CanvasOpen = ActiveMode == "code"
        };
        Conversations.Insert(0, conv);
        ActiveConversation = conv;
        SaveState();
    }

    [RelayCommand]
    private void DeleteConversation(Conversation conv)
    {
        if (conv == null) return;
        Conversations.Remove(conv);
        if (Conversations.Count == 0) NewChat();
        else if (ActiveConversation == conv) ActiveConversation = Conversations.First();
        SaveState();
    }

    [RelayCommand]
    private void ToggleMode()
    {
        ActiveMode = ActiveMode == "chat" ? "code" : "chat";
        _canvasOpen = ActiveMode == "code";
        UpdateConversationShellState();
        OnPropertyChanged(nameof(IsCanvasOpen));
        SaveState();
    }

    [RelayCommand]
    private void ToggleCanvas()
    {
        _canvasOpen = !_canvasOpen;
        UpdateConversationShellState();
        OnPropertyChanged(nameof(IsCanvasOpen));
        SaveState();
    }

    [RelayCommand]
    private void ToggleSidebar()
    {
        IsSidebarCollapsed = !IsSidebarCollapsed;
        OnPropertyChanged(nameof(SidebarWidth));
        SaveState();
    }

    [RelayCommand]
    private void ToggleSettings()
    {
        IsSettingsOpen = !IsSettingsOpen;
        SaveState();
    }

    [RelayCommand]
    private void CloseSettings()
    {
        IsSettingsOpen = false;
        SaveState();
    }

    [RelayCommand]
    private void SetMode(string mode)
    {
        ActiveMode = mode;
        UpdateConversationShellState();
        SaveState();
    }

    [RelayCommand]
    private void SetCanvasTab(string tab)
    {
        ActiveCanvasTab = tab;
        UpdateConversationShellState();
        SaveState();
    }

    [RelayCommand]
    private void RefreshPreview() => UpdatePreviewHtml();

    [RelayCommand]
    private void OpenWorkspaceFolder()
    {
        if (ActiveConversation == null) return;
        SyncWorkspaceToDisk();
        try { System.Diagnostics.Process.Start("explorer.exe", _workspaceService.WorkspaceDir(ActiveConversation.Id)); } catch { }
    }

    [RelayCommand]
    private void OpenInBrowser()
    {
        if (ActiveConversation == null) return;
        SyncWorkspaceToDisk();
        var indexFile = Path.Combine(_workspaceService.WorkspaceDir(ActiveConversation.Id), "index.html");
        if (File.Exists(indexFile))
        {
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(indexFile) { UseShellExecute = true }); } catch { }
        }
    }

    [RelayCommand]
    private void ExportProject()
    {
        if (ActiveConversation == null) return;
        SyncWorkspaceToDisk();
        try
        {
            var zip = _workspaceService.ExportZip(ActiveConversation.Id);
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{zip}\"");
        }
        catch { }
    }

    [RelayCommand]
    private void RegenerateMessage(ChatMessage msg)
    {
        if (msg == null || ActiveConversation == null || IsGenerating) return;
        var idx = ActiveConversation.Messages.IndexOf(msg);
        if (idx < 0) return;

        var lastUser = ActiveConversation.Messages.Take(idx).LastOrDefault(m => m.Role == "user");
        if (lastUser == null) return;

        while (ActiveConversation.Messages.Count > idx)
            ActiveConversation.Messages.RemoveAt(idx);

        InputText = lastUser.Content;
        ActiveConversation.Messages.Remove(lastUser);
        ActiveConversation.UpdateEmptyState();
        OnPropertyChanged(nameof(IsEmpty));
        SendMessageCommand.Execute(null);
    }

    [RelayCommand]
    private void CopyMessage(ChatMessage msg)
    {
        if (msg == null) return;
        System.Windows.Clipboard.SetText(msg.Content);
    }

    [RelayCommand]
    private void StopGeneration() => _cancellationTokenSource?.Cancel();

    [RelayCommand]
    private void SetInputText(string text)
    {
        InputText = text;
        SendMessageCommand.Execute(null);
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(InputText) || IsGenerating || ActiveConversation == null)
            return;

        var currentInput = InputText.Trim();
        var userMsg = new ChatMessage { Role = "user", Content = currentInput };
        ActiveConversation.Messages.Add(userMsg);

        if (ActiveConversation.Messages.Count == 1)
            ActiveConversation.Title = currentInput.Length > 48 ? currentInput[..48] + "..." : currentInput;

        ActiveConversation.UpdateEmptyState();
        OnPropertyChanged(nameof(IsEmpty));

        InputText = "";
        IsGenerating = true;
        CurrentCode = "";

        var assistantMsg = new ChatMessage { Role = "assistant", Content = "", IsStreaming = true };
        ActiveConversation.Messages.Add(assistantMsg);
        SaveState();

        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var systemPrompt = ActiveMode == "code"
                ? BuildCodeSystemPrompt()
                : "You are Gemma, a helpful local AI assistant. You are in DIALOGUE mode. Focus on conversation and answering questions. Do not attempt to use code-execution tools or write files to the workspace unless the user specifically asks you to 'Build' something. Use markdown for formatting.";
            var modelInput = ActiveMode == "code"
                ? BuildCodeUserMessage(currentInput)
                : BuildChatUserMessage(currentInput);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var rawContent = "";
            Dictionary<string, string> latestFiles = new();
            
            if (ActiveMode == "code")
            {
                UpdatePreviewHtml();
            }

            await foreach (var token in _llamaService.ChatStreamAsync(systemPrompt, modelInput, Temperature, _cancellationTokenSource.Token))
            {
                rawContent += token;

                // Throttle UI updates to ~15 FPS (64ms) to avoid saturating the dispatcher
                if (sw.ElapsedMilliseconds > 64)
                {
                    sw.Restart();
                    var parsed = ParseCodeBlocks(rawContent);
                    latestFiles = parsed.Files;
                    var displayText = parsed.DisplayText;
                    var cards = parsed.Cards;
                    var activeName = parsed.ActiveFileName ?? (latestFiles.Count > 0 ? latestFiles.Keys.Last() : null);
                    var activeCode = activeName != null ? latestFiles[activeName] : null;

                    System.Windows.Application.Current.Dispatcher.BeginInvoke(() => {
                        assistantMsg.Content = displayText;
                        SyncToolCards(assistantMsg, cards);
                        
                        if (ActiveMode == "code" && activeName != null && activeCode != null)
                        {
                            UpdateCurrentCode(activeName, activeCode);
                            _workspaceService.WriteFile(ActiveConversation?.Id ?? "default", activeName, activeCode);

                            if (!_canvasOpen)
                            {
                                _canvasOpen = true;
                                OnPropertyChanged(nameof(IsCanvasOpen));
                            }
                            if (ActiveCanvasTab != "code" && ActiveCanvasTab != "preview") ActiveCanvasTab = "code";
                        }
                    });
                }
            }

            // Final update to ensure everything is caught
            var finalParsed = ParseCodeBlocks(rawContent);
            latestFiles = finalParsed.Files;
            assistantMsg.Content = finalParsed.DisplayText;
            SyncToolCards(assistantMsg, finalParsed.Cards);
            
            if (latestFiles.Count > 0)
            {
                UpdateWorkspaceFiles(latestFiles);
                var lastFile = finalParsed.ActiveFileName ?? latestFiles.Keys.Last();
                UpdateCurrentCode(lastFile, latestFiles[lastFile]);
            }

            var actions = ParseActionBlocks(rawContent);
            if (actions.Count > 0)
            {
                assistantMsg.Content = StripActionBlocks(rawContent).Trim();
                foreach (var action in actions)
                    ExecuteToolAction(action, assistantMsg);
                latestFiles = WorkspaceFiles.ToDictionary(f => f.Name, f => f.Content);
            }

            UpdatePreviewHtml();
            ValidateProject(assistantMsg);
            if (ActiveMode == "code" && WorkspaceFiles.Count > 0)
            {
                foreach (var card in assistantMsg.ToolCards)
                    card.IsRunning = false;

                if (!assistantMsg.ToolCards.Any(c => c.Title == "Opening"))
                {
                    assistantMsg.ToolCards.Add(new AssistantToolCard
                    {
                        Title = "Opening",
                        Target = "preview",
                        IsOpen = false,
                        IsRunning = false
                    });
                }

                await Task.Delay(1400);
                if (AutoOpenPreview && ActiveCanvasTab == "code") ActiveCanvasTab = "preview";
                UpdateConversationShellState();
            }
        }
        catch (OperationCanceledException)
        {
            assistantMsg.Content += "\n[Stopped by user]";
            UpdatePreviewHtml();
        }
        catch (Exception ex)
        {
            assistantMsg.Content += $"\n[Error: {ex.Message}]";
        }
        finally
        {
            assistantMsg.IsStreaming = false;
            IsGenerating = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            SaveState();
        }
    }

    private void UpdateCurrentCode(string fileName, string code)
    {
        if (CurrentCode == code) return;
        CurrentFileName = fileName;
        CurrentCode = code;
        CurrentCharCount = code.Length;

        var lines = code.Split('\n');
        CurrentLineCount = lines.Length;
        LineNumbers = string.Join("\n", Enumerable.Range(1, lines.Length));
        UpdateConversationShellState();
    }

    private string BuildCodeSystemPrompt()
    {
        var conversationId = ActiveConversation?.Id ?? "default";
        var workspacePath = _workspaceService.WorkspaceDir(conversationId);
        var previewUrl = _workspaceService.PreviewUrl(conversationId);

        var prompt = string.Join("\n", [
            "You are Gemma, a local coding agent running entirely on the user's PC.",
            $"Workspace: {workspacePath}. Preview: {previewUrl}",
            "",
            "WHAT TO BUILD:",
            "You build small apps, pages, demos, and scripts. Quality matters.",
            "- Modern, polished design: clean typography, subtle gradients, rounded corners, smooth transitions.",
            "- Real-feeling copy, not lorem ipsum.",
            "- Make it actually work: click handlers wired, animations smooth.",
            "",
            "FILE STRUCTURE — PREFER MULTI-FILE:",
            "- One-off tiny demos → single index.html with <style> + <script> inline.",
            "- Landing pages, apps with state, anything non-trivial → split into:",
            "    index.html — structure + <link rel=\"stylesheet\" href=\"style.css\"> + <script src=\"app.js\" defer></script>",
            "    style.css  — all styling",
            "    app.js     — all behavior",
            "- Multi-file is easier to read and shows off modular thinking.",
            "",
            "CRITICAL RULES:",
            "- Always use markdown code blocks (fences) to output files.",
            "- Start your response with a short plan (one sentence), then immediately start the code blocks.",
            "- Return only changed files as complete fenced blocks.",
            "- Keep generated code complete for each changed file.",
            "- When done, use <action name=\"open_preview\"></action> and a short summary."
        ]);
        
        if (!string.IsNullOrWhiteSpace(CustomSystemPrompt))
            prompt += "\n\nUSER CUSTOM INSTRUCTIONS:\n" + CustomSystemPrompt.Trim();
        return prompt;
    }

    private string BuildCodeUserMessage(string currentInput)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("USER REQUEST:");
        sb.AppendLine(currentInput);
        sb.AppendLine();

        var files = WorkspaceFiles.ToList();
        if (files.Count > 0)
        {
            sb.AppendLine("CURRENT WORKSPACE FILES:");
            foreach (var file in files)
            {
                var fence = file.Name.EndsWith(".css", StringComparison.OrdinalIgnoreCase)
                    ? "css"
                    : file.Name.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
                        ? "js"
                        : "html";
                sb.AppendLine($"--- {file.Name} ---");
                sb.AppendLine($"```{fence}");
                sb.AppendLine(TruncateForContext(file.Content, 6000));
                sb.AppendLine("```");
            }
            sb.AppendLine();
            sb.AppendLine("Edit the current workspace above. Return only changed files as complete fenced blocks.");
        }
        else
        {
            sb.AppendLine("No workspace files exist yet. Build the requested project.");
        }

        var history = ActiveConversation?.Messages
            .Where(m => !string.IsNullOrWhiteSpace(m.Content))
            .TakeLast(8)
            .Select(m => $"{m.Role}: {TruncateForContext(m.Content, 900)}")
            .ToList() ?? [];

        if (history.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("RECENT CHAT CONTEXT:");
            foreach (var item in history)
                sb.AppendLine(item);
        }

        return sb.ToString();
    }

    private string BuildChatUserMessage(string currentInput)
    {
        var history = ActiveConversation?.Messages
            .Where(m => !string.IsNullOrWhiteSpace(m.Content))
            .TakeLast(8)
            .Select(m => $"{m.Role}: {TruncateForContext(m.Content, 900)}")
            .ToList() ?? [];

        if (history.Count == 0)
            return currentInput;

        return "RECENT CHAT CONTEXT:\n" + string.Join("\n", history) + "\n\nUSER REQUEST:\n" + currentInput;
    }

    private static string TruncateForContext(string value, int maxChars)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxChars)
            return value;
        return value[..maxChars] + "\n...[truncated]";
    }

    private void UpdatePreviewHtml()
    {
        if (ActiveConversation != null)
        {
            SyncWorkspaceToDisk();
            PreviewSource = _workspaceService.PreviewUrl(ActiveConversation.Id);
        }

        var htmlFile = WorkspaceFiles.FirstOrDefault(f => f.Name == "index.html")?.Content ?? "";
        var cssFile = WorkspaceFiles.FirstOrDefault(f => f.Name == "style.css")?.Content ?? "";
        var jsFile = WorkspaceFiles.FirstOrDefault(f => f.Name == "app.js")?.Content ?? "";

        if (string.IsNullOrEmpty(htmlFile) && !string.IsNullOrEmpty(CurrentCode))
            htmlFile = CurrentCode;

        if (string.IsNullOrEmpty(htmlFile))
        {
            PreviewHtml = "<html><body style='background:white;display:flex;align-items:center;justify-content:center;height:100vh;margin:0;font-family:sans-serif;color:#888;'>No HTML content to preview</body></html>";
            return;
        }

        var fullHtml = htmlFile;
        if (!string.IsNullOrEmpty(cssFile) && !fullHtml.Contains(cssFile))
            fullHtml = InjectBeforeClosingTag(fullHtml, "head", $"<style>{cssFile}</style>");

        if (!string.IsNullOrEmpty(jsFile) && !fullHtml.Contains(jsFile))
            fullHtml = InjectBeforeClosingTag(fullHtml, "body", $"<script>{jsFile}</script>");

        PreviewHtml = fullHtml;
    }

    private void ValidateProject(ChatMessage message)
    {
        if (ActiveMode != "code" || WorkspaceFiles.Count == 0) return;
        var html = WorkspaceFiles.FirstOrDefault(f => f.Name.Equals("index.html", StringComparison.OrdinalIgnoreCase))?.Content;
        if (string.IsNullOrWhiteSpace(html))
        {
            AddToolCard(message, "Check", "workspace", "", "No index.html found, so preview may be empty.");
            return;
        }

        var hasBody = html.Contains("<body", StringComparison.OrdinalIgnoreCase) || html.Contains("<main", StringComparison.OrdinalIgnoreCase);
        if (!hasBody)
            AddToolCard(message, "Check", "index.html", "", "index.html exists, but no visible body/main content was detected.");
    }

    private void SyncWorkspaceToDisk()
    {
        if (ActiveConversation == null) return;
        foreach (var file in WorkspaceFiles)
            _workspaceService.WriteFile(ActiveConversation.Id, file.Name, file.Content);
    }

    private void UpdateWorkspaceFiles(Dictionary<string, string> files)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            foreach (var (fileName, code) in files)
            {
                var existing = WorkspaceFiles.FirstOrDefault(f => f.Name == fileName);
                if (existing == null)
                {
                    var newFile = new ProjectFile { Name = fileName, Content = code, Language = System.IO.Path.GetExtension(fileName).TrimStart('.') };
                    WorkspaceFiles.Add(newFile);
                    SelectedFile = newFile;
                }
                else
                {
                    existing.Content = code;
                }
                _workspaceService.WriteFile(ActiveConversation?.Id ?? "default", fileName, code);
            }

            if (ActiveConversation != null)
                ActiveConversation.WorkspaceFiles = WorkspaceFiles;

            if (!IsCanvasOpen && WorkspaceFiles.Count > 0)
            {
                _canvasOpen = true;
                OnPropertyChanged(nameof(IsCanvasOpen));
            }
            UpdateConversationShellState();
            OnPropertyChanged(nameof(WorkspaceFileCountText));
            SaveState();
        });
    }

    private static (string DisplayText, Dictionary<string, string> Files, List<AssistantToolCard> Cards, string? ActiveFileName) ParseCodeBlocks(string raw)
    {
        var files = new Dictionary<string, string>();
        var cards = new List<AssistantToolCard>();
        string? activeFile = null;

        var completeMatches = Regex.Matches(raw, "```(?<lang>[a-zA-Z0-9_.-]*)\\s*\\r?\\n(?<code>[\\s\\S]*?)```");
        var display = raw;
        var offset = 0;

        foreach (Match m in completeMatches)
        {
            var lang = m.Groups["lang"].Value.Trim().ToLowerInvariant();
            var code = m.Groups["code"].Value.Trim('\r', '\n');
            var fileName = FileNameForLanguage(lang, files.Count);
            files[fileName] = code;
            
            cards.Add(new AssistantToolCard
            {
                Title = "Writing",
                Target = fileName,
                Content = code,
                Result = $"Wrote {fileName} ({code.Length} bytes).",
                IsOpen = false,
                IsRunning = false
            });

            var start = m.Index - offset;
            display = display.Remove(start, m.Length).Insert(start, "\n\n");
            offset += m.Length - 2;
        }

        var lastBlockMatch = Regex.Match(display, "```(?<lang>[a-zA-Z0-9_.-]*)\\s*\\r?\\n?(?<code>[\\s\\S]*)$");
        if (lastBlockMatch.Success)
        {
            var lang = lastBlockMatch.Groups["lang"].Value.Trim().ToLowerInvariant();
            var code = lastBlockMatch.Groups["code"].Value.TrimStart('\r', '\n');
            var fileName = FileNameForLanguage(lang, files.Count);
            files[fileName] = code;
            activeFile = fileName;

            cards.Add(new AssistantToolCard
            {
                Title = "Writing",
                Target = fileName,
                Content = code,
                IsOpen = true,
                IsRunning = true
            });

            display = display[..lastBlockMatch.Index];
        }

        return (display.Trim(), files, cards, activeFile);
    }

    private static void SyncToolCards(ChatMessage message, List<AssistantToolCard> cards)
    {
        for (var i = 0; i < cards.Count; i++)
        {
            if (message.ToolCards.Count <= i)
            {
                message.ToolCards.Add(cards[i]);
                continue;
            }

            var existing = message.ToolCards[i];
            existing.Title = cards[i].Title;
            existing.Target = cards[i].Target;
            existing.Content = cards[i].Content;
            existing.Result = cards[i].Result;
            existing.IsRunning = cards[i].IsRunning;
        }

        while (message.ToolCards.Count > cards.Count)
            message.ToolCards.RemoveAt(message.ToolCards.Count - 1);
    }

    private void ExecuteToolAction(ToolAction action, ChatMessage message)
    {
        try
        {
            var conversationId = ActiveConversation?.Id ?? "default";
            switch (action.Name)
            {
                case "write_file":
                {
                    var path = action.Args.GetValueOrDefault("path", "index.html").Trim();
                    var content = action.Args.GetValueOrDefault("content", "");
                    if (string.IsNullOrWhiteSpace(path)) path = "index.html";
                    _workspaceService.WriteFile(conversationId, path, content);
                    UpsertWorkspaceFile(path, content);
                    AddToolCard(message, "Writing", path, content, $"Wrote {path} ({content.Length} bytes, {content.Split('\n').Length} lines).");
                    UpdateCurrentCode(path, content);
                    ActiveCanvasTab = "code";
                    break;
                }
                case "edit_file":
                {
                    var path = action.Args.GetValueOrDefault("path", "");
                    var oldString = action.Args.GetValueOrDefault("old_string", "");
                    var newString = action.Args.GetValueOrDefault("new_string", "");
                    var replaceAll = action.Args.GetValueOrDefault("replace_all", "").Equals("true", StringComparison.OrdinalIgnoreCase);
                    var count = _workspaceService.EditFile(conversationId, path, oldString, newString, replaceAll);
                    var content = _workspaceService.ReadFile(conversationId, path);
                    UpsertWorkspaceFile(path, content);
                    AddToolCard(message, "Editing", path, newString, $"Edited {path} ({count} replacement{(count == 1 ? "" : "s")}).");
                    UpdateCurrentCode(path, content);
                    ActiveCanvasTab = "code";
                    break;
                }
                case "read_file":
                {
                    var path = action.Args.GetValueOrDefault("path", "");
                    var content = _workspaceService.ReadFile(conversationId, path);
                    AddToolCard(message, "Reading", path, content, $"Read {path} ({content.Length} chars).");
                    break;
                }
                case "list_files":
                {
                    var list = string.Join("\n", _workspaceService.ListFiles(conversationId).Select(f => $"{f.Name} ({f.Size}B)"));
                    AddToolCard(message, "Listing", "workspace", list, list.Length == 0 ? "Workspace is empty." : "Listed workspace.");
                    break;
                }
                case "delete_file":
                {
                    var path = action.Args.GetValueOrDefault("path", "");
                    _workspaceService.DeleteFile(conversationId, path);
                    var existing = WorkspaceFiles.FirstOrDefault(f => f.Name == path);
                    if (existing != null) WorkspaceFiles.Remove(existing);
                    AddToolCard(message, "Deleting", path, "", $"Deleted {path}.");
                    break;
                }
                case "open_preview":
                    AddToolCard(message, "Opening", "preview", "", "Preview refreshed.");
                    ActiveCanvasTab = "preview";
                    break;
            }

            UpdateConversationShellState();
            OnPropertyChanged(nameof(WorkspaceFileCountText));
            SaveState();
        }
        catch (Exception ex)
        {
            AddToolCard(message, "Tool error", action.Name, "", ex.Message);
        }
    }

    private void AddToolCard(ChatMessage message, string title, string target, string content, string result)
    {
        message.ToolCards.Add(new AssistantToolCard
        {
            Title = title,
            Target = target,
            Content = content,
            Result = result,
            IsOpen = title is "Writing" or "Editing",
            IsRunning = false
        });
    }

    private void UpsertWorkspaceFile(string path, string content)
    {
        var existing = WorkspaceFiles.FirstOrDefault(f => f.Name == path);
        if (existing == null)
        {
            WorkspaceFiles.Add(new ProjectFile { Name = path, Content = content, Language = Path.GetExtension(path).TrimStart('.') });
            SelectedFile = WorkspaceFiles.Last();
        }
        else
        {
            existing.Content = content;
        }
    }

    private static List<ToolAction> ParseActionBlocks(string raw)
    {
        var actions = new List<ToolAction>();
        var openRe = new Regex("<action\\s+name\\s*=\\s*[\"']?(?<name>[a-zA-Z_][\\w]*)[\"']?\\s*>", RegexOptions.IgnoreCase);
        var pos = 0;
        while (true)
        {
            var open = openRe.Match(raw, pos);
            if (!open.Success) break;
            var bodyStart = open.Index + open.Length;
            var close = Regex.Match(raw[bodyStart..], "</action\\s*>", RegexOptions.IgnoreCase);
            if (!close.Success) break;
            var body = raw.Substring(bodyStart, close.Index);
            actions.Add(new ToolAction(open.Groups["name"].Value, ParseActionArgs(body)));
            pos = bodyStart + close.Index + close.Length;
        }
        return actions;
    }

    private static Dictionary<string, string> ParseActionArgs(string body)
    {
        var args = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var contentOpen = body.IndexOf("<content>", StringComparison.OrdinalIgnoreCase);
        if (contentOpen >= 0)
        {
            var contentClose = body.LastIndexOf("</content>", StringComparison.OrdinalIgnoreCase);
            if (contentClose > contentOpen)
            {
                args["content"] = body[(contentOpen + "<content>".Length)..contentClose].Trim('\r', '\n');
                body = body[..contentOpen] + body[(contentClose + "</content>".Length)..];
            }
        }

        foreach (Match m in Regex.Matches(body, "<(?<key>[a-zA-Z_][\\w-]*)>(?<value>[\\s\\S]*?)</\\k<key>>"))
            args[m.Groups["key"].Value] = m.Groups["value"].Value.Trim('\r', '\n');
        return args;
    }

    private static string StripActionBlocks(string raw) =>
        Regex.Replace(raw, "<action\\s+name\\s*=\\s*[\"']?[a-zA-Z_][\\w]*[\"']?\\s*>[\\s\\S]*?</action\\s*>", "", RegexOptions.IgnoreCase);

    private sealed record ToolAction(string Name, Dictionary<string, string> Args);

    private void RestoreConversation(Conversation conversation)
    {
        if (_isRestoring) return;
        _isRestoring = true;
        try
        {
            ActiveMode = string.IsNullOrWhiteSpace(conversation.ActiveMode) ? "code" : conversation.ActiveMode;
            _canvasOpen = conversation.CanvasOpen;
            ActiveCanvasTab = string.IsNullOrWhiteSpace(conversation.ActiveCanvasTab) ? "preview" : conversation.ActiveCanvasTab;
            WorkspaceFiles = conversation.WorkspaceFiles ?? new ObservableCollection<ProjectFile>();
            var diskFiles = _workspaceService.LoadProjectFiles(conversation.Id);
            if (diskFiles.Count > 0)
                WorkspaceFiles = new ObservableCollection<ProjectFile>(diskFiles);
            CurrentFileName = string.IsNullOrWhiteSpace(conversation.CurrentFileName) ? "index.html" : conversation.CurrentFileName;

            var file = WorkspaceFiles.FirstOrDefault(f => f.Name == CurrentFileName) ?? WorkspaceFiles.FirstOrDefault();
            if (file != null)
                UpdateCurrentCode(file.Name, file.Content);
            else
            {
                CurrentCode = "";
                CurrentLineCount = 0;
                CurrentCharCount = 0;
                LineNumbers = "1";
            }

            UpdatePreviewHtml();
            OnPropertyChanged(nameof(IsCanvasOpen));
            OnPropertyChanged(nameof(WorkspaceFileCountText));
        }
        finally
        {
            _isRestoring = false;
        }
    }

    private void UpdateConversationShellState()
    {
        if (ActiveConversation == null) return;
        ActiveConversation.ActiveMode = ActiveMode;
        ActiveConversation.CanvasOpen = _canvasOpen;
        ActiveConversation.ActiveCanvasTab = ActiveCanvasTab;
        ActiveConversation.CurrentFileName = CurrentFileName;
        ActiveConversation.WorkspaceFiles = WorkspaceFiles;
    }

    private bool LoadState()
    {
        try
        {
            if (!File.Exists(StorePath)) return false;
            var json = File.ReadAllText(StorePath);
            var state = JsonSerializer.Deserialize<SavedState>(json);
            if (state?.Conversations == null || state.Conversations.Count == 0) return false;

            IsSidebarCollapsed = state.IsSidebarCollapsed;
            CanvasWidth = state.CanvasWidth.Value > 0 ? state.CanvasWidth : new System.Windows.GridLength(520);
            Temperature = state.Temperature;
            GpuLayers = state.GpuLayers;
            CustomSystemPrompt = state.CustomSystemPrompt ?? "";
            AutoOpenPreview = state.AutoOpenPreview;
            OnPropertyChanged(nameof(SidebarWidth));
            Conversations = new ObservableCollection<Conversation>(state.Conversations);
            var active = Conversations.FirstOrDefault(c => c.Id == state.ActiveConversationId) ?? Conversations.First();
            ActiveConversation = active;
            RestoreConversation(active);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void SaveState()
    {
        if (_isRestoring) return;
        try
        {
            UpdateConversationShellState();
            Directory.CreateDirectory(Path.GetDirectoryName(StorePath)!);
            var state = new SavedState
            {
                ActiveConversationId = ActiveConversation?.Id ?? "",
                Conversations = Conversations.ToList(),
                IsSidebarCollapsed = IsSidebarCollapsed,
                CanvasWidth = CanvasWidth,
                Temperature = Temperature,
                GpuLayers = GpuLayers,
                CustomSystemPrompt = CustomSystemPrompt,
                AutoOpenPreview = AutoOpenPreview
            };
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(StorePath, json);
        }
        catch
        {
            // Persistence should never break chatting.
        }
    }

    private sealed class SavedState
    {
        public string ActiveConversationId { get; set; } = "";
        public List<Conversation> Conversations { get; set; } = new();
        public bool IsSidebarCollapsed { get; set; }
        public System.Windows.GridLength CanvasWidth { get; set; } = new(520);
        public double Temperature { get; set; } = 0.7;
        public int GpuLayers { get; set; }
        public string CustomSystemPrompt { get; set; } = "";
        public bool AutoOpenPreview { get; set; } = true;
    }

    private static string FileNameForLanguage(string lang, int index) => lang switch
    {
        "html" or "htm" => "index.html",
        "css" => "style.css",
        "javascript" or "js" or "typescript" or "ts" => "app.js",
        "" => index == 0 ? "index.html" : $"artifact-{index + 1}.txt",
        _ when lang.Contains('.') => lang,
        _ => $"artifact-{index + 1}.{lang}"
    };

    private static string InjectBeforeClosingTag(string html, string tag, string injection)
    {
        var pattern = $"</{tag}>";
        if (Regex.IsMatch(html, pattern, RegexOptions.IgnoreCase))
            return Regex.Replace(html, pattern, injection + pattern, RegexOptions.IgnoreCase);

        return html + injection;
    }
}
