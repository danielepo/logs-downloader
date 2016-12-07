module Bo

let getPage baseUrl (link:string) =
    try 
        printf "%s\n" link 
        let geturl = (System.Net.WebRequest.Create (baseUrl + link))

        baseUrl, Some <| geturl.GetResponse().GetResponseStream()

    with :? System.Exception -> 
        printf "exception opening %s%s" baseUrl link
        baseUrl, None