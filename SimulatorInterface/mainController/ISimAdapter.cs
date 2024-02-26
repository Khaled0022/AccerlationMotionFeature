using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulatorInterface.mainController
{
    public class RecvSimObjectDataEventArgs : EventArgs
    {
        public uint dwRequestID { get; }
        public uint dwObjectID { get; }
        public object[] dwData { get; }

        public RecvSimObjectDataEventArgs(uint dwRequestID, uint dwObjectID, object[] dwData)
        {
            this.dwRequestID = dwRequestID;
            this.dwObjectID = dwObjectID;
            this.dwData = dwData;
        }
    }

    public interface ISimAdapter : IDisposable
    {
        event EventHandler OnRecvOpen;

        event EventHandler OnRecvQuit;

        event EventHandler<string> OnRecvException;

        event EventHandler<RecvSimObjectDataEventArgs> OnRecvSimobjectData;

        public void Connect(string szName);

        void AddDoubleToDataDefinition(Enum DefineID, string DatumName, String UnitsName);

        void RegisterDataDefineStruct<T>(Enum dwID);

        void MapClientEventToSimEvent(Enum EventID, string EventName);

        void ReceiveMessage();

        void SetDataOnSimObject(Enum DefineID, object pDataSet);

        void TransmitClientEvent(Enum EventID, uint dwData);

        void RequestDataOnSimObject(Enum RequestID, uint interval);
    }
}
