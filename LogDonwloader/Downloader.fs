// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
module Downloader
open Types
open FSharp.Data
open LinkBuilder
open System.IO
open System

open log4net
open System.Configuration
open Logger


let private getFileLinks host date logType (log:Logger.ILogger) (server,path) = 
    server, (PathFinder.getLinks host logType date path server log)

let private mkDir dir =
    if not (Directory.Exists dir) then Directory.CreateDirectory dir |> ignore

let private downloadFilesInServer brach textToFind folderName (logger:ILogger) (server,files)  = 
    let esureFolderExists() = 
        sprintf "%s\\%s" LinkBuilder.downloadFolder folderName
        |> mkDir 
        
    esureFolderExists()
    let downloader = StreamDownloader.SaveLogs.download brach folderName server textToFind logger
        
    Seq.map downloader files 

type DownloaderDto = {
    Date: SpecialDateTime option
    Environment: Types.Environment
    Program: string
    TextToFind: string
    FolderName: string
    LogType: LogType
    Logger: Logger.ILogger
}    


let downloadLogs data =
    let date = data.Date
    let environment = data.Environment
    let program = data.Program
    let textToFind = data.TextToFind 
    let folderName = data.FolderName 
    let logType = data.LogType
    let logger = data.Logger
    mkDir LinkBuilder.downloadFolder
    let toDateString (specialDate:SpecialDateTime) =
        match specialDate with
        | Timed d | JustDate d -> d.ToString("yyyy-MM-gg")

    let brach = programsBranchMap.[program]

    let host = LinkBuilder.host brach
    match date with
    | Some d ->
        logger.info <| sprintf "\nDate: %s\nEnvirnoment: %s" (toDateString d) (environment.ToString())
        getLinksFor program environment   
        |> Array.ofList
        |> Array.Parallel.map (getFileLinks host d logType logger >> downloadFilesInServer host textToFind folderName logger)
        |> Seq.ofArray
        |> Seq.concat 
        |> Seq.filter (fun x -> x = DownloadResult.Downloaded)
        |> (not << Seq.isEmpty)
    | None ->
        logger.info "Errore parsing data"
        false
       
    
type DownloaderDtoExtended = {
    Date: string
    Time: string
    Environment: Types.Environment
    Program: string 
    TextToFind: string
    FolderName: string
    LogType: LogType
    Logger: Logger.ILogger
}    

let programToProgramApp x= programsMap.[x]

let DownladLogs (data:DownloaderDtoExtended) =
    let date =
        if String.IsNullOrEmpty(data.Time) then
            match DateTime.TryParse data.Date with
            | true, d -> Some (JustDate d)
            | false, _ -> None
        else 
            match DateTime.TryParse (data.Date + " " + data.Time) with
            | true, d -> Some (Timed d)
            | false, _ -> None
    let dto:DownloaderDto = {
        Date = date
        Environment= data.Environment
        Program= data.Program
        TextToFind= data.TextToFind
        FolderName= data.FolderName
        LogType= data.LogType
        Logger= data.Logger
    }
    downloadLogs dto