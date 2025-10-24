open System
open System.IO
open System.Text.Json
open FSharp.SystemTextJson
open System.Text.Json.Serialization

type Mark = { Name: string; Note: decimal; Fach: string }

// persistence
module Storage =
    let private file = Path.Combine("data", "marks.json")

    let private opts =
        let o = JsonSerializerOptions(WriteIndented = true)
        o.Converters.Add(JsonFSharpConverter())
        o

    let ensureDir () =
        Directory.CreateDirectory(Path.GetDirectoryName(file)) |> ignore

    let save (marks: Mark list) =
        ensureDir ()
        let json = JsonSerializer.Serialize(marks, opts)
        File.WriteAllText(file, json)

    let load () : Mark list =
        if File.Exists(file) then
            let json = File.ReadAllText(file)
            JsonSerializer.Deserialize<Mark list>(json, opts)
        else
            []

// test data
module Factory =
    let createInitialMarks () : Mark list =
        [ { Name = "Vektoren";           Note = 4.2m; Fach = "Mathematik" }
          { Name = "Potenzen";           Note = 4.8m; Fach = "Mathematik" }
          { Name = "Logarithmen";        Note = 3.5m; Fach = "Mathematik" }
          { Name = "Kinematik";          Note = 2.3m; Fach = "Physik" }
          { Name = "Elektrotechnik";     Note = 3.7m; Fach = "Physik" }
          { Name = "Druck";              Note = 4.0m; Fach = "Physik" }
          { Name = "Terror";             Note = 3.2m; Fach = "Deutsch" }
          { Name = "Die Schwarze Spinne";Note = 1.2m; Fach = "Deutsch" }
          { Name = "Vocab Test 1";       Note = 5.0m; Fach = "Englisch" }
          { Name = "Vocab Test 2";       Note = 5.5m; Fach = "Englisch" } ]

// showing
let printItems items =
    items |> List.iter (fun m -> printfn "%s (%s): %M" m.Name m.Fach m.Note)

let averagePerCategory items =
    items
    |> List.groupBy (fun m -> m.Fach)
    |> List.map (fun (fach, marks) -> fach, (marks |> List.averageBy (fun m -> float m.Note)))

let isPassedMark (note: float) =
    note >= 4.0

// ---------- Strategy ----------
type PassStrategy = float -> bool

module Strategy =
    let passByThreshold threshold : PassStrategy =
        fun note -> note >= threshold

    let forSubject =
        function
        | "Mathe"     -> passByThreshold 4.0
        | "Physik"    -> passByThreshold 4.0
        | "Deutsch"   -> passByThreshold 4.0
        | "Englisch"  -> passByThreshold 4.0
        | "Informatik"-> passByThreshold 4.0
        | _           -> passByThreshold 4.0

// ---------- App-Start ----------
//printfn "Wilkommen zu dem Notenrechner"

// Laden oder erstellen falls es nicht gibt (Seed)
let items =
    match Storage.load () with
    | [] ->
        printfn "Keine Daten gefunden - schreibe Seed (dummy daten) nach data/marks.json"
        let seed = Factory.createInitialMarks ()
        Storage.save seed
        seed
    | xs ->
        //printfn "Daten vom Speicher geladen"
        xs

let args = Environment.GetCommandLineArgs() |> Array.skip 1

let printHelp () =
    printfn """
Verwendung:
    list (Fach)                Zeigt liste gespeicherten Noten an, optional nach Fach gefiltert
    add <Name> <Note> <Fach>   Fügt eine neue Note hinzu
    remove <Name>              Entfernt eine Note nach Name
    stats                      Zeigt Durchschnitt pro Fach
    help                       Zeigt diese Hilfe
<> - Obligatorisch
() - Optional
"""

if args.Length = 0 then
    printfn "Keine Parameter angegeben."
    printHelp()
else
    match args.[0].ToLower() with
    | "list" ->
        printItems items
    | "add" when args.Length = 4 ->
        let name = args.[1]
        // abbruch wenn name schon existiert
        if items |> List.exists (fun m -> String.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase)) then
            printfn "Eintrag mit Namen '%s' existiert bereits. Abbruch." name
        else
            let newItem = { Name = name; Note = decimal args.[2]; Fach = args.[3] }
            let updated = newItem :: items
            Storage.save updated
            printfn "Hinzugefügt: %s (%s) %M" newItem.Name newItem.Fach newItem.Note
    | "remove" when args.Length = 2 ->
        let updated = items |> List.filter (fun m -> m.Name <> args.[1])
        Storage.save updated
        printfn "Entfernt (falls vorhanden): %s" args.[1]
    | "stats" ->
        printfn "Durchschnitt pro Fach:"
        averagePerCategory items |> List.iter (fun (fach, avg) ->
            let passed = Strategy.forSubject fach avg
            let status = if passed then "" else "Nicht "
            printfn "%s: %0.2f (%sBestanden)" fach avg status)
    | "help" ->
        printHelp()
    | _ ->
        printfn "Ungültige Parameter oder unzureichende Argumente"
        printHelp()
