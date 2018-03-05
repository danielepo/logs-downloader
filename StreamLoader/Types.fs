module Types

open FSharp.Data
open System

type Servers = JsonProvider<"""./config.json""">
type Application =  WebApp | WebService


let mutable config:Servers.Root = Servers.Load "./config.json"

let updateConfig (file:string) = 
    config <- Servers.Load file


type TestServers = string List
type PreProdServers = string List
type ProdServers = string List

type Environment = Test | Preprod | Production 

type Branch = RV | Auto

type DownloadResult =
    | Downloaded
    | Error
    | Skipped

type Result<'a> =
    | Success of 'a
    | Error of string                 
    
let programsBranchMap =               
    let autoPrograms = 
        Array.concat [|config.Programs.Auto.Application ; config.Programs.Auto.Webservice|]
        |> List.ofArray |> List.map (fun x -> x, Branch.Auto)
    let rvPrograms = 
        Array.concat [|config.Programs.RamiVari.Application ; config.Programs.RamiVari.Webservice|]
        |> List.ofArray |> List.map (fun x -> x, Branch.RV)

    [ autoPrograms; rvPrograms ]
    |> List.concat
    |> Map.ofList

let programsMap = 
    let webapps =  
        Array.concat [|config.Programs.Auto.Application ; config.Programs.RamiVari.Application|]
        |> List.ofArray |> List.filter (fun x -> x <> "_") |> List.map (fun x -> x,Application.WebApp)

    let webservices = 
        Array.concat [|config.Programs.Auto.Webservice; config.Programs.RamiVari.Webservice |]
        |> List.ofArray |> List.filter (fun x -> x <> "_") |> List.map (fun x -> x,Application.WebService)
     
    [webapps; webservices] 
    |> List.concat
    |> Map.ofList

type EnvironmentServer = {
    Test: TestServers
    Preprod: PreProdServers
    PreprodFMO: PreProdServers
    Production: ProdServers
}

type FileType = 
    | Unknown
    | Log of string
    | Gz of string

type Link = Link of string

type LogType = 
    | Unknown
    | ClientPerformance
    | DebugTrace
    | PplTrace
    | Security
    | Requests
    | Functional

type Log = 
    | ClientPerformance of FileType * DateTime option
    | DebugTrace of FileType * DateTime option
    | PplTrace of FileType * DateTime option
    | Security of FileType * DateTime option
    | Requests of FileType * DateTime option
    | Functional of FileType * DateTime option
    | Unknown

let mapToLogType =
    function 
    | ClientPerformance _-> LogType.ClientPerformance
    | DebugTrace _-> LogType.DebugTrace 
    | PplTrace _-> LogType.PplTrace
    | Security _-> LogType.Security
    | Requests _-> LogType.Requests
    | Functional _-> LogType.Functional
    | Unknown -> LogType.Unknown

let mapLogType fn =
    function 
    | ClientPerformance (f,d)-> ClientPerformance (fn (f,d))
    | DebugTrace (f,d)-> DebugTrace (fn (f,d))
    | PplTrace (f,d)-> PplTrace (fn (f,d))
    | Security (f,d)-> Security (fn (f,d))
    | Requests (f,d)-> Requests (fn (f,d))
    | Functional (f,d)-> Requests (fn (f,d))
    | Unknown -> Unknown

let applyLogType fn =
    function 
    | ClientPerformance (f,d)-> fn (f,d)
    | DebugTrace (f,d)-> fn (f,d)
    | PplTrace (f,d)-> fn (f,d)
    | Security (f,d)-> fn (f,d)
    | Requests (f,d)-> fn (f,d)
    | Functional (f,d)-> fn (f,d)
    | Unknown -> DownloadResult.Error

type LinkType = 
    | Folder of LinkType seq
    | File of Log*Link

type SpecialDateTime =
    | Timed of DateTime
    | JustDate of DateTime


