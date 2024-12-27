namespace SoftlogyMaui.Modelos
{
    public class Pasajero
    {
        public Pasajero(string nombre,string direccion, string celulares)
        {
            this.nombre = nombre;
            this.celulares = celulares;
            this.direccion=direccion;
        }
        public int id { get; set; }

        public string? identificacion { get; set; }

        public string nombre { get; set; }

        public string celulares { get; set; }

        public string? direccion { get; set; }

        public string? barrio { get; set; }

        public string? municipio { get; set; }

        public double lat { get; set; }

        public double lng { get; set; }

        public string? email { get; set; }

        public string? ceco { get; set; }

        public string? servicio { get; set; }
    }
}
