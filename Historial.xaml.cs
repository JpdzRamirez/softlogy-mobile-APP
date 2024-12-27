using Newtonsoft.Json;
using TaxistasMaui.Modelos;

namespace TaxistasMaui;

public partial class Historial : ContentPage
{
    HttpClientHandler httpHandler = new HttpClientHandler
    {
        AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
    };
    private HttpClient clientehttp;

    public Historial()
    {
        InitializeComponent();
        clientehttp = new HttpClient(httpHandler);
        Preferences.Set("mapa", false);
        /*if (Preferences.Get("estilo", "null") == "1")
        {
            stackhistorial.BackgroundColor = Colors.Black;
        }
        else
        {
            stackhistorial.BackgroundColor = Colors.White;
        }*/
        CargarHistorial();
    }

    private async void CargarHistorial()
    {
        var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("idtaxista", Preferences.Get("idtaxista", 0).ToString()),
                new KeyValuePair<string, string>("app", "aplicacion"),
            });
        try
        {
            var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/historial", content);
            HistorialServicio[]? historial = JsonConvert.DeserializeObject<HistorialServicio[]>(response.Content.ReadAsStringAsync().Result);

            lsthistorial.ItemsSource = historial;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error de conexión", "No fue posible cargar su historial de servicios. " + ex.Message, "Aceptar");
        }
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        bool toogle = ((Application.Current.MainPage as NavigationPage).CurrentPage as FlyoutPage).IsPresented;
        ((Application.Current.MainPage as NavigationPage).CurrentPage as FlyoutPage).IsPresented = !toogle;
    }
}