module WebScratcher
open System.IO
open System.Configuration
open System.Net
open System
open Parser
type RunEnvironment = Prod = 0 | Test = 1 | Preprod = 2

type FileType = 
    | Log
    | Gzip

type ParsedLink = {
    Uri: string; 
    Description: string;
    FileType: FileType}

let prodServers = [
    "BREPADAS3S01"; "BREPADAS3S02"; "BREPADAS3S03"; "BREPADAS3S04";
    "BREVADAS3S02"; "BREVADAS3S03"; "BREVADAS3S05"; "BREVADAS3S06";
    "BREVADAS3S07"; "BREVADAS3S08"; "BREVADAS3S09"; "BREVADAS3S10";
    "BREVADAS3S11"; "BREVADAS3S12"; "BREVADAS3S13"; "BREVADAS3S14"
    ]
let preProdServers = [ "BREPADPP3S01"; "BREPADPP3S02"; "BREPADTF3S03"; "BREPADTF3S03"  ]
let testServers = [ "BREPADTF3S01"; "BREPADTF3S02"; "BREPADTF3S03" ]


let streamUrl (url:string)= 
    let geturl = (WebRequest.Create url)
    let logUser = ConfigurationManager.AppSettings.Get("log_user")
    let logPass = ConfigurationManager.AppSettings.Get("log_pass")
    let logGroup = ConfigurationManager.AppSettings.Get("log_group")
    let credentials =  new NetworkCredential(logUser, logPass, logGroup)
    geturl.Credentials <- credentials;
    geturl.GetResponse().GetResponseStream();

let TextStreamReader url =
    new StreamReader(streamUrl url)

let FolderLoader (url:string)=
    let stream = TextStreamReader url
    let rec buildString (stream:StreamReader) =
        if  not stream.EndOfStream then stream.ReadLine() + buildString stream else ""
    buildString stream

///Get's links to download based on date
let GetLinkInDate (date:DateTime) environment server timeSpecified = 
    let filteredLinks url filter fileType= 
        let filterOfType = filter (if fileType = Gzip then "gz" else "log")
        try
            let links = linkAndDescription (FolderLoader url) server GetFolderLinks 
            let dateRanges = 
                let createdAt (uri:option<string>,desc:option<string>) = 
                    if uri.IsNone || desc.IsNone then None
                    else 
                        let description = desc.Value
                        match description with
                        |  "DebugTrace.log" ->  None
                        |  "DebugTrace.log.gz" ->  None
                        | _ ->
                            let fileDateAndTime = description.Substring(11,19).Split('_')
                            let fileDate = fileDateAndTime.[0].Split('-') |> Seq.map Int32.Parse |> List.ofSeq
                            let fileTime = fileDateAndTime.[1].Split('.') |> Seq.map Int32.Parse |> List.ofSeq
                            Some (DateTime(fileDate.[0],fileDate.[1],fileDate.[2],fileTime.[0],fileTime.[1],fileTime.[2]))

                links 
                |> List.filter (fun (x,y) -> x.IsSome && x.Value.[x.Value.Length - 1] <> '/' && x.Value.Contains("DebugTrace"))
                |> List.map createdAt

            links
            |> List.filter (filterOfType dateRanges)
            |> List.map (fun (x,y) -> {Uri = x.Value; Description = y.Value; FileType = fileType})
            |> Some

        with :? Exception -> None


    let filter ext (dates:DateTime option list)(uri:option<string>,desc:option<string>) = 
        let isInCorrectMoment description = 
            let isInRange (logDate:DateTime) = 
            // TODO date deve essere tra il corrente log date e quello successivo
                date <= logDate && date >= (logDate.AddHours(-1.0))
            match description with
            |  "DebugTrace.log" ->  isInRange DateTime.Now
            |  "DebugTrace.log.gz" ->  true
            | _ -> 
                let createdAt = 
                    let fileDateAndTime = description.Substring(11,19).Split('_')
                    let fileDate = fileDateAndTime.[0].Split('-') |> Seq.map Int32.Parse |> List.ofSeq
                    let fileTime = fileDateAndTime.[1].Split('.') |> Seq.map Int32.Parse |> List.ofSeq
                    DateTime(fileDate.[0],fileDate.[1],fileDate.[2],fileTime.[0],fileTime.[1],fileTime.[2])
                isInRange createdAt 

        let isSome = uri.IsSome && desc.IsSome
        isSome && 
        desc.Value.StartsWith("DebugTrace",StringComparison.InvariantCultureIgnoreCase) &&
        desc.Value.EndsWith(ext,StringComparison.InvariantCultureIgnoreCase) &&
        (not timeSpecified || isInCorrectMoment desc.Value)

    let FoldernameFromDate = 
        match environment with 
            | RunEnvironment.Prod -> date.AddDays(1.0).ToString( "yyyyMMdd");
            | _ -> date.ToString( "yyyyMMdd");

    let urlParent = sprintf @"http://logauto.allianzit/%s/IncassoDA/Logs/" server
    let urlChild = sprintf @"%s/%s/DebugTrace/" urlParent FoldernameFromDate
  
    if date.Date <> DateTime.Today 
    then filteredLinks urlChild filter Gzip
    else filteredLinks urlParent filter Log

let FileNameToDate (filename:string)=
    filename.Substring(11, 19).Replace('-', '/').Replace('_', ' ').Replace('.', ':') |>
    DateTime.Parse

let FileToDownload (date:DateTime) environment server timeSpecified =
    GetLinkInDate date environment server timeSpecified
    |> function 
        | Some link -> List.filter (fun x -> true) link
        | None -> []
  

