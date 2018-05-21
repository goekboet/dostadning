using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using dostadning.domain.seller;

namespace dostadning.domain.auction
{
    public struct AuctionHandle
    {
        public AuctionHandle(
            string lotId,
            int iId,
            int rId)
        {
            ItemId = iId;
            RequestId = rId;
            LotId = lotId;
        }
        /// <summary>
        /// Identifies the input that initialized the request to begin the Auction.
        /// </summary>
        /// <returns>The Lot id</returns>
        public string LotId { get; }
        /// <summary>
        /// Traderas identifier for the auction. Used to query the status of the Auction once it is
        /// successfully started.
        /// </summary>
        /// <returns>the tradera ItemId</returns>
        public int ItemId { get; }
        /// <summary>
        /// Identifies the request itself so that we can query the current status of image uploads et.c.
        /// </summary>
        /// <returns>The ItemId</returns>
        public int RequestId { get; }

        public override string ToString() =>
        $"LotId: {LotId} ItemId: {ItemId} RequestId: {RequestId}";
    }

    public enum Process { Add, UploadImage, Commit }
    public struct BatchProcessResult
    {
        public BatchProcessResult(
            Process p, AuctionHandle h)
        {
            Process = p;
            Auction = h;
        }
        public Process Process { get; }
        public AuctionHandle Auction { get; }

        public override string ToString() => $"Process: {Process, 11} {Auction}";
    }

    public class Lot
    {
        public string Id { get; set; }
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

        public override string ToString() => $"Lot Id: {Id}";
    }

    public abstract class ImageType
    {
        public const string Gif = "image/gif";
        public const string Jpeg = "image/jpeg";
        public const string Png = "image/png";
        protected ImageType(string mime) { MIME = mime; }
        public string MIME { get; }
        public override string ToString() => MIME;

        public static ImageType ImageSupport(string mimetype)
        {
            switch (mimetype.ToLower())
            {
                case Gif:
                case Jpeg:
                case Png:
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
        public Image(
            ImageType t,
            Base64Encoded b)
        {
            if (t is Valid)
            {
                Type = t;
                Data = b;
            }
            else
            {
                throw new Error($"Invalid imagetype {t}");
            }
        }
        public string Key { get; }
        public ImageType Type { get; }

        Base64Encoded Data { get; }
        public byte[] Bytes => Data.Bytes;
        public override string ToString() => $"Type: {Type.ToString()} Size: {Data.Bytes.Length}";
    }

    /// <summary>
    /// Represents an Update on the result of a queued process.
    /// </summary>
    public sealed class Status
    {
        private Status(int rId, string type, string msg)
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
            new[] { Status.Done, Status.Pending, Status.Retry, Status.Error };
        /// <summary>
        /// Create a new result.
        /// </summary>
        /// <param name="rId">RequestId</param>
        /// <param name="t">Resulttype</param>
        /// <param name="msg">Optional string</param>
        /// <returns></returns>
        /// <exception cref="dostadning.domain.features.Error">If type parameter is not valid</exception>
        public static Status Create(int rId, string t, string msg = null) =>
            ValidTypes.Contains(t)
                ? new Status(rId, t, msg)
                : throw new Error($"Invalid result type: {t}");
    }

    public interface IImageLookup
    {
        IEnumerable<Image> Get(string key);
    }
    
    public static class AuctionFeature
    {
        static BatchProcessResult Add(AuctionHandle h) => new BatchProcessResult(Process.Add, h);
        static BatchProcessResult Image(AuctionHandle h) => new BatchProcessResult(Process.UploadImage, h);
        static BatchProcessResult Commit(AuctionHandle h) => new BatchProcessResult(Process.Commit, h);
        
        public static IObservable<BatchProcessResult> UploadBatch(
            IAuctionProcedures soap,
            IImageLookup imgs,
            Consent c,
            IEnumerable<Lot> input) => 
                input.ToObservable().SelectMany(x => Upload(soap, imgs, c, x));
        static IObservable<BatchProcessResult> Upload(IAuctionProcedures soap,
            IImageLookup imgs,
            Consent c,
            Lot input) => 
            CreateAuction(soap, c, input)
                .SelectMany(x => Observable.Concat(
                    Observable.Return(Add(x)),
                    AddImages(soap, imgs,c,x).Select(_ => Image(x)),
                    soap.Commit(c, x.RequestId).Select(_ => Commit(x)))
                );

        static ImmutableArray<int> empty => ImmutableArray<int>.Empty;        

        public static IObservable<IEnumerable<Status>> PollRequestOnQue(
            IAuctionProcedures soap,
            Consent c,
            IObservable<Unit> cue,
            IObservable<BatchProcessResult> requests) =>
                cue.WithLatestFrom(
                        requests
                            .Where(x => x.Process == Process.Add)
                            .Scan(empty, (ids, r) => ids.Add(r.Auction.RequestId)),
                        (_, rIds) => soap.GetResult(c, rIds))
                    .Merge();
        static IObservable<Unit> AddImages(
            IAuctionProcedures soap,
            IImageLookup imgs,
            Consent c,
            AuctionHandle a) => Observable.Merge(
                imgs.Get(a.LotId)
                .Select(x => soap.AddImage(c, x, a.RequestId)));

        public static IObservable<AuctionHandle> CreateAuction(
            IAuctionProcedures soap,
            Consent c,
            Lot l) => soap.AddLot(c, l);
    }
}