using Microsoft.VisualStudio.TestTools.UnitTesting;
using BenchmarkDotNet.Running;

namespace MPP_STM
{
    [TestClass]
    public class BenchmarkTest
    {       
        [TestMethod]
        public void RunTestMainClass()
        {
            BenchmarkRunner.Run<Program>();
        }
    }
}

