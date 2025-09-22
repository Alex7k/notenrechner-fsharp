// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"

type Mark = { Name: string; Note: decimal; Fach: string }

let items = [
    { Name = "Vektoren"; Note = 4.2m; Fach = "Mathe" }
    { Name = "Potenzen"; Note = 4.8m; Fach = "Mathe" }
    { Name = "Logarithmen"; Note = 3.5m; Fach = "Mathe" }
    { Name = "Kinematik"; Note = 2.3m; Fach = "Physik" }
    { Name = "Elektrotechnik"; Note = 3.7m; Fach = "Physik" }
    { Name = "Druck"; Note = 4.0m; Fach = "Physik" }
    { Name = "Terror"; Note = 3.2m; Fach = "Deutsch" }
    { Name = "Die Schwarze Spinne"; Note = 1.2m; Fach = "Deutsch" }
    { Name = "Vocab Test 1"; Note = 5.0m; Fach = "Englisch" }
    { Name = "Vocab Test 2"; Note = 5.5m; Fach = "Englisch" }
]

let printItems items = items |> List.iter (fun mark -> printfn "%s: %M" mark.Name mark.Note)
let averagePerCategory items = items |> List.groupBy (fun mark -> mark.Fach) |> List.map (fun (fach, marks) -> (fach, (marks |> List.averageBy (fun mark -> mark.Note))))

printItems items
printfn "---- Durchschnitt pro Fach ----"
averagePerCategory items |> List.iter (fun (fach, avg) -> printfn $"{fach}: {avg:F2}")