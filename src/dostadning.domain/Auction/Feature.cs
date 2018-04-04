using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using dostadning.domain.seller;

namespace dostadning.domain.auction
{
    public struct AuctionHandle
    {
        public AuctionHandle(int iId, int rId)
        {
            ItemId = iId;
            RequestId = rId;
        }

        public int ItemId { get; }
        public int RequestId { get; }

        public override string ToString() =>
        $"ItemId: {ItemId} RequestId: {RequestId}";
    }

    public class Lot
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int[] ItemAttributes { get; set; }

        public int Duration { get; set; }
        public int Restarts { get; set; }
        public int StartPrice { get; set; }
        public int ReservePrice { get; set; }
        public int BuyItNowPrice { get; set; }
        public int VAT { get; set; }
        public int AcceptedBidderId { get; set; }

        public int[] PaymentOptionIds { get; set; }
        public string ShippingCondition { get; set; }
        public string PaymentCondition { get; set; }

        public override string ToString() => "The request in json format.";
    }

    public abstract class ImageType
    {
        protected ImageType(string mime) { MIME = mime; }
        public string MIME { get; }
        public override string ToString() => MIME;

        public static ImageType ImageSupport(string mimetype)
        {
            switch (mimetype.ToLower())
            {
                case "image/gif":
                case "image/jpeg":
                case "image/png":
                    return new Valid(mimetype);
                default:
                    return new Invalid(mimetype);
            }
        }
    }

    public sealed class Valid : ImageType
    {
        internal Valid(string mime) : base(mime) { }
    }

    public sealed class Invalid : ImageType
    {
        internal Invalid(string mime) : base(mime) { }
    }

    public sealed class Base64Encoded
    {
        public Base64Encoded(byte[] bs) => Bytes = bs;
        public byte[] Bytes { get; }
        public override string ToString() =>
            string.Format("{0:N0} K", Bytes.Length / 1024);
    }

    public sealed class Image
    {
        public Image(Valid t, Base64Encoded b)
        {
            Type = t;
            Data = b;
        }
        public Valid Type { get; }

        Base64Encoded Data { get; }
        public byte[] Bytes => Data.Bytes;
        public override string ToString() => $"Type: {Type.ToString()} Size: {Data.Bytes.Length}";
    }

    /// <summary>
    /// Represents an Update on the result of a queued process.
    /// </summary>
    public sealed class Update
    {
        private Update(int rId, string type, string msg)
        {
            RequestId = rId;
            Type = type;
            Message = msg ?? "";
        }
        /// <summary>
        /// Identifier for the request
        /// </summary>
        /// <returns></returns>
        public int RequestId { get; }
        /// <summary>
        /// The state of the request at a point in Time.
        /// </summary>
        /// <returns>A valid type</returns>
        public string Type { get; }
        /// <summary>
        /// A message from the service.
        /// </summary>
        /// <returns>Message or empty string</returns>
        public string Message { get; }

        public override string ToString() =>
            $"RequestId: {RequestId} Type: {Type} Message: {Message}";

        public const string Done = "Done";
        public const string Pending = "Pending";
        public const string Retry = "Retry";
        public const string Error = "Error";

        /// <summary>
        /// The result has one of the following types:
        /// Done: Tradera has processed the request
        /// Pending: Tradera has not processed the request
        /// Retry: We should retry the request 
        /// Error: Tradera could not fulfil the request
        /// </summary>
        /// <returns>Enumeration of valid types</returns>
        public static IEnumerable<string> ValidTypes { get; } =
            new[] { Update.Done, Update.Pending, Update.Retry, Update.Error };
        /// <summary>
        /// Create a new result.
        /// </summary>
        /// <param name="rId">RequestId</param>
        /// <param name="t">Resulttype</param>
        /// <param name="msg">Optional string</param>
        /// <returns></returns>
        /// <exception cref="dostadning.domain.features.Error">If type parameter is not valid</exception>
        public static Update Create(int rId, string t, string msg = null) =>
            ValidTypes.Contains(t)
                ? new Update(rId, t, msg)
                : throw new Error($"Invalid result type: {t}");
    }

    public sealed class Auction
    {
        public int Id { get; }
        public string Status { get; }
        public string Error { get; }
    }

    public sealed class JSonAuction
    {
        public string Json {get;}
    }

    public interface IAuction
    {
        IObservable<AuctionHandle> Add(
            Consent c,
            Lot l);

        IObservable<Unit> AddImage(
            Consent c,
            AuctionHandle h);
        IObservable<Unit> Commit(
            Consent c,
            AuctionHandle h);

        IObservable<Auction> List(
            Consent c);

        IObservable<JSonAuction> Get(
            Consent c,
            AuctionHandle h);

    }

    public static class AuctionFeature
    {
        public static IObservable<AuctionHandle> CreateAuction(
            IAuctionProcedures soap,
            Consent c,
            Lot l) => soap.AddLot(c, l);

    }
}