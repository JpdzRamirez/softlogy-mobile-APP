using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using System.Globalization;
using TaxistasMaui.Interfaces;
using TaxistasMaui.Modelos;

namespace TaxistasMaui
{
    public class Sincronizador
    {
        HttpClientHandler httpHandler = new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        };
        private HttpClient? clientehttp;
        public static int limite;

        public async Task RunSincro(CancellationToken token, string funcion)
        {
            clientehttp = new HttpClient(httpHandler);
            string[] ops = funcion.Split('-');
            limite = 0;
            Preferences.Set("sincronizando", ops[0]);
            await Task.Run(async () =>
            {
                if (ops[0] == "1")
                {
                    for (long i = 0; i < long.MaxValue; i++)
                    {
                        token.ThrowIfCancellationRequested();

                        await Task.Delay(25000);
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

                                    if(servicios != null)
                                    {
                                        if (servicios.Length != Preferences.Get("disponibles", 0))
                                        {
                                            MainThread.BeginInvokeOnMainThread(() =>
                                            {
                                                if (servicios.Length != 0)
                                                {
                                                    DependencyService.Get<IUpVolume>().subirVolumen();
                                                    Preferences.Set("disponibles", servicios.Length);
                                                    //Home.disponibles = servicios.Length;
                                                    App.playernoti.Play();

                                                    if (limite == 0)
                                                    {
                                                        if (servicios.Length == 1)
                                                        {
                                                            if ((Application.Current.MainPage as NavigationPage).Navigation.ModalStack.Count > 0)
                                                            {
                                                                (Application.Current.MainPage as NavigationPage).Navigation.PopModalAsync();
                                                            }
                                                            (Application.Current.MainPage as NavigationPage).Navigation.PushModalAsync(new ModalAceptar(servicios[0]));
                                                        }
                                                        else
                                                        {
                                                            ((Application.Current.MainPage as NavigationPage).CurrentPage as FlyoutPage).Detail = new NavigationPage(new ServiciosDisponibles(servicios));                                                         
                                                        }

                                                        if (Preferences.Get("fondo", "0").ToString() == "1")
                                                        {
                                                            DependencyService.Get<IOpenAppService>().Abrir("abrir");
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    Preferences.Set("disponibles", 0);
                                                    if ((Application.Current.MainPage as NavigationPage).Navigation.ModalStack.Count > 0)
                                                    {
                                                        (Application.Current.MainPage as NavigationPage).Navigation.PopModalAsync();
                                                    }
                                                    else
                                                    {
                                                        ((FlyoutPage)((NavigationPage)Application.Current.MainPage).CurrentPage).Detail = new NavigationPage(new HomeDetail());
                                                    }
                                                }
                                            });
                                        }
                                    }
                                    
                                }
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
                else if (ops[0] == "2")
                {
                    KeyValuePair<string, string>[]? valores = null;
                    string lat="", lon="";
                    Location? location;
                    for (long i = 0; i < long.MaxValue; i++)
                    {
                        token.ThrowIfCancellationRequested();

                        await Task.Delay(60000);

                        try
                        {
                            location = await Geolocation.GetLocationAsync();
                            if(location != null) {
                                lat = location.Latitude.ToString("G", CultureInfo.InvariantCulture);
                                lon = location.Longitude.ToString("G", CultureInfo.InvariantCulture);
                            }

                            if (ops[1] == "2")
                            {
                                valores = new KeyValuePair<string, string>[5];
                            }
                            else
                            {
                                valores = new KeyValuePair<string, string>[4];
                            }
                            valores[0] = new KeyValuePair<string, string>("idservicio", Preferences.Get("idservicio", "0").ToString());
                            valores[1] = new KeyValuePair<string, string>("app", "aplicacion");
                            valores[2] = new KeyValuePair<string, string>("latitud", lat);
                            valores[3] = new KeyValuePair<string, string>("longitud", lon);
                            if (ops[1] == "2")
                            {
                                valores[4] = new KeyValuePair<string, string>("seguir", "si");
                            }

                            var content = new FormUrlEncodedContent(valores);
                            var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/revisar_servicio", content);

                            Servicio? serv = JsonConvert.DeserializeObject<Servicio>(response.Content.ReadAsStringAsync().Result);
                            if (serv?.estado == "Cancelado")
                            {
                                Preferences.Remove("servicio");
                                Preferences.Remove("estado");
                                WeakReferenceMessenger.Default.Send(new UbicarMessage("0_0_1"));
                                //MessagingCenter.Send("0_0_1", "ubicar");
                            }
                            else
                            {
                                if (location == null)
                                {
                                    WeakReferenceMessenger.Default.Send(new UbicarMessage("null_null_0"));
                                    //MessagingCenter.Send("null_null_0", "ubicar");
                                }
                                else
                                {
                                    WeakReferenceMessenger.Default.Send(new UbicarMessage(location.Latitude + "_" + location.Longitude + "_0"));
                                    //MessagingCenter.Send(location.Latitude + "_" + location.Longitude + "_0", "ubicar");
                                }
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }, token);
        }
    }
}
