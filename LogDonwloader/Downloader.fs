// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
module Downloader
open Types
open LinkBuilder
open System.IO
open System
open System.Linq
open log4net
open System.Configuration
open Logger


let private links host date logType (log:Logger.ILogger) (server,path) = server, PathFinder.getLinks <| host <| logType <| date <| path <| server <| log

let private mkDir dir =
    if not (Directory.Exists dir) then Directory.CreateDirectory dir |> ignore

let private downloadFilesInServer brach textToFind folderName (logger:ILogger) (server,files)  = 
    let esureFolderExists() = 
        sprintf "Log\\%s" <| folderName
        |> mkDir 
        
    esureFolderExists()
    let folder = folderName
    let downloader = StreamDownloader.SaveLogs.download brach folder server textToFind logger
        
    Seq.iter downloader files 

type DownloaderDto = {
    Date: SpecialDateTime option
    Environment: Types.Environment
    Program: Program
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
    mkDir "Log"
    let toDateString (specialDate:SpecialDateTime) =
        match specialDate with
        | Timed d | JustDate d -> d.ToString("yyyy-MM-gg")

    let brach = programsBranchMap.[program]
    let host = 
        match brach with
        | Branch.Auto -> "http://logauto2.servizi.allianzit"
        | Branch.RV -> "http://logdanni2.servizi.allianzit"
    match date with
    | Some d ->
        logger.info <| sprintf "\nDate: %s\nEnvirnoment: %s" (toDateString d) (environment.ToString())
        getLinksFor program environment   
        |> Array.ofList
        |> Array.Parallel.iter (links host d logType logger >> downloadFilesInServer brach textToFind folderName logger)

    | None ->
        logger.info "Errore parsing data"
       
    
type DownloaderDtoExtended = {
    Date: string
    Time: string
    Environment: Types.Environment
    Program: Program 
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