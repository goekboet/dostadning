using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using dostadning.domain;
using dostadning.domain.account;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

using Sut = dostadning.domain.account.AccountFeature;

namespace unit
{
    [TestClass]
    public class CreateAccountTest
    {
        static IRepository<Account, Guid> Repo(
            IScheduler s,
            List<Account> users, 
            int added)
        {
            var m = new Mock<IRepository<Account, Guid>>();
            m.Setup(x => x.Add(It.IsAny<Account>()))
                .Callback<Account>(x => users.Add(x))
                .Returns(m.Object);
            m.Setup(x => x.Commit()).Returns(Observable.Return(added, s));
            return m.Object;
        }

        [TestMethod]
        public void CreateAccount()
        {
            var s = new TestScheduler();
            var users = new List<Account>();

            var a = s.LetRun(() => Sut.Create(Repo(s, users, 1)));

            var output = a.GetValues().Single();
            var record = users.Select(x => x.Id.ToString()).Single();

            Assert.AreEqual(output, record);
        }
    }
}
