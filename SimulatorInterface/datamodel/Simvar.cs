using CommonLogic;

namespace SimulatorInterface.datamodel;

public class Simvar
{
    private static readonly int busCount = Enum.GetNames(typeof(Bus)).Length;

    public string sName { get; set; }

    public Bus eBusId { get; set; }

    public uint iCanId { get; set; }

    public double? dValue { get; set; } = null;

    public double LastValueSendToServer { get; set; }

    public string sUnits { get; set; }

    public bool bPending = true;

    public bool bStillPending { get; set; }

    public DEFINITION eDef
    {
        get
        {
            return (DEFINITION)(busCount * iCanId + (int)eBusId);
        }
    }

    public TransmissionSlot eTransmissionSlot { get; set; }
    /*{
        get
        {
            switch (eBusId)
            {
                case Bus.canAero:
                    switch (iCanId)
                    {
                        case 300:
                        case 301:
                        case 302:
                        case 303:
                        case 304:
                        case 305:
                        case 311:
                        case 312:
                        case 321:
                            return TransmissionSlot.A;

                        case 314:
                        case 315:
                        case 316:
                        case 317:
                        case 319:
                        case 320:
                        case 322:
                        case 331:
                        case 402:
                        case 405:
                        case 500:
                        case 501:
                        case 502:
                        case 503:
                        case 504:
                        case 505:
                        case 506:
                        case 507:
                        case 520:
                        case 524:
                        case 528:
                        case 532:
                        case 536:
                        case 540:
                        case 668:
                        case 669:
                        case 684:
                        case 920:
                        case 930:
                        case 1030:
                        case 1036:
                        case 1037:
                        case 1039:
                        case 1040:
                        case 1071:
                        case 1075:
                        case 1079:
                        case 1087:
                        case 1088:
                        case 1091:
                        case 1092:
                        case 1126:
                        case 1127:
                        case 1083:
                            return TransmissionSlot.D;

                        case 324:
                        case 1001: //DELETE
                        case 1002: //DELETE
                        case 1003: //DELETE
                            return TransmissionSlot.G;

                        case 439:
                        case 440:
                        case 556:
                        case 1100:
                        case 1101:
                        case 1116:
                        case 1108:
                        case 1104:
                        case 1105:
                            return TransmissionSlot.G | TransmissionSlot.Event;
                    }
                    break;

                case Bus.canOpen:
                    break;
            }
            return TransmissionSlot.Undefined;
        
        }
    }*/
};

[Flags]
public enum TransmissionSlot : byte
{
    Undefined = 0x00,
    A = 1,
    B = 2,
    C = 4,
    D = 8,
    E = 16,
    F = 32,
    G = 80,
    Event = 0x80 //first bit high
}
