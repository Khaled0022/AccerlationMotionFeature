namespace CommonLogic
{
    public class BinaryCanStream : ICanStream
    {
        private readonly BinaryReader _reader;
        private readonly BinaryWriter _writer;

        public BinaryCanStream(Stream stream) {
            _reader = new BinaryReader(stream);
            _writer = new BinaryWriter(stream);
        }

        public void Dispose()
        {
            _reader.Dispose();
            _writer.Dispose();
        }

        public CanFrame Read()
        {
            CanFrame frame = new CanFrame()
            {
                Bus = (Bus)_reader.ReadByte(),
                CanId = _reader.ReadUInt32()
            };
            int valueLength = _reader.ReadByte();
            frame.Value = _reader.ReadBytes(valueLength);
            return frame;
        }

        public void Write(CanFrame frame)
        {
            //BusID + ArbitrationID + Length of Value + Value
            Span<byte> toSend = stackalloc byte[1 + 4 + 1 + frame.Value.Length];

            toSend[0] = (byte)frame.Bus;
            BitConverter.TryWriteBytes(toSend[1..], (uint)frame.CanId);
            toSend[5] = (byte)frame.Value.Length;
            frame.Value.CopyTo(toSend[6..]);

            lock (_writer)
            {
                _writer.Write(toSend);
            }
        }
    }
}
