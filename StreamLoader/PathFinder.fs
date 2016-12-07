module PathFinder

open System
open System.IO
open FSharp.Data

open Types
open Bo

let (|Prefix|_|) (p:string) (s:string) = if s.StartsWith(p) then Some s else None
        

let fileNameToDate (filename:string)=    
    let strDate = 
        try 
            match filename with 
            | Prefix "ClientPerformance." str -> Some (str.Substring(18,10))
            | Prefix "DebugTrace." str -> Some (str.Substring(11, 19))
            | Prefix "ppl_trace." str -> Some (str.Substring(10, 10))
            | Prefix "Security." str -> Some (str.Substring(9, 10))
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
    | Unknown -> None


let toLogType (name:string) =
    let toTuple = toFileType name , fileNameToDate name
    let lType = 
        match name with 
            | Prefix "ClientPerformance." _ -> ClientPerformance
            | Prefix "DebugTrace." _ -> DebugTrace
            | Prefix "ppl_trace." _ -> PplTrace
            | Prefix "Security." _ -> Security
            | _ -> (fun _ -> LogType.Unknown)

    lType toTuple




let rec allLinks (baseUrl, page:System.IO.Stream option):LinkType = 
    printf "allLinks\n" 
    let extractLinkAndDesc (r:HtmlNode) = r.InnerText() , r.AttributeValue("href")
    let toLinkType (description,link:string) = 
        printf "\t%s\n" description
        if link.EndsWith("/") then       
            link
            |> getPage baseUrl
            |> allLinks
        else 
            File (toLogType description,Link link)
    

    match page with
    | Some stream ->
        let logFolder = HtmlDocument.Load(stream)
        let links = logFolder.Descendants ["a"]
    
        links 
        |> Seq.filter (fun r -> r.InnerText() <> "[To Parent Directory]")
        |> Seq.map (extractLinkAndDesc >> toLinkType)
        |> Folder
    | None -> Folder ([] |> Seq.ofList)



let linksInDate  (date:DateTime) (link:LinkType) =
    let rec worker (tree:LinkType) = 
        match tree with
        | Folder (f) -> f |> Seq.map worker |> Seq.concat
        | File (logType,link) -> seq{ yield (logType,link) }
    
    worker link
    |> Seq.filter (
        fun (log,_) ->
                match log with
                | LogTypeDate d -> d.Date = date.Date 
                | _ -> false)


let fiterByLogType index link= 
    let log,_ = link
    match log with 
    | ClientPerformance (_) when index = 1-> true
    | DebugTrace (_) when index = 2 -> true
    | PplTrace (_) when index = 3 -> true
    | Security (_) when index = 4 -> true
    | _ -> false



let getLinks (logIndex) date path =
    path
    |> getPage "http://brepaddc2s01.azgroup.itad.corpnet/"
    |> allLinks 
    |> linksInDate date
    |> Seq.filter (fun x -> fiterByLogType logIndex x)

