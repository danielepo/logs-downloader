namespace Logger

open log4net

[<AllowNullLiteral>]
type ILogger = 
    abstract debug : string -> unit
    abstract debug : string * ex:exn -> unit
    abstract error : string -> unit
    abstract error : string * ex:exn -> unit
    abstract info : string -> unit
    abstract warn : string -> unit
    abstract warn : string * ex:exn -> unit

[<AllowNullLiteral>]
type Logger(_log : ILog) = 
    new(name : string) = Logger(LogManager.GetLogger(name))
    new(``type`` : System.Type) = Logger(LogManager.GetLogger(``type``))
    member x.info format = (x :> ILogger).info format
    member x.debug format = (x :> ILogger).debug format
    member x.warn format = (x :> ILogger).warn format
    member x.error format = (x :> ILogger).error format
    member x.debug (format, ex) = (x :> ILogger).debug(format, ex)
    member x.warn (format, ex) = (x :> ILogger).warn(format, ex)
    member x.error (format, ex) = (x :> ILogger).error(format, ex)
    interface ILogger with
        member x.info format = _log.Info format
        member x.debug format = _log.Debug format
        member x.debug (format, ex) = _log.Debug(format, ex)
        member x.warn format = _log.Warn format
        member x.warn (format, ex) = _log.Warn(format, ex)
        member x.error format = _log.Error format
        member x.error (format, ex) = _log.Error(x, ex)
