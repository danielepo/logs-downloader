module StreamRetreiver
open Logger
open Polly
open System.Configuration
open System.IO
open System.Net
open System

let getPage baseUrl (logger:Logger) (link:string)=
    let execute () =
        try
            let geturl = (System.Net.WebRequest.Create (baseUrl + link))
            
            geturl.Timeout <- 300000
            baseUrl, Some <| geturl.GetResponse().GetResponseStream()
        with :? System.Net.WebException as ex->
            logger.error "%s - %s" link ex.Message
            baseUrl, None

    Policy
        .Handle<System.Net.WebException>()
        .Retry(3,fun x i -> 
            logger.info "exception opening %s%s" baseUrl link
            logger.error ("exeption opening",x))
        .Execute(execute)