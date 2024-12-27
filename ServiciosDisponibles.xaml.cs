using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using SoftlogyMaui.Interfaces;
using SoftlogyMaui.Modelos;

namespace SoftlogyMaui;

public partial class ServiciosDisponibles : ContentPage
{
    HttpClientHandler httpHandler = new HttpClientHandler
    {
        AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
    };
    private HttpClient? clientehttp;
    int oneserv;
    public ServiciosDisponibles(Servicio[] servicios)
    {
        InitializeComponent();
        try
        {
            clientehttp = new HttpClient(httpHandler);
            grillalogin.IsVisible = true;
            Preferences.Set("mapa", false);
            oneserv = 0;
            Listarservicios(servicios);
        }
        catch (Exception ex)
        {
            DependencyService.Get<IToastService>().ShowToast(ex.Message);
        }
    }

    private void Listarservicios(Servicio[] servicios)
    {
        ObservableCollection<ListaServicios> itemList = new ObservableCollection<ListaServicios>();
        string pago;
        foreach (var servicio in servicios)
        {
            if (servicio.valesavid != 0)
            {
                pago = servicio.pago + " ( AVIANCA )";
            }
            else
            {
                pago = servicio.pago;
            }
            itemList.Add(new ListaServicios(servicio.id, servicio.direccion + ", " + servicio.adicional, pago, servicio.latitud, servicio.longitud));
        }
        lstservicios.ItemsSource = itemList;
        grillalogin.IsVisible = false;
    }

    private async void Lstservicios_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new SincronizarMessage("2_1"));
        //MessagingCenter.Send("2_1", "sincronizar");
        Preferences.Set("sincronizando", "0");
        ListaServicios serv = (ListaServicios)e.SelectedItem;

        if (serv != null)
        {
            bool respuesta = await DisplayAlert("Servicio", "¿Tomar el servicio?", "Aceptar", "Cancelar");

            if (respuesta)
            {
                HomeDetail.parar = false;
                grillalogin.IsVisible = true;
                var content = new FormUrlEncodedContent(new[]
                {
                        new KeyValuePair<string, string>("idservicio", serv.id.ToString()),
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
                        if (resp != null)
                        {
                            if (resp.tomar == "tomado")
                            {
                                Preferences.Set("disponibles", 0);
                                if (oneserv == 0)
                                {
                                    Sincronizador.limite = 1;
                                    grillalogin.IsVisible = false;
                                    Preferences.Set("sincronizando", "1");
                                    oneserv++;
                                    ((FlyoutPage)((NavigationPage)Application.Current.MainPage).CurrentPage).Detail = new NavigationPage(new MapaServicio(resp));
                                }
                            }
                            else if (resp.tomar == "bloqueado")
                            {
                                grillalogin.IsVisible = false;
                                Preferences.Remove("sesion");
                                Preferences.Set("salir", false);
                                await DisplayAlert("Usuario bloqueado", "No puede tomar servicios porqué su usuario ha sido bloqueado", "Aceptar");
                                Application.Current.MainPage = new NavigationPage(new MainPage());
                            }
                            else
                            {
                                grillalogin.IsVisible = false;
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
                                ((FlyoutPage)((NavigationPage)Application.Current.MainPage).CurrentPage).Detail = new NavigationPage(new HomeDetail());
                            }
                        }
                    }
                    else
                    {
                        WeakReferenceMessenger.Default.Send(new SincronizarMessage("1_1-1"));
                        //MessagingCenter.Send("1_1-1", "sincronizar");
                        grillalogin.IsVisible = false;
                        await DisplayAlert("Error", "No ha sido posible realizar la solicitud. " + response.ReasonPhrase, "Aceptar");
                    }
                }
                catch (Exception ex)
                {
                    WeakReferenceMessenger.Default.Send(new SincronizarMessage("1_1-1"));
                    //MessagingCenter.Send("1_1-1", "sincronizar");
                    grillalogin.IsVisible = false;
                    await DisplayAlert("Error de conexión", "No fue posible conectarse al servidor para validar el servicio. " + ex.Message, "Aceptar");
                }
            }
            else
            {
                lstservicios.SelectedItem = null;
                WeakReferenceMessenger.Default.Send(new SincronizarMessage("1_1-1"));
                //MessagingCenter.Send("1_1-1", "sincronizar");
            }
        }
    }
}