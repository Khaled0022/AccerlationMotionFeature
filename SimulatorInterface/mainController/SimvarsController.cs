using System.Globalization;
using System.Runtime.InteropServices;
using SimulatorInterface.datamodel;
using SimulatorInterface.mainController;

namespace SimulatorInterface.mainController;

public class SimvarsController : ISimvarsController
{
    public SimvarsController(ISimAdapter adapter, double timerInterval)
    {
        oSimConnect = adapter;

        lSimvarOutputs = new List<Simvar>();
        lSimvarInputs = new List<Simvar>();
        lSimvarEvents = new List<Simvar>();
        lErrorMessages = new List<string>();

        timer.Interval = timerInterval;
        timer.Elapsed += OnTick;
    }

    #region SimvarsController Variables

    /// User-defined win32 event
    public const int WM_USER_SIMCONNECT = 0x0402;

    /// Window handle
    private IntPtr hWnd = new IntPtr(0);

    /// SimConnect object
    private ISimAdapter oSimConnect;

    public bool bConnected { get; private set; }

    public static System.Timers.Timer timer = new System.Timers.Timer();
    public List<string> lErrorMessages { get; private set; }

    public List<Simvar> lSimvarOutputs { get; private set; }

    public List<Simvar> lSimvarInputs { get; private set; }

    public List<Simvar> lSimvarEvents { get; private set; }


    #endregion

    public void Connect()
    {
        // Console.WriteLine("Connect");

        try
        {
            /// The constructor is similar to SimConnect_Open in the native API
            oSimConnect.Connect("Simconnect - Simvar test");

            // Console.WriteLine("Connected");
            /// Listen to connect and quit msgs
            //oSimConnect.OnRecvOpen += SimConnect_OnRecvOpen;
            oSimConnect.OnRecvOpen += SimConnect_OnRecvOpen;
            oSimConnect.OnRecvQuit += SimConnect_OnRecvQuit;

            /// Listen to exceptions
            oSimConnect.OnRecvException += SimConnect_OnRecvException;

            /// Catch a simobject data request
            oSimConnect.OnRecvSimobjectData += SimConnect_OnRecvSimobjectData;

            timer.Enabled = true;
        }
        catch (COMException ex)
        {
            Console.WriteLine("Connection to KH failed: " + ex.Message);
            throw ex;
        }
    }

    public void Disconnect()
    {
        timer.Stop();

        if (oSimConnect != null)
        {
            /// Dispose serves the same purpose as SimConnect_Close()
            oSimConnect.Dispose();
            oSimConnect = null;
        }

        bConnected = false;
    }

    #region Connection Handlers 
    private void SimConnect_OnRecvQuit(object sender, EventArgs e)
    {
        Console.WriteLine("SimConnect_OnRecvQuit");
        Disconnect();
    }

    private void SimConnect_OnRecvOpen(object sender, EventArgs e)
    {
        bConnected = true;
        timer.Start();
    }

    private void SimConnect_OnRecvException(object sender, string exception)
    {
        Console.WriteLine("SimConnect_OnRecvException: " + exception.ToString());

        lErrorMessages.Add("SimConnect : " + exception.ToString());
    }

    private void SimConnect_OnRecvSimobjectData(object sender, RecvSimObjectDataEventArgs data)
    {
        uint iRequest = data.dwRequestID;
        uint iObject = data.dwObjectID;
        foreach (Simvar oSimvarRequest in lSimvarOutputs)
        {
            if (iRequest == (uint)oSimvarRequest.eDef)
            {
                double dValue = (double)data.dwData[0];
                // TODO remove
                /* if (iRequest == 609) { */
                /*     Console.WriteLine("SENDING HEADING: " + dValue); */
                /* } */
                oSimvarRequest.dValue = dValue;
            }
        }
    }
    #endregion

    // for registration a new Simvar 
    public bool RegisterVariablesToSimConnect(List<Simvar> simvars, SIMVARTYPE simvartype)
    {
        bool result = true;
        foreach (var simvar in simvars)
        {
            if (oSimConnect != null && simvar.iCanId != 0)
            {
                /// Define a data structure containing numerical value
                oSimConnect.AddDoubleToDataDefinition(simvar.eDef, simvar.sName, simvar.sUnits);
                /// IMPORTANT: Register it with the simconnect managed wrapper marshaller
                /// If you skip this step, you will only receive a uint in the .dwData field.
                oSimConnect.RegisterDataDefineStruct<double>(simvar.eDef);

                // simvar.bPending = false;
                // simvar.bStillPending = simvar.bPending;
                if (simvartype == SIMVARTYPE.Input)
                {
                    lSimvarInputs.Add(simvar);
                }
                else
                {
                    lSimvarOutputs.Add(simvar);

                    const uint expectedGameFps = 60;
                    const uint msPerSecond = 1000;
                    uint frameInterval = (uint)(expectedGameFps * timer.Interval / msPerSecond);
                    oSimConnect.RequestDataOnSimObject(simvar.eDef, frameInterval);
                }
            }
            else
            {
                result = false;
            }
        }
        return result;
    }

    public bool RegisterEventsToSimConnect(List<Simvar> simvars)
    {
        bool result = true;
        foreach (var simvar in simvars)
        {
            if (oSimConnect != null)
            {
                simvar.bPending = true;
                simvar.bStillPending = true;
                oSimConnect.MapClientEventToSimEvent((EVENT_CTRL)simvar.eDef, simvar.sName);
                lSimvarEvents.Add(simvar);
            }
            else
            {
                result = false;
            }
        }
        return result;
    }

    public void ReceiveSimConnectMessage()
    {
        oSimConnect.ReceiveMessage();
    }

    private void TrySetInputSimvarValuesInSimConnect()
    {
        // Console.WriteLine("TrySetValue");

        foreach (Simvar oSimvar in lSimvarInputs)
        {
            if (oSimvar.dValue.HasValue)
            {
                oSimConnect.SetDataOnSimObject(oSimvar.eDef, oSimvar.dValue.Value);
            }
        }
    }

    private void TrySendSimvarEventInSimConnect()
    {
        // Console.WriteLine("TrySetValue");

        foreach (Simvar oSimvar in lSimvarEvents)
        {
            if (!oSimvar.bPending)
            {
                oSimConnect.TransmitClientEvent(
                    (EVENT_CTRL)oSimvar.eDef,
                    Convert.ToUInt32(oSimvar.dValue)
                );
                oSimvar.bPending = true;
                oSimvar.bStillPending = true;
            }
        }
    }



    // May not be the best way to achieve regular requests.
    // See SimConnect.RequestDataOnSimObject
    public void OnTick(object sender, EventArgs e)
    {
        // Console.WriteLine("OnTick");
        // handle input simvars
        TrySetInputSimvarValuesInSimConnect();
        TrySendSimvarEventInSimConnect();
        // handle output simvars
        ReceiveSimConnectMessage();
    }
}
