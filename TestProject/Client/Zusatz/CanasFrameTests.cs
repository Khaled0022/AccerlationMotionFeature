using CommonLogic;
using SimulatorInterface.datamodel;

namespace SimulatorInterface.Client.Zusatz.Tests
{
    [TestClass]
    public class CanasFrameTests
    {
        [TestMethod]
        public void ToCanFrameTest()
        {
            var ex = Assert.ThrowsException<InvalidOperationException>(() =>
            {
                CanasFrame asframe = new CanasFrame()
                {
                    Bus = Bus.canAero,
                    CanId = 315,
                    NodeId = 1,
                    DataType = CanasDatatype.FLOAT,
                    ServiceCode = 0,
                    MessageCode = 0,
                    Value = new byte[0]
                };
                asframe.ToCanFrame();
            });
            StringAssert.Contains(ex.Message, "length of the value does not match");

            CanFrame frame = new CanasFrame()
            {
                Bus = Bus.canAero,
                CanId = 315,
                NodeId = 1,
                DataType = CanasDatatype.FLOAT,
                ServiceCode = 0,
                MessageCode = 0,
                Value = NetworkBitConverter.GetBytes(42f)
            }.ToCanFrame();
            Assert.AreEqual(Bus.canAero, frame.Bus);
            Assert.AreEqual((uint)315, frame.CanId);
            CollectionAssert.AreEqual(new byte[] { 1, 2, 0, 0, 0x42, 0x28, 0x00, 0x00 }, frame.Value);
        }

        [TestMethod]
        public void FromCanFrameTest()
        {
            CanasFrame asframe = CanasFrame.FromCanFrame(new CanFrame()
            {
                Bus = Bus.canAero,
                CanId = 315,
                Value = new byte[] { 1, 2, 0, 0, 0x42, 0x28, 0x00, 0x00 }
            });
            Assert.AreEqual(Bus.canAero, asframe.Bus);
            Assert.AreEqual((uint)315, asframe.CanId);
            Assert.AreEqual((byte)1, asframe.NodeId);
            Assert.AreEqual(CanasDatatype.FLOAT, asframe.DataType);
            Assert.AreEqual((byte)0, asframe.ServiceCode);
            Assert.AreEqual((byte)0, asframe.MessageCode);
            CollectionAssert.AreEqual(new byte[] { 0x42, 0x28, 0x00, 0x00 }, asframe.Value.ToArray());
        }

        [TestMethod]
        public void FromSimvarTest()
        {
            Simvar simvar = new Simvar()
            {
                sName = "Test",
                eBusId = Bus.canAero,
                iCanId = 315,
                dValue = 42,
                LastValueSendToServer = 42,
                sUnits = "Test",
                bPending = true,
                bStillPending = false,
                eTransmissionSlot = TransmissionSlot.D
            };
            CanasFrame asframe = CanasFrame.FromSimvar(simvar);
            Assert.AreEqual(Bus.canAero, asframe.Bus);
            Assert.AreEqual((uint)315, asframe.CanId);
            Assert.AreEqual((byte)1, asframe.NodeId);
            Assert.AreEqual(CanasDatatype.FLOAT, asframe.DataType);
            Assert.AreEqual((byte)0, asframe.ServiceCode);
            Assert.AreEqual((byte)0, asframe.MessageCode);
            CollectionAssert.AreEqual(new byte[] { 0x42, 0x28, 0x00, 0x00 }, asframe.Value.ToArray());

            CanasFrame asframe2 = CanasFrame.FromSimvar(simvar);
            Assert.AreEqual((byte)1, asframe2.MessageCode);
        }
    }
}
