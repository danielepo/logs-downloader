module Types

open System

type Application =  
    | WebApp = 0 
    | WebService = 1

type TestServers = string List
type PreProdServers = string List
type ProdServers = string List

type Environment = 
    | Test = 0 
    | Preprod = 1 
    | Production = 2

type Program = 
    | IncassoDA = 0
    | WSIncassi = 1
    | GestioneLibriMatricolaDA = 2
    | NGRA2013 = 3
let enumToString e =
    sprintf "%A" e

type EnvironmentServer = {
    Test: TestServers
    Preprod: PreProdServers
    Production: ProdServers
}

type FileType = 
    | Unknown
    | Log of string
    | Gz of string

type Link = Link of string

type LogType = 
    | Unknown = 0
    | ClientPerformance = 1
    | DebugTrace = 2
    | PplTrace = 3
    | Security = 4
    | Requests = 5

type Log = 
    | ClientPerformance of FileType * DateTime option
    | DebugTrace of FileType * DateTime option
    | PplTrace of FileType * DateTime option
    | Security of FileType * DateTime option
    | Requests of FileType * DateTime option
    | Unknown

let mapLogType fn =
    function 
    | ClientPerformance (f,d)-> ClientPerformance (fn (f,d))
    | DebugTrace (f,d)-> DebugTrace (fn (f,d))
    | PplTrace (f,d)-> PplTrace (fn (f,d))
    | Security (f,d)-> Security (fn (f,d))
    | Requests (f,d)-> Security (fn (f,d))
    | Unknown -> Unknown

let applyLogType fn =
    function 
    | ClientPerformance (f,d)-> fn (f,d)
    | DebugTrace (f,d)-> fn (f,d)
    | PplTrace (f,d)-> fn (f,d)
    | Security (f,d)-> fn (f,d)
    | Requests (f,d)-> fn (f,d)
    | Unknown -> ()

type LinkType = 
    | Folder of LinkType seq
    | File of Log*Link

type SpecialDateTime =
    | Timed of DateTime
    | JustDate of DateTime