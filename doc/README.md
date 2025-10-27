# Notenrechner AM

## Einführung

Der Notenrechner ist ein Kommandozeilen-Tool zur Verwaltung von Schulnoten. Er ermöglicht das Hinzufügen, Entfernen und Anzeigen von Noten sowie die Berechnung von Statistiken.

Dieses Programm ist mit Windows 11 Home getestet worden.

## Einrichtung

1. [dotnet (9.0)](https://dotnet.microsoft.com/en-us/download) installieren
1. Im PowerShell Terminal zum repo root navigieren
1. ```cd .\Notenrechner\Notenrechner```
1. (```dotnet build```)
1. ```dotnet run -- [Argumente]```

## Argumente

```txt
Verwendung:
    list (Fach)                Zeigt liste gespeicherten Noten an, optional nach Fach gefiltert
    add <Name> <Note> <Fach>   Fügt eine neue Note hinzu
    remove <Name>              Entfernt eine Note nach Name
    stats                      Zeigt Durchschnitt pro Fach an
    log                        Zeigt den Log an
    help                       Zeigt diese Hilfe
<> - Obligatorisch
() - Optional
```

### Beispiele

```ps1
dotnet run -- list
dotnet run -- list Mathematik # Nach Fach filtrieren

dotnet run -- add "FP Notenrechner Projekt M323" 6.0 "Informatik"

dotnet run -- remove "FP Notenrechner Projekt M323"

dotnet run -- stats

dotnet run -- help
```

### Hinweis

Um alle gespeicherten Daten zurückzusetzen, kann der Ordner `data` gelöscht werden.

### Fun fact

Daten werden in .\Notenrechner\Notenrechner\data gespeichert.
