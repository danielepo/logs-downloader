module LinkBuilder

open Types

let servers = 
    let listofservers = ([
    Environment.Test, ["BREPADTF2S01"]
    Environment.Preprod, ["BREPADPP2S01";"BREPADPP2S02"]
    Environment.Production, [
        "BREPADAS2S01"; "BREPADAS2S02"; "BREPADAS2S03"
        "BREPADAS2S04"; "BREPADAS2S05"; "BREPADAS2S06"
        "BREPADAS2S07"; "BREPADAS2S08"; "BREPADAS2S09"
        "BREPADAS2S10"; "BREPADAS2S11"; "BREPADAS2S12"
        "BREPADAS2S13"; "BREPADAS2S14"; "BREPADAS2S15"
        "BREPADAS2S16"; "BREPADAS2S17"; "BREPADAS2S18" ]
        ])
    listofservers |> Map.ofList
    
let getLinksFor (program:Program) environment=
    
    let app =  match programsMap.[program] with
                    | Application.WebApp ->  "Auto"
                    | Application.WebService -> "Autosem"
    let buildLink server =
        sprintf "%s/%s/%s/Logs/" server app (enumToString program)
    
        // TODO Gestire invece dei server, gli applicativi: IncassoDA, WSIncassi, NGRA2013 ecc
    servers.[environment]
    |> List.map (fun server ->  server, buildLink server)
