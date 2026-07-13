# Project Context: Gemma Local Desktop

This document provides essential context, architecture overview, and development guidelines for AI coding assistants and developers working on the Gemma Local Desktop project.

---

## Technical Stack

- **Target Framework**: .NET 10.0 (`net10.0-windows`)
- **Application Shell**: WPF (Windows Presentation Foundation)
- **UI Architecture**: MVVM (using `CommunityToolkit.Mvvm`)
- **Web Rendering Engine**: Microsoft Edge WebView2 (`Microsoft.Web.WebView2`)
- **Markdown Processing**: Markdig Wpf (`Markdig.Wpf`)
- **AI Model Inference Backend**: Local `llama.cpp` server (started as a background subprocess)

---

## Directory Structure

```
├── GemmaChatCsharp/               # Main WPF project directory
│   ├── App.xaml / App.xaml.cs     # Application entry point and startup configurations
│   ├── AssemblyInfo.cs            # WPF theme and assembly metadata
│   ├── Assets/                    # Images, icons, and application screenshots
│   ├── Converters.cs              # WPF UI ValueConverters
│   ├── GemmaChatWindows.csproj    # Project definition and dependencies
│   ├── MainWindow.xaml / .cs      # Core layout containing Chat, Code Editor, and WebView canvas
│   ├── Services/                  # Business logic and system services
│   └── ViewModels/                # MVVM ViewModel implementations
├── docs/                          # Multilingual documentation
│   ├── README_RU.md               # Russian translation of README
│   └── README_DE.md               # German translation of README
└── README.md                      # Primary English documentation
```

---

## Architecture Overview

### 1. Main UI Layout (`MainWindow.xaml`)
The UI is divided into two primary sections:
- **Left Panel (Chat Pane)**: Renders the markdown chat history, user input controls, and model setup screens.
- **Right Panel (Build Canvas)**: Shows the active code editor (generated files/artifacts) and the live interactive 720px web preview utilizing `WebView2`.

### 2. Model Backend (`llama.cpp` Integration)
On launch:
- The application detects hardware (checking for CUDA support via NVIDIA GPU drivers).
- If needed, it downloads `llama.cpp` pre-compiled server executables and the recommended model (Gemma) to a local appdata folder.
- Starts `llama.cpp` as a background server process listening on a local port.
- Communicates with the local server using standard OpenAI-compatible API requests for completion and streaming.

### 3. File & State Persistence
- Workspace data and code files generated in **Build Mode** are saved per-chat inside a local folder.
- This allows iterative reads/writes by the model, enabling developers to build and test code incrementally in the sandbox.

---

## Development Rules & Best Practices

1. **Keep it Offline**: Ensure no cloud dependencies or API keys are introduced. Everything must compile, run, and execute 100% offline.
2. **Modern Fluent Design**: Maintain consistent Windows 11/10 UI styles with clean margins, standard dark/light mode detection, and proper control layouts.
3. **Responsive UI & Async Operations**: Never block the WPF UI thread. Use `async/await` and task-based streams for model inference and long-running network downloads.
4. **Platform Restriction**: The project targets Windows platforms explicitly (`net10.0-windows`), targeting standard 64-bit architecture (`win-x64`).
5. **No Emojis in Source Docs**: Ensure any developer documentation created adheres to the emoji-free repo-styler policy.
