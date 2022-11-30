using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGNBIO.Model
{
    public class SendOffset
    {
        int id;
        int offset;
        int status;
        String name;
        int type;

        public int Id { get => id; set => id = value; }
        public int Offset { get => offset; set => offset = value; }
        /// <summary>
        /// 0为未发送，1为发送完成未接收，2为接收完成
        /// </summary>
        public int Status { get => status; set => status = value; }
        /// <summary>
        /// x=1,y2,r=3,z1=4,z2=5,z3=6
        /// </summary>
        public int Type { get => type; set => type = value; }
        public string Name { get => name; set => name = value; }
    }
}
