using System;
using NUnit.Framework;
using ActivitySampling;
using System.Linq;
using System.IO;

namespace ActivitySampling.Tests
{
    [TestFixture()]
    public class Test_ActivityLog
    {
        [SetUp]
        public void Setup() {
            Environment.CurrentDirectory = TestContext.CurrentContext.WorkDirectory;  
        }

        [Test()]
        public void Usecase()
        {
            File.Delete("./ActivityLog.csv");

            var sut = new ActivityLog(".");

            var a = new ActivityDto { Timestamp = new DateTime(2017,8,3,10,25,00), Description = "d1"};
            sut.Append(a);
            a = new ActivityDto { Timestamp = new DateTime(2017, 8, 3, 10, 26, 00), Description = "d2" };
            sut.Append(a);
            var activities = sut.Activities.ToArray();

            Assert.AreEqual(2, activities.Length);
            Assert.AreEqual("d1", activities[0].Description);
            Assert.AreEqual(new DateTime(2017, 8, 3, 10, 25, 00), activities[0].Timestamp);
            Assert.AreEqual("d2", activities[1].Description);
        }
    }
}
