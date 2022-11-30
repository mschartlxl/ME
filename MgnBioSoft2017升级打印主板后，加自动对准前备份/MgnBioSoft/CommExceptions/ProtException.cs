using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGNBIO.CommExceptions
{
    /// <summary>
    /// 通信协议异常
    /// </summary>
    public class ProtException : Exception
    {
        public ProtException()
            : base()
        {
        }

        public ProtException(string message)
            : base(message)
        {
        }

        public ProtException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
