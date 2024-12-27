using CommunityToolkit.Mvvm.Messaging;
using SoftlogyMaui.Modelos;

namespace SoftlogyMaui.Modales;

public partial class Cancelar : ContentPage
{
    HttpClientHandler httpHandler = new HttpClientHandler
    {
        AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
    };
    private HttpClient clientehttp;
    string servicio;
    string cuentac;

    public Cancelar(string servicio, string cuentac)
    {
        InitializeComponent();
        clientehttp = new HttpClient(httpHandler);
        this.servicio = servicio;
        this.cuentac = cuentac;
    }

    private async void btnno_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void btnsi_Clicked(object sender, EventArgs e)
    {
        if (!String.IsNullOrEmpty(justificacion.Text))
        {
            grillalogin.IsVisible = true;
            var content = new FormUrlEncodedContent(new[]
          {
                new KeyValuePair<string, string>("idservicio", servicio),
                new KeyValuePair<string, string>("idtaxista", cuentac),
                new KeyValuePair<string, string>("razon", justificacion.Text),
                new KeyValuePair<string, string>("app", "aplicacion"),
                });

            try
            {
                var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/cancelar", content);

                if (response.IsSuccessStatusCode)
                {
                    string resul = response.Content.ReadAsStringAsync().Result;
                    if (resul == "OK" || resul == "Duplicado")
                    {
                        MapaServicio.cancelado = true;
                        WeakReferenceMessenger.Default.Send(new SincronizarMessage("2_1"));
                        //MessagingCenter.Send("2_1", "sincronizar");
                        Preferences.Set("sincronizando", "0");
                        Preferences.Remove("servicio");
                        Preferences.Remove("estado");
                        grillalogin.IsVisible = false;
                        DependencyService.Get<Interfaces.IToastService>().ShowToast("Se registr� la cancelaci�n del servicio correctamente");
                        await Navigation.PopModalAsync();
                        ((FlyoutPage)((NavigationPage)Application.Current.MainPage).CurrentPage).Detail = new NavigationPage(new HomeDetail());
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo registrar la cancelaci�n. El estado del servicio es:" + resul, "Aceptar");
                    }
                }
                else
                {
                    await DisplayAlert("Problemas de conexi�n", "No se pudo registrar la cancelaci�n en el servidor.", "Aceptar");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Problemas de conexi�n", "No se pudo registrar la cancelaci�n en el servidor. " + ex.Message, "Aceptar");
            }
        }
        else
        {
            await DisplayAlert("Motivo obligatorio", "Debe ingresar el motivo de la cancelaci�n", "Aceptar");
            justificacion.PlaceholderColor = Colors.Red;
        }
    }
}