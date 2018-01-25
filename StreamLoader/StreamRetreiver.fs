module StreamRetreiver
open Logger
open Polly
open System.Net
open System
open FSharp.Data
open Types

let getPage baseUrl (logger:ILogger) (link:string)=

    let execute () =
        try
            let request = (WebRequest.Create (baseUrl + link))
            let credentials = config.Credentials
            if credentials.Use then
                request.Credentials <- 
                    NetworkCredential(credentials.User, credentials.Password, credentials.Domain)

            request.Timeout <- 300000
            baseUrl, Some <| request.GetResponse().GetResponseStream()
        with :? WebException as ex->
            logger.error <| sprintf "%s - %s" link ex.Message
            baseUrl, None

    Policy
        .Handle<WebException>()
        .Retry(3,fun x i -> 
            logger.info  <| sprintf "exception opening %s%s" baseUrl link
            logger.error ("exeption opening",x))
        .Execute(execute)