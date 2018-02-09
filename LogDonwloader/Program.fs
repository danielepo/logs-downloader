// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open Types
open LinkBuilder
open System.IO
open System
open System.Linq
open log4net
open System.Configuration

[<EntryPoint>]
let main argv = 
    log4net.Config.XmlConfigurator.Configure( ) |> ignore
    let log = new Logger.Logger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    log.info "Start Application"
    let getArgument argument = 
        let arg = sprintf "--%s=" argument
        let input : string = argv.FirstOrDefault(fun x -> x.StartsWith(arg))
        if not <| String.IsNullOrEmpty input then Some(input.Substring(arg.Length))
        else None
    
    let getInt rep = 
        printf "%s " rep
        System.Int32.TryParse(Console.ReadLine())
    
    let getDate() = 
        let askDate() = 
            let hasAnno, anno = getInt "Anno"
            let hasMese, mese = getInt "Mese"
            let hasGiorno, giorno = getInt "Giorno"
            let hasHora, hora = getInt "Hora"
            let hasMin, min = getInt "Minuti"
            if hasAnno && hasMese && hasGiorno then 
                if hasHora || hasMin then Some <| Timed(DateTime(anno, mese, giorno, hora, min, 0))
                else Some <| JustDate(DateTime(anno, mese, giorno, 0, 0, 0))
            else None
        log.info <| sprintf "Culture info: %s\n" System.Globalization.CultureInfo.CurrentCulture.Name
        log.info <| sprintf "UICulture info: %s\n" System.Globalization.CultureInfo.CurrentUICulture.Name
        let choosedDate = 
            let culture = new System.Globalization.CultureInfo("en-US")
            match getArgument "date" with
            | Some d -> 
                try 
                    match getArgument "time" with
                    | Some t -> Some <| Timed(DateTime.Parse(sprintf "%s %s" d t, culture))
                    | None -> Some <| JustDate(DateTime.Parse(d, culture))
                with :? FormatException -> None
            | None -> askDate()
        log.info <| sprintf "Download file in date %A" choosedDate
        choosedDate
    
    let getEnvironment() = 
        let askEnv() = 
            let testo = 
                "Su che ambiente si vuole fare la ricerca?\n" + "1 - Test\n" + "2 - PreProd\n" + "3 * Produzione\n"
            match getInt testo with
            | (true, 1) -> Environment.Test
            | (true, 2) -> Environment.Preprod
            | _ -> Environment.Production
        match getArgument "env" with
        | Some e when e = "test" -> Environment.Test
        | Some e when e = "preprod" -> Environment.Preprod
        | Some e when e = "prod" -> Environment.Production
        | Some _ | None -> askEnv()
    
    let getApplicazione() = 
        let askProgram() = 
            let testo = "Cercare una applicazione o webservice?\n" + "1 * IncassoDA\n" + "2 - WSIncassi\n"
            match getInt testo with
            | (true, 2) -> [ "WSIncassi" ]
            | _ -> [ "IncassoDA"; "WSIncassi" ]
        match getArgument "program" with
        | Some p -> 
            let programs = 
                p.Split(',')
                |> Seq.map (fun p -> 
                       let program = 
                           programsMap |> Seq.tryFind (fun x -> (x.Key.ToLower()) = p.ToLower())
                       match program with
                       | Some x -> Some x.Key
                       | None -> None)
                |> Seq.filter (fun p -> p.IsSome)
                |> Seq.map (fun p -> p.Value)
                |> List.ofSeq
            match programs |> List.isEmpty with
            | false -> programs
            | true -> [ "IncassoDA"; "WSIncassi" ]
        | None -> askProgram()
    
    let getTextToFind() = 
        match getArgument "text" with
        | Some t -> t
        | None -> 
            printf "Inserire il testo da cercare\n"
            Console.ReadLine()
    
    let getLogType() = 
        match getArgument "log-type" with
        | Some "debug" -> LogType.DebugTrace
        | Some "client" -> LogType.ClientPerformance
        | Some "ppl" -> LogType.PplTrace
        | Some "security" -> LogType.Security
        | Some "requests" -> LogType.Requests
        | Some "functional" -> LogType.Functional
        | _ -> LogType.DebugTrace
    
    let getFolderName() = 
        match getArgument "folder" with
        | Some x -> x
        | None -> "Default"

    let execute() = 
        try
            match argv |> Array.tryFind (fun x -> x = "--help") with
            | Some _ -> 
                printf "%s" <| System.IO.File.ReadAllText("help.txt")
                Console.ReadLine() |> ignore
                0
            | None -> 
                let dtoBuilder program : Downloader.DownloaderDto = 
                    { Environment = getEnvironment()
                      TextToFind = getTextToFind()
                      FolderName = getFolderName()
                      LogType = getLogType()
                      Logger = log
                      Date = getDate()
                      Program = program }

                let downloadInApp x = dtoBuilder x |> Downloader.downloadLogs
                let downloadResult = 
                    getApplicazione() 
                    |> List.map downloadInApp 
                    |> List.fold (||) false
                
                if downloadResult then 0 else -1

        with ex -> -2
    execute() // return an integer exit code
