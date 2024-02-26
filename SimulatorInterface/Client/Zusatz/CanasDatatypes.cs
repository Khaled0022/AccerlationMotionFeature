using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulatorInterface.Client.Zusatz
{
    public enum CanasDatatype : byte
    {
        NODATA,
        ERROR,
        FLOAT,
        LONG,
        ULONG,
        BLONG,
        SHORT,
        USHORT,
        BSHORT,
        CHAR,
        UCHAR,
        BCHAR,
        SHORT2,
        USHORT2,
        BSHORT2,
        CHAR4,
        UCHAR4,
        BCHAR4,
        CHAR2,
        UCHAR2,
        BCHAR2,
        MEMID,
        CHKSUM,
        ACHAR,
        ACHAR2,
        ACHAR4,
        CHAR3,
        UCHAR3,
        BCHAR3,
        ACHAR3,
        DOUBLEH,
        DOUBLEL,
        RESVD, //32-99
        UDEF = 100 //100-255
    }

    public static class CanasDatatypeExtensions
    {
        public static int ByteLength(this CanasDatatype dt)
        {
            switch (dt)
            {
                case CanasDatatype.NODATA:
                    return 0;

                case CanasDatatype.ERROR:
                    return 4;

                case CanasDatatype.FLOAT:
                    return 4;

                case CanasDatatype.LONG:
                case CanasDatatype.ULONG:
                case CanasDatatype.BLONG:
                    return 4;

                case CanasDatatype.SHORT:
                case CanasDatatype.USHORT:
                case CanasDatatype.BSHORT:
                    return 2;

                case CanasDatatype.CHAR:
                case CanasDatatype.UCHAR:
                case CanasDatatype.BCHAR:
                    return 1;

                case CanasDatatype.SHORT2:
                case CanasDatatype.USHORT2:
                case CanasDatatype.BSHORT2:
                    return 4;

                case CanasDatatype.CHAR4:
                case CanasDatatype.UCHAR4:
                case CanasDatatype.BCHAR4:
                    return 4;

                case CanasDatatype.CHAR2:
                case CanasDatatype.UCHAR2:
                case CanasDatatype.BCHAR2:
                    return 2;

                case CanasDatatype.MEMID: 
                    return 4;

                case CanasDatatype.CHKSUM:
                    return 4;

                case CanasDatatype.ACHAR:
                    return 1;

                case CanasDatatype.ACHAR2:
                    return 2;

                case CanasDatatype.ACHAR4:
                    return 4;

                case CanasDatatype.CHAR3:
                case CanasDatatype.UCHAR3:
                case CanasDatatype.BCHAR3:
                case CanasDatatype.ACHAR3:
                    return 3;

                case CanasDatatype.DOUBLEH:
                case CanasDatatype.DOUBLEL:
                    return 4;

                default:
                    throw new InvalidOperationException("Datatype is reserved or unknown.");
            }
        }
    }
}
