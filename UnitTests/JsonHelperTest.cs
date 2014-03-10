using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace sc2test
{
    
    
    /// <summary>
    ///This is a test class for JsonHelperTest and is intended
    ///to contain all JsonHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class JsonHelperTest
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
        ///A test for GetParametersJson
        ///</summary>
        [TestMethod()]
        public void GetJsonTest()
        {
            JObject j = new JObject();
            j.Add("k", "val");
            var expected = "{\"k\":\"val\"}";
            Assert.AreEqual(expected, j.ToString(Formatting.None));
        }

        [TestMethod()]
        public void GetJsonArrayTest()
        {
            JObject j = new JObject();
            j.Add("k", new JArray(new object[] {"a"}));
            var expected = "{\"k\":[\"a\"]}";
            Assert.AreEqual(expected, j.ToString(Formatting.None));
            j = new JObject {{"k", new JArray(new object[] {new JObject() {{"a", 1}}})}};
            expected = "{\"k\":[{\"a\":1}]}";
            Assert.AreEqual(expected, j.ToString(Formatting.None));
        }

        /// <summary>
        ///A test for ParseJson
        ///</summary>
        [TestMethod()]
        public void ParseJsonTest()
        {
            string json = "{\"k\":\"val\"}";
            JObject j = JObject.Parse(json);
            Assert.AreEqual("val", j["k"]);
        }

        [TestMethod]
        public void ParseSimpleJsonArray()
        {
            string json = "{'a' : [2,3]}";
            JObject j = JObject.Parse(json);
            Assert.IsTrue(j["a"].Type == JTokenType.Array);
            IEnumerator<JToken> e = j["a"].AsJEnumerable().GetEnumerator();
            e.MoveNext();
            Assert.AreEqual(Convert.ToInt64(2), e.Current);
            Assert.AreEqual(Convert.ToInt64(2), j["a"][0]);
            Assert.AreEqual(Convert.ToInt64(3), j["a"][1]);
        }

        [TestMethod]
        public void ParseObjectsJsonArray()
        {
            string json = "{'root' : [{'foo' : 1},{'bar' : 2}]}";
            JObject j = JObject.Parse(json);
            Assert.IsTrue(j["root"].Type == JTokenType.Array);
            IEnumerator<JToken> e = j["root"].AsJEnumerable().GetEnumerator();
            e.MoveNext();
            Assert.AreEqual(Convert.ToInt64(1), e.Current["foo"].Value<Int64>());
            Assert.AreEqual(Convert.ToInt64(1), j["root"][0]["foo"]);
        }

        [TestMethod]
        public void SelectTokenTest()
        {
            string json = "{'root' : {'foo' : 1,'bar' : 2}}";
            JObject j = JObject.Parse(json);
            Assert.AreEqual("1", j.SelectToken("root.foo").ToString());
        }
    }
}
