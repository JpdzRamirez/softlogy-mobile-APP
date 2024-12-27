using Newtonsoft.Json;
using System.ComponentModel;

namespace SoftlogyMaui.Modelos
{
    public class Servicio
    {
        public int id { get; set; }

        public string? fecha { get; set; }

        public required string estado { get; set; }

        public required string direccion { get; set; }

        public required string pago { get; set; }

        public string? adicional { get; set; }

        public string? fechaprogramada { get; set; }

        public double latitud { get; set; }

        public double longitud { get; set; }

        public string? usuarios { get; set; }

        public required string cuentasc_id { get; set; }

        public string? empresa { get; set; }

        public string? cobro { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(0)]
        public int valesid { get; set; } = 0;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(0)]
        public int valesavid { get; set; }

        public string? tomar { get; set; }

        public string? flotades { get; set; }

        public string? observaciones { get; set; }

        public string? rutas { get; set; }

        public string? passvale { get; set; }

        public string? contacto { get; set; }

        public RutaMajorel? ruta { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int CONTRATO_VALE { get; set; } = -1;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int SECUENCIA {  get; set; }

        public Pasajero[]? pasajeros { get; set; }
    }
}
