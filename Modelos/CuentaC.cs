using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxistasMaui.Modelos
{
    class CuentaC
    {
        public int id { get; set; }

        public string identificacion { get; set; }

        public string password { get; set; }

        public int saldo { get; set; }

        public int saldovales { get; set; }

        public string estado { get; set; }

        public string latitud { get; set; }

        public string longitud { get; set; }

        public string nombre { get; set; }

        public string placas { get; set; }

        public string placa { get; set; }

        public string foto { get; set; }

        public string fechabloqueo { get; set; }

        public string ocupadas { get; set; }

        public int version { get; set; }

        override
        public string ToString()
        {
            return this.identificacion;
        }
    }
}
