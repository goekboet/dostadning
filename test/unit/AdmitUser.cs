using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dostadning.domain.features;
using dostadning.domain.ourdata;
using dostadning.domain.result;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

using Sut = dostadning.domain.features.AccountFeature;

namespace unit
{
    [TestClass]
    public class CreateAccountTest
    {
        static IRepository<Account, Guid> Repo(List<Account> users, int added)
        {
            var m = new Mock<IRepository<Account, Guid>>();
            m.Setup(x => x.Add(It.IsAny<Account>()))
                .Callback<Account>(x => users.Add(x));
            m.Setup(x => x.Commit()).Returns(Task.FromResult(new Either<int>(added)));
            return m.Object;
        }

        [TestMethod]
        public async Task CreateAccount()
        {
            var users = new List<Account>();
            var a = await Sut.Create(Repo(users, 1));

            Assert.IsFalse(a.IsError);
            Assert.IsTrue(
                users.Select(x => x.Id.ToString()).Single().Equals(a.Result), 
                $"e: {string.Join(",", users.Select(x => x.Id.ToString()))} a: {a.Result}");
        }

        [TestMethod]
        public async Task ErrorCondition()
        {
            var users = new List<Account>();
            var a = await Sut.Create(Repo(users, 0));

            Assert.IsTrue(a.IsError);
        }
    }
}
