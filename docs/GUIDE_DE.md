# Gemma Local Desktop Benutzerhandbuch — Lokaler Offline-KI-Assistent und Arbeitsbereich für Windows

> [!NOTE]
> **TL;DR: Kurzzusammenfassung**
> - **KI ohne Cloud**: Läuft komplett offline unter Windows mit Google Gemma-Modellen über `llama.cpp`.
> - **Dual-Modus-Arbeitsbereich**: Wechseln Sie nahtlos zwischen klassischem Chat-Dialog und aktiver Code-Generierung.
> - **Interaktive Live-Vorschau**: Zeigen Sie generierte HTML/CSS/JS-Anwendungen direkt in einer WebView an.
> - **Persistente Sandbox**: Iteratives Editieren speichert den Code direkt im lokalen Projektverzeichnis.

---

Gemma Local Desktop ist ein Desktop-Assistent, der private, offline nutzbare KI-Power direkt auf Ihren Windows-PC bringt. Durch die lokale Ausführung optimierter Google Gemma-Modelle macht dieses Programm Cloud-Abhängigkeiten, Abonnements und Datenschutzbedenken überflüssig, sodass Sie Webanwendungen erstellen und komplexe Programmlogiken zu 100% offline analysieren können.

---

## Funktionen im Überblick

### Chat-Modus (Chat Mode)
Der Chat-Modus bietet eine Benutzeroberfläche, die für normale Textdialoge optimiert ist. Dieser Modus eignet sich am besten zum Erklären von Konzepten, Analysieren von Code-Auszügen, Ausarbeiten von Logiken und Beheben von Softwarefehlern. Die Textausgabe erfolgt in klar strukturiertem Markdown.

### Build-Modus (Build Mode)
Der Build-Modus bietet eine geteilte Bildschirmansicht für die Erstellung interaktiver Webanwendungen (HTML, CSS, JS). Sobald das Modell Code schreibt, werden die Dateien im lokalen Projektverzeichnis gespeichert und sofort im interaktiven 720px Live-Vorschaufenster angezeigt.

### Live-Vorschau (Live Preview Canvas)
Das Vorschaufenster nutzt Microsoft WebView2, um Web-Inhalte in Echtzeit auszuführen. Eine Schnellschaltfläche ermöglicht es Ihnen, Ihre Kreation im Standardbrowser Ihres Systems zu öffnen, um Tests im Vollbildmodus und Inspektionen über Entwickler-Tools durchzuführen.

### Projekt-Sandbox (Workspace Sandbox)
Jede Chat-Sitzung wird in einem eigenen Ordner auf Ihrer Festplatte gespeichert. Anpassungen werden iterativ durchgeführt – wenn Sie das Modell bitten, eine Schaltfläche zu ändern, schreibt es die Dateien im Projektverzeichnis direkt um, woraufhin sich das Vorschaufenster automatisch aktualisiert.

---

## Sprachunterstützung und Lokalisierung

Die Benutzeroberfläche von Gemma Local Desktop erkennt Ihre Windows-Systemsprache beim Start automatisch.

### Unterstützte Sprachen
- **Englisch** (Standard-Fallback)
- **Russisch**
- **Deutsch**

### Parameter zum Überschreiben der Sprache
Wenn Sie die Anwendung unabhängig von Ihren Systemeinstellungen in einer bestimmten Sprache ausführen möchten, können Sie die ausführbare Datei über die Eingabeaufforderung oder PowerShell mit dem Parameter `--lang` starten:
```powershell
# Anwendung auf Russisch starten
GemmaChatWindows.exe --lang ru

# Anwendung auf Deutsch starten
GemmaChatWindows.exe --lang de

# Anwendung auf Englisch starten
GemmaChatWindows.exe --lang en
```

---

## Schritt-für-Schritt-Anleitung für den Schnellstart

Folgen Sie diesen Schritten, um Ihren lokalen Assistenten in wenigen Minuten einzurichten:

1. **Schritt 1: Herunterladen und Klonen** — Klonen Sie das Repository mit Git oder laden Sie das ZIP-Archiv herunter und entpacken Sie es in einen beliebigen Ordner auf Ihrem PC.
2. **Schritt 2: Systemanforderungen prüfen** — Stellen Sie sicher, dass das .NET 10.0 SDK auf Ihrem System installiert ist, um das Projekt zu kompilieren.
3. **Schritt 3: Anwendung ausführen** — Öffnen Sie ein Terminal im Projektordner und starten Sie das Programm mit dem Befehl `dotnet run` im Verzeichnis `GemmaChatCsharp`.
4. **Schritt 4: Automatische Einrichtung** — Warten Sie, während das Programm Ihre Grafikkarte prüft, die passenden lokalen `llama.cpp`-Serverdateien herunterlädt und das empfohlene Gemma-Modell (~1,6 GB) abruft.
5. **Schritt 5: Vorlage auswählen** — Klicken Sie im leeren Chat-Status auf eine der Vorlagenkarten (wie **Mega Tetris** oder **Weather Dashboard**), um den Prompt zu laden, und drücken Sie die Eingabetaste, um die Code-Generierung zu starten.

---

## Tipps und Tastaturkürzel

Steigern Sie Ihre Produktivität mit den integrierten System-Tastaturkürzeln:

| Tastaturkürzel | Aktion / Beschreibung |
| --- | --- |
| `Ctrl + N` | Neue Chat-Sitzung starten und das Arbeitsbereich-Canvas leeren. |
| `Ctrl + B` | Geteilte Bildschirmansicht umschalten (Wechseln zwischen Chat- und Build-Modus). |
| `Ctrl + \` | Vorschau- / Code-Bereich ein- oder ausblenden. |

---

## FAQ und Fehlerbehebung

### Windows Defender SmartScreen blockiert den Start der App
Da ausführbare Dateien von kostenlosen Open-Source-Projekten nicht mit teuren kommerziellen Zertifikaten signiert sind, zeigt Windows Defender eine Warnung an. Klicken Sie auf **Weitere Informationen** und anschließend auf **Trotzdem ausführen**, um fortzufahren.

### Das Modell läuft sehr langsam oder verzögert
Gemma Local Desktop unterstützt GPU-Beschleunigung via NVIDIA CUDA. Vergewissern Sie sich, dass die neuesten NVIDIA-Treiber installiert sind. Ohne kompatible GPU läuft die Anwendung über die CPU, was die Inferenzgeschwindigkeit reduziert. Schließen Sie andere Programme, um mindestens 4 GB RAM freizugeben.

### Zurücksetzen der Konfiguration und erneuter Download
Wenn Downloads beschädigt wurden oder abgebrochen sind, navigieren Sie zum lokalen AppData-Ordner Ihres Benutzers (`%LocalAppData%\GemmaLocalDesktop`) und löschen Sie die Ordner `runtimes` oder `models`, um beim nächsten Start einen erneuten Download zu erzwingen.

---

## Community und Support

Wir bauen eine Community für private, lokale Software auf. Wenn Ihnen das Projekt gefällt:
- **Stern vergeben**: Unterstützen Sie das Projekt mit einem Stern auf [GitHub](https://github.com/Almanex/Gemma-Local-Desktop-).
- **Fehler melden**: Sie haben einen Fehler gefunden? Öffnen Sie ein Ticket unter Issues.
- **Pull Requests einreichen**: Lesen Sie unsere [CONTRIBUTING.md](../CONTRIBUTING.md), um zu erfahren, wie Sie Code und Übersetzungen beisteuern können.
