module Downloader

open System
open System.IO
open System.IO.Compression
open Types

let fileToMem (memStream:MemoryStream) (reader:Stream)=
    let sw = new System.Diagnostics.Stopwatch()
    sw.Start() 
    reader.CopyTo(memStream)
    sw.Stop()
    printf "%d" sw.Elapsed.Seconds
    memStream


let writeFile name (data, size) =
    use writter = new FileStream(name, FileMode.Create)
    writter.Write(data, 0, size)

module SaveLogs =
    
    let getFile file = 
        let _,reader = Bo.getPage "http://brepaddc2s01.azgroup.itad.corpnet/" file
        reader

    let downloadText name link shouldDownload=
        
        use memStream = new MemoryStream()
        match getFile link with 
        | Some r -> 
            use mem = fileToMem memStream r
            let data = mem.ToArray()
            mem.Close()
            let size = data.Length
            let stringRep = (System.Text.Encoding.ASCII.GetString data)
        
            if shouldDownload stringRep then
                (data,size) |> writeFile name |> ignore
        | None -> ()

    let downloadGzip name link shouldDownload= 
        let decompress reader =             
            use memStream = new MemoryStream()
            let memStream = fileToMem memStream reader
            use gzStream = new GZipStream(memStream,CompressionMode.Decompress)
            let data = memStream.ToArray()
            let size = BitConverter.ToInt32(data,data.Length - 4)
            let uncompressedData = Array.init size (fun i -> byte(i*i))

            memStream.Position <- 0L
            gzStream.Read(uncompressedData, 0, size) |> ignore
            uncompressedData,size
        
        let byteArrToString (data, size) = System.Text.Encoding.ASCII.GetString data , size
        let stringToByteArray (data:string,size) = System.Text.Encoding.ASCII.GetBytes data, size
        
        match getFile link with 
        | Some r -> 
            let stringRep,size = decompress r |> byteArrToString

            if shouldDownload stringRep then
                (stringRep,size) |> stringToByteArray |> writeFile name |> ignore

        | None -> ()

    
    let download (log:LogType * Link) server text=
        let downloadIfContains (content:string) =
            content.Contains text

        let downloadStrategy uri ((x,_)) =
            let fileName name = sprintf "%s_%s.log" server name
            match x with 
            | Log f -> downloadText (fileName f) uri downloadIfContains
            | Gz f -> downloadGzip (fileName f) uri downloadIfContains
            | FileType.Unknown -> ()

        let logtype,Link name = log
        applyLogType (downloadStrategy name) logtype