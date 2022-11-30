using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGNBIO.Model
{
    [Serializable]
    public class MoveStartPosition
    {
        /// <summary>
        /// 编号
        /// </summary>
        private int id;
        /// <summary>
        /// 名称
        /// </summary>
        private string name;
        private int x_axis;
        private int y_axis;
        private int r_axis;
        private int z1_axis;
        private int z2_axis;
        private int z3_axis;
        /// <summary>
        /// 当量
        /// </summary>
        private int equivalent;
        /// <summary>
        /// 状态，0为可用，1为不可用
        /// </summary>
        private int stutas;

        public int Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public int X_axis { get => x_axis; set => x_axis = value; }
        public int Y_axis { get => y_axis; set => y_axis = value; }
        public int R_axis { get => r_axis; set => r_axis = value; }
        public int Z1_axis { get => z1_axis; set => z1_axis = value; }
        public int Z2_axis { get => z2_axis; set => z2_axis = value; }
        public int Z3_axis { get => z3_axis; set => z3_axis = value; }
        //public int Equivalent { get => equivalent; set => equivalent = value; }
        public int Stutas { get => stutas; set => stutas = value; }
    }
}
