# Gemma Local Desktop

**Lokaler Windows-KI-Assistent und Entwicklungsumgebung basierend auf Google Gemma via llama.cpp.**

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../LICENSE)
[![Platform: Windows](https://img.shields.io/badge/Platform-Windows-0078d7.svg)](#systemanforderungen)
[![Language: C#](https://img.shields.io/badge/Language-C%23-239120.svg)](https://learn.microsoft.com/dotnet/csharp/)
[![Framework: .NET 10.0](https://img.shields.io/badge/Framework-.NET%2010.0-512bd4.svg)](https://dotnet.microsoft.com/)
[![Share](https://img.shields.io/twitter/url?style=social&url=https%3A%2F%2Fgithub.com%2FAlmanex%2FGemma-Local-Desktop-)](https://twitter.com/intent/tweet?text=Check%20out%20Gemma%20Local%20Desktop%20-%20native%20Windows%20AI%20assistant%20and%20coding%20workspace&url=https%3A%2F%2Fgithub.com%2FAlmanex%2FGemma-Local-Desktop-)

---

## Übersicht

Gemma Local Desktop ist ein experimentelles, voll funktionsfähiges Projekt, das entwickelt wurde, um die Grenzen der lokalen KI-Nutzung unter Windows zu erweitern. Durch die lokale Ausführung von Google Gemma-Modellen über die Integration von `llama.cpp` bietet die Anwendung eine sichere, vollständig Offline-fähige Umgebung für KI-gestützte Programmierung und Konversationen. Es werden keine API-Schlüssel, keine kostenpflichtigen Abonnements und keine Internetverbindungen benötigt.

---

## Hauptfunktionen

- **Chat-Modus (Chat Mode)**: Ein für reinen Dialog, Textzusammenfassungen und logisches Denken optimierter Arbeitsbereich.
- **Build-Modus (Build Mode)**: Eine geteilte Bildschirmumgebung, mit der HTML/JS-Web-Apps und Code-Artefakte generiert und direkt in einer interaktiven 720px Live-Vorschau angezeigt werden können.
- **Ein-Klick-Vorlagen**: Schneller Start von Vorlagen zur Generierung einfacher Spiele (wie Tetris), interaktiver Wetter-Dashboards oder Benutzeroberflächen-Elementen.
- **Vollbild-Vorschau**: Eine Funktion, um die generierte Web-App direkt im Standardbrowser des Systems zu öffnen.
- **Projekt-Speicherung**: Lokale Speicherung der Dateien für jedes Chat-Projekt, sodass das Modell Projektdateien iterativ lesen, bearbeiten und verfeinern kann.
- **Moderne Benutzeroberfläche**: Ein klares, natives Windows-UI-Design mit Glassmorphism-Effekten, anpassbarem Vorschaufenster und automatischer Unterstützung für den Dunkelmodus.
- **Hardwarebeschleunigung**: Automatische Erkennung von NVIDIA-Grafikkarten und Auslagerung von Berechnungen auf die GPU via CUDA mit automatischem Fallback auf CPU.
- **Einrichtung ohne Konfiguration**: Automatische Vorbereitung der Laufzeitumgebung, einschließlich des Downloads der `llama.cpp`-Serverdateien und des empfohlenen Gemma-Modells beim ersten Start.

---

## Technologie-Stack

| Ebene / Komponente | Technologie | Zweck / Beschreibung |
| --- | --- | --- |
| Anwendungs-Framework | WPF (.NET 10.0) | Desktop-Anwendung mit MVVM-Architektur über das CommunityToolkit.Mvvm |
| UI-Styling | WPF-UI / Eigene Stile | Modernes, natives Fluent Design |
| Web-Vorschau | Microsoft.Web.WebView2 | Anzeige des interaktiven Vorschaufensters für HTML/CSS/JS |
| Markdown-Parser | Markdig.Wpf (v0.5.0.1) | Formatierung von Chat-Nachrichten |
| Modell-Laufzeit | llama.cpp (Lokaler Server) | Lokaler Betrieb des LLM-Engines mit GPU- und CPU-Unterstützung |
| Zustandsspeicherung | Lokales JSON + Dateisystem | Speicherung von Chatverläufen und Projektdateien |

---

## Systemanforderungen

- **Betriebssystem**: Windows 10 oder Windows 11 (64-Bit)
- **Prozessor**: x64 CPU (Unterstützung für SSE3 empfohlen)
- **Grafikkarte**: NVIDIA GPU (optional, für CUDA-Beschleunigung)
- **Arbeitsspeicher**: 4 GB+ RAM (Minimum für das Gemma 2B Modell)

---

## Erste Schritte

### Voraussetzungen

Stellen Sie sicher, dass die folgenden Komponenten auf Ihrem System installiert sind:
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Git](https://git-scm.com/)

### Installation & Ausführen

1. Klonen Sie das Repository auf Ihren lokalen Computer:
   ```powershell
   git clone https://github.com/Almanex/Gemma-Local-Desktop-.git
   cd Gemma-Local-Desktop-
   ```

2. Starten Sie die Anwendung über die .NET-CLI:
   ```powershell
   cd GemmaChatCsharp
   dotnet run
   ```

Beim ersten Start führt die Anwendung folgende Schritte automatisch aus:
- Überprüfung der Systemleistung (NVIDIA CUDA GPU vs CPU-Fallback).
- Download der passenden `llama.cpp`-Laufzeitdateien.
- Download des empfohlenen Gemma-Modells (~1.6 GB).
- Erstellung lokaler Projektordner.

---

## Tests ausführen

Derzeit enthält dieses Repository keine automatisierten Unit-Tests. Wenn Unit-Tests implementiert sind, können sie aus dem Testprojektverzeichnis mit folgendem Befehl ausgeführt werden:
```powershell
dotnet test
```

---

## Bereitstellung

### Kompilierung

So kompilieren Sie die Anwendung im Release-Modus:
```powershell
dotnet build -c Release
```

### Eigenständige Veröffentlichung (Portable)

So veröffentlichen Sie die Anwendung als eine einzige, tragbare `.exe`-Datei, die alle nativen Abhängigkeiten enthält:
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```
Die Ausgabedatei wird unter `bin/Release/net10.0-windows/win-x64/publish/` gespeichert.

> [!WARNING]
> **Windows Defender SmartScreen Warnung**
>
> Da die kompilierte ausführbare Datei nicht kommerziell signiert ist (was bei kostenlosen Open-Source-Projekten üblich ist), zeigt Windows Defender SmartScreen möglicherweise beim ersten Start des Kompilats eine Warnung an.
>
> So führen Sie das Programm trotzdem aus:
> 1. Klicken Sie auf **Weitere Informationen** (More info).
> 2. Klicken Sie auf **Trotzdem ausführen** (Run anyway).

---

## Tastaturkürzel

- `Ctrl + N`: Neuen Chat erstellen
- `Ctrl + B`: Wechseln zwischen Chat- und Build-Modus
- `Ctrl + \`: Vorschau- / Code-Bereich ein- oder ausblenden

---

## Mitwirken

Beiträge aus der Community sind herzlich willkommen. Wenn Sie Fehler melden, Funktionen vorschlagen oder Pull Requests einreichen möchten:
1. Lesen Sie bitte unsere [CONTRIBUTING.md](../CONTRIBUTING.md).
2. Eröffnen Sie ein Issue oder erstellen Sie einen Pull Request auf GitHub.

---

## Versionierung

Dieses Projekt verwendet [SemVer](https://semver.org/) zur Versionierung. Verfügbare Versionen finden Sie in den Tags dieses Repositories.

---

## Autoren & Mitwirkende

- **Almanex** - *Ursprüngliche Entwicklung* - [Almanex GitHub](https://github.com/Almanex)

---

## Lizenz

Dieses Projekt lizenziert unter der MIT-Lizenz – Einzelheiten finden Sie in der Datei [LICENSE](../LICENSE).

---

## Danksagungen

- Dem Team hinter [llama.cpp](https://github.com/ggml-org/llama.cpp) für die Inferenz-Engine.
- Den Entwicklern von Markdig für die Markdown-Bibliotheken.
