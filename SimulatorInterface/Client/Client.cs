using CommonLogic;
using SimulatorInterface.Client.Zusatz;
using SimulatorInterface.datamodel;
using SimulatorInterface.mainController;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Numerics;

namespace SimulatorInterface.Client;

public class Client
{
    private readonly string _hostIp;
    private readonly int _port;

    private ICanStream _server;
    private readonly SimulatorsConnector _simconnect;
    private readonly CultureInfo _ci = new CultureInfo("de-DE");

    private CancellationTokenSource _cancellationToken;

    private readonly TtbScheduler _scheduler = new TtbScheduler();

    private Thread _threadInpolation;
    private CancellationTokenSource _ctsInterpolation;
    private Interpolator _interpolator;

    /// <summary>
    /// Server constructor
    /// </summary>
    /// <param name="port">Port number -> server </param>
    /// <param name="simconnect"> object -> Simconnector</param>
    public Client(string hostIp, Int32 port, SimulatorsConnector simconnect)
    {
        _simconnect = simconnect;

        _hostIp = hostIp;
        _port = port;

        ConnectToServer();
    }

    private void ConnectToServer()
    {
        Console.WriteLine("trying to connect to server");
        TcpClient serverSocket = null;

        do
        {
            try
            {
                serverSocket = new TcpClient(_hostIp, _port);
            }
            catch (SocketException)
            {
                Console.WriteLine("client could not connect to server, trying to reconnect...");
                Thread.Sleep(1000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        } while (serverSocket == null || !serverSocket.Connected);

        _server = new BinaryCanStream(serverSocket.GetStream());

        Console.WriteLine("Client Connected to server " + _hostIp + ":" + _port);

        _cancellationToken = new CancellationTokenSource();

        Thread dataThread = new Thread(SendDataThread);
        dataThread.IsBackground = true;
        dataThread.Start();

        Thread receiveThread = new Thread(ReceiveDataThread);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    /// <summary>
    /// repeatedly calls SendData in rateMs intervals
    /// </summary>
    private void SendDataThread()
    {
        Console.WriteLine(nameof(SendDataThread) + " started");
        try
        {
            //the interval is 12.5 ms as stated in canas_17.pdf
            TimeSpan interval = TimeSpan.FromMilliseconds(12);
            //Stopwatch uses a high-resolution performance counter if available
            Stopwatch watch = new Stopwatch();

            while (!_cancellationToken.IsCancellationRequested)
            {
                watch.Restart();
                SendData();
                _scheduler.GoToNextTimeFrame();
                watch.Stop();

                if (watch.Elapsed < interval)
                {
                    Thread.Sleep(interval - watch.Elapsed);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        Console.WriteLine(nameof(SendDataThread) + " ended");
    }

    /// <summary>
    ///Writes Bytes for each client in clientlist(if connected) of each Object (after processing the date in ProcessOutgoingData(object)) in received list in stream
    /// use ProcessOutgoingDataSerialize(obj) in code to serialize and send whole object (not recommended)
    /// </summary>
    private void SendData()
    {
        try
        {
            foreach (Simvar simvar in _simconnect.GetSimvars())
            {
                if (simvar.dValue.HasValue && _scheduler.ShouldSend(simvar))
                {
                    //Console.WriteLine(simvar.iCanId + "::" + simvar.sName + "::" + (simvar.dValue - simvar.LastValueSendToServer));
                    simvar.LastValueSendToServer = simvar.dValue.Value;

                    CanasFrame frame = CanasFrame.FromSimvar(simvar);
                    CanFrame baseFrame = frame.ToCanFrame();

                    _server.Write(baseFrame);
                }
            }
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("connection with server closed unexpectedly");
            _cancellationToken.Cancel();

            Thread reconnectThread = new Thread(ConnectToServer);
            reconnectThread.IsBackground = true;
            reconnectThread.Start();

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    /// <summary>
    /// for each client in clientlist()if connected) the Thread Reads byte[] from stream and calls ProcessIncomeData() with byte[] as input
    /// use ProcessIncomeDataDeserialize(data); in code to deserialize Objects (not recommended)
    /// </summary>
    private void ReceiveDataThread()
    {
        Console.WriteLine(nameof(ReceiveDataThread) + " started");
        while (!_cancellationToken.IsCancellationRequested)
        {
            try
            {
                CanFrame frame = _server.Read();
                ProcessIncomeData(frame);
            }
            catch (Exception)
            {
                //Console.WriteLine(e);
            }
        }
        Console.WriteLine(nameof(ReceiveDataThread) + " ended");
    }

    private void InterpolationLoop()
    {
        Console.WriteLine(nameof(InterpolationLoop) + " started");

        _interpolator = new ParabolicInterpolator();

        while (!_ctsInterpolation.IsCancellationRequested)
        {
            PlanePositionAndRotation? ret = _interpolator.Extrapolate();

            if (ret.HasValue)
            {
                PlanePositionAndRotation newPos = ret.Value;
                _simconnect.SetSimvarValue(Bus.canAero, 1006, newPos.Position.X);
                _simconnect.SetSimvarValue(Bus.canAero, 1005, newPos.Position.Y);
                _simconnect.SetSimvarValue(Bus.canAero, 1004, newPos.Position.Z);

                _simconnect.SetSimvarValue(Bus.canAero, 1008, newPos.Rotation.X);
                _simconnect.SetSimvarValue(Bus.canAero, 1009, newPos.Rotation.Y);
                _simconnect.SetSimvarValue(Bus.canAero, 1007, newPos.Rotation.Z);
            }
            Thread.Sleep(20);
        }

        _simconnect.SetSimvarValue(Bus.canAero, 1006, null);
        _simconnect.SetSimvarValue(Bus.canAero, 1005, null);
        _simconnect.SetSimvarValue(Bus.canAero, 1004, null);

        _simconnect.SetSimvarValue(Bus.canAero, 1008, null);
        _simconnect.SetSimvarValue(Bus.canAero, 1009, null);
        _simconnect.SetSimvarValue(Bus.canAero, 1007, null);

        Console.WriteLine(nameof(InterpolationLoop) + " stopped");
    }

    /******************************* Process Data Methods *******************************/


    private void ProcessIncomeData(CanFrame frame)
    {
        switch (frame.Bus)
        {
            case Bus.canAero:
                CanasFrame canas = CanasFrame.FromCanFrame(frame);

                float dNewValue = NetworkBitConverter.ToSingle(canas.Value);
                //runtime O(n+m) where n is lSimvarInputs.length and m is lSimvarEvents.length
                _simconnect.SetSimvarValue(canas.Bus, canas.CanId, dNewValue);
                //Console.WriteLine("BusID: " + canas.Bus + " :: ID :: " + canas.CanId + ":: Value :: " + dNewValue);
                break;

            case Bus.metaBus:
                switch (frame.CanId)
                {
                    case 1:
                        bool shouldStart = frame.Value[0] != 0;

                        if (shouldStart && (_threadInpolation == null || !_threadInpolation.IsAlive))
                        {
                            _ctsInterpolation = new CancellationTokenSource();
                            _threadInpolation = new Thread(InterpolationLoop);
                            _threadInpolation.Start();
                        }
                        else if (!shouldStart && _threadInpolation.IsAlive)
                        {
                            _ctsInterpolation.Cancel();
                        }
                        break;

                    case 2:
                        byte[] buffer = new byte[4];
                        Array.Copy(frame.Value, 0, buffer, 0, 4);
                        float lat = NetworkBitConverter.ToSingle(buffer);
                        Array.Copy(frame.Value, 4, buffer, 0, 4);
                        float lon = NetworkBitConverter.ToSingle(buffer);
                        Array.Copy(frame.Value, 8, buffer, 0, 4);
                        float alt = NetworkBitConverter.ToSingle(buffer);
                        _interpolator.AddNewDatapoint(new Vector3(lat, lon, alt));
                        //Console.WriteLine(lat + ", " + lon + ", " + alt);
                        break;
                }
                break;
        }
    }
}
