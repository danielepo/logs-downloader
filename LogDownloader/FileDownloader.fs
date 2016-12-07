module FileDownloader

open System
open System.IO
open System.IO.Compression

open WebScratcher
open Microsoft.FSharp.Collections

let SaveLogs (date:DateTime) (runEnvironment:RunEnvironment) (text:string) (timeSpecified:bool) =
    let downloadableFiles = FileToDownload date runEnvironment 

    let fileToMem (memStream:MemoryStream) (reader:Stream)=
        let sw = new System.Diagnostics.Stopwatch()
        sw.Start() 
        reader.CopyTo(memStream)
        sw.Stop()
        printf "%d" sw.Elapsed.Seconds
        memStream
    let writeFile name (data, size) =
        use writter = new FileStream(name+".log",FileMode.Create)
        writter.Write(data, 0, size)
    
    let downloadText relPath name =
        let url = sprintf @"http://logauto.allianzit%s" relPath

        use reader = streamUrl url
        use memStream = new MemoryStream()
        
        let mem = fileToMem memStream reader
        let data = mem.ToArray()
        let size = data.Length

        if (System.Text.Encoding.ASCII.GetString data).Contains(text) then
            (data,size) |> writeFile name
        else ()

    let downloadGzip relPath name =
        let url = sprintf @"http://logauto.allianzit%s" relPath

        let decompress () =             
            use reader = streamUrl url
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
        
        let stringRep,size = decompress() |> byteArrToString

        if stringRep.Contains(text) then
            (stringRep,size) |> stringToByteArray |> writeFile name
        else ()

    let downloadFilesInServer floder server = 
        if not (Directory.Exists floder) then Directory.CreateDirectory floder |> ignore
        for file in downloadableFiles server timeSpecified do 
            printf "Server: %s\tfile: %s\n" server file.Description
            match file.FileType with
            | Gzip -> downloadGzip file.Uri (sprintf @"%s\%s_%s" floder server file.Description)
            | Log -> downloadText file.Uri (sprintf @"%s\%s_%s" floder server file.Description)

    match runEnvironment with 
        | RunEnvironment.Test -> testServers |> List.iter (downloadFilesInServer "Logs")
        | RunEnvironment.Preprod -> preProdServers |> List.iter (downloadFilesInServer "Logs")
        | RunEnvironment.Prod -> prodServers |> Array.ofList |> Array.iter (downloadFilesInServer "Logs")

