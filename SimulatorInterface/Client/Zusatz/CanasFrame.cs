using CommonLogic;
using SimulatorInterface.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulatorInterface.Client.Zusatz
{
    public enum MessageType
    {
        EmergencyEvent,
        NodeServiceHigh,
        UserDefinedHigh,
        NormalOperation,
        UserDefinedLow,
        DebugService,
        NodeServiceLow
    }

    public ref struct CanasFrame
    {
        public Bus Bus { get; set; }
        public uint CanId { get; set; }
        public byte NodeId { get; set; }
        public CanasDatatype DataType { get; set; }
        public byte ServiceCode { get; set; }
        public byte MessageCode { get; set; }
        public Span<byte> Value { get; set; }

        public MessageType MessageType
        {
            get
            {
                if (CanId <= 127) return MessageType.EmergencyEvent;
                if (CanId <= 199) return MessageType.NodeServiceHigh;
                if (CanId <= 299) return MessageType.UserDefinedHigh;
                if (CanId <= 1799) return MessageType.NormalOperation;
                if (CanId <= 1899) return MessageType.UserDefinedLow;
                if (CanId <= 1999) return MessageType.DebugService;
                if (CanId <= 2031) return MessageType.NodeServiceLow;
                throw new InvalidOperationException("The can id is out of the range.");
            }
        }

        public CanFrame ToCanFrame()
        {
            if (DataType.ByteLength() != Value.Length)
            {
                throw new InvalidOperationException("Invalid canas frame. The length of the value does not match the datatype.");
            }
            byte[] toSend = new byte[4 + Value.Length];
            toSend[0] = NodeId;
            toSend[1] = (byte)DataType;
            toSend[2] = ServiceCode;
            toSend[3] = MessageCode;
            Value.CopyTo(toSend.AsSpan()[4..]);

            return new CanFrame()
            {
                Bus = Bus,
                CanId = CanId,
                Value = toSend
            };
        }

        public static CanasFrame FromCanFrame(CanFrame frame)
        {
            Span<byte> value = frame.Value.AsSpan();
            return new CanasFrame()
            {
                Bus = frame.Bus,
                CanId = frame.CanId,
                NodeId = value[0],
                DataType = (CanasDatatype)value[1],
                ServiceCode = value[2],
                MessageCode = value[3],
                Value = value[4..]
            };
        }

        private static byte _messageCodeCounter = 0;

        public static CanasFrame FromSimvar(Simvar simvar)
        {
            float siValue = simvar.sUnits switch
            {
                "Knots" => UnitConverter.KnotsToMetersPerSecond((float)simvar.dValue),
                "feet/second" => UnitConverter.FeetPerSecondToMetersPerSecond((float)simvar.dValue),
                "Radians per second" => UnitConverter.RadiansToDegrees((float)simvar.dValue),
                _ => (float)simvar.dValue,
            };
            //Console.WriteLine(simvar.iCanId + " = " + siValue);
            return new CanasFrame()
            {
                Bus = simvar.eBusId,
                CanId = simvar.iCanId,
                NodeId = 1,
                DataType = CanasDatatype.FLOAT,
                ServiceCode = 0,
                MessageCode = _messageCodeCounter++,
                Value = NetworkBitConverter.GetBytes(siValue)
            };
        }
    }
}
