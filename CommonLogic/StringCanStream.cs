using System.Net.Sockets;
using System.Text;

namespace CommonLogic
{
    public class StringCanStream : ICanStream
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;

        private CanFrame[]? readBuffer;
        private int readBufferPointer;

        private static string ByteArrayToString(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);

            foreach (byte b in bytes)
            {
                hex.Append(b.ToString("X2"));
            }
            return hex.ToString();
        }

        private static byte[] StringToByteArray(string hex)
        {
            byte[] bytes = new byte[hex.Length / 2];

            for (int x = 0; x < hex.Length; x += 2)
            {
                bytes[x / 2] = Convert.ToByte(hex.Substring(x, 2), 16);
            }
            return bytes;
        }

        public StringCanStream(TcpClient client)
        {
            _client = client;
            _stream = client.GetStream();
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public CanFrame Read()
        {
            if (readBuffer == null || readBufferPointer == readBuffer.Length)
            {
                byte[] data = new byte[_client.ReceiveBufferSize];
                _stream.Read(data, 0, _client.ReceiveBufferSize);

                string stringData = Encoding.ASCII.GetString(data);
                string[] msglist = stringData.Split(";");
                readBuffer = new CanFrame[msglist.Length - 1];

                for (int i = 0; i < (msglist.Length - 1); i++)
                {
                    string msg = msglist[i];
                    string[] list = msg.Split(":", 3);

                    readBuffer[i] = new CanFrame()
                    {
                        Bus = (Bus)Convert.ToByte(list[0]),
                        CanId = Convert.ToUInt32(list[1]),
                        Value = StringToByteArray(list[2])
                    };
                }
                readBufferPointer = 0;
            }
            return readBuffer[readBufferPointer++];
        }

        public void Write(CanFrame frame)
        {
            byte[] bytesToSend = Encoding.ASCII.GetBytes((byte)frame.Bus + ":" + frame.CanId + ":" + ByteArrayToString(frame.Value) + ";");

            lock (_stream)
            {
                _stream.Write(bytesToSend, 0, bytesToSend.Length);
                _stream.Flush();
            }
        }
    }
}
