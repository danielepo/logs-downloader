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
        
        let choosedDate = 
            match getArgument "date" with
            | Some d -> 
                try 
                    match getArgument "time" with
                    | Some t -> Some <| Timed(DateTime.Parse(sprintf "%s %s" d t))
                    | None -> Some <| JustDate(DateTime.Parse d)
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
            | (true, 2) -> Program.WSIncassi
            | _ -> Program.IncassoDA
        match getArgument "program" with
        | Some p -> 
            let program = programsMap |> Seq.tryFind (fun x -> ((enumToString x.Key).ToLower()) = p.ToLower())
            match program with
            | Some x -> x.Key
            | None -> Program.IncassoDA
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
        | _ -> LogType.DebugTrace
    
    let getFolderName() = 
        match getArgument "folder" with
        | Some x -> x
        | None -> "Default"
        
    match argv |> Array.tryFind (fun x -> x = "--help") with
        | Some _ ->
            let msg ="""
Usage: 
LogDownloader --text="Testo Da Cercare" --program="Nome Programma"               
              --env="Ambiente" --log-type="Tipo Log" --folder="Cartella"
              --date="Data" [--time="Ora evento"] 


Options: 
--text        Testo da cercare all'interno dei log

--program     Nome del programma per cui scaricare i log, quelli supportati 
              attualmente sono

--date        Giorno in cui cercare i log. Formato YYYY-MM-DD

--time        Ora in cui cercare i log. Formato HH.MM

--env         Ambiente da cercare. Valori possibili:
              test
              preprod
              prod

--log-type    Tipo di log da scaricare. Valori possibili:
              debug
              client
              ppl
              security 
              requests              
 
 --folder     Cartella nella quale salvare i log

 --program    tipo di log da scaricare. Valori possibili:
              Allianz1Web                
              Annullamenti_AD            
              Appendici_AD               
              AZ1CRABackend              
              BMWMotorInsurance          
              ClausolarioDirezionale     
              Conguagli_AD               
              ConvertitorePol            
              CruscottoPreventivi_AD     
              CumuliWS                   
              DichiarazioniDA            
              DirMPTF                    
              DuplicatiDA                
              FastQuoteAU_AD             
              FastQuoteRV_AD             
              FlexDA                     
              GesPlafondHost             
              GestioneAnnullamentiDA     
              GestioneClientiPPU_DA      
              GestioneDocumentaleWS      
              GestioneLibriMatricolaDA   
              GestionePolizzeAperteDA    
              GestionePortafoglio        
              GestioneRevocheDA          
              GRV_AD                     
              IncassoDA                 
              InquiryAgenzia_AD          
              InquiryPrjX                
              InquiryRV                  
              MicrostockDA               
              MMA                        
              ModifichePortafoglio    
              Mondial_Service            
              NGRA2013                   
              NGRA3_2                    
              PreventivatoreLMDA         
              PreventivoBMW              
              PreventivoDealer           
              PrevIsvap                  
              PrevIsvapObserver          
              RisanamentoDA              
              Sicurplus                  
              TestPU                     
              ToolTrattativeDA           
              WSArvato                   
              WSBordero                  
              WSConvenzioni              
              WsDealerQuote              
              WSIncassi                  
              WSMicrostock               
              WSPartnersGrad             
              WSPrevweb                  
              WSPromoFQ                  
              WSTOOLTRATTATIVE        

"""
            printf "%s" msg
            Console.ReadLine() |> ignore

        | None -> 

            let dto : Downloader.DownloaderDto = 
                { Environment = getEnvironment()
                  TextToFind = getTextToFind()
                  FolderName = getFolderName()
                  LogType = getLogType()
                  Logger = log
                  Date = getDate()
                  Program = getApplicazione() }
    
            Downloader.downloadLogs dto
    0 // return an integer exit code
