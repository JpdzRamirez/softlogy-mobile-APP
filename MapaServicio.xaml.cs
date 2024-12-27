using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Newtonsoft.Json;
using SoftlogyMaui.Modales;
using SoftlogyMaui.Modelos;
using System.Text;

namespace SoftlogyMaui;

public partial class MapaServicio : ContentPage
{
    Pin? ubicacion;
    Servicio? servicio;
    HttpClientHandler httpHandler = new HttpClientHandler
    {
        AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
    };
    private HttpClient clientehttp;
    public static bool cancelado = false;

    public MapaServicio(Servicio servicio)
    {
        InitializeComponent();
        clientehttp = new HttpClient(httpHandler);
        Preferences.Set("servicio", servicio.id);
        Preferences.Set("idservicio", servicio.id.ToString());
        Preferences.Set("estado", 0);
        Preferences.Set("taxista", Preferences.Get("idtaxista", 0).ToString());

        var listaUsuarios = servicio.usuarios.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        var listaPasajeros = new StringBuilder();

        // Recorre la lista de usuarios y agrega cada nombre con un formato de lista no ordenada
        foreach (var usuario in listaUsuarios)
        {
            listaPasajeros.AppendLine($"• {usuario.Trim()}"); 
        }

        lblnombrecliente.Text = listaPasajeros.ToString();

        lbldireccion.Text = servicio.direccion + "-" + servicio.adicional;
        lblpago.Text = servicio.pago + ", " + servicio.cobro;
        if (servicio.pago == "Vale electrónico")
        {
            lblempresa.Text = servicio.empresa;
            lblempresa.IsVisible = true;
        }
        if (!string.IsNullOrEmpty(servicio.observaciones) || !string.IsNullOrEmpty(servicio.rutas))
        {
            if (string.IsNullOrEmpty(servicio.observaciones))
            {
                lblobservaciones.Text = servicio.rutas;
            }
            else
            {
                lblobservaciones.Text = servicio.observaciones + " &#10" + servicio.rutas;
            }
            lblobservaciones.IsVisible = true;
        }
        if (servicio.ruta != null)
        {
            BtnPasajeros.IsVisible = true;
        }
        if (servicio.pasajeros != null)
        {
            if(servicio.pasajeros.Length > 0)
            {
                BtnPasajeros.IsVisible = true;
            }
        }
        if (!string.IsNullOrEmpty(servicio.contacto))
        {
            btncall.Text = btncall.Text + "   " + servicio.contacto;
            btncall.IsVisible = true;
        }

        this.servicio = servicio;
        Preferences.Set("mapa", true);
        Preferences.Set("sms", false);

        /*if (Preferences.Get("estilo", "null") == "1")
        {
            stackmapa.BackgroundColor = Colors.Black;
            lblempresa.TextColor = Colors.White;
            lblnombrecliente.TextColor = Colors.White;
            lblobservaciones.TextColor = Colors.White;
            lbldireccion.TextColor = Colors.White;
            lblpago.TextColor = Colors.White;
        }
        else
        {
            stackmapa.BackgroundColor = Colors.White;
            lblempresa.TextColor = Colors.Black;
            lblnombrecliente.TextColor = Colors.Black;
            lblobservaciones.TextColor = Colors.Black;
            lbldireccion.TextColor = Colors.Black;
            lblpago.TextColor = Colors.Black;
        }*/

        WeakReferenceMessenger.Default.Unregister<UbicarMessage>(this);
        WeakReferenceMessenger.Default.Register<UbicarMessage>(this, (r, m) =>
        {
            string[] pos = m.Value.Split('_');
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (pos[2] == "1")
                {
                    WeakReferenceMessenger.Default.Send(new SincronizarMessage("2_1"));
                    //MessagingCenter.Send("2_1", "sincronizar");
                    Preferences.Set("sincronizando", "0");
                    await DisplayAlert("Aviso", "El servicio fue cancelado por el usuario", "Aceptar");
                    ((FlyoutPage)((NavigationPage)Application.Current.MainPage).CurrentPage).Detail = new NavigationPage(new HomeDetail());
                }
                else
                {
                    if (pos[0] != "null")
                    {
                        mymap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(double.Parse(pos[0]), double.Parse(pos[1])), Distance.FromMeters(100)));
                    }
                }
            });
        });
        
        /*MessagingCenter.Unsubscribe<string>(this, "ubicar");
        MessagingCenter.Subscribe<string>(this, "ubicar", ubi =>
        {
            
        });*/

        if (Preferences.Get("sincronizando", "-1").ToString() == "1")
        {
            WeakReferenceMessenger.Default.Send(new SincronizarMessage("2_1-1"));
            //MessagingCenter.Send("2_1-1", "sincronizar");
            switch (this.servicio.estado)
            {
                case "En curso":
                    btnarribo.IsVisible = false;
                    btnfin.IsVisible = true;
                    WeakReferenceMessenger.Default.Send(new SincronizarMessage("1_2-2"));
                    //MessagingCenter.Send("1_2-2", "sincronizar");
                    break;
                default:
                    WeakReferenceMessenger.Default.Send(new SincronizarMessage("1_2-1"));
                    //MessagingCenter.Send("1_2-1", "sincronizar");
                    break;
            }
        }
        else
        {
            switch (this.servicio.estado)
            {
                case "En curso":
                    btnarribo.IsVisible = false;
                    btnfin.IsVisible = true;
                    break;
                default:
                    break;
            }
        }

        ajustarPantalla();
        Ubicar(servicio.latitud, servicio.longitud);
    }

    public MapaServicio(int servicio)
    {
        InitializeComponent();
        clientehttp = new HttpClient(httpHandler);
        Preferences.Set("mapa", true);
        Preferences.Set("sms", false);
        Preferences.Set("servicio", servicio);
        Restaurar(servicio);

        WeakReferenceMessenger.Default.Unregister<UbicarMessage>(this);
        WeakReferenceMessenger.Default.Register<UbicarMessage>(this, (r, m) =>
        {
            string[] pos = m.Value.Split('_');
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (pos[2] == "1")
                {
                    if (!cancelado)
                    {
                        WeakReferenceMessenger.Default.Send(new SincronizarMessage("2_1"));
                        //MessagingCenter.Send("2_1", "sincronizar");
                        Preferences.Set("sincronizando", "0");
                        await DisplayAlert("Aviso", "El servicio fue cancelado por el usuario", "Aceptar");
                        ((FlyoutPage)(Application.Current.MainPage as NavigationPage).CurrentPage).Detail = new NavigationPage(new HomeDetail());
                    }
                }
                else
                {
                    if (pos[0] != "null")
                    {
                        mymap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(double.Parse(pos[0]), double.Parse(pos[1])), Distance.FromMeters(100)));
                    }
                }
            });
        });
        
        /*MessagingCenter.Unsubscribe<string>(this, "ubicar");
        MessagingCenter.Subscribe<string>(this, "ubicar", ubi =>
        {
            
        });*/

        ajustarPantalla();
    }

    void ajustarPantalla()
    {
        if (Preferences.Get("escala", 0f) > 1f)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                stackmapa.MinimumHeightRequest = 1500;
                //stackmapa.ForceLayout();
                this.ForceLayout();
            });

        }
    }
    protected override bool OnBackButtonPressed()
    {
        return true;
    }

    private async void Restaurar(int servicio)
    {
        var content = new FormUrlEncodedContent(new[]
        {
                new KeyValuePair<string, string>("idservicio", servicio.ToString()),
                new KeyValuePair<string, string>("app", "aplicacion"),
            });

        try
        {
            var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/servicio_incompleto", content);
            this.servicio = JsonConvert.DeserializeObject<Servicio>(response.Content.ReadAsStringAsync().Result);
            if(this.servicio != null)
            {
                if (this.servicio.estado is "Cancelado" or "Finalizado")
                {
                    Preferences.Remove("servicio");
                    Preferences.Remove("estado");
                    WeakReferenceMessenger.Default.Send(new SincronizarMessage("2_1"));
                    //MessagingCenter.Send("2_1", "sincronizar");
                    Preferences.Set("sincronizando", "0");
                    await DisplayAlert("Aviso", "El servicio pendiente por finalizar fue cancelado por el usuario", "Aceptar");
                    ((FlyoutPage)((NavigationPage)Application.Current.MainPage).CurrentPage).Detail = new NavigationPage(new HomeDetail());
                }
                else
                {
                    lblnombrecliente.Text = this.servicio.usuarios;
                    lbldireccion.Text = this.servicio.direccion + "-" + this.servicio.adicional;
                    lblpago.Text = this.servicio.pago;

                    if (this.servicio.pago == "Vale electrónico")
                    {
                        lblempresa.Text = this.servicio.empresa;
                        lblempresa.IsVisible = true;
                    }

                    if (!string.IsNullOrEmpty(this.servicio.observaciones) || !string.IsNullOrEmpty(this.servicio.rutas))
                    {
                        if (string.IsNullOrEmpty(this.servicio.observaciones))
                        {
                            lblobservaciones.Text = this.servicio.rutas;
                        }
                        else
                        {
                            lblobservaciones.Text = this.servicio.observaciones + " &#10" + this.servicio.rutas;
                        }
                        lblobservaciones.IsVisible = true;
                    }

                    if (this.servicio.ruta != null)
                    {
                        BtnPasajeros.IsVisible = true;
                    }
                    if (this.servicio.pasajeros != null)
                    {
                        if (this.servicio.pasajeros.Length > 0)
                        {
                            BtnPasajeros.IsVisible = true;
                        }
                    }
                    if (!string.IsNullOrEmpty(this.servicio.contacto))
                    {
                        btncall.Text = btncall.Text + "   " + this.servicio.contacto;
                        btncall.IsVisible = true;
                    }

                    if (Preferences.Get("sincronizando", "-1").ToString() == "1")
                    {
                        WeakReferenceMessenger.Default.Send(new SincronizarMessage("2_1-1"));
                        //MessagingCenter.Send("2_1-1", "sincronizar");

                        switch (this.servicio.estado)
                        {
                            case "En curso":
                                btnarribo.IsVisible = false;
                                btnfin.IsVisible = true;
                                WeakReferenceMessenger.Default.Send(new SincronizarMessage("1_2-2"));
                                //MessagingCenter.Send("1_2-2", "sincronizar");
                                break;
                            default:
                                WeakReferenceMessenger.Default.Send(new SincronizarMessage("1_2-1"));
                                //MessagingCenter.Send("1_2-1", "sincronizar");
                                break;
                        }
                    }
                    else
                    {
                        switch (this.servicio.estado)
                        {
                            case "En curso":
                                btnarribo.IsVisible = false;
                                btnfin.IsVisible = true;
                                break;
                            default:
                                break;
                        }
                    }

                    Ubicar(this.servicio.latitud, this.servicio.longitud);
                }
            }
            
        }
        catch (Exception ex)
        {
            if (ex is JsonException)
            {
                Preferences.Remove("servicio");
                Preferences.Remove("estado");
                WeakReferenceMessenger.Default.Send(new SincronizarMessage("2_1-1"));
                //MessagingCenter.Send("2_1-1", "sincronizar");
                Preferences.Set("sincronizando", "0");
                await DisplayAlert("Aviso", "El servicio pendiente por finalizar ya fue atendido", "Aceptar");
                ((FlyoutPage)((NavigationPage)Application.Current.MainPage).CurrentPage).Detail = new NavigationPage(new HomeDetail());
            }
            else
            {
                await DisplayAlert("Problemas de conexión", "No se pudo recuperar el servicio pendiente. " + ex.Message, "Aceptar");
            }
        }
    }

    private async void Ubicar(double latitud, double longitud)
    {
        try
        {
            var posicion = new Location(latitud, longitud);
            mymap.MoveToRegion(MapSpan.FromCenterAndRadius(
                    posicion, Distance.FromMeters(100)));

            ubicacion = new Pin
            {
                Type = PinType.Place,
                Location = posicion,
                Label = "Localizado",
                Address = "Ubicación del servicio"
            };
            mymap.Pins.Add(ubicacion);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Problema detectado", "No se pudo ubicar el servicio en el mapa " + ex.Message, "Aceptar");
        }
    }

    private async void Btnarribo_Clicked(object sender, EventArgs e)
    {
        btnarribo.IsEnabled = false;
        var content = new FormUrlEncodedContent(new[]
        {
                new KeyValuePair<string, string>("idservicio", servicio.id.ToString()),
                new KeyValuePair<string, string>("app", "aplicacion"),
            });

        try
        {
            var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/arribo", content);
            string resul = response.Content.ReadAsStringAsync().Result;

            btnarribo.IsVisible = false;
            btnfin.IsVisible = true;
            WeakReferenceMessenger.Default.Send(new SincronizarMessage("2_1-1"));
            WeakReferenceMessenger.Default.Send(new SincronizarMessage("1_2-2"));
            //MessagingCenter.Send("2_1-1", "sincronizar");
            //MessagingCenter.Send("1_2-2", "sincronizar");

            if (resul == "OK")
            {
                Preferences.Set("iniciovale", DateTime.Now.ToString());
                DependencyService.Get<Interfaces.IToastService>().ShowToast("Se registró el arribo correctamente");
            }
            else
            {
                DependencyService.Get<Interfaces.IToastService>().ShowToast("No se pudo registrar el arribo en el servidor.");
            }
        }
        catch (Exception ex)
        {
            DependencyService.Get<Interfaces.IToastService>().ShowToast("No se pudo registrar el arribo en el servidor. " + ex.Message);
        }
    }

    private async void Btncancelar_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new Cancelar(servicio.id.ToString(), servicio.cuentasc_id.ToString()));
    }

    private async void Btnfin_Clicked(object sender, EventArgs e)
    {
        if (servicio.pago == "Vale electrónico")
        {
            if (servicio.valesavid != 0)
            {
                btnfin.IsEnabled = false;
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("idservicio", servicio.id.ToString()),
                    new KeyValuePair<string, string>("idtaxista", servicio.cuentasc_id),
                    new KeyValuePair<string, string>("app", "aplicacion"),
                });
                var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/avianca/finalizar", content);              

                if (response.IsSuccessStatusCode)
                {
                    string resul = response.Content.ReadAsStringAsync().Result;
                    string[] respuesta = resul.Split(new char[] { '_' });
                    if (respuesta[0] == "Falla icon")
                    {
                        btnfin.IsEnabled = true;
                        await DisplayAlert("Error integración", respuesta[1], "Aceptar");
                    }
                    else
                    {
                        WeakReferenceMessenger.Default.Send(new SincronizarMessage("2_1"));
                        //MessagingCenter.Send("2_1", "sincronizar");
                        Preferences.Set("sincronizando", "0");
                        Preferences.Remove("estado");
                        Preferences.Remove("servicio");
                        Preferences.Set("sms", true);

                        ((FlyoutPage)((NavigationPage)Application.Current.MainPage).CurrentPage).Detail = new NavigationPage(new Calificar(servicio.id));
                    }
                }
                else
                {
                    btnfin.IsEnabled = true;
                    await DisplayAlert("Error", "No se pudo registrar el fin del servicio. " + response.ReasonPhrase, "Aceptar");
                }
            }else if(servicio.CONTRATO_VALE != -1)
            {
                btnfin.IsEnabled = false;
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("idservicio", servicio.id.ToString()),
                    new KeyValuePair<string, string>("idtaxista", servicio.cuentasc_id),
                    new KeyValuePair<string, string>("cobro", "Rutas"),
                    new KeyValuePair<string, string>("app", "aplicacion"),
                });

                var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/finalizar/cobro_anticipado", content);

                if (response.IsSuccessStatusCode)
                {
                    string resul = response.Content.ReadAsStringAsync().Result;
                    string[] respuesta = resul.Split(new char[] { '_' });
                    if (respuesta[0] == "Falla icon")
                    {
                        btnfin.IsEnabled = true;
                        await DisplayAlert("Error integración", respuesta[1], "Aceptar");
                    }
                    else
                    {
                        WeakReferenceMessenger.Default.Send(new SincronizarMessage("2_1"));
                        //MessagingCenter.Send("2_1", "sincronizar");
                        Preferences.Set("sincronizando", "0");
                        Preferences.Remove("estado");
                        Preferences.Remove("servicio");
                        Preferences.Set("sms", true);

                        ((FlyoutPage)((NavigationPage)Application.Current.MainPage).CurrentPage).Detail = new NavigationPage(new Calificar(servicio.id));
                    }
                }
                else
                {
                    btnfin.IsEnabled = true;
                    await DisplayAlert("Error", "No se pudo registrar el fin del servicio. " + response.ReasonPhrase, "Aceptar");
                }
            }
            else
            {
                if (servicio.cobro == "Minutos")
                {
                    await Navigation.PushModalAsync(new Horas(servicio));
                }
                else if (servicio.cobro == "Ruta")
                {
                    var content = new FormUrlEncodedContent(new[]
                    {
                            new KeyValuePair<string, string>("idvale", servicio.valesid.ToString()),
                            new KeyValuePair<string, string>("app", "aplicacion"),
                        });

                    try
                    {
                        var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/rutas", content);

                        Ruta[]? rutas = JsonConvert.DeserializeObject<Ruta[]>(response.Content.ReadAsStringAsync().Result);

                        await Navigation.PushModalAsync(new Rutas(servicio, rutas));
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Problemas de conexión", "No se ha podido cargar las rutas habilitadas del cliente. Intente de nuevo. " + ex.Message, "Aceptar");
                    }
                }
                else
                {
                    await Navigation.PushModalAsync(new Unidades(servicio));
                }
            }
        }
        else
        {
            var content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("idservicio", servicio.id.ToString()),
                    new KeyValuePair<string, string>("idtaxista", servicio.cuentasc_id),
                    new KeyValuePair<string, string>("app", "aplicacion"),
                });

            try
            {
                var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/finalizar", content);

                string resul = response.Content.ReadAsStringAsync().Result;

                if (resul == "OK")
                {
                    WeakReferenceMessenger.Default.Send(new SincronizarMessage("2_1"));
                    //MessagingCenter.Send("2_1", "sincronizar");
                    Preferences.Set("sincronizando", "0");
                    Preferences.Remove("servicio");
                    Preferences.Remove("estado");
                    ((FlyoutPage)((NavigationPage)Application.Current.MainPage).CurrentPage).Detail = new NavigationPage(new Calificar(servicio.id));
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo registrar el fin del servicio. Intente de nuevo", "Aceptar");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Problemas de conexión", "No se ha podido registrar el fin del servicio en el servidor. " + ex.Message, "Aceptar");
            }
        }
    }

    private async void BtnPasajeros_Clicked(object sender, EventArgs e)
    {
        if (servicio.ruta != null)
        {
            await Navigation.PushModalAsync(new ModalPasajero(servicio.ruta.pasajeros));
        }
        else if (servicio.pasajeros.Length > 0) {
            await Navigation.PushModalAsync(new ModalPasajero(servicio.pasajeros));
        }
    }

    private void btncall_Clicked(object sender, EventArgs e)
    {
        try
        {
            PhoneDialer.Open(servicio.contacto);
        }
        catch (Exception) { }
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        bool toogle = ((Application.Current.MainPage as NavigationPage).CurrentPage as FlyoutPage).IsPresented;
        ((Application.Current.MainPage as NavigationPage).CurrentPage as FlyoutPage).IsPresented = !toogle;
    }
}