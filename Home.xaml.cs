using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using TaxistasMaui.Modelos;

namespace TaxistasMaui;

public partial class Home : FlyoutPage
{
    HttpClientHandler httpHandler = new HttpClientHandler
    {
        AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
    };
    private HttpClient clientehttp;

    public Home()
    {
        InitializeComponent();
        clientehttp = new HttpClient(httpHandler);
        MasterPage.collectionView.SelectionChanged += OnSelectionChanged;

        var timer = Application.Current?.Dispatcher.CreateTimer();
        if (timer != null)
        {
            timer.Interval = TimeSpan.FromSeconds(30);
            timer.Tick += (s, e) =>
            {
                if (Preferences.Get("sms", false))
                {
                    Checkmsj();
                }
                else
                {
                    timer.Stop();
                }
            };
            timer.Start();
        }

        Preferences.Set("sms", true);
        Preferences.Set("salir", true);
        Preferences.Set("mapa", false);
        Preferences.Set("disponibles", 0);
        Preferences.Set("inicio", "1");
        Preferences.Set("sincronizando", "0");
        Preferences.Set("status", "Disponible  \uf058");
        Preferences.Set("boton", "0");
        ConsultaServicio();
    }

    protected override bool OnBackButtonPressed()
    {

        if (Preferences.Get("mapa", false))
        {
            return true;
        }
        else
        {
            if (Preferences.ContainsKey("servicio"))
            {
                int serv = Preferences.Get("servicio", 0);
                if (serv != 0)
                {
                    Detail = new NavigationPage(new MapaServicio(serv));
                }
            }
            else
            {
                Preferences.Set("disponibles", 0);
                Detail = new NavigationPage(new HomeDetail());
            }
        }
        return true;
    }

    void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection.FirstOrDefault() as FlyoutPageItem;
        if (item != null)
        {
            Mensajes.sincronizar = false;

            Preferences.Set("disponibles", 0);

            if (item.Id == 0)
            {
                if (Preferences.ContainsKey("servicio"))
                {
                    int serv = Preferences.Get("servicio", 0);
                    if (serv != 0)
                    {
                        Detail = new NavigationPage(new MapaServicio(serv));
                    }
                }
                else
                {
                    Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetType));
                }
            }
            else if (item.Id == 7)
            {
                WeakReferenceMessenger.Default.Send(new SincronizarMessage("2_1"));
                //MessagingCenter.Send("2_1", "sincronizar");
                Preferences.Remove("sesion");

                Preferences.Set("salir", false);
                HomeDetail.parar = false;
                Sincronizador.limite = 1;

                var content = new FormUrlEncodedContent(new[]
                {
                        new KeyValuePair<string, string>("idtaxista",Preferences.Get("idtaxista", 0).ToString()),
                        new KeyValuePair<string, string>("app", "aplicacion"),
                    });
                try
                {
                    clientehttp.PostAsync(App.url + "aplicaciones/taxista/logout", content);
                }
                catch (Exception)
                {

                }
                Navigation.InsertPageBefore(new MainPage(), Navigation.NavigationStack[Navigation.NavigationStack.Count - 1]);
                Navigation.PopAsync();
            }
            else
            {
                Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetType));
            }

            IsPresented = false;
        }
    }

    private async void Checkmsj()
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("idtaxista", Preferences.Get("idtaxista", 0).ToString()),
            new KeyValuePair<string, string>("app", "aplicacion"),
        });

        try
        {
            var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/sincro_chat", content);
            var resul = response.Content.ReadAsStringAsync().Result;

            if (resul != "0")
            {
                App.playerplop.Play();
                //Detail = new NavigationPage(new Mensajes());

                if (Preferences.Get("fondo", false).ToString() == "1")
                {
                    DependencyService.Get<Interfaces.IOpenAppService>().Abrir("abrir");
                }
            }
        }
        catch (Exception)
        {

        }
    }

    private async void ConsultaServicio()
    {

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("idtaxista", Preferences.Get("idtaxista", 0).ToString()),
            new KeyValuePair<string, string>("app", "aplicacion"),
        });

        try
        {
            var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/consultar_servicio", content);
            Servicio? serv = JsonConvert.DeserializeObject<Servicio>(response.Content.ReadAsStringAsync().Result);
            if (serv != null)
            {
                if (Preferences.Get("sincronizando", "-1").ToString() == "0")
                {
                    Preferences.Set("sincronizando", "1");
                }
                Console.WriteLine(serv.CONTRATO_VALE + "<------------------------------>" + serv.SECUENCIA);
                Detail = new NavigationPage(new MapaServicio(serv));
            }
            else
            {
                if (Preferences.ContainsKey("servicio"))
                {
                    Preferences.Remove("servicio");
                }
                Detail = new NavigationPage(new HomeDetail());
            }
        }
        catch (Exception e)
        {
            if (Preferences.ContainsKey("servicio"))
            {
                Preferences.Remove("servicio");
            }
            Detail = new NavigationPage(new HomeDetail());
        }

    }
}