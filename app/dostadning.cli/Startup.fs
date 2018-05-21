namespace dostadning.cli
open dostadning.cli.EnvironmentVariables
open dostadning.records.pgres

open dostadning.soap.tradera.feature
open dostadning.domain.seller
open System
open dostadning.domain.auction

module Startup =
        let soapAuth = GetAuthorization.Init appId
        let soapLookups = GetLookups.Init appId
        let soapAuctions = AuctionSoapCalls.Init appId

        let users = new Pgres() |> Repos.Accounts
        let sellers = new Pgres() |> Repos.Sellers

        let now = Func<DateTimeOffset>(fun () -> DateTimeOffset.Now)
        let sell = new SellerFeature(users, sellers, soapAuth, appId, now)

        let uploadbatch' soap imgs c input =
            AuctionFeature.UploadBatch(soap, imgs, c, input)

        let uploadbatch imgs c input = uploadbatch' soapAuctions imgs c input
        let poll' soap c cues upload = AuctionFeature.PollRequestOnQue(soap, c, cues, upload)
        let poll c cues upload = poll' soapAuctions c cues upload

        let seller acct id = new Seller(acct, id)

        
        let cons' s tkn = new Consent(s, tkn) 
        let cons acct id tkn = cons'(seller acct id) tkn
                