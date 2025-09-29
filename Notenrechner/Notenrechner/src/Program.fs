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
let seed : Mark list =
    [ { Name = "Vektoren";           Note = 4.2m; Fach = "Mathe" }
      { Name = "Potenzen";           Note = 4.8m; Fach = "Mathe" }
      { Name = "Logarithmen";        Note = 3.5m; Fach = "Mathe" }
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

// ---------- App-Start ----------
printfn "Wilkommen zu dem Notenrechner"

// Laden oder erstellen falls es nicht gibt (Seed)
let items =
    match Storage.load () with
    | [] ->
        printfn "Keine Daten gefunden – schreibe Seed nach data/marks.json"
        Storage.save seed
        seed
    | xs ->
        printfn "Daten geladen"
        xs

printItems items
printfn "---- Durchschnitt pro Fach ----"
averagePerCategory items |> List.iter (fun (fach, avg) -> printfn $"{fach}: {avg:F2}")

// test
let items' = { Name = "Vocab Test 3"; Note = 5.3m; Fach = "Englisch" } :: items
Storage.save items'
