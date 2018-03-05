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

namespace unit
{
    sealed class Input
    {
        public (Account user, bool commits) OnOurRecords { get; set; }
        public int? OnTraderaRecords { get; set; }
    }

    sealed class Output
    {
        public Either<string> Result { get; set; }
        public int CalledTradera { get; set; }
    }

    sealed class TestCase
    {
        public Input Input { get; set; }
        public Output Output { get; set; }
    }

    

    [TestClass]
    public class GetTraderaConsentFeatureTest
    {
        static Error TestError => new DomainError("test");
        static Mock<IGetTraderaConsentCalls> Soap(int? id)
        {
            var m = new Mock<IGetTraderaConsentCalls>();
            m.Setup(x => x.Identify(It.IsAny<string>()))
                .Returns(Task.FromResult(id.HasValue
                    ? new Either<int>(id.Value)
                    : new Either<int>(TestError)));

            return m;
        }

        Mock<IRepository<Account, Guid>> Repo(Account user, bool commits)
        {
            var m = new Mock<IRepository<Account, Guid>>();
            m.Setup(x => x.Find(It.IsAny<Guid>()))
                .Returns(Task.FromResult(user != null
                    ? new Either<Account>(user)
                    : new Either<Account>(TestError)));
            
            m.Setup(x => x.Commit())
                .Callback(() => 
                { 
                    if (user != null)
                        ExpectedId = user.TraderaUsers.Select(x => x.Consent.Id).Single(); 
                })
                .Returns(Task.FromResult(commits
                    ? new Either<int>(1)
                    : new Either<int>(TestError)));

            return m;
        }
        static string Alias = "Test";
        static Guid AppuserId { get; } = Guid.NewGuid();
        static Account AppUser { get; } = new Account { Id = AppuserId };
        static Dictionary<string, TestCase> TestCases = new Dictionary<string, TestCase>
        {
            ["HandleNotOnRecord"] = new TestCase
            {
                Input = new Input
                {
                    OnOurRecords = (null, false),
                    OnTraderaRecords = null,
                },
                Output = new Output
                {
                    Result = new Either<string>(TestError),
                    CalledTradera = 0
                }
            },
            ["AlreadyAdded"] = new TestCase
            {
                Input = new Input
                {
                    OnOurRecords = (new Account 
                    { 
                        Id = AppuserId,
                        TraderaUsers = new [] { new TraderaUser { Alias = Alias }}.ToList() 
                    }, false),
                    OnTraderaRecords = 1,
                },
                Output = new Output
                {
                    Result = new Either<string>(TestError),
                    CalledTradera = 0
                }
            },
            ["NotOnTraderaRecord"] = new TestCase
            {
                Input = new Input
                {
                    OnOurRecords = (new Account 
                    { 
                        Id = AppuserId 
                    }, true),
                    OnTraderaRecords = null,
                },
                Output = new Output
                {
                    Result = new Either<string>(TestError),
                    CalledTradera = 1
                }
            },
            ["OnTraderaRecord"] = new TestCase
            {
                Input = new Input
                {
                    OnOurRecords = (new Account
                    {
                        Id = AppuserId
                    }, true),
                    OnTraderaRecords = 1
                },
                Output = new Output
                {
                    Result = new Either<string>(""),
                    CalledTradera = 1
                }
            },
            ["CannotSave"] = new TestCase
            {
                Input = new Input
                {
                    OnOurRecords = (new Account
                    {
                        Id = AppuserId
                    }, false),
                    OnTraderaRecords = 1
                },
                Output = new Output
                {
                    Result = new Either<string>(TestError),
                    CalledTradera = 1
                }
            }
        };

        Guid ExpectedId {get;set;}

        [DataRow("HandleNotOnRecord")]
        [DataRow("AlreadyAdded")]
        [DataRow("NotOnTraderaRecord")]
        [DataRow("OnTraderaRecord")]
        [DataRow("CannotSave")]
        [TestMethod]
        public async Task AddTraderaUser(string t)
        {
            var c = TestCases[t];

            var our = c.Input.OnOurRecords;
            var tradera = c.Input.OnTraderaRecords;
            var soap = Soap(tradera);
            var a = await Sut.AddTraderaUser(
                users: Repo(our.user, our.commits).Object,
                soap: soap.Object,
                claim: AppuserId.ToString(),
                alias: Alias);

            if (c.Output.Result.IsError)
            {
                Assert.IsTrue(a.IsError);
                soap.Verify(
                    x => x.Identify(It.Is<string>(y => y == Alias)),
                    Times.Exactly(c.Output.CalledTradera));
            }
            else
            {
                Assert.IsFalse(a.IsError);
                Assert.IsTrue(ExpectedId.ToString().Equals(a.Result), $"e: {c.Output.Result.Result} a: {a.Result}");
                soap.Verify(
                    x => x.Identify(It.Is<string>(y => y == Alias)),
                    Times.Exactly(c.Output.CalledTradera));
            }
        }

        static string alias = "test";
        static int id = 1;
        static Guid requestId = Guid.NewGuid();
        static Guid account = Guid.NewGuid();
        Account OnRecord = new Account
        {
            Id = account,
            TraderaUsers = new [] 
            {
                new TraderaUser
                {
                    Alias = alias,
                    Id = id,
                    Consent = new TraderaConsent { Id = requestId }
                }
            }.ToList()
        };

        

        static TraderaConsent Expected = new TraderaConsent
        {
            Id = requestId,
            Token = "token",
            Expires = new DateTimeOffset(2018, 12, 24, 15, 0, 0, 0, TimeSpan.Zero)
        };

        static IGetTraderaConsentCalls FetchCall(bool hasToken)
        {
            var m = new Mock<IGetTraderaConsentCalls>();

            m.Setup(x => x.FetchToken(
                It.Is<int>(i => i == id), 
                It.Is<string>(t => t == requestId.ToString())))
                .Returns(hasToken
                    ? Task.FromResult(new Either<(string, DateTimeOffset)>((Expected.Token, Expected.Expires.Value)))
                    : Task.FromResult(new Either<(string, DateTimeOffset)>(TestError)));

            return m.Object;    
        }

        [TestMethod]
        public async Task SuccessfulFetchConsentShould()
        {
            var repo = Repo(OnRecord, true);
            var a = await Sut.FetchConsent(repo.Object, FetchCall(true), account.ToString(), alias);

            Assert.IsFalse(a.IsError, $"Not expecting error {a.Error?.Code}");
            Assert.AreEqual(a.Result, Expected.Expires, $"e: {Expected.Expires} a: {a.Result}");

            var update = OnRecord.ConsentForAlias(alias);
            Assert.IsTrue(update.Token == Expected.Token && update.Expires == Expected.Expires);
            repo.Verify(x => x.Commit(), Times.Once);
        }

        [TestMethod]
        public async Task FailedFetchConsentShould()
        {
            var repo = Repo(OnRecord, true);
            var a = await Sut.FetchConsent(repo.Object, FetchCall(false), account.ToString(), alias);

            Assert.IsTrue(a.IsError, $"Expected error");
            repo.Verify(x => x.Commit(), Times.Never);
        }
    }
}
