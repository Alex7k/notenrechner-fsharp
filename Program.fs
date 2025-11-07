open System
open System.IO
open System.Text.Json
open FSharp.SystemTextJson
open System.Text.Json.Serialization

type Mark = { Name: string; Note: decimal; Subject: string }

// helpers

let ensureDirFor (path: string) =
    Path.GetDirectoryName(path)
    |> Directory.CreateDirectory
    |> ignore

let roundTo step =
    fun (x: float) ->
        Math.Round(x / step, MidpointRounding.AwayFromZero) * step

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
                printfn "Log file is empty."
            else
                printfn "--- Log ---"
                lines |> Array.iter (printfn "%s")
        else
            printfn "Log file not found."

// test data
module Factory =
    let createInitialMarks () : Mark list =
        [ { Name = "Vektoren";           Note = 4.2m; Subject = "Math" }
          { Name = "Potenzen";           Note = 4.8m; Subject = "Math" }
          { Name = "Logarithmen";        Note = 3.5m; Subject = "Math" }
          { Name = "Kinematik";          Note = 2.3m; Subject = "Physics" }
          { Name = "Elektrotechnik";     Note = 3.7m; Subject = "Physics" }
          { Name = "Druck";              Note = 4.0m; Subject = "Physics" }
          { Name = "Terror";             Note = 3.2m; Subject = "German" }
          { Name = "Die Schwarze Spinne";Note = 1.2m; Subject = "German" }
          { Name = "Vocab Test 1";       Note = 5.0m; Subject = "English" }
          { Name = "Vocab Test 2";       Note = 5.5m; Subject = "English" } ]

// showing
let printItems items =
    items |> List.iter (fun m -> printfn "%s (%s): %M" m.Name m.Subject m.Note)

let averagePerCategory items =
    items
    |> List.groupBy (fun m -> m.Subject)
    |> List.map (fun (subject, marks) -> subject, (marks |> List.averageBy (fun m -> float m.Note)))

let isPassedMark (note: float) =
    note >= 4.0

// Strategy
type PassStrategy = float -> bool

module Strategy =
    let passByThreshold threshold : PassStrategy =
        fun note -> note >= threshold

    // define custom passing threshholds
    let forSubject =
        function
        | "Math"             -> passByThreshold 4.0
        | "Physics"          -> passByThreshold 4.0
        | "German"           -> passByThreshold 4.0
        | "English"          -> passByThreshold 4.0
        | "Computer Science" -> passByThreshold 4.0
        | _                  -> passByThreshold 4.0

let rec countPassed (marks: Mark list) =
    match marks with
    | [] -> 0
    | m :: rest ->
        let passed = if (Strategy.forSubject m.Subject) (float m.Note) then 1 else 0
        passed + countPassed rest

// Decorator
module Decorator =
    type MarkDecorator = Mark -> Mark

    let rounded (step: float) : MarkDecorator =
        fun m ->
            let round = roundTo step
            let roundedNote = decimal (round (float m.Note))
            { m with Note = roundedNote }

    let compose (decorators: MarkDecorator list) : MarkDecorator =
        decorators |> List.reduce (>>)

// App-Start

// Load or create if doesn't already exist (seeding)
let items =
    match Storage.load () with
    | [] ->
        printfn "No data found - writing seed (dummy data) to data/marks.json"
        let seed = Factory.createInitialMarks ()
        Storage.save seed
        seed
    | xs ->
        //printfn "Data loaded from storage"
        xs

let args = Environment.GetCommandLineArgs() |> Array.skip 1

let printHelp () =
    printfn """
Usage:
    list (Subject)               Shows list of stored marks, optionally filtered by subject
    add <Name> <Mark> <Subject>  Adds a new mark
    remove <Name>                Removes a mark by name
    stats                        Shows average per subject
    log                          Shows the log
    help                         Shows this help
<> - Required
() - Optional
"""

if args.Length = 0 then
    printfn "No parameters given."
    printHelp()
else
    match args.[0].ToLower() with
    | "list" ->
        if args.Length = 2 then
            let subject = args.[1]
            let filtered = items |> List.filter (fun m -> m.Subject.Equals(subject, StringComparison.OrdinalIgnoreCase))
            printItems filtered
        else
            printItems items
    | "add" when args.Length = 4 ->
        let note = decimal args.[2]
        if note < 1.0m || note > 6.0m then
            printfn "The mark must be between 1.0 and 6.0."
        else
        let name = args.[1]
        if items |> List.exists (fun m -> String.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase)) then
            printfn "Entry '%s' already exists. Exiting." name
        else
            let newItem = { Name = name; Note = decimal args.[2]; Subject = args.[3] }
            let updated = newItem :: items
            Storage.save updated
            Observer.notify (sprintf "Added: %s (%s) %M" newItem.Name newItem.Subject newItem.Note)

    | "remove" when args.Length = 2 ->
        let target = args.[1]
        let updated = items |> List.filter (fun m -> m.Name <> target)
        Storage.save updated
        Observer.notify (sprintf "Removed (if existed): %s" target)
    | "stats" ->
        // rounding decorator
        let roundDecorator = Decorator.rounded 0.5
        let roundedItems = items |> List.map roundDecorator

        let passedCount = countPassed roundedItems
        printfn "Passed exams (post rounding): %d out of %d" passedCount roundedItems.Length

        let overallAverage =
            roundedItems
            |> List.fold (fun acc m -> acc + float m.Note) 0.0
            |> fun total -> total / float roundedItems.Length

        printfn "Total average (rounded): %.2f" overallAverage

        let roundHalf = roundTo 0.5
        printfn "Average per subject:"
        averagePerCategory items
        |> List.iter (fun (subject, avg) ->
            let rounded = roundHalf avg
            let passed = (Strategy.forSubject subject) rounded
            let status = if passed then "Passed" else "Failed"
            printfn "  %s: %.2f (%.1f rounded) -> %s"
                subject avg rounded status)

    | "log" ->
        Observer.showLog()

    | "help" ->
        printHelp()
    | _ ->
        printfn "Invalid parameter or insufficient arguments."
        printHelp()
