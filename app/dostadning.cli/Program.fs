// Learn more about F# at http://fsharp.org

open CommandLine
open dostadning.cli.Commands

[<EntryPoint>]
let main argv =
    let res = Parser.Default.ParseArguments<
                CreateOpts, 
                AddTraderaUserOpts,
                FetchConsentOpts,
                ListSellersOpts,
                GetConsentOpts,
                UploadBatchOpts> argv
    match res with
    | :? CommandLine.Parsed<obj> as command ->
         match command.Value with
         | :? CreateOpts                 -> RunCreateAccount ()
         | :? AddTraderaUserOpts as opts -> RunAddTraderaUser opts
         | :? FetchConsentOpts as opts   -> RunFetchConsent opts
         | :? ListSellersOpts as opts    -> RunListSellers opts
         | :? GetConsentOpts as opts     -> RunGetConsent opts
         | :? UploadBatchOpts as opts    -> RunUploadBatch opts
         | _                             -> 0
    | :? CommandLine.NotParsed<obj>      -> printfn "notparsed"
                                            0
    | _                                  -> 0