﻿module LinkBuilder

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

let programs = [
    (IncassoDA, WebApp)
    (WSIncassi, WebService)
]

let getLinksFor (programApp:Program*Application) environment=
    let program, application = programApp
    let servers = 
        match environment with 
        | Test -> environments.Test
        | Preprod -> environments.Preprod
        | Production -> environments.Production
        // TODO Gestire invece dei server, gli applicativi: IncassoDA, WSIncassi, NGRA2013 ecc
    servers
    |> List.map (fun x -> 
        let app =  match application with
                    | WebApp ->  "Auto"
                    | WebService -> "Autosem"
        x, sprintf "%s/%s/%s/Logs/" x app (enumToString program))
