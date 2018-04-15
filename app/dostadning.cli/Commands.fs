namespace dostadning.cli

open System.IO
open System
open System.Collections
open CommandLine
open ObservableConvenience
open Startup
open dostadning.domain.account
open dostadning.domain.lookups
open dostadning.domain.seller
open dostadning.domain.auction
open System.IO

module Commands =

    [<Verb("Create", HelpText = "Create an account")>]
    type CreateOpts = {
        [<Value(0)>]
        unused : string
    }

    let RunCreateAccount () =
        let log = Action<string> (fun a -> printfn "Account created: %s" a)
        LogResult log (fun _ -> AccountFeature.Create users) |> run

    [<Verb("AddTraderaUser", HelpText = "For a given [account] add traderauser with [alias]")>]
    type AddTraderaUserOpts = {
        [<Value(0)>]
        account : string
        [<Value(1)>]
        alias : string
    }
    
    let RunAddTraderaUser(o : AddTraderaUserOpts) =
        let withUrl (u : TraderaAlias) = sprintf "%s \n %s" (string u) (u.ConsentUrl EnvironmentVariables.appId) 
        let logr = Action<TraderaAlias>(withUrl >> printf "%s")
        let acct = Guid.Parse o.account
        LogResult logr (fun() -> sell.Add(acct, o.alias))
        |> run

    [<Verb("FetchConsent", HelpText = "For a given [account] fetch a consent-token for [traderaalias]")>]
    type FetchConsentOpts = {
        [<Value(0)>]
        account : string
        [<Value(1)>]
        id : int
        [<Value(2)>]
        consentId : string
    }

    let RunFetchConsent(o : FetchConsentOpts) =
        let logr = Action<Consent>(string >> printf "%s")
        let acct = Guid.Parse o.account
        let s = seller acct o.id
        let cId = Guid.Parse o.consentId
        LogResult logr (fun() -> sell.Confirm(s)) |> run

    [<Verb("ListSellers", HelpText = "For a given [account] list all sellers")>]
    type ListSellersOpts = {
        [<Value(0)>]
        account : string
    }

    let RunListSellers(o : ListSellersOpts) =
        let acct = Guid.Parse o.account
        let withUrl (u : TraderaAlias) = sprintf "%s \n %s" (string u) (u.ConsentUrl EnvironmentVariables.appId)
        let logr (l : Generic.IEnumerable<TraderaAlias>) = 
            String.Join(Environment.NewLine, Seq.map withUrl l) 
            |> printfn "%s"
        let l = Action<Generic.IEnumerable<TraderaAlias>>(logr)

        LogResult l (fun () -> sell.List acct) |> run

    [<Verb("GetConsent", HelpText = "For a given seller, get the consent")>]
    type GetConsentOpts = {
        [<Value(0)>]
        account : string
        [<Value(1)>]
        id : int
    }

    let RunGetConsent(o : GetConsentOpts) =
        let acct = Guid.Parse o.account
        let s = seller acct o.id
        let logr = Action<Consent>(string >> printf "%s")

        LogResult logr (fun () -> sell.Get s) |> run


    [<Verb("CreateAuction", HelpText = "Creates an Auction")>]
    type CreateAuctionOpts = {
        [<Value(0)>]
        account : string
        [<Value(1)>]
        id : int
        [<Value(2)>]
        token : string
    }

    [<Verb("UploadBatch", HelpText = "Upload a batch of auctions")>]
    type UploadBatchOpts = {
        [<Value(0)>]
        account : string
        [<Value(1)>]
        id : int
        [<Value(2)>]
        token : string
    }

    // let RunAddImage (o : AddImageOpts) = 
    //     let bytes =
    //         File.ReadAllBytes 
    //         >> Convert.ToBase64String
    //         >> Convert.FromBase64String
    //         >> Base64Encoded
    //     let jpg = (ImageType.ImageSupport "image/jpeg") :?> Valid
        
    //     let img p = new Image(jpg, (bytes p))

    //     let addImage' cons image requestId =
    //         soapAuctions.AddImage(cons, image, requestId)
        
    //     let addImage (o : AddImageOpts) =
    //         let acct = Guid.Parse o.account
    //         let c = seller acct o.traderaId
    //         let i = img o.path
    //         addImage' c i o.requestId
        
    //     let logAddImage = Action<Reactive.Unit> (fun _ -> printfn "Done")
        
    //     LogResult logAddImage (fun () -> (addImage o)) |> run

    let findInDirectory path k =
        let bytes =
            File.ReadAllBytes 
            >> Convert.ToBase64String
            >> Convert.FromBase64String
            >> Base64Encoded
        let jpg = (ImageType.ImageSupport "image/jpeg") :?> Valid
        
        let img p = new Image(jpg, (bytes p))
        
        Path.Combine(path, k) 
        |> Directory.EnumerateFiles
        |> Seq.map img
        

    type FSImageLookup(datadir) =
        interface IImageLookup with 
            member this.Get k = findInDirectory datadir k
                
    let RunUploadBatch (o : UploadBatchOpts) =
        let cd = Directory.GetCurrentDirectory()
        let csv = Path.Combine(cd, "testdata", "testdata.csv")
        let imageDirs = Path.Combine(cd, "testdata")

        let ls = File.ReadAllLines(csv)
        let values (s : string) = s.Split [|';'|]
        let (head, data) = (ls.[0] |> values, Seq.tail ls |> Seq.map values)
        let byCol h xs = Seq.zip h xs
        let toDict es = Seq.fold (fun acc (k, v) -> Map.add k v acc) Map.empty es
        let toLot m =
            let toInts (s : string) = s.Split([|','|]) |> Seq.map int |> Seq.toArray
            new Lot(
                    Id = (Map.find "Id" m),
                    Title = (Map.find "Title" m),
                    Description = (Map.find "Description" m),
                    ItemAttributes = (Map.find "ItemAttributes" m |> toInts),
                    Duration = (Map.find "Duration" m |> int),
                    Restarts = (Map.find "Restarts" m |> int),
                    StartPrice = (Map.find "StartPrice" m |> int), //StartPrice < ReservePrice < BytItNowPrice
                    ReservePrice = (Map.find "ReservePrice" m |> int),
                    BuyItNowPrice = (Map.find "BuyItNowPrice" m |> int),
                    VAT = (Map.find "VAT" m |> int),
                    AcceptedBidderId = (Map.find "AcceptedBidderId" m |> int),
                    PaymentOptionIds = (Map.find "PaymentOptionIds" m |> toInts),
                    ShippingCondition = (Map.find "ShippingCondition" m),
                    PaymentCondition = (Map.find "PaymentCondition" m))

        let lots = Seq.map (fun r -> byCol head r |> toDict |> toLot) data

        let upload (o : UploadBatchOpts) =
            let acct = Guid.Parse o.account
            let s = seller acct o.id
            let c = new Consent(s, o.token)
            uploadbatch (FSImageLookup(imageDirs)) c lots

        let logr = Action<BatchProcessResult>(string >> printfn "%s")
        
        LogResult logr (fun () -> (upload o)) |> run

    // let RunCreateAuction (o : CreateAuctionOpts) l =
    //     let logr = Action<AuctionHandle>(string >> printf "%s")
    //     let acct = Guid.Parse o.account
    //     let s = seller  acct o.id
    //     let c = cons s o.token
    //     LogResult logr (fun () -> AuctionFeature.CreateAuction(soapAuctions, c, l))
    //                                         |> run

    [<Verb("AddImage", HelpText = "Add Image to a request.")>]
    type AddImageOpts = {
        [<Value(0)>]
        account : string
        [<Value(1)>]
        traderaId : int
        [<Value(2)>]
        token : string
        [<Value(3)>]
        requestId : int
        [<Value(4)>]
        path : string
    }
    
    

    [<Verb("CommitRequest", HelpText = "Commit a request to add an auction indicating we're not going to add more images.")>]
    type CommitOpts = {
        [<Value(0)>]
        traderaId : int
        [<Value(1)>]
        token : string
        [<Value(2)>]
        requestId : int
    }

    // let RunCommit(o : CommitOpts) = 
    //     let log = Action<Reactive.Unit> (fun _ -> printfn "Done")
        
    //     LogResult log (fun _ -> soapAuctions.Commit(cons o.traderaId o.token, o.requestId)) 
            // |> run

    [<Verb("StatRequest", HelpText = "Get status of a request")>]
    type StatRequestOpts = {
        [<Value(0)>]
        traderaId : int
        [<Value(1)>]
        token : string
        [<Value(2, Min = 1)>]
        requestIds : Generic.IEnumerable<int>
    }

    // let RunStatRequest(o : StatRequestOpts) =
    //     let logUpdates (l : Generic.IEnumerable<Update>) = 
    //         String.Join(Environment.NewLine, Seq.map string l) 
    //         |> printfn "%s"
    //     let log = Action<Generic.IEnumerable<Update>> logUpdates

    //     LogResult log (fun _ -> soapAuctions.GetResult(cons o.traderaId o.token, o.requestIds |> Seq.toArray))
    //         |> run

    

    [<Verb("Lookups", HelpText = "Fetch lookups for items through tradera api")>]
    type LookupOpts = {
        [<Value(0)>]
        unused : string
    }

    let RunLookups () =
        let logLookups (l : Generic.IEnumerable<Lookup>) = 
            String.Join(Environment.NewLine, Seq.map string l) 
            |> printfn "%s"

        let log = Action<Generic.IEnumerable<Lookup>> logLookups 

        LogResult log (fun () -> LookupFeatures.GetLookups soapLookups) |> run
    
    [<Verb("CompareWatches", HelpText = "Prints a timestamp fetched from tradera and one made locally")>]
    type CompareWatchesOpts = {
        [<Value(0)>]
        unused : string
    }

    let RunCompareWatches () =
        let log = Action<WatchComparison> (string >> printf "%s")
        LogResult log (fun () -> LookupFeatures.CompareWatches soapLookups) |> run

    [<Verb("GetId", HelpText = "fetch corresponding traderaId for [alias]")>]
    type FetchIdOpts = {
        [<Value(0)>]
        traderaalias : string
    }

    // let RunFetchId(o : FetchIdOpts) = 
    //     let GetIdLog= printfn "%s is associated with %i"
    //     let log = Action<int> (GetIdLog o.traderaalias)

    //     LogResult log (fun () -> GetTraderaConsentFeature.PairWithId(soapAuth, o.traderaalias)) |> run


    