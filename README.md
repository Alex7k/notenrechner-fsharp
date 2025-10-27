# Notenrechner AM

## Einführung

Der Notenrechner ist ein Kommandozeilen-Tool zur Verwaltung von Schulnoten. Er ermöglicht das Hinzufügen, Entfernen und Anzeigen von Noten sowie die Berechnung von Statistiken.

## Einrichtung

1. [dotnet (9.0)](https://dotnet.microsoft.com/en-us/download) installieren
1. Im PowerShell Terminal zum repo root navigieren
1. ```cd .\Notenrechner```
1. ```dotnet build```
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
Notenrechner list
Notenrechner list Mathematik # Nach Fach filtrieren

Notenrechner add "FP Notenrechner Projekt M323" 6.0 "Informatik"

Notenrechner remove "FP Notenrechner Projekt M323"

Notenrechner stats

Notenrechner help
```
