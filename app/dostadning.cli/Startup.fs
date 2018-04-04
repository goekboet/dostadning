namespace dostadning.cli
open dostadning.cli.EnvironmentVariables
open dostadning.records.pgres

open dostadning.soap.tradera.feature
open dostadning.domain.seller
open System

module Startup =
        let soapAuth = GetAuthorization.Init appId
        let soapLookups = GetLookups.Init appId
        let soapAuctions = AuctionSoapCalls.Init appId

        let users = new Pgres() |> Repos.Accounts

        let now = Func<DateTimeOffset>(fun () -> DateTimeOffset.Now)
        let sell = new SellerFeature(users, soapAuth, appId, now) 

        let seller acct id = new Seller(acct, id)

        
        let cons' s tkn = new Consent(s, tkn) 
        let cons acct id tkn = cons'(seller acct id) tkn
                