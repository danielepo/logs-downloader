// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open Types
open LinkBuilder
open System.IO

[<EntryPoint>]
let main argv = 

    if not (Directory.Exists "Log") then Directory.CreateDirectory "Log" |> ignore


    let links date (server,path) = sprintf "Log/%s" server, PathFinder.getLinks LogType.DebugTrace date path

    let downloadFilesInServer (server,files) = 
        for file in files do 
            Downloader.SaveLogs.download file server "79288549"
    
    
    getLinksFor Application.WebService Environment.Preprod
    |> Array.ofList
    |> Array.Parallel.iter (links (System.DateTime(2016,12,15,0,0,0)) >> downloadFilesInServer)
        
    
    0 // return an integer exit code
