using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SwiftBinaryProtocol;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] buffer = encoding.GetBytes("");

            ushort result = Crc16CcittKermit.ComputeChecksum(buffer);
            Assert.AreEqual((ushort)0, result);
        }

        [TestMethod]
        public void TestMethod2()
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] buffer = encoding.GetBytes("123456789");

            ushort result = Crc16CcittKermit.ComputeChecksum(buffer);
            Assert.AreEqual((ushort)0x31C3, result);
        }

    }
}
