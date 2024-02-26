using System.Diagnostics;
using System.Numerics;
using System.Runtime.ExceptionServices;
using static System.Math;

namespace SimulatorInterface.Client.Zusatz
{
    public class ParabolicInterpolator : Interpolator
    {
        public override int WindowSize
        {
            get { return 3; }
        }

        protected override PlanePositionAndRotation ExtrapolateIntern(long currentTime)
        {
            (long timestamp, Vector3 pos) p0 = _knownData[0];
            (long timestamp, Vector3 pos) p1 = _knownData[1];
            (long timestamp, Vector3 pos) p2 = _knownData[2];

            float t1 = p0.timestamp - p1.timestamp;
            float t2 = p0.timestamp - p2.timestamp;

            float divisor = (t1 * t2) * (t2 - t1);

            Vector3 a = (-p1.pos * t2 + p0.pos * t2 + p2.pos * t1 - p0.pos * t1) / divisor;
            Vector3 b = (-p1.pos * t2 * t2 + p0.pos * t2 * t2 + p2.pos * t1 * t1 - p0.pos * t1 * t1) / divisor;

            float t = currentTime - p0.timestamp;
            Vector3 velocity = 2 * a * t + b;
            Vector3 acceleration = 2 * a;

            Vector3 pos2 = a * t * t + b * t + p0.pos;
            Vector3 pos1 = pos2 - velocity;

            return new PlanePositionAndRotation()
            {
                Position = pos2,

                Rotation = new Vector3(
                    InitialBearing(pos2, velocity),
                    //(float)Atan2(differential.Y, differential.X), //not so accurate
                    velocity.Z * (float)PI / 2,
                    CalculateRoll(pos2, p0.pos, p1.pos, velocity * Stopwatch.Frequency)
                )
            };
        }

        /*private static float InitialBearing(Vector3 pos1, Vector3 pos2)
        {
            double y = Sin(pos2.Y - pos1.Y) * Cos(pos2.X);
            double x = Cos(pos1.X) * Sin(pos2.X) - Sin(pos1.X) * Cos(pos2.X) * Cos(pos2.Y - pos1.Y);
            return (float) Atan2(y, x);
        }*/

        //pos1=pos2-dir
        /*private static float InitialBearing(Vector3 pos, Vector3 dir)
        {
            double y = Sin(dir.Y) * Cos(pos.X);
            double x = Cos(pos.X - dir.X) * Sin(pos.X) - Sin(pos.X - dir.X) * Cos(pos.X) * Cos(dir.Y);
            return (float)Atan2(y, x);
        }*/

        private static float InitialBearing(Vector3 pos, Vector3 dir)
        {
            double y = Sin(dir.Y) * Cos(pos.X);
            double x = (Cos(pos.X) * Cos(dir.X) + Sin(pos.X) * Sin(dir.X)) * Sin(pos.X) - (Sin(pos.X) * Cos(dir.X) - Cos(pos.X) * Sin(dir.X)) * Cos(pos.X) * Cos(dir.Y);
            return (float)Atan2(y, x);
        }

        private static float CalculateRoll(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 v)
        {
            const double g = 9.81;
            Vector3 center = PerpendicularBisectorCutPoint(p1, p2, p3, out bool isColinear);
            if (isColinear)
            {
                return 0f;
            }

            double radius = CalculateGeographicDistance(p1, center);
            double ms = CalculateMetersPerSecond(p1, v);
            //tan phi = tas^2 / (turn radius * g)
            float tmp = (float)Atan(ms * ms / (radius * g));
            Console.WriteLine(ms + "m/s\t" + radius + "m\t" + tmp + "rad\t" + RadToDeg(center) + "\t" + RadToDeg(p1) + "\t" + RadToDeg(p2) + "\t" + RadToDeg(p3));
            return tmp;

            /*float rad = (float)(Atan2(acc.Y, acc.X));
            float deg = (float)(rad * 180f / PI);
            Console.WriteLine("Radians: " + rad);
            Console.WriteLine("Degrees: " + deg);
            return rad;*/
        }

        private static Vector3 RadToDeg(Vector3 rad)
        {
            return new Vector3 { X = (float)(rad.X * 180f / PI), Y = (float)(rad.Y * 180f / PI), Z = rad.Z };
        }

        private static double CalculateGeographicDistance(Vector3 a, Vector3 b)
        {
            const int DIAMETER_EARTH_METER = 12_742_000;
            double distXY = DIAMETER_EARTH_METER * Asin(Sqrt(0.5 * (1 - Cos(a.X - b.X) + Cos(b.X) * Cos(a.X) * (1 - Cos(a.Y - b.Y)))));
            double distZ = b.Z - a.Z;
            double tmp = Sqrt(distXY * distXY + distZ * distZ);
            return tmp;
        }

        /*private static double CalculateGeographicDistance(Vector3 a, Vector3 b)
        {
            const int RADIUS_EARTH_METER = 6_371_000;
            double phi1 = a.X;
            double phi2 = b.X;
            double lambda1 = a.Y;
            double lambda2 = b.Y;
            double deltaPhi = phi2 - phi1;
            double deltaLambda = lambda2 - lambda1;

            double x = Sin(deltaPhi/2) * Sin(deltaPhi/2) + Cos(phi1) * Cos(phi2) * Sin(deltaLambda/2) * Sin(deltaLambda/2);
            double y = 2 * Atan2(Sqrt(x), Sqrt(1 - x));
            double d = RADIUS_EARTH_METER * y;
            return d;

            
            const R = radius;
            const φ1 = this.lat.toRadians(),  λ1 = this.lon.toRadians();
            const φ2 = point.lat.toRadians(), λ2 = point.lon.toRadians();
            const Δφ = φ2 - φ1;
            const Δλ = λ2 - λ1;

            const a = Math.sin(Δφ / 2) * Math.sin(Δφ / 2) + Math.cos(φ1) * Math.cos(φ2) * Math.sin(Δλ / 2) * Math.sin(Δλ / 2);
            const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
            const d = R * c;
        }*/

        private static double CalculateMetersPerSecond(Vector3 pos, Vector3 v)
        {
            const int DIAMETER_EARTH_METER = 12_742_000;
            //double distXY = DIAMETER_EARTH_METER * Asin(Sqrt(0.5 * (1 - Cos(v.X) + Cos(pos.X) * Cos(pos.X + v.X) * (1 - Cos(v.Y)))));
            double distXY = DIAMETER_EARTH_METER * Asin(Sqrt(0.5 * (1 - Cos(v.X) + Cos(pos.X) * (Cos(pos.X) * Cos(v.X) + Sin(pos.X) * Sin(v.X)) * (1 - Cos(v.Y)))));
            double distZ = v.Z;
            return Sqrt(distXY * distXY + distZ * distZ);
        }

        /*private static double CalculateMetersPerSecond(Vector3 current, Vector3 past, double time)
        {
            double dist = CalculateGeographicDistance(current, past);
            return dist / time;
        }*/

        private static Vector3 SphereCenterPoint(Vector3 p1, Vector3 p2, Vector3 p3, out bool isColinear)
        {
            Vector3 a = p3 - p2;
            Vector3 b = p1 - p3;
            Vector3 c = p2 - p1;

            if (CheckColinear(p1, p2, p3))
            {
                isColinear = true;
                return Center(p1, p2, p3);
            }

            if (a == Vector3.Zero || b == Vector3.Zero || c == Vector3.Zero)
            {
                isColinear = true;
                return (p1 + p2 + p3) / 3;
            }

            float u = Vector3.Dot(a, a) * Vector3.Dot(c, b);
            float v = Vector3.Dot(b, b) * Vector3.Dot(c, a);
            float w = Vector3.Dot(c, c) * Vector3.Dot(b, a);

            Vector3 tmp = u * p1 + v * p2 + w * p3;
            tmp.X /= u + v + w;
            tmp.Y /= u + v + w;
            tmp.Z /= u + v + w;

            if (float.IsNaN(tmp.X) || float.IsNaN(tmp.Y) || float.IsNaN(tmp.Z) ||
                float.IsInfinity(tmp.X) || float.IsInfinity(tmp.Y) || float.IsInfinity(tmp.Z))
            {
                isColinear = true;
                return (p1 + p2 + p3) / 3;
            }
            isColinear = false;
            return tmp;
        }

        private static Vector3 GeographicToPolar(Vector3 a)
        {
            return new Vector3 { X = (float)(Cos(a.X) * Cos(a.Y)), Y = (float)(Cos(a.X) * Sin(a.Y)), Z = (float)Sin(a.X) };
        }

        private static Vector3 Center(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 polarCenter = (GeographicToPolar(a) + GeographicToPolar(b) + GeographicToPolar(c)) / 3;
            return new Vector3 { X = (float)Atan2(polarCenter.Z, Sqrt(polarCenter.X * polarCenter.X + polarCenter.Y * polarCenter.Y)), 
                Y = (float)Atan2(polarCenter.Y, polarCenter.X), 
                Z = (a.Z + b.Z + c.Z) / 3 };
        }

        private static bool CheckColinear(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            const float MAX = 1E-6f;
            Vector3 v1 = p1 - p2;
            Vector3 v2 = p3 - p2;
            Vector3 cross = Vector3.Cross(v1, v2);
            return Abs(cross.X) < MAX && Abs(cross.Y) < MAX && Abs(cross.Z) < MAX;
        }

        private static Vector3 Midpoint(Vector3 a, Vector3 b)
        {
            return (a + b) / 2;
        }

        private static float Slope(Vector3 a, Vector3 b)
        {
            if (b.X == a.X)
            {
                return float.PositiveInfinity;
            }
            return (b.Y - a.Y) / (b.X - a.X);
        }

        private static float Inverse(float slope)
        {
            if (slope == 0)
            {
                return float.PositiveInfinity;
            }
            if (float.IsInfinity(slope))
            {
                return 0;
            }
            return -(1 / slope);
        }

        private static float YIntercept(Vector3 point, float slope)
        {
            return point.Y - point.X * slope;
        }

        private static Vector3 CutPoint(float m1, float b1, float m2, float b2, float altitude)
        {
            float x = (b1 - b2) / (m2 - m1);
            float y = m1 * x + b1;
            return new Vector3(x, y, altitude);
        }


        private static Vector3 PerpendicularBisectorCutPoint(Vector3 a, Vector3 b, Vector3 c, out bool isColinear)
        {
            Vector3 midpointAB = Midpoint(a, b);
            float m1 = Inverse(Slope(a, b));
            float b1 = YIntercept(midpointAB, m1);

            Vector3 midpointBC = Midpoint(b, c);
            float m2 = Inverse(Slope(b, c));
            float b2 = YIntercept(midpointBC, m2);

            isColinear = Math.Abs(m2 - m1) <= 0.05;

            if (float.IsInfinity(m1))
            {
                return new Vector3(midpointAB.X, m2 * midpointAB.X + b2, 0f);
            }
            if (float.IsInfinity(m2))
            {
                return new Vector3(midpointBC.X, m1 * midpointBC.X + b1, 0f);
            }
            return CutPoint(m1, b1, m2, b2, (a.Z + b.Z + c.Z) / 3);
        }
    }

}
