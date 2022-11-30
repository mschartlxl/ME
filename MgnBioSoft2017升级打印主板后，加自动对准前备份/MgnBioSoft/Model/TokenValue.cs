using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MGNBIO.Model
{
    [StructLayout(LayoutKind.Explicit,Size =4)]
    internal struct TokenValue
    {
        [FieldOffset(0)]
        public int i_data;
        [FieldOffset(0)]
        public uint u_data;
        [FieldOffset(0)]
        public float f_data;
    }
}
