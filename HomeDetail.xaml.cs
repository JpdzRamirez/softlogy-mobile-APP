using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using Plugin.Connectivity;
using System.Globalization;
using TaxistasMaui.Interfaces;
using TaxistasMaui.Modelos;

namespace TaxistasMaui;

public partial class HomeDetail : ContentPage
{
    HttpClientHandler httpHandler = new HttpClientHandler
    {
        AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
    };
    private HttpClient clientehttp;
    public static bool parar = true;
    int saldor, saldov, inicio;
    string[] placas, ocupadas;

    public HomeDetail()
    {
        InitializeComponent();
        clientehttp = new HttpClient(httpHandler);
        UpdateThemeIcon();
        string nombre, doc;
        nombre = Preferences.Get("nombretaxista", "");
        doc = "\uf007 Usuario: " + Preferences.Get("doctaxista", "");
        placas = Preferences.Get("placas", "").ToString().Split(['_']);
        ocupadas = Preferences.Get("ocupadas", "").ToString().Split(['_']);
        lblnombre.Text = nombre;
        lbldocumento.Text = doc;

        foreach (var item in placas)
        {
            if (!string.IsNullOrEmpty(item))
            {
                picplaca.Items.Add(item);
            }

        }
        foreach (var item in ocupadas)
        {
            if (!string.IsNullOrEmpty(item))
            {
                picplaca.Items.Add(item);
            }
        }
        definirPlaca();
        parar = true;

        /*if (Preferences.Get("estilo", "null") == "1")
        {
            stackdetail.BackgroundColor = Colors.Black;
            lblnombre.TextColor = Colors.White;
            lbldocumento.TextColor = Colors.White;
            lblplaca.TextColor = Colors.White;
            picplaca.TextColor = Colors.White;
            lblestado.TextColor = Colors.White;
            lblinfored.TextColor = Colors.White;
            lblinfogps.TextColor = Colors.White;
        }
        else
        {
            stackdetail.BackgroundColor = Colors.White;
            lblnombre.TextColor = Colors.Black;
            lbldocumento.TextColor = Colors.Black;
            lblplaca.TextColor = Colors.Black;
            picplaca.TextColor = Colors.Black;
            lblestado.TextColor = Colors.Black;
            lblinfored.TextColor = Colors.Black;
            lblinfogps.TextColor = Colors.Black;
        }*/

        CheckEstado();
        Saldos();
        var timer = Application.Current?.Dispatcher.CreateTimer();
        if(timer != null)
        {
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => {
                if (parar)
                {
                    Conexion();
                }
                else
                {
                    timer.Stop();
                }
            };
            timer.Start();
        }
        

        if (Preferences.Get("inicio", "-1").ToString() == "2")
        {
            VerificarServicio();
        }

        CheckAndRequestLocationPermission();
        ajustarPantalla();
    }

    void ajustarPantalla()
    {
        if (Preferences.Get("escala", -1f) > 1)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                stackdetail.HeightRequest = 1500;
                //stackdetail.ForceLayout();
                this.ForceLayout();
            });

        }
    }

    private async void Saldos()
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("id", Preferences.Get("idtaxista", 0).ToString()),
            new KeyValuePair<string, string>("app", "aplicacion"),
        });
        try
        {
            var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/saldos", content);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                CuentaC? taxista = JsonConvert.DeserializeObject<CuentaC>(response.Content.ReadAsStringAsync().Result);
                if (taxista != null)
                {
                    Preferences.Set("saldortaxista", taxista.saldo);
                    Preferences.Set("saldovtaxista", taxista.saldovales);
                    saldor = taxista.saldo;
                    saldov = taxista.saldovales;

                    lblsaldorecargas.Text = "Saldo Recargas: " + saldor.ToString("C", CultureInfo.CurrentCulture);
                    lblsaldovales.Text = "Saldo Vales: " + saldov.ToString("C", CultureInfo.CurrentCulture);
                } 
            }
            else
            {
                DependencyService.Get<IToastService>().ShowToast("No se ha podido obtener sus saldos actuales");
            }
        }
        catch (Exception)
        {
            DependencyService.Get<IToastService>().ShowToast("No se ha podido obtener sus saldos actuales");
        }
    }

    private async void Btnalerta_Clicked(object sender, EventArgs e)
    {
        string tipo = await DisplayActionSheet("Enviar alerta", "Cancelar", null, new string[] { "Ambulancia", "Bomberos", "Grúa", "Policia", "Pánico", "Transbordo", "Monitoreo especial" });

        if (!string.IsNullOrEmpty(tipo))
        {
            if (tipo != "Cancelar")
            {
                try
                {
                    Location? location = await Geolocation.GetLocationAsync();
                    string lat, lon;
                    lat = location.Latitude.ToString("G", CultureInfo.InvariantCulture);
                    lon = location.Longitude.ToString("G", CultureInfo.InvariantCulture);

                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("latitud", lat ),
                        new KeyValuePair<string, string>("longitud", lon),
                        new KeyValuePair<string, string>("tipo", tipo),
                        new KeyValuePair<string, string>("id", Preferences.Get("idtaxista", 0).ToString()),
                        new KeyValuePair<string, string>("placa",Preferences.Get("placataxista", "").ToString()),
                        new KeyValuePair<string, string>("app", "aplicacion"),
                    });

                    var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/alerta", content);

                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        await DisplayAlert("Error de conexión", "No se pudo enviar la alerta", "Aceptar");
                    }
                    else
                    {
                        await DisplayAlert("Alerta enviada", "La alerta fue enviada exitosamente", "Aceptar");
                    }
                }
                catch (Exception ex)
                {
                    if (ex is FeatureNotEnabledException)
                    {
                        await DisplayAlert("Error", "No ha sido posible obtener su ubicación. Encender GPS. " + ex.Message, "Aceptar");
                    }
                    else if (ex is PermissionException)
                    {
                        await DisplayAlert("Error", "No ha sido posible obtener su ubicación." + ex.Message, "Aceptar");
                    }
                    else
                    {
                        await DisplayAlert("Error de conexión", "No se pudo enviar la alerta. " + ex.Message, "Aceptar");
                    }
                }
            }
        }
    }

    private void definirPlaca()
    {
        inicio = 0;
        if (Preferences.ContainsKey("placataxista"))
        {
            for (int i = 0; i < placas.Length; i++)
            {
                if (placas[i] == Preferences.Get("placataxista", "").ToString())
                {
                    picplaca.SelectedIndex = i;
                }
            }
        }
        else
        {
            picplaca.SelectedIndex = 0;
        }
        inicio = 1;
    }

    private async void Picplaca_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (inicio != 0)
        {
            string cplaca = picplaca.SelectedItem.ToString();
            var content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("idtaxista", Preferences.Get("idtaxista", 0).ToString()),
                    new KeyValuePair<string, string>("placa", cplaca),
                    new KeyValuePair<string, string>("app", "aplicacion"),
                });

            try
            {
                var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/cambiar_placa", content);

                string respuesta = response.Content.ReadAsStringAsync().Result;
                if (respuesta == "OK")
                {
                    Preferences.Set("placataxista", cplaca);
                }
                else
                {
                    await DisplayAlert("Vehículo ocupado", "El vehículo con la placa " + cplaca + " no se encuentra disponible", "Aceptar");
                    definirPlaca();
                }
            }
            catch (Exception ex)
            {
                definirPlaca();
                DependencyService.Get<IToastService>().ShowToast("No ha sido posible registrar el cambio de placa. " + ex.Message);
            }
        }
    }

    private async void Btnestado_Clicked(object sender, EventArgs e)
    {
        FormUrlEncodedContent content;
        string estado;
        Color color;

        if (btnestado.Text == "Ocupado  \uf05e")
        {
            estado = "Disponible  \uf058";
            color = Colors.Green;
            Preferences.Set("sms", true);
            WeakReferenceMessenger.Default.Send(new SincronizarMessage("1_1-1"));
            //MessagingCenter.Send("1_1-1", "sincronizar");
            content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("idtaxista", Preferences.Get("idtaxista", 0).ToString()),
                    new KeyValuePair<string, string>("estado", "Libre"),
                    new KeyValuePair<string, string>("app", "aplicacion"),
                });
        }
        else
        {
            estado = "Ocupado  \uf05e";
            color = Colors.Red;
            Preferences.Set("sms", false);
            WeakReferenceMessenger.Default.Send(new SincronizarMessage("2_1"));
            //MessagingCenter.Send("2_1", "sincronizar");
            content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("idtaxista", Preferences.Get("idtaxista", 0).ToString()),
                    new KeyValuePair<string, string>("estado", "Ocupado Propio"),
                    new KeyValuePair<string, string>("app", "aplicacion"),
                });
        }

        try
        {
            var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/estado", content);
            btnestado.Text = estado;
            btnestado.BackgroundColor = color;
            Preferences.Set("status", estado);
        }
        catch (Exception)
        {
            await DisplayAlert("Error de conexión", "No ha sido posible registrar el cambio de estado.", "Aceptar");
        }
    }

    private void CheckEstado()
    {
        if (Preferences.Get("status", "").ToString() == "Disponible  \uf058")
        {
            btnestado.Text = "Disponible  \uf058";
            btnestado.BackgroundColor = Colors.Green;
        }
        else
        {
            btnestado.Text = "Ocupado  \uf05e";
            btnestado.BackgroundColor = Colors.Red;
        }
    }

    private async void Btngestiones_Clicked(object sender, EventArgs e)
    {
        string tipo = await DisplayActionSheet("Gestiones", "Cancelar", null, new string[] { "Enturnamiento", "Solicitar préstamo" });

        if (tipo == "Solicitar préstamo")
        {
            await Navigation.PushAsync(new WebPage());
        }
        else
        {
            DependencyService.Get<IToastService>().ShowToast("Este módulo estará disponible próximamente");
        }
    }

    private async void Conexion()
    {

        if (!CrossConnectivity.Current.IsConnected)
        {
            lblinternet.Text = "\uf057";
            lblinternet.TextColor = Colors.Red;
        }
        else
        {
            lblinternet.Text = "\uf058";
            lblinternet.TextColor = Colors.Green;

            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();

                if (location == null)
                {
                    lblgeo.Text = "\uf057";
                    lblgeo.TextColor = Colors.Red;
                }
                else
                {
                    lblgeo.Text = "\uf058";
                    lblgeo.TextColor = Colors.Green;
                }
            }
            catch (Exception ex)
            {
                if (ex is FeatureNotEnabledException)
                {
                    lblgeo.Text = "\uf057";
                    lblgeo.TextColor = Colors.Red;
                }
                else if (ex is PermissionException)
                {
                    lblgeo.Text = "\uf057";
                    lblgeo.TextColor = Colors.Red;
                }
                else
                {
                    lblinternet.Text = "\uf057";
                    lblinternet.TextColor = Colors.Red;
                }
            }
        }
    }
    private void VerificarServicio()
    {
        if (Preferences.ContainsKey("servicio"))
        {
            int serv = Preferences.Get("servicio", 0);
            if (serv != 0)
            {
                Preferences.Set("sincronizando", "1");
                ((Application.Current.MainPage as NavigationPage).CurrentPage as FlyoutPage).Detail = new NavigationPage(new MapaServicio(serv));
            }
        }
    }

    public async void CheckAndRequestLocationPermission()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            bool resp = await Application.Current.MainPage.DisplayAlert("Permiso de ubicación", "Esta aplicación recopila datos de ubicación para habilitar la recepción de servicios de taxi por geolocalización y almacenar el recorrido realizado durante el servicio, incluso cuando la aplicación está cerrada o no está en uso", "Aceptar", "Cancelar");
            if (resp)
            {
                status = await Permissions.RequestAsync<Permissions.LocationAlways>();
            }

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status == PermissionStatus.Granted)
                {
                    if (Preferences.Get("sincronizando", "-1").ToString() == "0")
                    {
                        WeakReferenceMessenger.Default.Send(new SincronizarMessage("1_1-1"));
                        //MessagingCenter.Send("1_1-1", "sincronizar");
                    }
                    DependencyService.Get<IToastService>().ShowToast("La aplicación solo obtendra acceso a la ubicación mientras está abierta");
                }
                else
                {
                    DependencyService.Get<IToastService>().ShowToast("La aplicación no funcionará a plenitud sin acceso a la ubicación");
                }
            }
            else
            {
                if (Preferences.Get("sincronizando", "-1").ToString() == "0")
                {
                    WeakReferenceMessenger.Default.Send(new SincronizarMessage("1_1-1"));
                    //MessagingCenter.Send("1_1-1", "sincronizar");
                }
            }
        }
        else
        {
            if (!Preferences.ContainsKey("bateria"))
            {
                Preferences.Set("bateria", "bateria");
                bool resultBat = DependencyService.Get<IBatteryService>().RevisarOptimizacion();
                if (!resultBat)
                {
                    bool resp = await Application.Current.MainPage.DisplayAlert("Ahorro de energía activado", "Esta aplicación necesita ser exceptuada del ahorro de energía para funcionar adecuadamente en segundo plano.", "Aceptar", "Cancelar");
                    if (resp)
                    {
                        DependencyService.Get<IBatteryService>().AbrirOptimizacion();
                    }
                }
            }
            else
            {
                if (Preferences.Get("inicio", "-1").ToString() == "1")
                {
                    ServiciosRapido();
                }
            }
        }

        WeakReferenceMessenger.Default.Send(new SincronizarMessage("1_1-1"));
    }

    private async void ServiciosRapido()
    {
        try
        {
            var location = await Geolocation.GetLocationAsync();

            if (location != null)
            {
                string lat, lon;
                lat = location.Latitude.ToString("G", CultureInfo.InvariantCulture);
                lon = location.Longitude.ToString("G", CultureInfo.InvariantCulture);
                App.latitud = lat;
                App.longitud = lon;

                var content = new FormUrlEncodedContent(new[]
                {
                        new KeyValuePair<string, string>("latitud", lat),
                        new KeyValuePair<string, string>("longitud", lon),
                        new KeyValuePair<string, string>("id", Preferences.Get("idtaxista", 0).ToString()),
                        new KeyValuePair<string, string>("app", "aplicacion"),
                    });

                var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/servicios", content);

                if (response.IsSuccessStatusCode)
                {
                    Servicio[]? servicios = JsonConvert.DeserializeObject<Servicio[]>(response.Content.ReadAsStringAsync().Result);

                    if (servicios?.Length != Preferences.Get("disponibles", 0))
                    {
                        if (servicios?.Length != 0)
                        {
                            DependencyService.Get<IUpVolume>().subirVolumen();
                            Preferences.Set("disponibles", servicios.Length);
                            App.playernoti.Play();
                            Preferences.Set("inicio", "2");

                            if (servicios.Length == 1)
                            {
                                if ((Application.Current.MainPage as NavigationPage).Navigation.ModalStack.Count > 0)
                                {
                                    await (Application.Current.MainPage as NavigationPage).Navigation.PopModalAsync();
                                }
                                await (Application.Current.MainPage as NavigationPage).Navigation.PushModalAsync(new ModalAceptar(servicios[0]));
                            }
                            else
                            {
                                ((Application.Current.MainPage as NavigationPage).CurrentPage as FlyoutPage).Detail = new NavigationPage(new ServiciosDisponibles(servicios));                            
                            }
                        }
                    }
                    else
                    {
                        var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
                        if (status != PermissionStatus.Granted)
                        {
                            bool resp = await Application.Current.MainPage.DisplayAlert("Permiso de notificaciones", "Esta aplicación requiere permiso para notificar y asi poder avisar cuando se detecten servicios cercanos", "Aceptar", "Cancelar");
                            if (resp)
                            {
                                status = await Permissions.RequestAsync<Permissions.PostNotifications>();
                            }
                            if (status != PermissionStatus.Granted)
                            {
                                DependencyService.Get<IToastService>().ShowToast("La aplicación no podrá notificar los servicios cercanos");
                            }
                        }

                        WeakReferenceMessenger.Default.Send(new SincronizarMessage("1_1-1"));
                    }
                }
            }
        }
        catch (Exception)
        {

        }
    }
    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        bool toogle = ((Application.Current.MainPage as NavigationPage).CurrentPage as FlyoutPage).IsPresented;
        ((Application.Current.MainPage as NavigationPage).CurrentPage as FlyoutPage).IsPresented = !toogle;
    }
    private void UpdateThemeIcon()
    {
       
        if (Application.Current.UserAppTheme == AppTheme.Dark)
        {
            ThemeToggleIcon.Text = "\uf186"; // Código HTML para el ícono de luna
        }
        else
        {
            ThemeToggleIcon.Text = "\uf185"; // Código HTML para el ícono de sol
        }
    }

    private void OnThemeToggleTapped(object sender, EventArgs e)
    {
        // Alterna entre modo claro y oscuro
        if (Application.Current.UserAppTheme == AppTheme.Dark)
        {
            Application.Current.UserAppTheme = AppTheme.Light;
        }
        else
        {
            Application.Current.UserAppTheme = AppTheme.Dark;
        }

        UpdateThemeIcon(); // Actualiza el ícono después de cambiar el tema
    }
}