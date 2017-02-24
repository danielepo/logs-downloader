﻿module PathFinder

open System
open System.IO
open FSharp.Data

open Types
open StreamRetreiver
open Logger

let (|Prefix|_|) (p:string) (s:string) = if s.StartsWith(p) then Some s else None      

let fileNameToDate (filename:string)=    
    let strDate = 
        try 
            match filename with 
            | Prefix "ClientPerformance." str -> Some (str.Substring(18,10))
            | Prefix "DebugTrace." str -> Some (str.Substring(11, 19))
            | Prefix "ppl_trace." str -> Some (str.Substring(10, 10))
            | Prefix "Security." str -> Some (str.Substring(9, 10))
            | Prefix "requests." str ->  Some (str.Substring(9, 10))
            | _ -> None

        with :? ArgumentOutOfRangeException -> None
    
    strDate 
    |> Option.map (fun d -> 
        d.Replace('-', '/').Replace('_', ' ').Replace('.', ':') 
        |> DateTime.Parse)
    
let folderNameToDate (folder:string)=
    sprintf "%s/%s/%s" (folder.Substring(0,4)) (folder.Substring(4,2)) (folder.Substring(6))
    |> DateTime.Parse

let toFileType = 
        let (|Postfix|_|) (p:string) (s:string) = 
            if s.EndsWith(p) then Some s else None
        
        function
        | Postfix ".log" x -> Log x
        | Postfix ".gz" x -> Gz x
        | _ -> FileType.Unknown


let (|LogTypeDate|_|) = 
    function
    | ClientPerformance (_,d) -> d
    | DebugTrace (_,d) -> d
    | PplTrace (_,d) -> d
    | Security (_,d) -> d
    | Requests (_,d) -> d
    | Unknown -> None


let toLogType (name:string) =
    let toTuple = toFileType name , fileNameToDate name
    let lType = 
        match name with 
            | Prefix "ClientPerformance." _ -> ClientPerformance
            | Prefix "DebugTrace." _ -> DebugTrace
            | Prefix "ppl_trace." _ -> PplTrace
            | Prefix "Security." _ -> Security
            | Prefix "requests." _ -> Requests
            | _ -> (fun _ -> Log.Unknown)

    lType toTuple




let rec allLinks (log:Logger) (baseUrl, page:System.IO.Stream option):LinkType = 
    let extractLinkAndDesc (r:HtmlNode) = r.InnerText() , r.AttributeValue("href")
    let toLinkType (description,link:string) = 
        if link.EndsWith("/") then       
            link
            |> getPage baseUrl log
            |> allLinks log
        else 
            File (toLogType description,Link link)
    

    match page with
    | Some stream ->
        use reader = new StreamReader(stream)
        let file = reader.ReadToEnd()
        let logFolder = HtmlDocument.Parse file
        let links = logFolder.Descendants ["a"]
    
        links 
        |> Seq.filter (fun r -> r.InnerText() <> "[To Parent Directory]")
        |> Seq.map (extractLinkAndDesc >> toLinkType)
        |> Folder
    | None -> Folder ([] |> Seq.ofList)



let linksInDate (logger:Logger) (date:SpecialDateTime) (link:LinkType) =
    let rec worker (tree:LinkType) = 
        match tree with
        | Folder (f) -> f |> Seq.map worker |> Seq.concat
        | File (logType,link) -> seq{ yield (logType,link) }
    
    let rec findStartAndEndDate (links:(Log*Link) list):(DateTime option * DateTime option * (Log*Link)) list =
        let extractDate loglinks = 
            let flog,_ = loglinks
            match flog with
            | ClientPerformance (_,d)  -> d
            | DebugTrace (_,d)  -> d
            | PplTrace (_,d)  -> d
            | Security (_,d)  -> d
            | Requests (_,d)  -> d
            | Unknown -> None

        match links with
        | f::s::is -> 
            let fd = extractDate f
            let sd = extractDate s
            (fd, sd, s) :: (findStartAndEndDate (s::is))
        | [f] ->
            let fd = extractDate f
            let sd = None
            [fd, sd, f] 
        | [] -> []

    let isInBetween date ((f:DateTime option),(s:DateTime option),a') = 
        match date with
        | Timed dateTime -> 
            logger.debug "Date and time"
            logger.debug "Error Date Time: %s" <| dateTime.ToString()
            logger.debug "Before Date Time: %A" <| Option.map (fun (d:DateTime) -> d.ToString()) f
            logger.debug "After Date Time: %A" <|  Option.map (fun (d:DateTime) -> d.ToString()) s
            let biggerThanFirst = f |> Option.map (fun d -> dateTime >= d)
            let lowerThanFirst = s |> Option.map (fun d -> dateTime <= d)
            match biggerThanFirst with
            | Some first -> 
                logger.debug "Bigger Than First"
                match lowerThanFirst with
                | Some second -> 
                    if  (first && second) then
                        logger.info "%s is in between %s and %s"
                            <| dateTime.ToString() 
                            <| f.ToString() 
                            <| s.ToString()
                    else
                        logger.debug "%s is NOT in between %s and %s"
                            <| dateTime.ToString() 
                            <| f.ToString() 
                            <| s.ToString()

                    first && second
                | None -> 
                    logger.debug "Is in between %b" true
                    true
            | None -> true
        | JustDate just->
            logger.debug "Just Dates"
            match f with
            | Some d -> 
                logger.debug "Error Date Time: %s" (just.ToString())
                logger.debug "Log Date Time %s" (d.Date.ToString())
                logger.debug "Is in between %b" (d.Date = just.Date)
                d.Date = just.Date
            | None -> true
            

    worker link
    |> List.ofSeq
    |> findStartAndEndDate
    |> List.filter (isInBetween date)
    |> List.map (fun (_,_,v) -> v)

let fiterByLogType index link= 
    let log,_ = link
    match log with 
    | ClientPerformance (_) when index = LogType.ClientPerformance-> true
    | DebugTrace (_) when index = LogType.DebugTrace -> true
    | PplTrace (_) when index = LogType.PplTrace -> true
    | Security (_) when index = LogType.Security -> true
    | Requests (_) when index = LogType.Requests -> true
    | _ -> false



let getLinks (logIndex) date path (server:string)=
    let log = new Logger.Logger (server)
    
    path 
    |> getPage "http://brepaddc2s01.azgroup.itad.corpnet/" log
    |> allLinks log
    |> linksInDate log date
    |> Seq.filter (fun x -> fiterByLogType logIndex x)

