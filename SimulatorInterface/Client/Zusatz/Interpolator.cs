using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SimulatorInterface.Client.Zusatz
{
    public struct PlanePositionAndRotation
    {
        //Latitude, Longitude, Altitude
        public Vector3 Position;
        //Heading, Pitch, Bank
        public Vector3 Rotation;
    }

    public abstract class Interpolator
    {
        protected class Ringbuffer
        {
            private readonly (long, Vector3)[] _arr;
            private int _pos;
            private int _count;

            public Ringbuffer(int size)
            {
                _arr = new (long, Vector3)[size];
                _pos = 0;
                _count = 0;
            }

            public void AddNewElement((long, Vector3) elem)
            {
                _arr[_pos] = elem;

                if (_pos == _arr.Length - 1)
                {
                    _pos = 0;
                }
                else
                {
                    _pos++;
                }
                _count++;
            }

            public (long, Vector3) this[int idx]
            {
                get
                {
                    if (idx < 0 || idx >= _count) throw new IndexOutOfRangeException();
                    int adjustedIdx = _pos - 1 - idx;
                    if (adjustedIdx < 0) adjustedIdx += _arr.Length;
                    return _arr[adjustedIdx];
                }
            }

            public bool IsFull
            {
                get
                {
                    return _count >= _arr.Length;
                }
            }
        }

        public abstract int WindowSize { get; }

        protected abstract PlanePositionAndRotation ExtrapolateIntern(long currentTime);

        protected Ringbuffer _knownData;

        public Interpolator()
        {
            _knownData = new Ringbuffer(WindowSize);
        }

        public void AddNewDatapoint(Vector3 datapoint)
        {
            _knownData.AddNewElement((Stopwatch.GetTimestamp(), datapoint));
        }

        public PlanePositionAndRotation? Extrapolate()
        {
            if (!_knownData.IsFull)
            {
                return null;
            }
            return ExtrapolateIntern(Stopwatch.GetTimestamp());
        }
    }
}
