using CommonLogic;
using SimulatorInterface.datamodel;
using SimulatorInterface.dummyfs;
using SimulatorInterface.mfs;
using SimulatorInterface.p3d;
using SimulatorInterface.steering;
using System.Diagnostics;

namespace SimulatorInterface.mainController;

public class SimulatorsConnector
{
    // general Configurations
    private const bool useSimvarsLogFile = false;
    private const SIMULATORTYPE useSimulatortype = SIMULATORTYPE.STEERING;
    private double interval = 20;
    // Simulator Configurations
    private bool loadAndRegisterXMLSimvars = true;
    private SimvarService _simvarService;



    public enum SIMULATORTYPE
    {
        MFS,
        P3D,
        DUMMY,
        STEERING
    };


    public SimulatorsConnector()
    {
#pragma warning disable CS0162
        switch (useSimulatortype)
        {
            case SIMULATORTYPE.MFS:
                _simvarService = new SimvarService(new SimvarsController(new MFSSimAdapter(), interval));
                break;

            case SIMULATORTYPE.P3D:
                _simvarService = new SimvarService(new SimvarsController(new P3DSimAdapter(), interval));
                break;

            case SIMULATORTYPE.STEERING:
                _simvarService = new SimvarService(new SimvarsController(new SteeringAdapter(), interval));
                break;

            case SIMULATORTYPE.DUMMY:
                _simvarService = new SimvarService(new DummySimvarsController(interval));
                break;
        }
#pragma warning restore CS0162
    }

    public bool Connect()
    {
        bool simulatorConnected = _simvarService.Connect();

        if (loadAndRegisterXMLSimvars)
        {
            LoadAndRegisterXMLSimvars();
        }

        if (simulatorConnected && useSimvarsLogFile)
        {
            CreateLogFileForSimvars();
        }

        return simulatorConnected;
    }


    public double GetSimvarValue(Bus busId, uint simvarId)
    {
        return _simvarService.GetSimvarValue(busId, simvarId);
    }

    public List<Simvar> GetSimvars()
    {
        return _simvarService.GetSimvars();
    }

    public void SetSimvarValue(Bus busId, uint canasId, double? dNewValue)
    {
        _simvarService.SetSimvarValue(busId, canasId, dNewValue);
    }

    private bool LoadAndRegisterXMLSimvars()
    {
        bool result = _simvarService.LoadAndRegisterXMLSimvars("SimvarsOutput.xml", SIMVARTYPE.Output);
        result &= _simvarService.LoadAndRegisterXMLSimvars("SimvarsInput.xml", SIMVARTYPE.Input);
        result &= _simvarService.LoadAndRegisterXMLEventSimvars();
        return result;
    }

    private void CreateLogFileForSimvars()
    {
        var path = "SimvarsLogFile.csv";
        Console.WriteLine(path);
        //string delimiter = ", ";

        System.Timers.Timer logTimer = new System.Timers.Timer();
        logTimer.Interval = 5000;
        Stopwatch stopWatch = Stopwatch.StartNew();
        logTimer.Elapsed += (sender, e) =>
        {
            float altitude = (float) GetSimvarValue(Bus.canAero, 1014);
            float longitude = (float) GetSimvarValue(Bus.canAero, 1015);
            float latitude = (float) GetSimvarValue(Bus.canAero, 1016);

            string appendText = stopWatch.Elapsed.ToString() + " " + latitude + " " + longitude + " " + altitude + Environment.NewLine;
            Console.WriteLine(appendText);
            File.AppendAllText(path, appendText);
        };
        logTimer.Enabled = true;
        logTimer.Start();

        /*if (!File.Exists(path))
        {
            string createText = "Simvar BusId" + delimiter + "Simvar Id" + delimiter + "Simvar Name" + delimiter + "Value" + delimiter +
                                Environment.NewLine;
            File.WriteAllText(path, createText);
        }
        
        Thread.Sleep(5000);
        System.Timers.Timer logTimer = new System.Timers.Timer();

        logTimer.Interval = interval;
        logTimer.Elapsed += (sender, e) =>
        {
            foreach (var simvar in GetSimvars())
            {
                string appendText = simvar.eBusId + delimiter + simvar.iCanId + delimiter + simvar.sName + delimiter +
                                    simvar.dValue.ToString().Replace(',', '.') + delimiter + Environment.NewLine;
                File.AppendAllText(path, appendText);
            }
        };
        logTimer.Enabled = true;
        logTimer.Start();*/
    }
}
