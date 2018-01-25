module Types

open FSharp.Data
open System

type Application =  WebApp | WebService


type Servers = JsonProvider<"""./config.json""">

type TestServers = string List
type PreProdServers = string List
type ProdServers = string List

type Environment = Test | Preprod | Production 

type Branch = RV | Auto
type Program = 
    | IncassoDA                    = 1
    | WSIncassi                    = 2
    | GestioneLibriMatricolaDA     = 3
    | NGRA2013                     = 4
    | DichiarazioniDA              = 5
    | DuplicatiDA                  = 6
    | FastQuoteAU_AD               = 7
    | FlexDA                       = 8
    | GesPlafondHost               = 9
    | GestioneAnnullamentiDA       = 10
    | GestioneClientiPPU_DA        = 11
    | GestionePolizzeAperteDA      = 12
    | GestioneRevocheDA            = 13
    | InquiryAgenzia_AD            = 14
    | MicrostockDA                 = 15
    | PreventivatoreLMDA           = 16
    | RisanamentoDA                = 17
    | ToolTrattativeDA             = 18
    | Allianz1Web                  = 19
    | BMWMotorInsurance            = 20
    | GestioneDocumentaleWS        = 31
    | Mondial_Service              = 32
    | NGRA3_2                      = 33
    | PreventivoBMW                = 34
    | PreventivoDealer             = 35
    | PrevIsvap                    = 36
    | PrevIsvapObserver            = 37
    | Sicurplus                    = 38
    | TestPU                       = 39
    | WSArvato                     = 40
    | WSBordero                    = 41
    | WSConvenzioni                = 42
    | WsDealerQuote                = 43
    | WSMicrostock                 = 44
    | WSPartnersGrad               = 45
    | WSPrevweb                    = 46
    | WSPromoFQ                    = 47
    | WSTOOLTRATTATIVE             = 48
    | Annullamenti_AD              = 49
    | Appendici_AD                 = 50
    | AZ1CRABackend                = 51
    | ClausolarioDirezionale       = 52
    | Conguagli_AD                 = 53
    | ConvertitorePol              = 54
    | CruscottoPreventivi_AD       = 55
    | CumuliWS                     = 56
    | DirMPTF                      = 57
    | FastQuoteRV_AD               = 58
    | GestionePortafoglio          = 59
    | GRV_AD                       = 60
    | InquiryPrjX                  = 61
    | InquiryRV                    = 62
    | MMA                          = 63
    | ModifichePortafoglio         = 64
                                      
let programsBranchMap =               
    let autoPrograms = [              
        Program.IncassoDA             
        Program.GestioneLibriMatricolaDA
        Program.NGRA2013              
        Program.DichiarazioniDA
        Program.DuplicatiDA
        Program.FastQuoteAU_AD
        Program.FlexDA
        Program.GesPlafondHost
        Program.GestioneAnnullamentiDA
        Program.GestioneClientiPPU_DA
        Program.GestionePolizzeAperteDA
        Program.GestioneRevocheDA
        Program.InquiryAgenzia_AD
        Program.MicrostockDA
        Program.PreventivatoreLMDA
        Program.RisanamentoDA
        Program.ToolTrattativeDA
        Program.Allianz1Web
        Program.BMWMotorInsurance
        Program.GestioneDocumentaleWS
        Program.Mondial_Service
        Program.NGRA3_2
        Program.PreventivoBMW
        Program.PreventivoDealer
        Program.PrevIsvap
        Program.PrevIsvapObserver
        Program.Sicurplus
        Program.TestPU
        Program.WSArvato
        Program.WSBordero
        Program.WSConvenzioni
        Program.WsDealerQuote
        Program.WSIncassi
        Program.WSMicrostock
        Program.WSPartnersGrad
        Program.WSPrevweb
        Program.WSPromoFQ
        Program.WSTOOLTRATTATIVE ] |> List.map (fun x -> x, Branch.Auto)
    let rvPrograms = [
        Program.Annullamenti_AD
        Program.Appendici_AD 
        Program.AZ1CRABackend
        Program.ClausolarioDirezionale
        Program.Conguagli_AD
        Program.ConvertitorePol
        Program.CruscottoPreventivi_AD
        Program.CumuliWS
        Program.DirMPTF
        Program.FastQuoteRV_AD
        Program.GestionePortafoglio
        Program.GRV_AD
        Program.InquiryPrjX
        Program.InquiryRV
        Program.MMA
        Program.ModifichePortafoglio ] |> List.map (fun x -> x, Branch.RV)

    [ autoPrograms; rvPrograms ]
    |> List.concat
    |> Map.ofList

let programsMap = 
    let webapps = [    
        Program.IncassoDA
        Program.GestioneLibriMatricolaDA
        Program.NGRA2013
        Program.DichiarazioniDA
        Program.DuplicatiDA
        Program.FastQuoteAU_AD
        Program.FlexDA
        Program.GesPlafondHost
        Program.GestioneAnnullamentiDA
        Program.GestioneClientiPPU_DA
        Program.GestionePolizzeAperteDA
        Program.GestioneRevocheDA
        Program.InquiryAgenzia_AD
        Program.MicrostockDA
        Program.PreventivatoreLMDA
        Program.RisanamentoDA
        Program.ToolTrattativeDA
        Program.Annullamenti_AD
        Program.Appendici_AD 
        Program.AZ1CRABackend
        Program.ClausolarioDirezionale
        Program.Conguagli_AD
        Program.ConvertitorePol
        Program.CruscottoPreventivi_AD
        Program.CumuliWS
        Program.DirMPTF
        Program.FastQuoteRV_AD
        Program.GestionePortafoglio
        Program.GRV_AD
        Program.InquiryPrjX
        Program.InquiryRV
        Program.MMA
        Program.ModifichePortafoglio ] |> List.map (fun x -> x,Application.WebApp)

    let webservices = [
        Program.Allianz1Web
        Program.BMWMotorInsurance
        Program.GestioneDocumentaleWS
        Program.Mondial_Service
        Program.NGRA3_2
        Program.PreventivoBMW
        Program.PreventivoDealer
        Program.PrevIsvap
        Program.PrevIsvapObserver
        Program.Sicurplus
        Program.TestPU
        Program.WSArvato
        Program.WSBordero
        Program.WSConvenzioni
        Program.WsDealerQuote
        Program.WSIncassi
        Program.WSMicrostock
        Program.WSPartnersGrad
        Program.WSPrevweb
        Program.WSPromoFQ
        Program.WSTOOLTRATTATIVE ] |> List.map (fun x -> x,Application.WebService)
     
    [webapps; webservices] 
    |> List.concat
    |> Map.ofList

let enumToString e =
    sprintf "%A" e

type EnvironmentServer = {
    Test: TestServers
    Preprod: PreProdServers
    PreprodFMO: PreProdServers
    Production: ProdServers
}

type FileType = 
    | Unknown
    | Log of string
    | Gz of string

type Link = Link of string

type LogType = 
    | Unknown
    | ClientPerformance
    | DebugTrace
    | PplTrace
    | Security
    | Requests
    | Functional

type Log = 
    | ClientPerformance of FileType * DateTime option
    | DebugTrace of FileType * DateTime option
    | PplTrace of FileType * DateTime option
    | Security of FileType * DateTime option
    | Requests of FileType * DateTime option
    | Functional of FileType * DateTime option
    | Unknown

let mapToLogType =
    function 
    | ClientPerformance _-> LogType.ClientPerformance
    | DebugTrace _-> LogType.DebugTrace 
    | PplTrace _-> LogType.PplTrace
    | Security _-> LogType.Security
    | Requests _-> LogType.Requests
    | Functional _-> LogType.Functional
    | Unknown -> LogType.Unknown

let mapLogType fn =
    function 
    | ClientPerformance (f,d)-> ClientPerformance (fn (f,d))
    | DebugTrace (f,d)-> DebugTrace (fn (f,d))
    | PplTrace (f,d)-> PplTrace (fn (f,d))
    | Security (f,d)-> Security (fn (f,d))
    | Requests (f,d)-> Requests (fn (f,d))
    | Functional (f,d)-> Requests (fn (f,d))
    | Unknown -> Unknown

let applyLogType fn =
    function 
    | ClientPerformance (f,d)-> fn (f,d)
    | DebugTrace (f,d)-> fn (f,d)
    | PplTrace (f,d)-> fn (f,d)
    | Security (f,d)-> fn (f,d)
    | Requests (f,d)-> fn (f,d)
    | Functional (f,d)-> fn (f,d)
    | Unknown -> ()

type LinkType = 
    | Folder of LinkType seq
    | File of Log*Link

type SpecialDateTime =
    | Timed of DateTime
    | JustDate of DateTime