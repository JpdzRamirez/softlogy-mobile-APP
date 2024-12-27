using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using TaxistasMaui.Modelos;

namespace TaxistasMaui;

public partial class ModalAceptar : ContentPage
{
    HttpClientHandler httpHandler = new HttpClientHandler
    {
        AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
    };
    HttpClient clientehttp;
    Servicio servicio;
    int oneserv;
    public ModalAceptar(Servicio servicio)
    {
        InitializeComponent();
        oneserv = 0;
        lblnombrecliente.Text = "\uf007  " + servicio.usuarios;
        lbldireccion.Text = "\uf041  " + servicio.direccion + "-" + servicio.adicional;
        lblpago.Text = "\uf0d6  " + servicio.pago + ", " + servicio.cobro;
        if (!string.IsNullOrEmpty(servicio.observaciones))
        {
            if (string.IsNullOrEmpty(servicio.observaciones))
            {
                lblobservaciones.Text = "\uf27a  " + servicio.rutas;
            }
            else
            {
                lblobservaciones.Text = "\uf27a " + servicio.observaciones;
            }
            lblobservaciones.IsVisible = true;
            boxobservaciones.IsVisible = true;
        }

        /*if (Preferences.Get("estilo", "null") == "1")
        {
            modalservicio.BackgroundColor = Colors.Black;
            lblnombrecliente.TextColor = Colors.White;
            lblobservaciones.TextColor = Colors.White;
            lbldireccion.TextColor = Colors.White;
            lblpago.TextColor = Colors.White;
        }
        else
        {
            modalservicio.BackgroundColor = Colors.White;
            lblnombrecliente.TextColor = Colors.Black;
            lblobservaciones.TextColor = Colors.Black;
            lbldireccion.TextColor = Colors.Black;
            lblpago.TextColor = Colors.Black;
        }*/
        clientehttp = new HttpClient(httpHandler);
        this.servicio = servicio;
    }

    private async void btnaceptar_Clicked(object sender, EventArgs e)
    {
        btnaceptar.IsEnabled = false;
        WeakReferenceMessenger.Default.Send(new SincronizarMessage("2_1"));
        //MessagingCenter.Send("2_1", "sincronizar");
        Preferences.Set("sincronizando", "0");
        HomeDetail.parar = false;
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("idservicio", servicio.id.ToString()),
            new KeyValuePair<string, string>("idtaxista", Preferences.Get("idtaxista", 0).ToString()),
            new KeyValuePair<string, string>("placataxista", Preferences.Get("placataxista", "").ToString()),
            new KeyValuePair<string, string>("app", "aplicacion"),
        });
        try
        {
            var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/tomarservicio", content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Servicio? resp = JsonConvert.DeserializeObject<Servicio>(response.Content.ReadAsStringAsync().Result);
                if(resp != null)
                {
                    if (resp.tomar == "tomado")
                    {
                        Preferences.Set("disponibles", 0);
                        if (oneserv == 0)
                        {
                            Navigation.PopModalAsync();
                            Sincronizador.limite = 1;
                            Preferences.Set("sincronizando", "1");
                            oneserv++;
                            ((FlyoutPage)((NavigationPage)Application.Current.MainPage).CurrentPage).Detail = new NavigationPage(new MapaServicio(resp));
                        }
                    }
                    else if (resp.tomar == "bloqueado")
                    {
                        Preferences.Remove("sesion");
                        Preferences.Set("salir", false);
                        await DisplayAlert("Usuario bloqueado", "No puede tomar servicios porqué su usuario ha sido bloqueado", "Aceptar");
                        Application.Current.MainPage = new NavigationPage(new MainPage());
                    }
                    else
                    {
                        Preferences.Set("sincronizando", "0");
                        Preferences.Set("disponibles", 0);
                        if (resp.tomar == "sinsaldo")
                        {
                            await DisplayAlert("Saldo insuficiente", "No posee saldo suficiente para tomar un servicio.", "Aceptar");
                        }
                        else if (resp.tomar == "ocupado")
                        {
                            await DisplayAlert("Servicio ocupado", "El servicio ya no está disponible", "Aceptar");
                        }

                        else if (resp.tomar == "listanegra")
                        {
                            await DisplayAlert("Servicio no disponible", "La empresa solicitante del servicio te ha bloqueado", "Aceptar");
                        }
                        else if (resp.tomar == "noflota")
                        {
                            await DisplayAlert("Servicio no disponible", "El usuario desea un vehículo inscrito en la flota: " + resp.flotades, "Aceptar");
                        }
                        WeakReferenceMessenger.Default.Send(new SincronizarMessage("1_1-1"));
                        //MessagingCenter.Send("1_1-1", "sincronizar");
                        await Navigation.PopModalAsync();
                        HomeDetail.parar = true;
                    }
                }
            }
            else
            {
                btnaceptar.IsEnabled = true;
                WeakReferenceMessenger.Default.Send(new SincronizarMessage("1_1-1"));
                //MessagingCenter.Send("1_1-1", "sincronizar");
                await DisplayAlert("Error", "No ha sido posible realizar la solicitud. " + response.Content.ReadAsStringAsync().Result, "Aceptar");
            }
        }
        catch (Exception ex)
        {
            btnaceptar.IsEnabled = true;
            WeakReferenceMessenger.Default.Send(new SincronizarMessage("1_1-1"));
            //MessagingCenter.Send("1_1-1", "sincronizar");
            await DisplayAlert("Error de conexión", "No fue posible conectarse al servidor para validar el servicio. " + ex.Message, "Aceptar");
        }
    }
}