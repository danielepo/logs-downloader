module LinkBuilder

open Types

let configServers = Servers.Load "./config.json"
let servers = 
    let auto = 
        [
            Environment.Test, configServers.Servers.Auto.Test |> Array.toList
            Environment.Preprod, configServers.Servers.Auto.PreProd |> Array.toList
            Environment.Production, configServers.Servers.Auto.Prod |> Array.toList
        ] |> Map.ofList
    let rv = 
        [
            Environment.Test, configServers.Servers.RamiVari.Test |> Array.toList
            Environment.Preprod, configServers.Servers.RamiVari.PreProd |> Array.toList
            Environment.Production, configServers.Servers.RamiVari.Prod |> Array.toList
        ] |> Map.ofList

    [(Branch.Auto, auto); (Branch.RV, rv) ] |> Map.ofList

let host = 
    function
    | Branch.Auto -> configServers.Hosts.Auto
    | Branch.RV -> configServers.Hosts.RamiVari

let downloadFolder = configServers.DownloadFolder

let getLinksFor (program:Program) environment=
    
    let branch = programsBranchMap.[program]
    let ``type`` = programsMap.[program]
    let app =  match ``type``, branch with
                    | Application.WebApp,Branch.Auto ->  "Auto"
                    | Application.WebApp,Branch.RV ->  "Danni"
                    | Application.WebService,_ -> "Autosem"

    let buildLink server =
        sprintf "/%s/%s/%s/Logs/" server app (enumToString program)
    
        // TODO Gestire invece dei server, gli applicativi: IncassoDA, WSIncassi, NGRA2013 ecc
    servers.[branch].[environment]
    |> List.map (fun server ->  server, buildLink server)
