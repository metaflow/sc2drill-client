using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Probe.Utility.ScreenCapture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace sc2test
{
    
    
    /// <summary>
    ///This is a test class for ImageProcessorTest and is intended
    ///to contain all ImageProcessorTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ImageProcessorTest
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
        ///A test for RecognizeLetter
        ///</summary>
        [TestMethod()]
        public void RecognizeLetterTest()
        {
            var b = ImageProcessor.ThresholdGrays(200, 40, Resources._129552993571305435);
            var letters = ImageProcessor.GetLetterMasks(50, b);
            Assert.AreEqual(10, letters.Count);
            var testChars = new char[10] {'1', '7', '5', '2', '6', '8', '5', '5', '6', '8'};
            for (int i = 0; i < testChars.Length; i++)
            {
                AssertLetter(testChars[i], ImageProcessor.DebugRecognizeLetter(letters[i]));    
            }
        }

        private void AssertLetter(char expected, Dictionary<char, KeyValuePair<string, int>> data)
        {
            Assert.IsTrue(data[expected].Value > 8);
            foreach (var pair in data)
            {
                if (pair.Key == expected) continue;
                Assert.IsTrue(pair.Value.Value < 8, "expected char '{0}', char '{1}' has score of {2}", expected, pair.Key, pair.Value.Value);
            }
        }

        [TestMethod()]
        public void RecognizeVerticalsTest()
        {
            var b = ImageProcessor.DetectVerticalLinesColor(Resources._129552993571305435);
            Assert.AreEqual(Color.FromArgb(206, 118, 59).GetHashCode(), b.GetHashCode());
        }
    }
}
