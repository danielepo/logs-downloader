﻿// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
module Downloader
open Types
open LinkBuilder
open System.IO
open System
open System.Linq
open log4net
open System.Configuration

    
//    log4net.Config.XmlConfigurator.Configure( ) |> ignore
//
//    let log = new Logger.Logger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
//    log.info "Start Application"
//
//    let getArgument argument= 
//        let arg = sprintf "--%s=" argument
//        let input:string = argv.FirstOrDefault (fun x -> x.StartsWith(arg))
//        if not <| String.IsNullOrEmpty input then 
//            Some (input.Substring (arg.Length))
//        else
//            None
//    
//    let getInt rep=
//        printf "%s " rep
//        System.Int32.TryParse (Console.ReadLine())
//
//    let getDate () = 
//        let askDate() =
//            let hasAnno, anno = getInt "Anno" 
//            let hasMese, mese = getInt "Mese"
//            let hasGiorno, giorno = getInt "Giorno"
//            let hasHora, hora = getInt "Hora"
//            let hasMin, min = getInt "Minuti"
//            if hasAnno && hasMese && hasGiorno then
//                if hasHora || hasMin then
//                    Some <| Timed (DateTime(anno,mese,giorno,hora,min,0))
//                else
//                    Some <| JustDate (DateTime(anno,mese,giorno,0,0,0))
//            else
//                None
//        let choosedDate =
//            match getArgument "date" with
//            | Some d -> 
//                try
//                    match getArgument "time" with
//                    | Some t -> Some <| Timed (DateTime.Parse(sprintf "%s %s" d t))
//                    | None -> Some <| JustDate (DateTime.Parse d)
//                with 
//                    | :? FormatException -> None
//            | None -> askDate()
//        log.info "Download file in date %A" choosedDate
//        choosedDate
//
//    let getEnvironment() =
//        let askEnv () =
//            let testo = 
//                "Su che ambiente si vuole fare la ricerca?\n"+
//                "1 - Test\n" +
//                "2 - PreProd\n" +
//                "3 * Produzione\n"
//
//            match getInt testo with 
//            | (true, 1) -> Environment.Test
//            | (true, 2) -> Environment.Preprod
//            | _ -> Environment.Production
//    
//        match getArgument "env" with
//        | Some e when e = "test" -> Environment.Test
//        | Some e when e = "preprod" -> Environment.Preprod
//        | Some e when e = "prod" -> Environment.Production
//        | Some _ | None -> askEnv()
//
//
//    let getApplicazione() =
//        let askProgram () =             
//            let testo = 
//                "Cercare una applicazione o webservice?\n"+
//                "1 * IncassoDA\n" +
//                "2 - WSIncassi\n"
//
//            match getInt testo with 
//            | (true, 2) -> (WSIncassi, WebService)
//            | _ -> (IncassoDA, WebApp)
//        
//        match getArgument "program" with
//        | Some p  ->
//            let program = LinkBuilder.programs |> Seq.tryFind (fun (x,_) -> ((enumToString x).ToLower()) = p.ToLower())
//            match program with
//            | Some x -> x
//            | None -> (IncassoDA, WebApp)
//
//        | None -> askProgram()
//
//
//    let getTextToFind() = 
//        match getArgument "text" with
//        | Some t -> t
//        | None ->
//            printf "Inserire il testo da cercare\n"
//            Console.ReadLine()
//
//    let logType =
//        match getArgument "log-type" with
//        | Some "debug" -> LogType.DebugTrace
//        | Some "client" -> LogType.ClientPerformance
//        | Some "ppl" -> LogType.PplTrace
//        | Some "security" -> LogType.Security
//        | Some "requests" -> LogType.Requests
//        | _ -> LogType.DebugTrace
//    
//    let getFolderName() =
//        match getArgument "folder" with
//        | Some x -> x
//        | None -> "Default"

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
    let downloader = Downloader.SaveLogs.download folder server textToFind
        
    Seq.iter downloader files 

type DonwloaderDto = {
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
       
    