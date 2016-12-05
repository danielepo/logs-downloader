module Parser
open System

let page = "fdsfsd<A href=\"/server/IncassoDA/Logs/\">dsfsa</A>fsdfs<A href=\"/server/IncassoDA/Logs/\">dsfsa</A><A href=\"/servers/IncassoDA/Logs/\">dsfsa</A>"

let GenericSplitter (page:string) startString endString= 
    let rec toList (page:string) = 
        let anchor (p:string) = 
            let start = p.IndexOf(startString, StringComparison.OrdinalIgnoreCase)
            let _end = if start <> -1 then p.IndexOf(endString, start, StringComparison.OrdinalIgnoreCase) else -1 
            if _end = -1 || start = -1 
                then None 
                else Some <| p.Substring(start, _end - start + 4)
        match anchor page with
            | Some x ->  x :: toList (page.Substring(page.IndexOf(x) + x.Length))
            | None -> []
    toList page

let GetFolderLinks (page:string) server = 
    GenericSplitter page ("<A href=\"/" + server + "/IncassoDA/Logs/") "</A>"

let linkAndDescription (page:string) server splitter= 
    let divide (anchor:string) = 
        let cut (startString:string) (endString:string) = 
            let startHref = anchor.IndexOf(startString,StringComparison.InvariantCultureIgnoreCase)
            let startLink = startHref + startString.Length
            let _end = if startHref <> -1 then anchor.IndexOf(endString,startLink) else -1
            if _end <> -1 then Some (anchor.Substring(startLink, _end - startLink))  else None
        (cut "href=\"" "\"",cut ">" "</A>")
    splitter page server |> List.map divide


