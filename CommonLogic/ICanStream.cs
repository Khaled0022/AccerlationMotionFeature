using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLogic
{
    public interface ICanStream : IDisposable
    {
        CanFrame Read();

        void Write(CanFrame frame);
    }
}
