using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.FlightSimulator.SimConnect;
using SimulatorInterface.datamodel;
using SimulatorInterface.mfs;
using SimulatorInterface.Client.Zusatz;
using Timer = System.Timers.Timer;

namespace SimulatorInterface.mainController;

public class Program
{
    private static void Main(string[] args)
    {
        SimulatorsConnector simulatorsConnector = new SimulatorsConnector();

        if (simulatorsConnector.Connect())
        {
            Console.WriteLine("Connected to Simconnect");
            Int32 port = 5000;
            string otherip = "137.248.121.40";
            //IPAddress ip = IPAddress.Parse(ipString);
            new Client.Client(otherip, port, simulatorsConnector);
            // Thread.Sleep(1500000);
            Console.ReadLine();
        }
        else
        {
            Console.WriteLine("can not Connect");
        }
        Console.WriteLine("Press any key to exit...");

    }

    ////////// OLD PROGRAM
    // SimulatorsConnector simulatorsConnector = new SimulatorsConnector();
    // System.Timers.Timer testTimer = new System.Timers.Timer();
    // public static void Main(string[] args)
    // {
    //     Program program = new Program();
    //     if (program.simulatorsConnector.Connect())
    //     {
    //         Thread.Sleep(2000);
    //         program.testTimer = new System.Timers.Timer();
    //         program.createTimer(program.testTimer);
    //         while (true);
    //     }
    //     else
    //     {
    //         Console.WriteLine("can not Connect");
    //     }
    //     Console.WriteLine("Press any key to exit...");
    //     while (true) ;
    // }
    //
    // private void createTimer(Timer testTimer)
    // {
    //     testTimer.Interval = 4000;
    //     testTimer.Elapsed += (sender, e) => trySetTestValue(sender, e);
    //     testTimer.Enabled = true;
    //     testTimer.Start();
    // }
    //
    // /// <summary>
    // /// For Test and Debug
    // /// </summary>
    //
    // // this method is just for test Input
    // private bool ison = false;
    //
    // public void trySetTestValue(object sender, EventArgs e)
    // {
    //     // Simvar simvar = _mfsSimvarsController.lSimvarInputs[0];
    //     if (ison)
    //     {
    //         ison = !ison;
    //         this.simulatorsConnector.SetSimvarValue(39,"0");
    //     }
    //     else
    //     {
    //         ison = !ison;
    //         this.simulatorsConnector.SetSimvarValue(39,"0");
    //     }
    // }
}
