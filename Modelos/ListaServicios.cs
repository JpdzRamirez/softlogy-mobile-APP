namespace SoftlogyMaui.Modelos
{
    class ListaServicios
    {
        public int id { get; set; }

        public string direccion { get; set; }

        public string pago { get; set; }

        public double latitud { get; set; }

        public double longitud { get; set; }


        public ListaServicios(int id, string direccion, string pago, double latitud, double longitud)
        {
            this.id = id;
            this.direccion = direccion;
            this.pago = pago;
            this.latitud = latitud;
            this.longitud = longitud;
        }
    }
}
