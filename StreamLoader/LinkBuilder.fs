module LinkBuilder

open Types

let environments = {
    Test = ["BREPADTF2S01"]
    Preprod = ["BREPADPP2S01";"BREPADPP2S02"]
    Production = 
    [
        "BREPADAS2S01"
        "BREPADAS2S02"
        "BREPADAS2S03"
        "BREPADAS2S04"
        "BREPADAS2S05"
        "BREPADAS2S06"
        "BREPADAS2S07"
        "BREPADAS2S08"
        "BREPADAS2S09"
        "BREPADAS2S10"
        "BREPADAS2S11"
        "BREPADAS2S12"
        "BREPADAS2S13"
        "BREPADAS2S14"
        "BREPADAS2S15"
        "BREPADAS2S16"
        "BREPADAS2S17"
        "BREPADAS2S18"
    ]
}

let getLinksFor (application) (environment) =
    let servers = 
        match environment with 
        | Test -> environments.Test
        | Preprod -> environments.Preprod
        | Production -> environments.Production

    servers
    |> List.map (fun x -> 
        match application with
        | WebApp ->  x, sprintf "%s/Auto/IncassoDA/Logs/" x
        | WebService -> x, sprintf "%s/Autosem/WSIncassi/Logs/" x)

