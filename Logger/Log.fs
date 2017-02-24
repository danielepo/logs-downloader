namespace Logger
open log4net
type Logger(_log:ILog) = 
    
    new(name:string) =  
        Logger(LogManager.GetLogger(name))
       
    new(``type``:System.Type) = 
        Logger(LogManager.GetLogger(``type``))

    member x.info format = 
        Printf.ksprintf _log.Info format

    member x.debug format = 
        Printf.ksprintf _log.Debug format

    member x.debug (format,ex) = 
        Printf.ksprintf (fun x -> _log.Debug (x,ex)) format

    member x.warn format = 
        Printf.ksprintf _log.Warn format

    member x.warn (format,ex) = 
        Printf.ksprintf (fun x -> _log.Warn (x,ex)) format

    member x.error format = 
        Printf.ksprintf _log.Error format

    member x.error (format,ex) = 
        Printf.ksprintf (fun x -> _log.Error (x,ex)) format