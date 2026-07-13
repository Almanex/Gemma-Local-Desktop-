# Contributing to Gemma Local Desktop

Thank you for your interest in contributing to Gemma Local Desktop. We welcome and appreciate contributions of all kinds, including bug reports, feature suggestions, documentation updates, and pull requests.

Please follow these guidelines to ensure a smooth collaboration process.

---

## Code of Conduct

By participating in this project, you agree to maintain a respectful, constructive, and professional environment for everyone.

---

## How Can I Contribute?

### Reporting Bugs

If you find a bug or unexpected behavior:
1. Check the existing issues to see if it has already been reported.
2. If not, open a new issue.
3. Provide a clear and concise description of the problem, steps to reproduce it, expected versus actual behavior, and relevant logs or system specifications (e.g. Windows version, GPU details).

### Suggesting Enhancements

If you have ideas for new features or improvements:
1. Search existing issues and discussions to see if the idea has already been proposed.
2. If not, open a new issue describing the proposed change, the problem it solves, and how it benefits the project.

### Submitting Pull Requests

1. Fork the repository and create your branch from the `main` branch.
2. Ensure your code follows the existing style, remains clean, and runs fully offline.
3. If you add dependencies, make sure they are open-source and compatible with the MIT license.
4. Ensure your changes compile successfully under .NET 10.0:
   ```powershell
   dotnet build -c Release
   ```
5. Commit your changes with clear and descriptive commit messages.
6. Push your branch to your fork and submit a Pull Request (PR) to the `main` branch of this repository.

---

## Style Guidelines

- **No Emojis**: Avoid using graphical emojis in documentation files (READMEs, logs, manuals) to maintain a professional technical aesthetic.
- **Asynchronous Code**: When working with UI or file system operations, always prefer task-based asynchronous patterns (`async/await`) to prevent UI freezes.
- **WPF MVVM Architecture**: Do not put business or model logic in code-behinds (`MainWindow.xaml.cs`). Keep views separate from ViewModels and Services.

---

## License

By contributing to this project, you agree that your contributions will be licensed under the project's MIT License.
