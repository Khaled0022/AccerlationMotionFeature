namespace CommonLogic
{
    public class NetworkBitConverter
    {
        private static Span<byte> TransformOrder(Span<byte> bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                bytes.Reverse();
            }
            return bytes;
        }

        public static Span<byte> GetBytes(float f)
        {
            return TransformOrder(BitConverter.GetBytes(f));
        }

        public static float ToSingle(Span<byte> bytes)
        {
            return BitConverter.ToSingle(TransformOrder(bytes));
        }
    }
}
