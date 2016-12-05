module PathFinder

open System
open System.IO
open FSharp.Data

type ServerLogFolder = HtmlProvider<"./LogPageSample.html">


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

type FileType = 
    | Unknown
    | Log of string
    | Gz of string

let toFileType = 
        let (|Postfix|_|) (p:string) (s:string) = if s.EndsWith(p) then Some s else None
        
        function
        | Postfix ".log" x -> Log x
        | Postfix ".gz" x -> Gz x
        | _ -> FileType.Unknown

type Link = Link of string

type LogType  = 
    | ClientPerformance of FileType * DateTime option
    | DebugTrace of FileType * DateTime option
    | PplTrace of FileType * DateTime option
    | Security of FileType * DateTime option
    | Unknown

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

type LinkType = 
    | Folder of LinkType list
    | File of LogType*Link

let rec allLinks (page:System.IO.Stream):LinkType = 
    printf "allLinks\n" 
    let extractLinkAndDesc (r:HtmlNode) = r.InnerText() , r.AttributeValue("href")
    let toLinkType (description,link:string) = 
        printf "\t%s\n" description
        if link.EndsWith("/") 
        then       
            printf "%s\n" link       
            let geturl = (System.Net.WebRequest.Create link)
//            let credentials =  new System.Net.NetworkCredential("ru18431", "29adaneda", "azgroup")
//            geturl.Credentials <- credentials
            
            geturl.GetResponse().GetResponseStream() 
            |> allLinks
            
            
        else File (toLogType description,Link link)
    
    let logFolder = HtmlDocument.Load(page)

    let links = logFolder.Descendants ["a"]
    
    links 
    |> List.ofSeq 
    |> List.filter (fun r -> r.InnerText() <> "[To Parent Directory]")
    |> List.map (extractLinkAndDesc >> toLinkType)
    |> Folder



let linksInDate  (date:DateTime) (folder:LinkType)=
    match folder with 
    | Folder links -> 
        links 
        |> List.ofSeq
        |> List.filter (fun x -> 
            match x with 
            | File (log,_) ->
                match log with
                | LogTypeDate d -> d.Date = date.Date 
                | _ -> false
            | Folder (d) -> true)
        |> Some
    | File _ -> None

let getPage (link:string) =
    printf "%s\n" link 
    let geturl = (System.Net.WebRequest.Create link)
//            let credentials =  new System.Net.NetworkCredential("ru18431", "29adaneda", "azgroup")
//            geturl.Credentials <- credentials
    geturl.GetResponse().GetResponseStream() 

"http://brepaddc2s01.azgroup.itad.corpnet/BREPADAS2S06/Auto/IncassoDA/Logs/"
|> getPage
|> allLinks 
|> linksInDate (DateTime.Now)


// sembra che ci sia un limite al numero di descendans che può vedere
let page = 
    "https://mitpress.mit.edu/sicp/"
    |> getPage 
(HtmlDocument.Load (page)).Descendants ["a"]
|> List.ofSeq |> List.length 