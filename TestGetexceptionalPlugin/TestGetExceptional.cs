using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GetexceptionalPlugin;

namespace TestGetexceptionalPlugin
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class TestGetExceptional
    {
        private const string API_KEY = "aebdf951ce97b0535a3659f8c0c3934424d8b9e5";

        private GetExceptional exSender;

        public TestGetExceptional()
        {

            //
            // TODO: Add constructor logic here
            //
            exSender = new GetExceptional(API_KEY);
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
        public void SendException()
        {
            try
            {
                throw new Exception("This is a test exception!!!!");
            }
            catch (Exception ex)
            {
                exSender.ReportException(ex, false);
            }
        }
    }
}
