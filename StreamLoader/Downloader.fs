module StreamDownloader

open System
open System.IO
open System.IO.Compression
open Types
open Logger
open StreamRetreiver

let fileToMem (memStream : MemoryStream) (reader : Stream) = 
    let sw = new System.Diagnostics.Stopwatch()
    sw.Start()
    reader.CopyTo(memStream)
    sw.Stop()
    memStream

let writeFile name (data, size) = 
    use writter = new FileStream(name, FileMode.Create)
    writter.Write(data, 0, size)

module SaveLogs = 
    let mutable logger : ILogger = new Logger("") :> ILogger
    
    let private getFile file = 
        let _, reader = getPage "" logger file
        reader
    
    let private downloadText name link shouldDownload = 
        use memStream = new MemoryStream()
        match getFile link with
        | Some r -> 
            use mem = fileToMem memStream r
            let data = mem.ToArray()
            mem.Close()
            let size = data.Length
            let stringRep = (System.Text.Encoding.ASCII.GetString data)
            if shouldDownload stringRep then 
                (data, size)
                |> writeFile name
                |> ignore
        | None -> ()
    
    let private downloadGzip name link shouldDownload = 
        let decompress reader = 
            use memStream = new MemoryStream()
            let memStream = fileToMem memStream reader
            use gzStream = new GZipStream(memStream, CompressionMode.Decompress)
            let data = memStream.ToArray()
            let size = BitConverter.ToInt32(data, data.Length - 4)
            
            let uncompressedData() = 
                logger.info <| sprintf "link: %s" link
                logger.info <| sprintf "size: %d" size
                if size >= 0 then Some <| Array.init size (fun i -> byte (i * i))
                else None
            memStream.Position <- 0L
            match uncompressedData() with
            | Some data -> 
                gzStream.Read(data, 0, size) |> ignore
                data, size
            | None -> [||], 0
        
        let byteArrToString (data, size) = System.Text.Encoding.ASCII.GetString data, size
        let stringToByteArray (data : string, size) = System.Text.Encoding.ASCII.GetBytes data, size
        match getFile link with
        | Some r -> 
            let stringRep, size = decompress r |> byteArrToString
            if shouldDownload stringRep then 
                (stringRep, size)
                |> stringToByteArray
                |> writeFile name
                |> ignore
        | None -> ()
    
    let download brach folder (server : string) text (_logger:ILogger) (log : Log * Link) = 
        logger <- _logger
        let logtype, Link name = log
        
        let downloadIfContains name (content : string) = 
            let contains = content.Contains text
            logger.info <| sprintf "File '%s' %s '%s'" name (if contains then "CONTAINS"
                                                           else "DOESN'T contains") text
            contains
        
        let downloadStrategy url ((x, _)) = 
            let uri = 
                let host = 
                    match brach with
                    | Branch.Auto -> "http://logauto2.servizi.allianzit"
                    | Branch.RV -> "http://logdanni2.servizi.allianzit"
                sprintf "%s/%s" host url
            let fileName name = 
                sprintf "Log/%s/%s_%s.log" folder server name
            match x with
            | Log f -> downloadText (fileName f) uri (downloadIfContains f)
            | Gz f -> downloadGzip (fileName f) uri (downloadIfContains f)
            | FileType.Unknown -> ()
        
        logger.info  <| sprintf "Downloading file %s" name
        applyLogType (downloadStrategy name) logtype
