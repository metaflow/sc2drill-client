using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Probe;

namespace sc2test
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class SandBoxTest
    {
        public SandBoxTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

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
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestCommunication()
        {
            var pipeServerName = Guid.NewGuid().ToString();
            var pipeClientName = Guid.NewGuid().ToString();

            var testMessage = "test message";
            var testData = "test data";

            var location = @"C:\Projects\castor\PluginsSandBox\bin\Debug\PluginsSandBox.exe";

            var info = new ProcessStartInfo(location);
            info.Arguments = string.Format("server:{0} client:{1}", pipeServerName, pipeClientName);

            var process = Process.Start(info);

            using (var pipeServer = new NamedPipeServerStream(pipeServerName))
            {
                pipeServer.WaitForConnection();

                using (var pipeClient = new NamedPipeClientStream(pipeClientName))
                {
                    pipeClient.Connect();
                    
                    var writer = new StreamWriter(pipeClient);

                    writer.WriteLine(testMessage);
                    writer.WriteLine(testData);
                    writer.Flush();

                    using (var reader = new StreamReader(pipeServer))
                    {
                        string recivedMessage = reader.ReadLine();
                        string recivedData = reader.ReadLine();

                        Assert.AreEqual(testMessage, recivedMessage);
                        Assert.AreEqual(testData, recivedData);
                    }

                    Thread.Sleep(5000);

                    writer.WriteLine("Exit");
                    writer.Flush();

                    Thread.Sleep(1000);

                    Assert.AreEqual(process.HasExited, true);
                }
            }
        }
    }
}
