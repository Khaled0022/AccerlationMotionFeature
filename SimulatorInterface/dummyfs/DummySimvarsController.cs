using System.Globalization;
using System.Runtime.InteropServices;
using SimulatorInterface.datamodel;
using SimulatorInterface.mainController;

namespace SimulatorInterface.dummyfs;
public class DummySimvarsController : ISimvarsController
{
    public DummySimvarsController(double timerInterval)
    {
        lSimvarOutputs = new List<Simvar>();
        lSimvarInputs = new List<Simvar>();
        lSimvarEvents = new List<Simvar>();

        timer.Interval = timerInterval;
        timer.Elapsed += OnTick;
    }

    #region DummySimvarsController Variables 

    /// SimConnect object
    public bool bConnected { get; private set; }

    public static System.Timers.Timer timer = new System.Timers.Timer();

    public List<Simvar> lSimvarOutputs { get; private set; }

    public List<Simvar> lSimvarInputs { get; private set; }

    public List<Simvar> lSimvarEvents { get; private set; }


    #endregion

    public void Connect()
    {
        bConnected = true;
        timer.Start();
    }

    public void Disconnect()
    {
        timer.Stop();
        bConnected = false;
    }

    // for registration a new Simvar 
    public bool RegisterVariablesToSimConnect(List<Simvar> simvars, SIMVARTYPE simvartype)
    {
        foreach (var simvar in simvars)
        {
            // simvar.bPending = false;
            // simvar.bStillPending = simvar.bPending;
            if (simvartype == SIMVARTYPE.Input)
            {
                lSimvarInputs.Add(simvar);
            }
            else
            {
                lSimvarOutputs.Add(simvar);
            }
        }
        return true;
    }

    public bool RegisterEventsToSimConnect(List<Simvar> simvars)
    {
        foreach (var simvar in simvars)
        {
            simvar.bPending = true;
            simvar.bStillPending = true;
            lSimvarEvents.Add(simvar);
        }
        return true;
    }

    private void TrySetInputSimvarValuesInSimConnect()
    {
        // Console.WriteLine("TrySetValue");

        foreach (Simvar oSimvar in lSimvarInputs)
        {
            //Console.WriteLine("Set Value " + oSimvar.eBusId + ", " + oSimvar.iCanId + " = " + oSimvar.dValue);
        }
    }

    private void TrySendSimvarEventInSimConnect()
    {
        // Console.WriteLine("TrySetValue");

        foreach (Simvar oSimvar in lSimvarEvents)
        {
            if (!oSimvar.bPending)
            {
                //Console.WriteLine("SIM Event " + oSimvar.eBusId + ", " + oSimvar.iCanId + " = " + Convert.ToUInt32(oSimvar.dValue));
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
        // handle output simvars
        TrySetInputSimvarValuesInSimConnect();
        TrySendSimvarEventInSimConnect();
    }
}