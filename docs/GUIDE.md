# Gemma Local Desktop User Guide — Native Offline AI Assistant and Coding Workspace for Windows

> [!NOTE]
> **TL;DR: Quick Summary**
> - **Zero-Cloud AI**: Runs entirely offline on Windows using Google's Gemma models via `llama.cpp`.
> - **Dual-Mode Canvas**: Switch seamlessly between standard chat dialogue and active code generation.
> - **Live Interactive Preview**: Instantly view generated HTML/CSS/JS applications in an embedded WebView.
> - **Persistent Sandbox**: Iterative file editing saves code directly to local workspaces on your disk.

---

Gemma Local Desktop is a desktop assistant that puts private, offline AI power directly onto your Windows machine. By integrating Google's optimized Gemma model weight files locally, this workspace removes cloud dependencies, subscription costs, and privacy concerns, allowing you to generate web applications and analyze complex logic 100% offline.

---

## Feature Breakdown

### Chat Mode
Chat Mode provides an interface optimized for standard text dialogues. This mode is best for explaining concepts, reviewing existing code snippets, brainstorming logic, and debugging software errors. It outputs formatted text utilizing WPF Markdown rendering for crisp readability.

### Build Mode
Build Mode introduces a split-screen canvas designed specifically for generating interactive web applications (HTML, CSS, JS). As the model writes code, the app saves these files to a local sandbox directory and renders them immediately in the integrated 720px live interactive preview panel.

### Live Preview Canvas
The preview canvas leverages the Microsoft WebView2 runtime to execute generated web content in real time. It features a quick-launch button to run your creation in the default system browser, enabling full-screen testing and developer tool inspection.

### Workspace Sandbox
Every chat session creates a unique folder inside your local directory. Code edits are performed iteratively—meaning when you ask the model to modify or extend a feature, it updates the files directly on disk, prompting the preview window to reload the new state automatically.

---

## Interface Languages & Localization

The user interface of Gemma Local Desktop detects your active Windows system language automatically on startup. 

### Supported Languages
- **English** (default fallback)
- **Russian** (Русский)
- **German** (Deutsch)

### Language Override Settings
If you want to run the application in a specific language regardless of your system settings, you can launch the executable via Command Prompt or PowerShell using the `--lang` override parameter:
```powershell
# Launch the app in Russian
GemmaChatWindows.exe --lang ru

# Launch the app in German
GemmaChatWindows.exe --lang de

# Launch the app in English
GemmaChatWindows.exe --lang en
```

---

## Quick-Start Instructions

Follow these steps to get your offline assistant up and running in minutes:

1. **Step 1: Download and Clone** — Download the codebase using Git or download the repository zip archive, and extract it to a directory on your machine.
2. **Step 2: Check System Prerequisites** — Ensure you have the .NET 10.0 SDK installed on your system to compile and build the source files.
3. **Step 3: Run the Application** — Open a terminal window in the project folder and start the shell using the command `dotnet run` inside the `GemmaChatCsharp` directory.
4. **Step 4: Automatic Resource Setup** — Allow the application to automatically check your graphics driver, download the required local `llama.cpp` server binaries, and fetch the recommended Gemma model weights (~1.6GB).
5. **Step 5: Pick a Preset Template** — Click on one of the one-click template cards (such as **Mega Tetris** or **Weather Dashboard**) to load the initial setup prompt, then press Enter to trigger local code generation.

---

## Tips & Shortcuts

Maximize your productivity using the built-in system shortcuts and tips:

| Keyboard Shortcut | Action / Description |
| --- | --- |
| `Ctrl + N` | Start a new chat session and clear the workspace canvas. |
| `Ctrl + B` | Toggle split-pane view (switch between Chat Mode and Build Mode). |
| `Ctrl + \` | Toggle the Visibility of the right-side Canvas pane (Preview and Code editor). |

---

## FAQ & Troubleshooting

### Windows Defender SmartScreen blocks the app on launch
Since compiled desktop executables for free, open-source projects are not signed with expensive commercial certificates, Windows Defender will flag the launch. Click **More info** followed by **Run anyway** to proceed.

### The model is running very slowly or lagging
Gemma Local Desktop utilizes GPU acceleration via NVIDIA CUDA. Ensure you have the latest NVIDIA drivers installed. If your system lacks a compatible GPU, the runtime will default to CPU execution, which has slower inference speeds. Close resource-heavy apps to free up at least 4GB of RAM.

### How to reset the model or redownload the binaries
If your downloads get interrupted or corrupted, navigate to the local AppData folder (`%LocalAppData%\GemmaLocalDesktop`) and delete the `runtimes` or `models` directories to force a fresh download on the next application launch.

---

## Join the Community & Support

We are building a community of developers passionate about private, local-first software. If you enjoy using the workspace:
- **Star the Repository**: Show your support by starring our project on [GitHub](https://github.com/Almanex/Gemma-Local-Desktop-).
- **Report Issues**: Found a bug or have a suggestion? Open an issue on our tracker.
- **Submit Pull Requests**: Check our [CONTRIBUTING.md](../CONTRIBUTING.md) to learn how to contribute code and localizations.
