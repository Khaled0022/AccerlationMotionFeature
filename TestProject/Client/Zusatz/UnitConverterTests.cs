using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimulatorInterface.Client.Zusatz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulatorInterface.Client.Zusatz.Tests
{
    [TestClass()]
    public class UnitConverterTests
    {
        [TestMethod()]
        public void KnotsToMetersPerSecondTest()
        {
            Assert.AreEqual(5.14444444f, UnitConverter.KnotsToMetersPerSecond(10), 0.01f);
        }

        [TestMethod()]
        public void FeetPerSecondToMetersPerSecondTest()
        {
            Assert.AreEqual(3.048f, UnitConverter.FeetPerSecondToMetersPerSecond(10), 0.01f);
        }

        [TestMethod()]
        public void RadiansToDegreesTest()
        {
            Assert.AreEqual(11.4591559f, UnitConverter.RadiansToDegrees(0.2f), 0.01f);
        }
    }
}