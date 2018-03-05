// Learn more about F# at http://fsharp.org

open CommandLine
open dostadning.records.pgres
open dostadning.domain.features
open System
open dostadning.domain.result
open dostadning.soap.tradera.feature
open System.Threading.Tasks

[<Verb("Create", HelpText = "Create an account")>]
type AdmitOpts = {
    [<Value(0)>]
    alias : string
}
let Admitlog id = sprintf "Created account with id: %s" id

[<Verb("GetId", HelpText = "fetch corresponding traderaId for [alias]")>]
type FetchIdOpts = {
    [<Value(0)>]
    traderaalias : string
}
let GetIdLog = sprintf "%s is associated with %i" 

[<Verb("AddTraderaUser", HelpText = "For a given [account] add traderauser with [alias]")>]
type AddTraderaUserOpts = {
    [<Value(0)>]
    account : string
    [<Value(1)>]
    alias : string
}
let appId = new AppIdentity(
                Environment.GetEnvironmentVariable "dostadning_tradera_appid" |> int, 
                Environment.GetEnvironmentVariable "dostadning_tradera_appkey")
let pKey = Environment.GetEnvironmentVariable("dostadning_tradera_pkey")
let AddTraderauserLog = sprintf "https://api.tradera.com/tokenlogin.aspx?appId=%s&pkey=%s&skey=%s" (string appId.Id) pKey 

[<Verb("FetchConsent", HelpText = "For a given [account] fetch a consent-token for [traderaalias]")>]
type FetchConsentOpts = {
    [<Value(0)>]
    account : string
    [<Value(1)>]
    traderaalias : string
}

let FetchConsentLog alias exp= sprintf "We have consent for traderauser %s until %s" alias (string exp)

let Run (f : unit -> Task<Either<'a>>) =
    async {
        return! f() |> Async.AwaitTask
    } |> Async.RunSynchronously 

let Log l (r : Either<'a>) =
    match r.IsError with
    | false -> printfn "%s" (r.Result |> l)
               1
    | _  -> match r.Error with
            | :? DomainError as e -> printfn "Domanierror: %s" e.Code
            | :? ExceptionalError as e -> printfn "Exception %s" e.Exception.InnerException.Message
            | _ -> printfn "Unknown error"
            0

let users = new Pgres() |> Repos.Accounts
let soapClient = GetTraderaConsent.Init appId

[<EntryPoint>]
let main argv =
    let res = Parser.Default.ParseArguments<AdmitOpts, FetchIdOpts, AddTraderaUserOpts, FetchConsentOpts> argv
    match res with
    | :? CommandLine.Parsed<obj> as command ->
        match command.Value with 
        | :? AdmitOpts -> Run(fun () -> AccountFeature.Create users) 
                          |> (Log Admitlog)
        | :? FetchIdOpts as opts -> Run(fun () -> GetTraderaConsentFeature.PairWithId(soapClient, opts.traderaalias))
                                    |> Log (GetIdLog opts.traderaalias)
        | :? AddTraderaUserOpts as opts -> Run (fun() -> GetTraderaConsentFeature.AddTraderaUser(users, soapClient, opts.account, opts.alias))
                                           |> Log AddTraderauserLog
        | :? FetchConsentOpts as opts -> Run (fun() -> GetTraderaConsentFeature.FetchConsent(users, soapClient, opts.account, opts.traderaalias))
                                         |> Log (FetchConsentLog opts.traderaalias)
        | _ -> -1
    | :? CommandLine.NotParsed<obj> -> printfn "notparsed"
                                       0
    | _ -> -1