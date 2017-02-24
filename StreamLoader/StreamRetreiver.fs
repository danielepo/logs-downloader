module StreamRetreiver
open Logger
open Polly

let getPage baseUrl (logger:Logger) (link:string)=
    let execute () =
            let geturl = (System.Net.WebRequest.Create (baseUrl + link))
            geturl.Timeout <- 300000
            baseUrl, Some <| geturl.GetResponse().GetResponseStream()

    Policy
        .Handle<System.Net.WebException>()
        .Retry(3,fun x i -> 
            logger.info "exception opening %s%s" baseUrl link
            logger.error ("exeption opening",x))
        .Execute(execute)