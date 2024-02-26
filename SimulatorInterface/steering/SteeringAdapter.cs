using SimulatorInterface.mainController;

namespace SimulatorInterface.steering
{
    public class SteeringAdapter : ISimAdapter
    {
        Enum defineIDaileronPosition; // position (16K)
        Enum defineIDplaneHeadingDegrees; // radians

        double planeHeading = 0.0;
        double controlInputNormalized = 0.0;

        // update
        const double ticksPerSecond = 50.0;
        const double maxAngularSpeedInDegPerSec = 90.0;
        System.Timers.Timer headingUpdateTimer = new System.Timers.Timer();

        public event EventHandler OnRecvOpen;
        public event EventHandler OnRecvQuit;
        public event EventHandler<string> OnRecvException;
        public event EventHandler<RecvSimObjectDataEventArgs> OnRecvSimobjectData;

        public SteeringAdapter()
        {
            headingUpdateTimer.Elapsed += (_, _) => UpdatePlaneHeading();
            headingUpdateTimer.Interval = (int)(1000.0 / ticksPerSecond);
            headingUpdateTimer.Enabled = true;
        }

        void UpdatePlaneHeading()
        {
            double degPerSec = maxAngularSpeedInDegPerSec * controlInputNormalized;
            Console.Write("updating heading: ");
            Console.Write(", input = " + controlInputNormalized);
            Console.Write(", speed = " + degPerSec + "°/s");

            double elapsedTime = 1.0 / ticksPerSecond;
            double delta = degPerSec * elapsedTime;
            planeHeading = (planeHeading + delta) % 360.0;
            Console.Write(", delta = " + delta + "°");
            Console.WriteLine(", heading = " + planeHeading + "°");
        }

        public void AddDoubleToDataDefinition(Enum DefineID, string DatumName, string UnitsName)
        {
            Console.WriteLine("AddDoubleToDataDefinition: " + DefineID + " " + DatumName);
            switch (DatumName) {
                case "AILERON POSITION":
                    defineIDaileronPosition = DefineID;
                    Console.WriteLine("Registered ID for AILERON POSITION: " + DefineID);
                    break;
                case "PLANE HEADING DEGREES GYRO":
                    defineIDplaneHeadingDegrees = DefineID;
                    Console.WriteLine("Registered ID for PLANE HEADING DEGREES GYRO: " + DefineID);
                    break;
                default:
                    break;
            }
        }

        public void Connect(string szName)
        {
            Console.WriteLine("Connect");
        }

        public void Dispose()
        {
            Console.WriteLine("Dispose");
        }

        public void MapClientEventToSimEvent(Enum EventID, string EventName)
        {
            Console.WriteLine("MapClientEventToSimEvent");
        }

        public void ReceiveMessage()
        {
            // just send it out continuously
            object[] data = new object[] { (object)planeHeading };
            uint rid = Convert.ToUInt32(defineIDplaneHeadingDegrees);
            OnRecvSimobjectData.Invoke(this, new RecvSimObjectDataEventArgs(rid, 0, data));
        }

        public void RegisterDataDefineStruct<T>(Enum dwID)
        {
            Console.WriteLine("RegisterDataDefineStruct: " + dwID);
        }

        public void RequestDataOnSimObject(Enum RequestID, uint interval)
        {
            if (RequestID.Equals(defineIDplaneHeadingDegrees)) {
                // TODO remove
                /* Console.WriteLine("GOT HEADING REQUEST: " + interval); */
                uint rid = Convert.ToUInt32(RequestID);
            }
        }

        public void SetDataOnSimObject(Enum DefineID, object pDataSet)
        {
            if (DefineID.Equals(defineIDaileronPosition)) {
                // TODO remove
                /* Console.WriteLine("GOT AILERON POSITION: " + (double)pDataSet); */
                controlInputNormalized = (double)pDataSet / 16000.0;
            }
        }

        public void TransmitClientEvent(Enum EventID, uint dwData)
        {
            Console.WriteLine("TransmitClientEvent");
        }
    }
}
