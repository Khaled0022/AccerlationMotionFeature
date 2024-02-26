using RasCanServer.Server;
using System.Diagnostics;

namespace client
{
    class Program
    {
        static void Main(string[] args)
        {
            try {
                Int32 portSimulator = 5000;
                Int32 portMotionCueue = 5001;
                new Server(portSimulator, portMotionCueue, 100, "SimvarsLogFile.csv");
                Console.ReadLine();
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }
    }
}
