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


let private links date logType (server,path) = server, PathFinder.getLinks <| logType <| date <| path <| server

let mutable private log:Logger.Logger = null
let private mkDir dir =
    if not (Directory.Exists dir) then Directory.CreateDirectory dir |> ignore

let private downloadFilesInServer textToFind folderName (server,files)  = 
    let esureFolderExists() = 
        sprintf "Log\\%s" <| folderName
        |> mkDir 
        
    esureFolderExists()
    let folder = folderName
    let downloader = StreamDownloader.SaveLogs.download folder server textToFind
        
    Seq.iter downloader files 

type DownloaderDto = {
    Date: SpecialDateTime option
    Environment: Types.Environment
    Program: Program * Application
    TextToFind: string
    FolderName: string
    LogType: LogType
    Logger: Logger.Logger
}    


let downloadLogs data =
    let date = data.Date
    let environment = data.Environment
    let appType = data.Program
    let textToFind = data.TextToFind 
    let folderName = data.FolderName 
    let logType = data.LogType
    let logger = data.Logger
    mkDir "Log"
    log <- logger
    let toDateString (specialDate:SpecialDateTime) =
        match specialDate with
        | Timed d | JustDate d -> d.ToString("yyyy-MM-gg")

    match date with
    | Some d ->
            
        log.info "\nDate: %s\nEnvirnoment: %s" <| toDateString d <| environment.ToString()
        getLinksFor appType environment   
        |> Array.ofList
        |> Array.Parallel.iter (links d logType >> downloadFilesInServer textToFind folderName)
    | None ->
        log.info "Errore parsing data"
       
    
type DownloaderDtoExtended = {
    Date: string
    Time: string
    Environment: Types.Environment
    Program: Program 
    TextToFind: string
    FolderName: string
    LogType: LogType
    Logger: Logger.Logger
}    

let programToProgramApp = 
    function
    | Types.Program.IncassoDA -> Types.Program.IncassoDA, Types.Application.WebApp
    | Types.Program.GestioneLibriMatricolaDA -> Types.Program.GestioneLibriMatricolaDA, Types.Application.WebApp
    | Types.Program.WSIncassi -> Types.Program.WSIncassi, Types.Application.WebService

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
        Program= programToProgramApp data.Program
        TextToFind= data.TextToFind
        FolderName= data.FolderName
        LogType= data.LogType
        Logger= data.Logger
    }
    downloadLogs dto