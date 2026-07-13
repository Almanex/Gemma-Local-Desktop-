[ English ](GUIDE.md) • [ Русский ](GUIDE_RU.md) • [ Deutsch ](GUIDE_DE.md)

# Benutzerhandbuch für Gemma Local Desktop

Willkommen im Benutzerhandbuch für Gemma Local Desktop. Dieses Dokument enthält detaillierte Informationen zur Konfiguration, Ausführung und Optimierung Ihrer lokalen KI-Entwicklungsumgebung.

---

## 1. Benutzeroberfläche und Betriebsmodi

Die Anwendung verfügt über eine duale Benutzeroberfläche, die entwickelt wurde, um standardmäßige dialogbasierte KI-Aufgaben und die interaktive Anwendungsentwicklung auszubalancieren.

### Chat-Modus (Chat Mode)
- **Zweck**: Optimiert für den klassischen Textdialog, Codeanalysen, Debugging-Hilfen und allgemeines logisches Denken.
- **Benutzeroberfläche**: Ein einspaltiger Chat-Verlauf mit Markdown-Formatierung für lesbare Code-Auszüge und strukturierten Text.

### Build-Modus (Build Mode)
- **Zweck**: Entwickelt, um Webanwendungen (HTML, CSS, JS) und Code-Artefakte zu generieren und iterativ zu verbessern.
- **Benutzeroberfläche**: Teilt den Bildschirm, sodass links das Chat-Panel und rechts der **Build-Arbeitsbereich (Build Canvas)** angezeigt wird.
- **Build-Arbeitsbereich (Build Canvas)**:
  - Enthält einen Code-Editor, der die aktuellen generierten Quelldateien anzeigt.
  - Bietet ein interaktives 720px WebView-Live-Vorschaufenster.
  - Enthält eine Schnellschaltfläche, um die generierte Anwendung im Standardbrowser Ihres Systems zu öffnen.

---

## 2. Automatische lokale Einrichtung

Die Anwendung ist so konzipiert, dass sie vollständig offline und ohne manuelle Konfigurationen läuft. Beim ersten Start wird folgende automatische Sequenz ausgeführt:

### Hardware-Erkennung
- Die App prüft das Vorhandensein einer NVIDIA-Grafikkarte und ob das System CUDA-Beschleunigung unterstützt.
- Wenn CUDA unterstützt wird, konfiguriert sie `llama.cpp` so, dass Modellschichten auf die GPU ausgelagert werden, um eine schnellere Inferenz zu ermöglichen.
- Wenn keine kompatible GPU erkannt wird, wird die CPU als Fallback verwendet.

### Laufzeitumgebung und Modell-Download
- **llama.cpp Server**: Die App lädt die passenden vorkompilierten Binärdateien des `llama.cpp`-Servers für Ihre Hardwarekonfiguration herunter.
- **Empfohlenes Modell**: Die Modellgewichte für Gemma (~1,6 GB) werden im GGUF-Format aus einem sicheren öffentlichen Repository heruntergeladen.
- **Erstellung der Sandbox**: Ein lokaler Arbeitsbereichsordner wird erstellt, um temporäre Assets, Chats und Code-Dateien zu speichern.

---

## 3. Nutzung der Ein-Klick-Vorlagen

Um die Fähigkeiten der lokalen Code-Generierung zu demonstrieren, enthält die Anwendung vier voreingestellte Vorlagen:

1. **Mega Portfolio**
   - *Beschreibung*: Generiert ein einseitiges Entwickler-Portfolio mit einer transparenten Navigationsleiste (Glassmorphism), Einblende-Animationen beim Scrollen über IntersectionObserver und einem responsiven Grid-Layout.
2. **Weather Dashboard**
   - *Beschreibung*: Erstellt eine Wetter-App-Oberfläche mit animierten SVG-Wettersymbolen, Logik zum Wechseln des Design-Themas (sonnig, regnerisch, schneebedeckt) über CSS-Variablen und Vorhersage-Grids.
3. **Mega Tetris**
   - *Beschreibung*: Generiert einen funktionsfähigen Tetris-Klon unter Verwendung von Matrixberechnungen auf Basis von 2D-Arrays, Wall-Kick-Rotationslogik, Kollisionserkennung und einer neonfarbenen CSS-Benutzeroberfläche.
4. **Premium Calculator**
   - *Beschreibung*: Baut einen wissenschaftlichen Rechner mit einer auf einem Zustandsautomaten basierenden Formelauswertung, einem Verlauf der letzten Berechnungen und responsiven Schaltflächenstilen.

So starten Sie eine Vorlage:
1. Starten Sie einen neuen Chat.
2. Klicken Sie auf eine der vier Vorlagenkarten, die im leeren Chat-Status angezeigt werden.
3. Der Prompt wird automatisch in das Eingabefeld geladen; drücken Sie die Eingabetaste, um die Generierung zu starten.

---

## 4. Projekt-Speicherung und iterative Entwicklung

Jede Chat-Sitzung wird in einem persistenten Verzeichnis auf Ihrer lokalen Festplatte gespeichert:

- **Lokaler Speicher**: Vom Assistenten generierte Dateien (z. B. `index.html`, Stylesheets, Skripte) werden direkt in einem speziellen Unterordner im Arbeitsbereichspfad des Chats gespeichert.
- **Iterative Updates**: Wenn Sie den Assistenten bitten, die generierte Anwendung zu ändern (z. B. „Ändere die Designfarbe in Blau“ oder „Füge eine Reset-Schaltfläche hinzu“), gibt das Modell den aktualisierten Code nicht nur im Chat aus, sondern überschreibt oder bearbeitet die Zieldatei direkt im Arbeitsbereichsverzeichnis.
- **Automatische Vorschau-Aktualisierung**: Das Live-Vorschaufenster im Build-Modus wird automatisch aktualisiert, sobald eine Dateiänderung im Arbeitsbereich gespeichert wird.

---

## 5. Tastaturkürzel

Verwenden Sie diese Tastenkombinationen, um schnell in der Anwendung zu navigieren:

- `Ctrl + N`: Startet eine neue Chat-Sitzung.
- `Ctrl + B`: Wechselt die geteilte Bildschirmansicht (Umschalten zwischen Chat- und Build-Modus).
- `Ctrl + \`: Blendet die rechte Canvas-Ansicht ein oder aus (Vorschau und Code-Editor).

---

## 6. Optimierung und Fehlerbehebung

### Leistungstipps
- **RAM-Beschränkungen**: Wenn das System während der Inferenz verzögert reagiert, stellen Sie sicher, dass Sie vor dem Start des Modells mindestens 4 GB freien RAM haben.
- **Grafikkartentreiber**: Installieren Sie für eine optimale CUDA-Leistung die neuesten NVIDIA-Treiber.
- **llama.cpp-Protokolle**: Fehlerbehebungsprotokolle für den lokalen Server-Subprozess werden in Ihren temporären Windows-Verzeichnissen oder App-Datenordnern gespeichert. Wenn der Server nicht startet, prüfen Sie, ob der Standardport von einem anderen Prozess belegt ist.
