using CommonLogic;
using SimulatorInterface.datamodel;
using System.Xml.Serialization;

namespace SimulatorInterface.mainController;

public class SimvarService
{
    private readonly ISimvarsController _simvarsController;

    public SimvarService(ISimvarsController simvarsController)
    {
        _simvarsController = simvarsController;
    }

    public bool Connect()
    {
        try
        {
            _simvarsController.Connect();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }

        return true;
    }

    private static List<Simvar> DeserializeSimvars(string xmlFileName)
    {
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "datamodel", xmlFileName);
        XmlSerializer deserializer = new XmlSerializer(typeof(List<Simvar>));
        List<Simvar> simvars;

        using (TextReader textReader = new StreamReader(path))
        {
            simvars = (List<Simvar>)deserializer.Deserialize(textReader);
        }
        // Register only those where a Can ID is set
        return simvars
            .Where(simvar => simvar.iCanId != 0)
            .ToList();
    }

    public bool LoadAndRegisterXMLSimvars(string xmlFileName, SIMVARTYPE simvartype)
    {
        List<Simvar> simvars = DeserializeSimvars(xmlFileName);

        if (simvartype == SIMVARTYPE.Output)
        {
            WarnAboutSimvarsWithUnsetTransmissionSlot(simvars);
        }
        return _simvarsController.RegisterVariablesToSimConnect(simvars, simvartype);
    }

    public bool LoadAndRegisterXMLEventSimvars()
    {
        List<Simvar> simvars = DeserializeSimvars("SimvarsEvent.xml");
        WarnAboutSimvarsWithUnsetTransmissionSlot(simvars);
        return _simvarsController.RegisterEventsToSimConnect(simvars);
    }

    private static void WarnAboutSimvarsWithUnsetTransmissionSlot(IEnumerable<Simvar> simvars)
    {
        foreach (Simvar simvar in simvars)
        {
            if (simvar.eTransmissionSlot == TransmissionSlot.Undefined)
            {
                Console.WriteLine("The simvar " + simvar.sName + " (" + simvar.iCanId + ") " +
                    "has no transmission slot assigned. " +
                    "It won't get send to the server. " +
                    "Assign it a slot in Simvar.cs if you want to transmit it.");
            }
        }
    }

    public void SetSimvarValue(Bus busId, uint canasId, double? dNewValue)
    {
        foreach (Simvar oSimvar in _simvarsController.lSimvarInputs)
        {
            oSimvar.bPending = true;
            oSimvar.bStillPending = true;
            if (canasId == oSimvar.iCanId && busId == oSimvar.eBusId)
            {
                oSimvar.dValue = dNewValue;
            }

            oSimvar.bPending = false;
            oSimvar.bStillPending = false;

        }
        foreach (Simvar oSimvar in _simvarsController.lSimvarEvents)
        {
            if (canasId == oSimvar.iCanId && busId == oSimvar.eBusId)
            {
                oSimvar.dValue = dNewValue;
                oSimvar.bPending = false;
                oSimvar.bStillPending = false;
            }
        }
    }

    public double GetSimvarValue(Bus busId, uint canasId)
    {
        foreach (var simvar in _simvarsController.lSimvarOutputs)
        {
            if (simvar.iCanId == canasId && simvar.eBusId == busId)
            {
                return simvar.dValue.Value;
            }
        }
        throw new KeyNotFoundException();
    }

    public List<Simvar> GetSimvars()
    {
        List<Simvar> result = new List<Simvar>(_simvarsController.lSimvarOutputs);
        return result;
    }
}