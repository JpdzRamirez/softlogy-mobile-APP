namespace SoftlogyMaui.Modelos
{
    public class RutaMajorel
    {
        public int id { get; set; }

        public string? tipo { get; set; }

        public int numero { get; set; }

        public string? fecha { get; set; }

        public string? hora { get; set; }

        public string? ceco { get; set; }

        public string? servicio { get; set; }

        public string? llegada { get; set; }

        public int servicios_id { get; set; }

        public int programaciones_id { get; set; }

        public Pasajero[]? pasajeros { get; set; }
    }
}
