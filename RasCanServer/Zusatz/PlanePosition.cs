using CommonLogic;

namespace RasCanServer.Zusatz
{
    public class PlanePosition
    {
        public TimeSpan TimeStamp { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set;}
        public float Altitude { get; set; }

        public CanFrame ToCanFrame()
        {
            byte[] value = new byte[12];
            Span<byte> valueSpan = value.AsSpan();

            NetworkBitConverter.GetBytes(Latitude).CopyTo(valueSpan);
            NetworkBitConverter.GetBytes(Longitude).CopyTo(valueSpan[4..]);
            NetworkBitConverter.GetBytes(Altitude).CopyTo(valueSpan[8..]);

            return new CanFrame
            {
                Bus = Bus.metaBus,
                CanId = 2,
                Value = value
            };
        }
    }
}
