using System.IO;
using Probe.Replay;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace sc2test
{


    /// <summary>
    ///This is a test class for ReplayParserTest and is intended
    ///to contain all ReplayParserTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ReplayParserTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for Load
        ///</summary>
        [TestMethod()]
        public void Replay2v2()
        {
            var tempPath = Path.ChangeExtension(Path.GetTempFileName(), "sc2replay");
            File.WriteAllBytes(tempPath, Resources._2v2_alodia_daddynick_hydrokat_ifry);
            var info = ReplayParser.Parse(tempPath);
            var t = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(info.GameCTime);
            Assert.AreEqual(new DateTime(2010, 09, 25, 14, 49, 02, 0), t);
            Assert.AreEqual("beee03242eabec78e645b33a5fe15cb9", info.Hash);
            Assert.AreEqual((UInt32)2153500, info.ProfileNumber);
            File.Delete(tempPath);
        }

        [TestMethod()]
        public void WithComputer()
        {
            var tempPath = Path.ChangeExtension(Path.GetTempFileName(), "sc2replay");
            File.WriteAllBytes(tempPath, Resources.meta_1);
            var info = ReplayParser.Parse(tempPath);
            Assert.AreEqual("d6f95f6e4dc7382f3d7a7c90c969f14c", info.Hash);
            var t = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(info.GameCTime);
            Assert.AreEqual(new DateTime(2011, 6, 19, 8, 42, 0, 0), t);
            Assert.AreEqual((UInt32)1860931, info.ProfileNumber);
            File.Delete(tempPath);
        }

        [TestMethod()]
        public void BadReplay()
        {
            var tempPath = Path.ChangeExtension(Path.GetTempFileName(), "sc2replay");
            File.WriteAllBytes(tempPath, Resources.bad);
            var info = ReplayParser.Parse(tempPath);
            File.Delete(tempPath);
        }
    }
}
