// Learn more about F# at http://fsharp.org

open CommandLine
open dostadning.cli.Commands
open dostadning.domain.auction

let Lot = new Lot(
            Title="title",
            Description= "Testdescription",
            ItemAttributes = [| 1 |],
            Duration = 14,
            Restarts = 1,
            StartPrice = 100, //StartPrice < ReservePrice < BytItNowPrice
            ReservePrice = 101,
            BuyItNowPrice = 102,
            VAT = 25,
            AcceptedBidderId = 1,
            PaymentOptionIds = [| 8192 |],
            ShippingCondition = "Ok",
            PaymentCondition = "Ok")

[<EntryPoint>]
let main argv =
    let res = Parser.Default.ParseArguments<
                CreateOpts, 
                AddTraderaUserOpts,
                FetchConsentOpts,
                ListSellersOpts,
                GetConsentOpts> argv
    match res with
    | :? CommandLine.Parsed<obj> as command ->
         match command.Value with
         | :? CreateOpts                 -> RunCreateAccount ()
         | :? AddTraderaUserOpts as opts -> RunAddTraderaUser opts
         | :? FetchConsentOpts as opts   -> RunFetchConsent opts
         | :? ListSellersOpts as opts    -> RunListSellers opts
         | :? GetConsentOpts as opts     -> RunGetConsent opts
        //  | :? FetchConsentOpts as opts   -> RunFetchConsent opts
        //  | :? CommitOpts as opts         -> RunCommit opts
        //  | :? StatRequestOpts as opts    -> RunStatRequest opts
         | _                             -> 0
    | :? CommandLine.NotParsed<obj>      -> printfn "notparsed"
                                            0
    | _                                  -> 0