[ English ](README.md) • [ Русский ](docs/README_RU.md) • [ Deutsch ](docs/README_DE.md)

# Gemma Local Desktop

**Native Windows AI assistant and coding workspace powered by Google Gemma via llama.cpp.**

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Platform: Windows](https://img.shields.io/badge/Platform-Windows-0078d7.svg)](#requirements)
[![Language: C#](https://img.shields.io/badge/Language-C%23-239120.svg)](https://learn.microsoft.com/dotnet/csharp/)
[![Framework: .NET 10.0](https://img.shields.io/badge/Framework-.NET%2010.0-512bd4.svg)](https://dotnet.microsoft.com/)
[![Share](https://img.shields.io/twitter/url?style=social&url=https%3A%2F%2Fgithub.com%2FAlmanex%2FGemma-Local-Desktop-)](https://twitter.com/intent/tweet?text=Check%20out%20Gemma%20Local%20Desktop%20-%20native%20Windows%20AI%20assistant%20and%20coding%20workspace&url=https%3A%2F%2Fgithub.com%2FAlmanex%2FGemma-Local-Desktop-)

---

<details open>
  <summary style="cursor: pointer; padding: 6px; font-family: sans-serif;"><b>[ Show ] 1. Initial Setup Screen</b></summary>
  <br/>
  <p align="center"><img src="GemmaChatCsharp/Assets/gemma-chat-win_1.png" width="95%" /></p>
</details>
<details>
  <summary style="cursor: pointer; padding: 6px; font-family: sans-serif;"><b>[ Show ] 2. Chat and Build Workspace</b></summary>
  <br/>
  <p align="center"><img src="GemmaChatCsharp/Assets/gemma-chat-win_2.png" width="95%" /></p>
</details>
<details>
  <summary style="cursor: pointer; padding: 6px; font-family: sans-serif;"><b>[ Show ] 3. Code Editor and Live Preview</b></summary>
  <br/>
  <p align="center"><img src="GemmaChatCsharp/Assets/gemma-chat-win_3.png" width="95%" /></p>
</details>

---

## Overview

Gemma Local Desktop is an experimental, fully functional project designed to push the boundaries of local AI development on Windows. By utilizing Google's Gemma models locally through `llama.cpp` integration, it offers a secure, offline environment for AI-assisted coding and conversation. There are no API keys, no subscription fees, and no cloud-dependency.

---

## Key Features

- **Chat Mode**: A conversation workspace optimized for pure dialogue, text summarization, and complex reasoning.
- **Build Mode**: A split-screen environment that generates code, HTML/JS web apps, and artifacts side-by-side with a 720px live interactive web preview.
- **One-Click Templates**: Instant high-fidelity template starters to quickly generate functional mini-games, weather dashboards, or interactive UI components.
- **Full-Screen Preview**: A quick-launch option to open generated web apps directly in the system's default browser.
- **Workspace Persistence**: Automatic file storage on disk per chat project, enabling the model to read, edit, and reference project files iteratively.
- **Modern User Experience**: A clean, Windows-native UI implementation with glassmorphism, resizable canvas, and automatic dark mode support.
- **Hardware Acceleration**: Automatic NVIDIA GPU detection and intelligent model offloading utilizing CUDA, with fallback to CPU execution.
- **Zero Configuration Setup**: Automatic handling of environment setup, including downloading required `llama.cpp` binaries and the recommended Gemma model on first launch.

---

## Tech Stack

| Layer / Component | Technology | Details / Purpose |
| --- | --- | --- |
| Application Shell | WPF (.NET 10.0) | Desktop application framework with CommunityToolkit.Mvvm |
| UI styling | WPF-UI / Custom styles | Modern native Fluent Design look and feel |
| Web View | Microsoft.Web.WebView2 | Renders the HTML/CSS/JS preview canvas |
| Markdown Parser | Markdig.Wpf (v0.5.0.1) | Renders rich text messages and chats |
| Model Runtime | llama.cpp (Local server) | Runs the LLM engine offline with GPU/CPU support |
| State Persistence | Local JSON + Directory files | Saves workspaces and chats locally |

---

## Requirements

- **Operating System**: Windows 10 or Windows 11 (64-bit)
- **Processor**: x64 CPU (SSE3 support recommended)
- **Graphics Card**: NVIDIA GPU (optional, for CUDA acceleration)
- **Memory**: 4GB+ RAM (minimum recommended for Gemma 2B)

---

## Getting Started

### Prerequisites

Ensure you have the following installed on your machine:
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Git](https://git-scm.com/)

### Installation & Running

1. Clone this repository to your local system:
   ```powershell
   git clone https://github.com/Almanex/Gemma-Local-Desktop-.git
   cd Gemma-Local-Desktop-
   ```

2. Run the application using the .NET CLI:
   ```powershell
   cd GemmaChatCsharp
   dotnet run
   ```

On first startup, the application performs the following automated steps:
- Checks system specs (NVIDIA CUDA support vs CPU fallback).
- Downloads the compatible `llama.cpp` runtime server binaries.
- Downloads the recommended Gemma model (~1.6GB).
- Creates the local workspace folders.

---

## Running the Tests

Currently, this repository does not include automated unit tests. When unit tests are implemented, they can be executed from the test project directory using:
```powershell
dotnet test
```

---

## Deployment

### Compilation & Build

To compile the application in Release mode:
```powershell
dotnet build -c Release
```

### Self-Contained Deployment

To publish the application as a single, portable executable file (`.exe`) containing all native dependencies:
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```
The output file will be saved in `bin/Release/net10.0-windows/win-x64/publish/`.

> [!WARNING]
> **Windows Defender SmartScreen Warning**
>
> Because the compiled executable is self-signed/unsigned (which is standard for free, open-source projects), Windows Defender SmartScreen may display a warning on the first launch of the compiled binary.
>
> To run the application:
> 1. Click **More info**.
> 2. Click **Run anyway**.

---

## Shortcuts

- `Ctrl + N`: New chat
- `Ctrl + B`: Toggle between Chat and Build modes
- `Ctrl + \`: Toggle Canvas split-pane visibility (Preview / Code)

---

## Contributing

We welcome contributions from the community. If you would like to report bugs, suggest features, or submit pull requests:
1. Please read our [CONTRIBUTING.md](CONTRIBUTING.md).
2. Open an issue or submit a pull request on GitHub.

---

## Versioning

This project uses [SemVer](https://semver.org/) for versioning. For the versions available, see the tags on this repository.

---

## Authors & Contributors

- **Almanex** - *Initial Work* - [Almanex GitHub](https://github.com/Almanex)

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## Acknowledgments

- The [llama.cpp](https://github.com/ggml-org/llama.cpp) project team for the model inference engine.
- Markdig contributors for the Markdown parser libraries.