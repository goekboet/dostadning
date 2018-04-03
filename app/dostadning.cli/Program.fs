// Learn more about F# at http://fsharp.org

open CommandLine
open dostadning.records.pgres
open dostadning.domain.features
open System
open dostadning.soap.tradera.feature
open dostadning.domain.result
open System.Collections
open dostadning.domain.service.tradera
open dostadning.cli.EnvironmentVariables
open dostadning.cli.ObservableConvenience

[<Verb("Create", HelpText = "Create an account")>]
type CreateOpts = {
    [<Value(0)>]
    alias : string
}
let Admitlog id = printfn "Created account with id: %s" id

[<Verb("Lookups", HelpText = "Fetch lookups for items through tradera api")>]
type LookupOpts = {
    [<Value(0)>]
    unused : string
}
let Lookuplog (l : Generic.IEnumerable<Lookup>) =  String.Join(Environment.NewLine, Seq.map string l) |> printfn "%s"

[<Verb("CompareWatches", HelpText = "Prints a timestamp fetched from tradera and one made locally")>]
type CompareWatchesOpts = {
    [<Value(0)>]
    unused : string
}

let CompareWatcheslog t = (string t) |> printfn "%s" 

[<Verb("CreateAuction", HelpText = "Creates an Auction")>]
type CreateAuctionOpts = {
    [<Value(0)>]
    id : int
    [<Value(1)>]
    token : string
}

let logr = Action<AuctionHandle>(string >> printf "%s")


[<Verb("GetId", HelpText = "fetch corresponding traderaId for [alias]")>]
type FetchIdOpts = {
    [<Value(0)>]
    traderaalias : string
}
let GetIdLog = printfn "%s is associated with %i" 

[<Verb("AddTraderaUser", HelpText = "For a given [account] add traderauser with [alias]")>]
type AddTraderaUserOpts = {
    [<Value(0)>]
    account : string
    [<Value(1)>]
    alias : string
}

let AddTraderauserLog = printfn "https://api.tradera.com/tokenlogin.aspx?appId=%s&pkey=%s&skey=%s" (string appId.Id) pKey 

[<Verb("FetchConsent", HelpText = "For a given [account] fetch a consent-token for [traderaalias]")>]
type FetchConsentOpts = {
    [<Value(0)>]
    account : string
    [<Value(1)>]
    traderaalias : string
}

let FetchConsentLog alias exp = printfn "We have consent for traderauser %s until %s" alias (string exp)

let users = new Pgres() |> Repos.Accounts
let soapAuth = GetAuthorization.Init appId
let soapLookups = GetLookups.Init appId
let soapAuctions = AuctionSoapCalls.Init appId

let Consent id t = new Consent(id, t)

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
           


//let runCreateAuction = LogResult (Action<AuctionHandle> logr) Errorlog (fun () -> AuctionFeature.CreateAuction(soapAuctions, Consent, Lot))

[<EntryPoint>]
let main argv =
    let res = Parser.Default.ParseArguments<CreateOpts, LookupOpts, FetchIdOpts, AddTraderaUserOpts, FetchConsentOpts, CompareWatchesOpts, CreateAuctionOpts> argv
    match res with
    | :? CommandLine.Parsed<obj> as command ->
         match command.Value with
         | :? CreateAuctionOpts as opts -> LogResult logr (fun () -> AuctionFeature.CreateAuction(soapAuctions, new Consent(opts.id, opts.token), Lot))
                                            |> run
         | :? CreateOpts -> LogResult (Action<string> Admitlog)  (fun () -> AccountFeature.Create users) 
                            |> run
         | :? LookupOpts -> LogResult (Action<Generic.IEnumerable<Lookup>> Lookuplog)  (fun () -> LookupFeatures.GetLookups soapLookups)
                            |> run
         | :? CompareWatchesOpts -> LogResult (Action<WatchComparison> CompareWatcheslog) (fun () -> LookupFeatures.CompareWatches soapLookups)
                                    |> run
         | :? FetchIdOpts as opts -> LogResult (Action<int> (GetIdLog opts.traderaalias)) (fun () -> GetTraderaConsentFeature.PairWithId(soapAuth, opts.traderaalias))
                                     |> run
         | :? AddTraderaUserOpts as opts -> LogResult (Action<string> AddTraderauserLog) (fun() -> GetTraderaConsentFeature.AddTraderaUser(users, soapAuth, opts.account, opts.alias))
                                            |> run
         | :? FetchConsentOpts as opts -> LogResult (Action<DateTimeOffset> (FetchConsentLog opts.traderaalias)) (fun() -> GetTraderaConsentFeature.FetchConsent(users, soapAuth, opts.account, opts.traderaalias))
                                          |> run
         | _ -> -1
    | :? CommandLine.NotParsed<obj> -> printfn "notparsed"
                                       0
    | _ -> -1