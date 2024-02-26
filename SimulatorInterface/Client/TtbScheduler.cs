using SimulatorInterface.datamodel;

namespace SimulatorInterface.Client
{
    public class TtbScheduler
    {
        private int _timeFrame = 0;

        public bool ShouldSend(Simvar simvar)
        {
            TransmissionSlot slot = simvar.eTransmissionSlot;

            //check if event flag is set
            if ((slot & TransmissionSlot.Event) != 0)
            {
                if (simvar.dValue != simvar.LastValueSendToServer)
                {
                    //events should be send on every state change
                    return true;
                }
                //clear event flag to only leave raw slot
                slot &= ~TransmissionSlot.Event;
            }
            if (slot == TransmissionSlot.Undefined)
            {
                //user should assign slot in Simvar.cs
                return false;
            }
            if (_timeFrame % (int)slot == 0)
            {
                //resend simvar on timer
                return true;
            }
            return false;
        }

        public void GoToNextTimeFrame()
        {
            _timeFrame++;
            
            //wrap around on overflow
            if (_timeFrame == (int)TransmissionSlot.G)
            {
                _timeFrame = 0;
            }
        }
    }
}
