namespace LogManager
open System.Collections.Generic
open WebScratcher
open Parser
open FileDownloader
open System


type Downloader() = 
    member this.Download() = ()
    member this.SaveLogs(date:DateTime) (runEnv:RunEnvironment) (search:string) (timeSpecified:bool)= SaveLogs (date) runEnv search timeSpecified