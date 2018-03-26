using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dostadning.domain.service.tradera;
using dostadning.domain.features;
using dostadning.domain.ourdata;
using dostadning.domain.result;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

using Sut = dostadning.domain.features.GetTraderaConsentFeature;
using Dto = dostadning.domain.features;
using Our = dostadning.domain.ourdata;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using System.Reactive.Concurrency;

namespace unit
{
    [TestClass]
    public class GetTraderaConsentFeatureTest
    {
        static Exception TestException { get; } = new ApplicationException("");

        static string Alias = "Test";
        static string NotAlias = "NotAlias";
        static Mock<IAuthorizationCalls> Soap(int? id)
        {
            var m = new Mock<IAuthorizationCalls>();
            m.Setup(x => x.Identify(It.Is<string>(a => a == Alias)))
                .Returns(id.HasValue
                    ? Observable.Return(id.Value)
                    : Observable.Throw<int>(TestException));

            return m;
        }

        static Guid AccountId { get; } = Guid.NewGuid();
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
                () => Sut.AddTraderaUser(repo.Object, soap.Object, AccountId.ToString(), Alias));

            var r = a.GetValues().Single();
            var recorded = GetTraderaUser(account, id, Alias);

            Assert.AreEqual(recorded.Consent.Id.ToString(), r);
            repo.Verify(x => x.Commit(), Times.Once);
        }

        static int id = 1;
        static Guid requestId = Guid.NewGuid();

        Account AccountRecord() => new Account
        {
            Id = AccountId,
            TraderaUsers = new[]
            {
                new TraderaUser
                {
                    Alias = Alias,
                    Id = id,
                    Consent = new TraderaConsent { Id = requestId }
                }
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

        [TestMethod]
        public void SuccessfulFetchConsentShould()
        {
            var s = new TestScheduler();
            var account = AccountRecord();
            var repo = Repo(account, true, s);

            var a = s.LetRun(() => 
                Sut.FetchConsent(repo.Object, FetchCall(true), AccountId.ToString(), Alias));

            var r = a.GetValues().Single();
            var record = ConsentRecord(account);

            Assert.IsTrue(Expected.Token == record.Token && Expected.Expires == record.Expires);
            repo.Verify(x => x.Commit(), Times.Once);
        }

        [TestMethod]
        public void ShouldThrowOnAliasNotFoundOnAccount()
        {
            var s = new TestScheduler();
            var soap = new Mock<IAuthorizationCalls>();
            var account = AccountRecord();
            var repo = Repo(account, true, s).Object;

            var a = s.LetRun(() => 
                Sut.FetchConsent(repo, soap.Object, AccountId.ToString(), NotAlias));

            (bool errored, Exception e) = a.Errored();

            Assert.IsTrue(errored && e is Error, 
                $"e: true and Error a: {errored} {e?.GetType().ToString()}");
            soap.Verify(x => x.FetchToken(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }
    }
}
