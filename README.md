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

- 💬 **Chat Mode** — Optimized for pure dialogue and reasoning.
- 🛠 **Build Mode** — Specialized workspace for generating code, web apps, and artifacts with a 720px live preview.
- 🚀 **One-Click Templates** — Instant high-fidelity starters like *Mega Tetris* and *Dynamic Weather Dashboard*.
- 🖥 **Full-Screen Preview** — One button to view your creation in your default system browser.
- 📂 **Workspace Persistence** — Per-chat projects saved on disk. The model can read and edit existing files iteratively.
- 🎨 **Modern UX** — Clean Windows design with glassmorphism, resizable canvas, and dark mode.
- 🎮 **GPU Acceleration** — Intelligent NVIDIA GPU detection with partial offloading for large models (MoE 26B).
- 📦 **Zero Config** — Fully automated environment setup: downloads runtimes and models on first start.

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