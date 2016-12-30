module Types

open System

type Application =  WebApp | WebService

type TestServers = string List
type PreProdServers = string List
type ProdServers = string List

type Environment = Test | Preprod | Production


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
    ClientPerformance     
    | DebugTrace 
    | PplTrace 
    | Security 
    | Unknown

type Log = 
    | ClientPerformance of FileType * DateTime option
    | DebugTrace of FileType * DateTime option
    | PplTrace of FileType * DateTime option
    | Security of FileType * DateTime option
    | Unknown

let mapLogType fn =
    function 
    | ClientPerformance (f,d)-> ClientPerformance (fn (f,d))
    | DebugTrace (f,d)-> DebugTrace (fn (f,d))
    | PplTrace (f,d)-> PplTrace (fn (f,d))
    | Security (f,d)-> Security (fn (f,d))
    | Unknown -> Unknown

let applyLogType fn =
    function 
    | ClientPerformance (f,d)-> fn (f,d)
    | DebugTrace (f,d)-> fn (f,d)
    | PplTrace (f,d)-> fn (f,d)
    | Security (f,d)-> fn (f,d)
    | Unknown -> ()

type LinkType = 
    | Folder of LinkType seq
    | File of Log*Link

type SpecialDateTime =
    | Timed of DateTime
    | JustDate of DateTime