# Notenrechner AM

## Verwendung

```txt
Verwendung:
    list (Fach)                Zeigt liste gespeicherten Noten an, optional nach Fach gefiltert
    add <Name> <Note> <Fach>   Fügt eine neue Note hinzu
    remove <Name>              Entfernt eine Note nach Name
    stats                      Zeigt Durchschnitt pro Fach
    help                       Zeigt diese Hilfe
<> - Obligatorisch
() - Optional
```

### Beispiele

```ps1
Notenrechner.exe list
Notenrechner.exe list Mathematik # Nach Fach filtrieren

Notenrechner.exe add "FP Notenrechner Projekt M323" 6.0 "Informatik"

Notenrechner.exe remove "FP Notenrechner Projekt M323"

Notenrechner.exe stats

Notenrechner.exe help
```
