using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLogic.Tests
{
    [TestClass()]
    public class BinaryCanStreamTests
    {
        [TestMethod()]
        public void ReadTest()
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(memStream, Encoding.UTF8, true))
                {
                    writer.Write((byte)Bus.canAero);
                    writer.Write((uint)315);
                    writer.Write((byte)1);
                    writer.Write((byte)42);
                }
                memStream.Seek(0, SeekOrigin.Begin);

                using (BinaryCanStream canStream = new BinaryCanStream(memStream))
                {
                    CanFrame frame = canStream.Read();
                    Assert.AreEqual(Bus.canAero, frame.Bus);
                    Assert.AreEqual((uint)315, frame.CanId);
                    CollectionAssert.AreEqual(new byte[] { 42 }, frame.Value);
                }
            }
        }

        [TestMethod()]
        public void WriteTest()
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                CanFrame frame = new CanFrame()
                {
                    Bus = Bus.canAero,
                    CanId = 315,
                    Value = new byte[] { 42 }
                };
                BinaryCanStream canStream = new BinaryCanStream(memStream);
                canStream.Write(frame);

                Assert.AreEqual((long)7, memStream.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                byte[] buffer = new byte[7];
                memStream.Read(buffer, 0, 7);
                CollectionAssert.AreEqual(new byte[] { (byte)Bus.canAero, 0x3B, 0x01, 0x00, 0x00, 1, 42 }, buffer);
            }
        }
    }
}