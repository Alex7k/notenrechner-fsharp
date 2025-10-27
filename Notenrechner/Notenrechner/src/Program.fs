open System
open System.IO
open System.Text.Json
open FSharp.SystemTextJson
open System.Text.Json.Serialization

type Mark = { Name: string; Note: decimal; Fach: string }

let ensureDirFor (path: string) =
    Path.GetDirectoryName(path)
    |> Directory.CreateDirectory
    |> ignore

// persistence
module Storage =
    let private file = Path.Combine("data", "marks.json")

    let private opts =
        let o = JsonSerializerOptions(WriteIndented = true)
        o.Converters.Add(JsonFSharpConverter())
        o

    let save (marks: Mark list) =
        ensureDirFor (file)
        let json = JsonSerializer.Serialize(marks, opts)
        File.WriteAllText(file, json)

    let load () : Mark list =
        if File.Exists(file) then
            let json = File.ReadAllText(file)
            JsonSerializer.Deserialize<Mark list>(json, opts)
        else
            []

// Observer (log)
module Observer =
    let private logFile = Path.Combine("data", "log.txt")

    let private append (msg: string) =
        ensureDirFor (logFile)
        let timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        let entry = $"{timestamp} - {msg}{Environment.NewLine}"
        File.AppendAllText(logFile, entry)

    let notify msg =
        printfn "%s" msg
        append msg

    let showLog () =
        ensureDirFor (logFile)
        if File.Exists(logFile) then
            let lines = File.ReadAllLines(logFile)
            if lines.Length = 0 then
                printfn "Logdatei ist leer."
            else
                printfn "--- Log ---"
                lines |> Array.iter (printfn "%s")
        else
            printfn "Keine Logdatei gefunden."

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

// Strategy
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

let rec countPassed (marks: Mark list) =
    match marks with
    | [] -> 0
    | m :: rest ->
        let passed = if (Strategy.forSubject m.Fach) (float m.Note) then 1 else 0
        passed + countPassed rest

// App-Start
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
    stats                      Zeigt Durchschnitt pro Fach an
    log                        Zeigt den Log an
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
        if args.Length = 2 then
            let fach = args.[1]
            let filtered = items |> List.filter (fun m -> m.Fach.Equals(fach, StringComparison.OrdinalIgnoreCase))
            printItems filtered
        else
            printItems items
    | "add" when args.Length = 4 ->
        let note = decimal args.[2]
        if note < 1.0m || note > 6.0m then
            printfn "Die Note muss zwischen 1.0 und 6.0 liegen."
        else
        let name = args.[1]
        if items |> List.exists (fun m -> String.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase)) then
            printfn "Eintrag mit Namen '%s' existiert bereits. Abbruch." name
        else
            let newItem = { Name = name; Note = decimal args.[2]; Fach = args.[3] }
            let updated = newItem :: items
            Storage.save updated
            Observer.notify (sprintf "Hinzugefügt: %s (%s) %M" newItem.Name newItem.Fach newItem.Note)

    | "remove" when args.Length = 2 ->
        let target = args.[1]
        let updated = items |> List.filter (fun m -> m.Name <> target)
        Storage.save updated
        Observer.notify (sprintf "Entfernt (falls vorhanden): %s" target)
    | "stats" ->
        let roundTo n =
            let factor = Math.Pow(10.0, float n)
            fun (x: float) -> Math.Round(x * factor) / factor

        let passedCount = countPassed items
        printfn "Bestande Prüfungen: %d von %d" passedCount items.Length

        let overallAverage =
            items
            |> List.fold (fun acc m -> acc + float m.Note) 0.0
            |> fun total -> total / float items.Length

        printfn "Gesamtdurchschnitt: %.2f" overallAverage

        let round2 = roundTo 2
        printfn "Durchschnitt pro Fach:"
        averagePerCategory items
        |> List.iter (fun (fach, avg) ->
            let rounded = round2 avg
            let passed = (Strategy.forSubject fach) avg
            let status = if passed then "" else "Nicht "
            printfn "  %s: %0.2f (%sBestanden)" fach rounded status)

    | "log" ->
        Observer.showLog()

    | "help" ->
        printHelp()
    | _ ->
        printfn "Ungültige Parameter oder unzureichende Argumente"
        printHelp()
