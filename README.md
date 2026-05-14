# Gemma Local Desktop

<p align="center">
  <img src="GemmaChatCsharp/Assets/gemma-logo.png" alt="Gemma Local Desktop" width="128" />
</p>

<h1 align="center">Gemma Local Desktop</h1>

<p align="center">
  <strong>Native multimodal AI assistant and coding workspace.</strong><br/>
  Powered by Google's Gemma via llama.cpp.<br/>
  No API keys. No cloud. Works 100% offline.
</p>

---

Gemma Local Desktop is a native Windows application designed for high-performance local AI interaction. It combines a powerful chat interface with a sandboxed coding workspace.

## Key Features

- 💬 **Chat Mode** — Conversational AI running locally on your PC.
- 🛠 **Build Mode** — Coding agent with live preview. Creates and edits websites directly.
- 📂 **Workspace Persistence** — Per-chat project workspaces saved on disk. Iterative edits preserve existing code.
- 🎨 **Modern UI** — Custom native Windows design with resizable layouts, sidebar, and dark theme.
- 🎮 **GPU Acceleration** — Automatic NVIDIA GPU detection and CUDA optimization.
- 📦 **Zero Config** — Automatically downloads `llama.cpp` runtimes and models on first start.

## Requirements

- **Windows 10/11** (64-bit)
- **NVIDIA GPU** (optional, for CUDA acceleration) or CPU
- **4GB+ RAM** for Gemma 2B model

## Quick Start

1. Clone the repository.
2. Ensure you have the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) installed.
3. Run the application:
   ```powershell
   cd GemmaChatCsharp
   dotnet run
   ```

On first launch, the app will:
1. Detect your hardware (CPU vs NVIDIA GPU).
2. Download the appropriate `llama.cpp` server binaries.
3. Download the recommended Gemma model (~1.6GB).
4. Initialize your local workspace.

## Tech Stack

| Layer | Tech |
|-------|------|
| App Shell | WPF (.NET 10) + CommunityToolkit.Mvvm |
| WebView | Microsoft.Web.WebView2 |
| Markdown | Markdig.Wpf |
| Model Runtime | [llama.cpp](https://github.com/ggml-org/llama.cpp) (local server) |
| persistence | Local JSON state + Workspace filesystem |

## Shortcuts

- `Ctrl + N`: New chat
- `Ctrl + B`: Toggle Chat/Build mode
- `Ctrl + \`: Toggle Canvas (Preview/Code)
- `Ctrl + E`: Export project to ZIP

## License

MIT