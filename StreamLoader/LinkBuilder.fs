module LinkBuilder

open Types

let servers = 
    let auto = 
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
    let rv = 
        let listofservers = ([
        Environment.Test, ["BREDNNTF2S01_LBD"]
        Environment.Preprod, ["BREDNNPP2S01_LBD"; "BREDNNPP2S02_LBD"]
        Environment.Production, [
            "BREDNNAS2S01_LBD";"BREDNNAS2S02_LBD"
            "BREDNNAS2S03_LBD";"BREDNNAS2S04_LBD"
            "BREDNNAS2S05_LBD";"BREDNNAS2S06_LBD"
            "BREDNNAS2S07_LBD";"BREDNNAS2S08_LBD"
            "BREDNNAS2S09_LBD";"BREDNNAS2S10_LBD"
            "BREDNNAS2S11_LBD" ]
            ])
        listofservers |> Map.ofList

    [(Branch.Auto, auto)
     (Branch.RV, rv) ] |> Map.ofList
    
let getLinksFor (program:Program) environment=
    
    let branch = programsBranchMap.[program]
    let ``type`` = programsMap.[program]
    let app =  match ``type``, branch with
                    | Application.WebApp,Branch.Auto ->  "Auto"
                    | Application.WebApp,Branch.RV ->  "Danni"
                    | Application.WebService,_ -> "Autosem"
    let host = 
        match branch with
        | Branch.Auto -> "http://logauto2.servizi.allianzit"
        | Branch.RV -> "http://logdanni2.servizi.allianzit"
    let buildLink server =
        sprintf "/%s/%s/%s/Logs/" server app (enumToString program)
    
        // TODO Gestire invece dei server, gli applicativi: IncassoDA, WSIncassi, NGRA2013 ecc
    servers.[branch].[environment]
    |> List.map (fun server ->  server, buildLink server)
