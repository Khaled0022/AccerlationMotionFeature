using Microsoft.FlightSimulator.SimConnect;
using SimulatorInterface.datamodel;
using SimulatorInterface.mainController;

namespace SimulatorInterface.mfs
{
    public class MFSSimAdapter : ISimAdapter
    {
        /// SimConnect object
        private SimConnect oSimConnect;

        /// Window handle
        private IntPtr hWnd = new IntPtr(0);

        /// User-defined win32 event
        private const int WM_USER_SIMCONNECT = 0x0402;

        public event EventHandler OnRecvOpen
        {
            add { oSimConnect.OnRecvOpen += (_, _) => value.Invoke(this, EventArgs.Empty); }
            remove { throw new NotImplementedException(); }
        }

        public event EventHandler OnRecvQuit
        {
            add { oSimConnect.OnRecvQuit += (_, _) => value.Invoke(this, EventArgs.Empty); }
            remove { throw new NotImplementedException(); }
        }

        public event EventHandler<string> OnRecvException
        {
            add { oSimConnect.OnRecvException += (_, args) => value.Invoke(this, ((SIMCONNECT_EXCEPTION)args.dwException).ToString()); }
            remove { throw new NotImplementedException(); }
        }

        public event EventHandler<RecvSimObjectDataEventArgs> OnRecvSimobjectData
        {
            add {
                oSimConnect.OnRecvSimobjectData +=
                    (_, args) => value.Invoke(this, new RecvSimObjectDataEventArgs(args.dwRequestID, args.dwObjectID, args.dwData));
            }
            remove { throw new NotImplementedException(); }
        }

        public void AddDoubleToDataDefinition(Enum DefineID, string DatumName, string UnitsName)
        {
            oSimConnect.AddToDataDefinition(DefineID, DatumName, UnitsName,
                        SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
        }

        public void Connect(string szName)
        {
            oSimConnect = new SimConnect(szName, hWnd, WM_USER_SIMCONNECT, null, 0);
        }

        public void Dispose()
        {
            oSimConnect.Dispose();
            oSimConnect = null;
        }

        public void MapClientEventToSimEvent(Enum EventID, string EventName)
        {
            oSimConnect.MapClientEventToSimEvent(EventID, EventName);
        }

        public void ReceiveMessage()
        {
            oSimConnect.ReceiveMessage();
        }

        public void RegisterDataDefineStruct<T>(Enum dwID)
        {
            oSimConnect.RegisterDataDefineStruct<T>(dwID);
        }

        public void RequestDataOnSimObject(Enum RequestID, uint interval)
        {
            oSimConnect.RequestDataOnSimObject(RequestID, RequestID, (uint)SIMCONNECT_SIMOBJECT_TYPE.USER, SIMCONNECT_PERIOD.VISUAL_FRAME, SIMCONNECT_DATA_REQUEST_FLAG.CHANGED, 0, interval, 0);
        }

        public void SetDataOnSimObject(Enum DefineID, object pDataSet)
        {
            oSimConnect.SetDataOnSimObject(DefineID, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, pDataSet);
        }

        public void TransmitClientEvent(Enum EventID, uint dwData)
        {
            oSimConnect.TransmitClientEvent(0, EventID, dwData, GROUP_IDS.GROUP, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
        }
    }
}
