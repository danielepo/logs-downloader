// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open Types
open LinkBuilder
open System.IO
open System
open System.Linq
open log4net
open System.Configuration

type LogLevel = Info | Debug | Warning | Error

let logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    
let log = 
    function
    | Info -> logger.Info 
    | Debug -> logger.Debug 
    | Warning -> logger.Warn 
    | Error -> logger.Error

[<EntryPoint>]

let main argv = 
    log4net.Config.XmlConfigurator.Configure( ) |> ignore

    let log = new Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    log.info "Start Application"

    let getArgument argument= 
        let arg = sprintf "--%s=" argument
        let input:string = argv.FirstOrDefault (fun x -> x.StartsWith(arg))
        if not <| String.IsNullOrEmpty input then 
            Some (input.Substring (arg.Length))
        else
            None

    let links date (server,path) = sprintf "Log/%s" server, PathFinder.getLinks LogType.DebugTrace date path

    let downloadFilesInServer textToFind (server,files) = 
        for file in files do 
            Downloader.SaveLogs.download file server textToFind
    
    let getInt rep=
        printf "%s " rep
        System.Int32.TryParse (Console.ReadLine())

    let getDate () = 
        let askDate() =
            let hasAnno, anno = getInt "Anno" 
            let hasMese, mese = getInt "Mese"
            let hasGiorno, giorno = getInt "Giorno"
            let hasHora, hora = getInt "Hora"
            let hasMin, min = getInt "Minuti"
            if hasAnno && hasMese && hasGiorno then
                if hasHora || hasMin then
                    Some <| Timed (DateTime(anno,mese,giorno,hora,min,0))
                else
                    Some <| JustDate (DateTime(anno,mese,giorno,0,0,0))
            else
                None

        match getArgument "date" with
        | Some d -> 
            try
                match getArgument "time" with
                | Some t -> Some <| Timed (DateTime.Parse(sprintf "%s %s" d t))
                | None -> Some <| JustDate (DateTime.Parse d)
            with 
                | :? FormatException -> None
        | None -> askDate()

    let getEnvironment() =
        let askEnv () =
            let testo = 
                "Su che ambiente si vuole fare la ricerca?\n"+
                "1 - Test\n" +
                "2 - PreProd\n" +
                "3 * Produzione\n"

            match getInt testo with 
            | (true, 1) -> Environment.Test
            | (true, 2) -> Environment.Preprod
            | _ -> Environment.Production
    
        match getArgument "env" with
        | Some e when e = "t" -> Environment.Test
        | Some e when e = "pr" -> Environment.Preprod
        | Some e when e = "p" -> Environment.Production
        | Some _ | None -> askEnv()


    let getTipoApplicazione() =
        let askType () =             
            let testo = 
                "Cercare una applicazione o webservice?\n"+
                "1 * App\n" +
                "2 - WebService\n"

            match getInt testo with 
            | (true, 2) -> Application.WebService
            | _ -> Application.WebApp

        match getArgument "type" with
        | Some t when t = "w" || t = "W" -> Application.WebService
        | Some t when t = "a" || t = "A" -> Application.WebApp
        | Some _ | None -> askType()


    let getTextToFind() = 
        match getArgument "text" with
        | Some t -> t
        | None ->
            printf "Inserire il testo da cercare\n"
            Console.ReadLine()


    let downloadLogs (date:SpecialDateTime option) (environment:Types.Environment) appType textToFind =
        let toDateString (specialDate:SpecialDateTime) =
            match specialDate with
            | Timed d | JustDate d -> d.ToString("yyyy-MM-gg")
            

        match date with
        | Some d ->
            
            printf "\nDate: %s\nEnvirnoment: %s" <| toDateString d <| environment.ToString()
            getLinksFor appType environment   
            |> Array.ofList
            |> Array.Parallel.iter (links d >> downloadFilesInServer textToFind)
        | None ->
            printf "Errore parsing data"
       
    if not (Directory.Exists "Log") then Directory.CreateDirectory "Log" |> ignore
    
    downloadLogs <| getDate() <| getEnvironment() <| getTipoApplicazione()  <| getTextToFind()
    0 // return an integer exit code
