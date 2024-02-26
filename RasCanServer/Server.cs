using CommonLogic;
using RasCanServer.Zusatz;
using System.Globalization;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RasCanServer.Server;

public class Server
{
    private readonly TcpListener _server;
    private readonly int _rateMs;

    private ICanStream simCANStream;
    private ICanStream busPostCANStream;
    private NetworkStream motionCueueNetStream;

    // The problem of handling three connections can be solved either by
    // launching a new thread once a disconnect is detected (i.e. Write/Read
    // throws an IOException) or by first accepting a connection and then
    // reading in an infinite loop. The second choice requires the use of
    // semaphores for threads that can't detect on their own whether their
    // connection is up, i.e. threads that don't read, i.e.
    // motionCueueNetStream. We choose the second way since then only three
    // threads exist at one time with one explicit synchronization primitive.

    // initialize to 1 since it should reconnect in the beginning
    private Semaphore motionCueueNetStreamShouldReconnect = new Semaphore(1, 1);

    private static readonly CultureInfo _ci = new CultureInfo("de-DE");

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="port">Port from Server</param>
    /// <param name="servername">hostname or IP-Address</param>
    /// <param name="sValuesSend">Class with get and set for send/received Values </param>
    public Server(Int32 portSimulator, Int32 portMotionCueue, int rateMs, string logFile)
    {
        Thread simThread = new Thread(() => SimThread(portSimulator));
        Thread busPostThread = new Thread(() => BusPostThread());
        Thread motionCueueThread = new Thread(() => MotionCueueThread(portMotionCueue));

        simThread.Start();
        busPostThread.Start();
        motionCueueThread.Start();

        simThread.Join();
        busPostThread.Join();
        motionCueueThread.Join();
    }

    private void SimThread(Int32 port)
    {
        while (true) {
            Console.WriteLine("waiting for connection from simulator...");
            NetworkStream simNetStream = WaitForTCPConnection(port);
            simCANStream = new BinaryCanStream(simNetStream);
            Console.WriteLine("got connection from simulator");

            while (true) {
                try {
                    CanFrame frame = simCANStream.Read();
                    // TODO debug why program stops
                    Console.WriteLine("got frame with can_id=" + frame.CanId);

                    if (busPostCANStream != null) {
                        lock (busPostCANStream) {
                            try {
                                busPostCANStream.Write(frame);
                            } catch (Exception) {
                                // no need to do anything here as the other
                                // thread detects this condition on its own
                            }
                        }
                    }

                    if (motionCueueNetStream != null) {
                        lock (motionCueueNetStream) {
                            try {
                                // TODO move into motionCueueNetStream-lock and send command
                                // as defined in SimvarsOutput.xml
                                if (frame.CanId == 203) {
                                    byte[] bytes = frame.Value;
                                    float heading = NetworkBitConverter.ToSingle(bytes);
                                    string command = "position absolute yaw " + heading  + "\n";
                                    Console.WriteLine("sending command: '" + command);
                                    motionCueueNetStream.Write(Encoding.ASCII.GetBytes(command));
                                }
                            } catch (Exception e) {
                                // tell other thread to reconnect
                                Console.WriteLine("motion cueue disconnected: " + e.ToString());
                                motionCueueNetStreamShouldReconnect.Release(1);
                            }
                        }
                    }
                } catch (Exception e) {
                    Console.WriteLine("sim disconnected: " + e.ToString());
                    break;
                }
            }
        }
    }

    private void BusPostThread()
    {
        while (true) {
            Console.WriteLine("waiting for connection from bus post script...");
            // es gibt auch den Pipetransmissionmode message, welcher auf den ersten Blick besser geeignet erscheint,
            // jedoch funktioniert dies nicht auf UNIXsystemen, es muss auf Byte bleiben!
            var busPostPipeStream = new NamedPipeServerStream("testpipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            busPostPipeStream.WaitForConnection();
            Console.WriteLine("got connection from pipe client");
            busPostCANStream = new BinaryCanStream(busPostPipeStream);

            while (true) {
                try {
                    CanFrame frame = busPostCANStream.Read();
                    if (simCANStream != null) {
                        lock (simCANStream) {
                            try {
                                simCANStream.Write(frame);
                            } catch (Exception) {
                                // no need to do anything here as the other
                                // thread detects this condition on its own
                            }
                        }
                    }
                } catch (Exception) {
                    Console.WriteLine("bus post disconnected");
                    break;
                }
            }
        }
    }

    private void MotionCueueThread(Int32 port)
    {
        while (true) {
            // nothing to read here
            motionCueueNetStreamShouldReconnect.WaitOne();

            Console.WriteLine("waiting for connection from motion cueue...");
            motionCueueNetStream = WaitForTCPConnection(port);
            Console.WriteLine("got connection from motion cueue");
        }
    }

    private NetworkStream WaitForTCPConnection(Int32 port)
    {
        IPAddress serverIP = IPAddress.Any;
        Console.WriteLine("opening listener on " + serverIP + ":" + port);
        TcpListener listener = new TcpListener(serverIP, port);
        listener.Start();

        TcpClient client = listener.AcceptTcpClient();
        IPEndPoint remoteEP = (IPEndPoint)client.Client.RemoteEndPoint;
        Console.WriteLine("received connection from host " + remoteEP.Address.ToString() + ":" + remoteEP.Port.ToString());
        NetworkStream stream = client.GetStream();

        listener.Stop();
        return stream;
    }
}
