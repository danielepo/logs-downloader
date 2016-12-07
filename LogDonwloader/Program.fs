// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open Types
open LinkBuilder
open System.IO

[<EntryPoint>]
let main argv = 

//
//    let downloadFilesInServer floder server = 
//        if not (Directory.Exists floder) then Directory.CreateDirectory floder |> ignore
//        for file in downloadableFiles server timeSpecified do 
//            printf "Server: %s\tfile: %s\n" server file.Description
//            match file.FileType with
//            | Gzip -> downloadGzip file.Uri (sprintf @"%s\%s_%s" floder server file.Description)
//            | Log -> downloadText file.Uri (sprintf @"%s\%s_%s" floder server file.Description)
//
//    match runEnvironment with 
//        | RunEnvironment.Test -> testServers |> List.iter (downloadFilesInServer "Logs")
//        | RunEnvironment.Preprod -> preProdServers |> List.iter (downloadFilesInServer "Logs")
//        | RunEnvironment.Prod -> prodServers |> Array.ofList |> Array.Parallel.iter (downloadFilesInServer "Logs")
//
    if not (Directory.Exists "Log") then Directory.CreateDirectory "Log" |> ignore


    let links date (server,path) = sprintf "Log/%s" server, PathFinder.getLinks 2 date path

    let downloadFilesInServer (server,files) = 
        for file in files do 
            Downloader.SaveLogs.download file server ""
    
    
    getLinksFor Application.WebApp Environment.Production
    |> Array.ofList
    |> Array.Parallel.iter (links (System.DateTime(2016,12,6,0,0,0)) >> downloadFilesInServer)
        
    
    0 // return an integer exit code
