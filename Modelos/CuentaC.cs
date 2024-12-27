using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftlogyMaui.Modelos
{
    class CuentaC
    {
        public int id { get; set; }

        public string name { get; set; }

        public string password { get; set; }

        public string realname { get; set; }
        public string firstname { get; set; }

        public string phone { get; set; }
        public string phone2 { get; set; }
        public string mobile { get; set; }

        public string is_active { get; set; }

        public string picture { get; set; }
        public string tickets { get; set; }

        public int version { get; set; }

        override
        public string ToString()
        {
            return this.name;
        }
    }
}
