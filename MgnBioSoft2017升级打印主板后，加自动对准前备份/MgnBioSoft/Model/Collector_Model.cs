using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DustCollector.Model
{
    public class Collector_Model
    {
        private string collector_type = "";

        public string Collector_type
        {
            get { return collector_type; }
            set { collector_type = value; }
        }
        private string collector_time = "";

        public string Collector_time
        {
            get { return collector_time; }
            set { collector_time = value; }
        }
        private float value = 0;

        public float Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
        private string file_name = "";

        public string File_name
        {
            get { return file_name; }
            set { file_name = value; }
        }
    }
}
