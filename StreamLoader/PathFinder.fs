module PathFinder

open System
open System.IO
open FSharp.Data

open Types
open StreamRetreiver
open Logger

let (|Prefix|_|) (p:string) (s:string) = if s.StartsWith(p) then Some s else None      

let fileNameToDate (filename:string)=    
    let strDate = 
        let substring (str:string) from len=
            if str.Length < from + len then None
            else Some <| str.Substring(from,len)
        try 
            match filename with 
            | Prefix "ClientPerformance." str -> substring str 18 10
            | Prefix "DebugTrace." str -> substring str 11 19
            | Prefix "ppl_trace." str -> substring str 10 10
            | Prefix "Security." str -> substring str 9 10
            | Prefix "requests." str ->  substring str 9 19
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
    let toTuple = toFileType name ,  fileNameToDate name
    let lType = 
        match name with 
            | Prefix "ClientPerformance." _ -> ClientPerformance
            | Prefix "DebugTrace." _ -> DebugTrace
            | Prefix "ppl_trace." _ -> PplTrace
            | Prefix "Security." _ -> Security
            | Prefix "requests." _ -> Requests
            | _ -> (fun _ -> Log.Unknown)

    lType toTuple

let makeDateTime s1 s2 s3 = 
    let frmat = sprintf "%s %s %s" s1 s2 s3
    DateTime.ParseExact(frmat, "M/d/yyyy h:mm tt", System.Globalization.CultureInfo.InvariantCulture)



let rec allLinks (log:Logger) (baseUrl, page:System.IO.Stream option):LinkType = 
    let extractLinkAndDesc (r:HtmlNode) = r.InnerText() , r.AttributeValue("href")
    let toLinkType (description: string,link:string) = 
        if link.EndsWith("/") then       
            link
            |> getPage baseUrl log
            |> allLinks log
        else 
            File (toLogType description, Link link)
    


    match page with
    | Some stream ->
        use reader = new StreamReader(stream)
        let file = reader.ReadToEnd()
        let logFolder = HtmlDocument.Parse file
        let links = logFolder.Descendants ["a"]
        
        let newLinks = 
            List.ofSeq(logFolder.Descendants ["pre"]).[0].InnerText().Split([|'\r'; '\n'|]) 
            |> Array.map (fun x -> x.Split(' '))  
            |> Array.filter (fun x -> x.Length = 6) 
            |> Array.map (fun x-> x.[5], makeDateTime x.[1] x.[2] x.[3])
        

        links
        |> Seq.filter (fun r -> r.InnerText() <> "[To Parent Directory]")
        |> Seq.map extractLinkAndDesc
        |> Seq.map toLinkType
        |> Folder
    | None -> Folder ([] |> Seq.ofList)

let extractDate loglinks = 
    let flog,_ = loglinks
    match flog with
    | ClientPerformance (_,d)  -> d
    | DebugTrace (_,d)  -> d
    | PplTrace (_,d)  -> d
    | Security (_,d)  -> d
    | Requests (_,d)  -> d
    | Unknown -> None

let linksInDate (logger:Logger) (date:SpecialDateTime) (link:LinkType) =
    let rec worker (tree:LinkType) = 
        match tree with
        | Folder (f) -> f |> Seq.map worker |> Seq.concat
        | File (logType,link) -> seq{ yield (logType,link) }
    
    let rec findStartAndEndDate (links:(Log*Link) list):(DateTime option * DateTime option * (Log*Link)) list =


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
            logger.debug  <| sprintf "Error Date Time: %s" ( dateTime.ToString())
            logger.debug  <| sprintf "Before Date Time: %A" ( Option.map (fun (d:DateTime) -> d.ToString()) f)
            logger.debug  <| sprintf "After Date Time: %A" ( Option.map (fun (d:DateTime) -> d.ToString()) s)
            let biggerThanFirst = f |> Option.map (fun d -> dateTime >= d)
            let lowerThanFirst = s |> Option.map (fun d -> dateTime <= d)
            match biggerThanFirst with
            | Some first -> 
                logger.debug "Bigger Than First"
                match lowerThanFirst with
                | Some second -> 
                    if  (first && second) then
                        logger.info <| sprintf "%s is in between %s and %s" (dateTime.ToString()) (f.ToString()) (s.ToString())
                    else
                        logger.debug <| sprintf "%s is NOT in between %s and %s" (dateTime.ToString()) (f.ToString()) (s.ToString())

                    first && second
                | None -> 
                    logger.debug  <| sprintf"Is in between %b" true
                    true
            | None -> true
        | JustDate just->
            logger.debug "Just Dates"
            match f with
            | Some d -> 
                logger.debug  <| sprintf "Error Date Time: %s" (just.ToString())
                logger.debug <| sprintf "Log Date Time %s" (d.Date.ToString())
                logger.debug <| sprintf "Is in between %b" (d.Date = just.Date)
                d.Date = just.Date
            | None -> true
    
    let getInCorrectDate (links:(Log*Link)list) (searchDate:SpecialDateTime) =        
        let rec worker (links:(Log*Link)list) (lastDate:DateTime) =
            let mutable isHigher = false
            match links with
            | [] -> []
            | l::ls -> 
                match extractDate l with
                | None -> l::(worker ls lastDate)
                | Some d -> 
                    if lastDate = DateTime.MinValue then worker (l::ls) (d.Date)
                    else 
                        match searchDate with
                        | JustDate date -> 
                            if date.Date = d.Date then l::(worker ls d)
                            else worker ls d
                        | Timed date ->
                            if date > lastDate && date <= d then [l]
                            else worker ls d


        worker links (DateTime.MinValue)

    let links =
        worker link
        |> Seq.filter (fun x -> (fst x) <> Unknown)
        |> List.ofSeq
        |> List.sortBy (fun x -> extractDate x)
        |> List.groupBy (fun (x,y) -> (mapToLogType x))
        |> List.map (fun (logType, links) -> getInCorrectDate links date)
        |> List.concat

    links
//        |> findStartAndEndDate
//    let links   =
//        links |> List.filter (isInBetween date)

//    links |> List.map (fun (_,_,v) -> v)


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
    |> getPage "http://logauto2.servizi.allianzit/" log
    |> allLinks log
    |> linksInDate log date
    |> Seq.filter (fun x -> fiterByLogType logIndex x)

