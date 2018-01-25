module LinkBuilder

open Types

let servers = 
    let auto = 
        [
            Environment.Test, config.Servers.Auto.Test |> Array.toList
            Environment.Preprod, config.Servers.Auto.PreProd |> Array.toList
            Environment.Production, config.Servers.Auto.Prod |> Array.toList
        ] |> Map.ofList
    let rv = 
        [
            Environment.Test, config.Servers.RamiVari.Test |> Array.toList
            Environment.Preprod, config.Servers.RamiVari.PreProd |> Array.toList
            Environment.Production, config.Servers.RamiVari.Prod |> Array.toList
        ] |> Map.ofList

    [(Branch.Auto, auto); (Branch.RV, rv) ] |> Map.ofList

let host = 
    function
    | Branch.Auto -> config.Hosts.Auto
    | Branch.RV -> config.Hosts.RamiVari

let downloadFolder = config.DownloadFolder

let getLinksFor (program:string) environment=
    
    let branch = programsBranchMap.[program]
    let ``type`` = programsMap.[program]
    let app =  match ``type``, branch with
                    | Application.WebApp,Branch.Auto ->  "Auto"
                    | Application.WebApp,Branch.RV ->  "Danni"
                    | Application.WebService,_ -> "Autosem"

    let buildLink server =
        sprintf "/%s/%s/%s/Logs/" server app program
    
        // TODO Gestire invece dei server, gli applicativi: IncassoDA, WSIncassi, NGRA2013 ecc
    servers.[branch].[environment]
    |> List.map (fun server ->  server, buildLink server)
