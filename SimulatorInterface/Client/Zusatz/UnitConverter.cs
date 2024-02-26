using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulatorInterface.Client.Zusatz
{
    public class UnitConverter
    {
        public static float KnotsToMetersPerSecond(float knots)
        {
            return knots * 0.514444444f;
        }

        public static float FeetPerSecondToMetersPerSecond(float fps)
        {
            return fps * 0.3048f;
        }

        public static float RadiansToDegrees(float rad)
        {
            return (float)(rad * 180f / Math.PI);
        }
    }
}
