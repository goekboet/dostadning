using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using dostadning.domain.auction;
using dostadning.domain.seller;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

using T = unit.TestabilityFunctions;
using Sut = dostadning.domain.auction.AuctionFeature;

namespace unit
{
    [TestClass]
    public class PollRequestStatusShould
    {
        static Consent C => new Consent(default(Seller), string.Empty);
        static (long t, BatchProcessResult)[] Adds = new (long, BatchProcessResult)[] 
        { 
            (120, r(Process.Add, 1)),
            (130, r(Process.UploadImage, 1)),
            (180, r(Process.Add, 2)),
            (190, r(Process.Commit, 1)),
            (200, r(Process.Commit, 1)), 
            (350, r(Process.Add, 3)),
            (510, r(Process.Commit, 1)), 
        };
        static IEnumerable<(long, Unit)> Cues = 
            new long[] { 100, 300, 500, 700, 800 }
            .Select(x => (x, Unit.Default));

        static IEnumerable<HashSet<int>> Expected = new [] 
        { 
            new HashSet<int>(new int[] {1, 2}), 
            new HashSet<int>(new int[] {1, 2, 3}), 
            new HashSet<int>(new int[] {1, 2, 3}),
            new HashSet<int>(new int[] {1, 2, 3})
        };

        static string Show(IEnumerable<IEnumerable<int>> sets) => 
            string.Join("\n", sets.Select(x => string.Join("-", x)));

        // static Status s(int rId, bool t) => 
        //     Status.Create(rId, t ? Status.Done : Status.Pending, "");

        
        // static (long, IEnumerable<Status>)[] Expected = new[]
        // {
        //     (110L, new Status[] {}.AsEnumerable()),
        //     (310L, new Status[] {s(1, false), s(2, false)}),
        //     (510L, new Status[] {s(1, true), s(2, true), s(3, true)}),
        //     (710L, new Status[] {s(1, true), s(2, true), s(3, true)})
        // };
        static BatchProcessResult r(Process p, int rId) =>
            new BatchProcessResult(p, new AuctionHandle("", 0, rId));

        static Mock<IAuctionProcedures> Soap(
            IScheduler s,
            List<IEnumerable<int>> args)
        {
            var m = new Mock<IAuctionProcedures>();

            m.Setup(x => x.GetResult(It.IsAny<Consent>(), It.IsAny<IEnumerable<int>>()))
                .Returns<Consent, IEnumerable<int>>((_, rs) => 
                {
                    args.Add(rs);

                    return Observable.Return(new Status[0], s);
                });

            return m;    
        }
        
        [TestMethod]
        public void PollRequeststatusOnQueShould()
        {
            var s = new TestScheduler();

            var cues = T.TestStreamHot(s, Cues);
            var adds = T.TestStreamHot(s, Adds);

            var res = new List<IEnumerable<int>>();
            var soap = Soap(s, res);

            var a = s.LetRun(() => Sut.PollRequestOnQue(soap.Object, C, cues, adds));

            Assert.IsTrue(
                Expected.Zip(res, (exp, rec) => exp.Count == rec.Count() && 
                                                exp.SetEquals(rec))
                .All(x => x),
                $"\nexpected:\n{Show(Expected)}\n" +
                $"actual:\n{Show(res)}\n\n");
        }
    }
}
