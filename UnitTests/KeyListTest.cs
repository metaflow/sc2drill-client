using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;
using Probe.Utility;
using System;

namespace sc2test
{
    
    
    /// <summary>
    ///This is a test class for KeyListTest and is intended
    ///to contain all KeyListTest Unit Tests
    ///</summary>
    [TestClass()]
    public class KeyListTest
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
        ///A test for Add
        ///</summary>
        [TestMethod()]
        public void AddTest()
        {
            var target = new KeyList();
            target.Add(Keys.A);
            Assert.AreEqual("A", target.ToString());
            target.Add(Keys.B);
            Assert.AreEqual("A + B", target.ToString());
        }


        [TestMethod]
        public void ContainsTest()
        {
            var l1 = new KeyList() {Keys.A, Keys.B};
            var l2 = new KeyList() {Keys.A};
            Assert.IsTrue(l1.Contains(l2));
            Assert.IsFalse(l2.Contains(l1));
            Assert.IsTrue(l1.FastContains(l2));
            Assert.IsFalse(l2.FastContains(l1));
            l1.Remove(Keys.A);
            Assert.IsFalse(l2.FastContains(l1));
            Assert.IsFalse(l1.FastContains(l2));
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod()]
        public void ToStringTest()
        {
            KeyList target = new KeyList();
            target.Add(Keys.LControlKey);
            target.Add(Keys.K);
            Assert.AreEqual("Ctrl + K", target.ToString());
            Assert.AreEqual("Shift + 1", new KeyList() {Keys.D1, Keys.Shift}.ToString());
            Assert.AreEqual("Shift + Scroll Lock", new KeyList() { Keys.Scroll, Keys.Shift }.ToString());
            Assert.AreEqual("Num 1", new KeyList() { Keys.NumPad1 }.ToString());
        }
    }
}
