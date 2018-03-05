# logs-downloader
## Usage
```
LogDownloader --text="Testo Da Cercare" --program="Nome Programma" \ 
  --env="Ambiente" --log-type="Tipo Log" --folder="Cartella" \ 
  --date="Data" [--time="Ora evento"] [--config="path"]
```


## Options
--config: Path al file di configurazione, se non presente usa quello di default nella cartella Debug

--text: Testo da cercare all'interno dei log

--date: Giorno in cui cercare i log. Formato YYYY-MM-DD

--time: Ora in cui cercare i log. Formato HH.MM

--env: Ambiente da cercare. Valori possibili:
  test
  preprod
  prod

--log-type: Tipo di log da scaricare. Valori possibili:
* debug
* client
* ppl
* security 
* requests   
* functional           
 
--folder: Cartella nella quale salvare i log
 
--program: Nome del programma per cui scaricare i log, quelli supportati attualmente sono
*  Allianz1Web                
*  Annullamenti_AD            
*  Appendici_AD               
*  AZ1CRABackend              
*  BMWMotorInsurance          
*  ClausolarioDirezionale     
*  Conguagli_AD               
*  ConvertitorePol            
*  CruscottoPreventivi_AD     
*  CumuliWS                   
*  DichiarazioniDA            
*  DirMPTF                    
*  DuplicatiDA                
*  FastQuoteAU_AD             
*  FastQuoteRV_AD             
*  FlexDA                     
*  GesPlafondHost             
*  GestioneAnnullamentiDA     
*  GestioneClientiPPU_DA      
*  GestioneDocumentaleWS      
*  GestioneLibriMatricolaDA   
*  GestionePolizzeAperteDA    
*  GestionePortafoglio        
*  GestioneRevocheDA          
*  GRV_AD                     
*  IncassoDA                 
*  InquiryAgenzia_AD          
*  InquiryPrjX                
*  InquiryRV                  
*  MicrostockDA               
*  MMA                        
*  ModifichePortafoglio    
*  Mondial_Service            
*  NGRA2013                   
*  NGRA3_2                    
*  PreventivatoreLMDA         
*  PreventivoBMW              
*  PreventivoDealer           
*  PrevIsvap                  
*  PrevIsvapObserver          
*  RisanamentoDA              
*  Sicurplus                  
*  TestPU                     
*  ToolTrattativeDA           
*  WSArvato                   
*  WSBordero                  
*  WSConvenzioni              
*  WsDealerQuote              
*  WSIncassi                  
*  WSMicrostock               
*  WSPartnersGrad             
*  WSPrevweb                  
*  WSPromoFQ                  
*  WSTOOLTRATTATIVE  
