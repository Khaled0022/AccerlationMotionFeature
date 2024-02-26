using System.Numerics;
using TestProject;

namespace SimulatorInterface.Client.Zusatz.Tests
{
    [TestClass]
    public class ParabolicInterpolatorTests
    {
        static PrivateObject tester = default!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            ParabolicInterpolator x = new ParabolicInterpolator();
            tester = new PrivateObject(x);
        }

        [TestMethod]
        public void CalculateGeographicDistanceTest()
        {
            const string METHOD_NAME = "CalculateGeographicDistance";

            // Marburg
            // 50.80991796079788°, 8.768403657048028°
            // 0.88680036108413 rad, 0.1530375139594 rad
            Vector3 marburg = new Vector3(0.88680036108413f, 0.1530375139594f, 0f);
            // Frankfurt
            // 50.106562216961876°, 8.662206587038282°
            // 0.87452448754137 rad, 0.15118402543176 rad
            Vector3 frankfurt = new Vector3(0.87452448754137f, 0.15118402543176f, 0f);
            const int DISTANCE_METER = 78600;

            double res = (double)tester.InvokeStatic(METHOD_NAME, marburg, frankfurt)!;
            Assert.AreEqual(DISTANCE_METER, res, 50);
        }

        [TestMethod]
        public void CalculateMetersPerSecondTest()
        {
            const string METHOD_NAME = "CalculateMetersPerSecond";

            // Marburg
            // 50.80991796079788°, 8.768403657048028°
            // 0.88680036108413 rad, 0.1530375139594 rad
            Vector3 marburg = new Vector3(0.88680036108413f, 0.1530375139594f, 0f);
            // Frankfurt
            // 50.106562216961876°, 8.662206587038282°
            // 0.87452448754137 rad, 0.15118402543176 rad
            //Vector3 frankfurt = new Vector3(0.87452448754137f, 0.15118402543176f, 0f);
            // Von Marburg nach Frankfurt
            // -0.703355743836004°, -0.106197070009746
            // -0.0122758735427521 rad, -0.001853488527641 rad
            Vector3 diff = new Vector3(-0.0122758735427521f, -0.001853488527641f, 0f);
            const int DISTANCE_METER = 78600;

            double res = (double)tester.InvokeStatic(METHOD_NAME, marburg, diff)!;
            Assert.AreEqual(DISTANCE_METER, res, 50);
        }

        [TestMethod]
        public void SphereCenterPointTest()
        {
            const string METHOD_NAME = "SphereCenterPoint";

            // on a line with zero
            Vector3 res = (Vector3)tester.InvokeStatic(METHOD_NAME, new Vector3(0, 0, 0), new Vector3(1, 1, 1), new Vector3(2, 2, 2))!;
            Assert.AreEqual(new Vector3(1, 1, 1), res);

            // on a line without zero
            res = (Vector3)tester.InvokeStatic(METHOD_NAME, new Vector3(1, 1, 1), new Vector3(2, 2, 2), new Vector3(3, 3, 3))!;
            Assert.AreEqual(new Vector3(2, 2, 2), res);

            // circle
            res = (Vector3)tester.InvokeStatic(METHOD_NAME, new Vector3(-1, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 0, 0))!;
            Assert.AreEqual(new Vector3(0, 0, 0), res);
        }

        [TestMethod]
        public void CheckNearlyEqualTest()
        {
            const string METHOD_NAME = "CheckNearlyEqual";

            Vector3 a = Vector3.Zero;
            Vector3 b = Vector3.Zero;
            Assert.IsTrue((bool)tester.InvokeStatic(METHOD_NAME, a, b)!);

            a = Vector3.Zero;
            b = Vector3.One;
            Assert.IsFalse((bool)tester.InvokeStatic(METHOD_NAME, a, b)!);

            a = new Vector3(-0.2f, -0.2f, -0.2f);
            b = new Vector3(0.2f, 0.2f, 0.2f);
            Assert.IsTrue((bool)tester.InvokeStatic(METHOD_NAME, a, b)!);
        }

        [TestMethod]
        public void CalculateRollTest()
        {
            const string METHOD_NAME = "CalculateRoll";

            // Georg Gaßmann Stadion
            // 50.79762721811242°, 8.758394362970856°
            // 0.88658585 rad, 0.15286282 rad
            Vector3 stadion = new Vector3(0.88658585f, 0.15286282f, 0f);
            // ATU Marburg
            // 50.81608203357627°, 8.778022246992043°
            // 0.88690794 rad, 0.15320539 rad
            Vector3 atu = new Vector3(0.88690794f, 0.15320539f, 0f);
            // Botanischer Garten
            // 50.802611541914274°, 8.809169596347768°
            // 0.88667284 rad, 0.15374901 rad
            Vector3 garten = new Vector3(0.88667284f, 0.15374901f, 0f);
            // Geschwindigkeitsvektor
            // -1.858E-5 rad, -2.72E-6 rad
            // entspricht 100 m/s
            Vector3 v = new Vector3(-1.858E-5f, -2.72E-6f, 0f);
            // Radius: 1810 m

            float res = (float)tester.InvokeStatic(METHOD_NAME, stadion, atu, garten, 20)!;
            Assert.AreEqual(0.51291f, res, 0.1f);
        }
    }
}