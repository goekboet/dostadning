using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using System.Reactive.Concurrency;
using dostadning.domain.seller;
using dostadning.domain;
using dostadning.domain.account;
using Sut = dostadning.domain.seller.SellerFuntions;

namespace unit
{
    [TestClass]
    public class GetTraderaConsentFeatureTest
    {
        static Exception TestException { get; } = new ApplicationException("");

        static string Alias = "Test";
        static Mock<IAuthorizationCalls> Soap(int? id)
        {
            var m = new Mock<IAuthorizationCalls>();
            m.Setup(x => x.Identify(It.Is<string>(a => a == Alias)))
                .Returns(id.HasValue
                    ? Observable.Return(id.Value)
                    : Observable.Throw<int>(TestException));

            return m;
        }

        
        Mock<IRepository<Account, Guid>> Repo(
            Account user,
            bool commits,
            IScheduler s)
        {
            var m = new Mock<IRepository<Account, Guid>>();
            m.Setup(x => x.Find(It.IsAny<Guid>()))
                .Returns(user != null
                    ? Observable.Return(user, s)
                    : Observable.Throw<Account>(TestException));

            m.Setup(x => x.Commit())
                .Returns(commits
                    ? Observable.Return(1, s)
                    : Observable.Throw<int>(TestException));

            return m;
        }

        static AppIdentity TestIdentity => new AppIdentity(0, "", "");

        static TraderaUser GetTraderaUser(Account a, int id, string alias) =>
            a.TraderaUsers.Single(x => x.Id == id && x.Alias == alias);

        [TestMethod]
        public void AddTraderaUser()
        {
            var s = new TestScheduler();
            var account = new Account { Id = AccountId };
            var repo = Repo(account, true, s);
            var soap = Soap(id);

            var a = s.LetRun(
                () => Sut.AddTraderaUser(repo.Object, soap.Object, TestIdentity, AccountId, Alias));

            var r = a.GetValues().Single();
            var recorded = GetTraderaUser(account, id, Alias);

            Assert.IsTrue(r.ConsentId == recorded.Consent.Id);
            repo.Verify(x => x.Commit(), Times.Once);
        }

        static Guid AccountId { get; } = Guid.NewGuid();
        static int id = 1;
        static Guid requestId = Guid.NewGuid();

        static Seller Seller => new Seller(AccountId, id);
        static Seller NoSeller => new Seller(AccountId, 0);

        static TraderaUser User => new TraderaUser
        {
            Alias = Alias,
            Id = id,
            Consent = new TraderaConsent { Id = requestId }
        };

        Account AccountRecord() => new Account
        {
            Id = AccountId,
            TraderaUsers = new[]
            {
                User
            }.ToList()
        };

        static TraderaConsent ConsentRecord(Account a) => a.TraderaUsers.Single().Consent;

        static TraderaConsent Expected = new TraderaConsent
        {
            Id = requestId,
            Token = "token",
            Expires = new DateTimeOffset(2018, 12, 24, 15, 0, 0, 0, TimeSpan.Zero)
        };

        static IAuthorizationCalls FetchCall(bool hasToken)
        {
            var m = new Mock<IAuthorizationCalls>();

            m.Setup(x => x.FetchToken(
                It.Is<int>(i => i == id),
                It.Is<string>(t => t == requestId.ToString())))
                .Returns(hasToken
                    ? Observable.Return(new Token(Expected.Token, Expected.Expires.Value))
                    : Observable.Throw<Token>(TestException));

            return m.Object;
        }

        Mock<IDataCommand<TraderaUser, Seller>> Sellers(
            TraderaUser user,
            bool commits,
            IScheduler s)
        {
            var m = new Mock<IDataCommand<TraderaUser, Seller>>();
            m.Setup(x => x.Find(It.IsAny<Seller>()))
                .Returns(user != null
                    ? Observable.Return(user, s)
                    : Observable.Throw<TraderaUser>(TestException));

            m.Setup(x => x.Commit())
                .Returns(commits
                    ? Observable.Return(1, s)
                    : Observable.Throw<int>(TestException));

            return m;
        }

        [TestMethod]
        public void SuccessfulFetchConsentShould()
        {
            var s = new TestScheduler();
            var account = AccountRecord();
            var repo = Sellers(account.TraderaUser(id), true, s);

            var a = s.LetRun(() => 
                Sut.FetchConsent(repo.Object, FetchCall(true), Seller));

            var r = a.GetValues().Single();
            var record = ConsentRecord(account);

            Assert.IsTrue(Expected.Token == record.Token && Expected.Expires == record.Expires);
            repo.Verify(x => x.Commit(), Times.Once);
        }
    }
}
