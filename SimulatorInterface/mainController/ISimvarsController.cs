using SimulatorInterface.datamodel;

namespace SimulatorInterface.mainController
{
    public interface ISimvarsController
    {
        List<Simvar> lSimvarInputs { get; }
        List<Simvar> lSimvarEvents { get; }
        List<Simvar> lSimvarOutputs { get; }

        void Connect();

        bool RegisterEventsToSimConnect(List<Simvar> simvars);

        bool RegisterVariablesToSimConnect(List<Simvar> simvars, SIMVARTYPE simvartype);

        void Disconnect();
    }
}