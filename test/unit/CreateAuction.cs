using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using dostadning.domain.auction;
using dostadning.domain.seller;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace unit
{
    [TestClass]
    public class CreateAuctionTest
    {
        static Lot[] Testcase = new[]
        {
            new Lot { Id = "1" },
            new Lot { Id = "2" },
        };

        class Images : IImageLookup
        {
            static ImageType Mime(string t) => ImageType.ImageSupport(t);
            static Base64Encoded Bytes(int i) => new Base64Encoded(BitConverter.GetBytes(i));
            Dictionary<string, Image[]> Imgs { get; } = new Dictionary<string, Image[]>
            {
                ["1"] = new[]
                {
                    new Image(Mime(ImageType.Png), Bytes(1)),
                    new Image(Mime(ImageType.Png), Bytes(2))
                },
                ["2"] = new Image[0]
            };

            // public ImmutableHashSet<int> Keys =>
            //     Testcase.Select(x => x.Id).ToImmutableHashSet();

            public IEnumerable<Image> Get(string key) => Imgs[key];
        }

        Mock<IAuctionProcedures> SoapDependency(
            IScheduler s)
        {
            var requestId = 0;

            var m = new Mock<IAuctionProcedures>();
            m.Setup(x => x.AddLot(
                    It.IsAny<Consent>(),
                    It.IsAny<Lot>()))
                .Returns((Consent c, Lot l) => 
                    Observable.Return(new AuctionHandle(l.Id, 0, ++requestId), s));

            m.Setup(x => x.AddImage(It.IsAny<Consent>(), It.IsAny<Image>(), It.IsAny<int>()))
                .Returns(Observable.Return(Unit.Default, s));

            m.Setup(x => x.Commit(It.IsAny<Consent>(), It.IsAny<int>()))
                .Returns(Observable.Return(Unit.Default, s));

            return m;
        }

        ImmutableHashSet<(int, int)> ExpectedUploads = new[]
        {
            (1, "first".GetHashCode()),
            (1, "second".GetHashCode()),
            (2, "third".GetHashCode())
        }.ToImmutableHashSet();

        IImageLookup ImageCache => new Images();

        Consent EmptyConsent => new Consent(default(Seller), string.Empty);

        Consent AnyConsent => It.IsAny<Consent>();

        static AuctionHandle handle(string lotId, int iId, int rId) =>
            new AuctionHandle(lotId, iId, rId);

        static BatchProcessResult result(Process p, AuctionHandle h) => 
            new BatchProcessResult(p, h);
        IEnumerable<BatchProcessResult> Expected = new []
        {
            result(Process.Add, handle("1", 0, 1)),
            result(Process.UploadImage, handle("1", 0, 1)),
            result(Process.UploadImage, handle("1", 0, 1)),
            result(Process.Commit, handle("1", 0, 1)),
            result(Process.Add, handle("2", 0, 2)),
            result(Process.Commit, handle("2", 0, 2))
        };

        [TestMethod]
        public void CreateBatch()
        {
            var s = new TestScheduler();
            var soap = SoapDependency(s);

            var a = s.LetRun(
                () => AuctionFeature.UploadBatch(
                    soap.Object,
                    ImageCache,
                    EmptyConsent,
                    Testcase));

            var r = a.GetValues();

            soap.Verify(
                x => x.AddLot(AnyConsent, It.IsAny<Lot>()),
                Times.Exactly(Testcase.Length));
            
            soap.Verify(
                x => x.AddImage(AnyConsent, It.IsAny<Image>(), It.IsAny<int>()),
                Times.Exactly(2)
            );
                
            Assert.IsFalse(a.Errored().Item1, $"Run errored: {a.Errored().Item2}");
            Assert.IsTrue(r.ToHashSet().SetEquals(Expected.ToHashSet()),
            $"\nExpected:\n {string.Join("\n", Expected.Select(x => x.ToString()))}" +
            $"\nActual:\n {string.Join("\n", r.Select(x => x.ToString()))}");
        }
    }
}