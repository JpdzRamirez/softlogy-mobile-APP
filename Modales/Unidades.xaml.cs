using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using System.Collections;
using System.Globalization;
using TaxistasMaui.Modelos;

namespace TaxistasMaui.Modales;

public partial class Unidades : ContentPage
{
    HttpClientHandler httpHandler = new HttpClientHandler
    {
        AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
    };
    Servicio servicio;
    private HttpClient clientehttp;
    NumberFormatInfo nfi = new CultureInfo("es-CO", false).NumberFormat;
    ArrayList fechas = new ArrayList();

    public Unidades(Servicio servicio)
    {
        InitializeComponent();
        nfi.CurrencyDecimalDigits = 0;
        clientehttp = new HttpClient(httpHandler);
        this.servicio = servicio;
        txtunivale.Keyboard = Keyboard.Create(KeyboardFlags.CapitalizeCharacter);

        if (servicio.valesid != 0)
        {
            lblunivale.IsVisible = true;
            txtunivale.IsVisible = true;
            txtunivale.Text = servicio.passvale;
        }

        if (servicio.cobro == "Unidades")
        {
            btnmedios.IsVisible = false;
        }
    }

    private async void btnokunidades_Clicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(txtunivale.Text) && !string.IsNullOrEmpty(txtunidades.Text))
        {
            btnokunidades.IsEnabled = false;
            grillacobrar.IsVisible = true;
            int unidades = int.Parse(txtunidades.Text);
            string clave = txtunivale.Text;

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("idservicio", servicio.id.ToString()),
                new KeyValuePair<string, string>("idtaxista", servicio.cuentasc_id.ToString()),
                new KeyValuePair<string, string>("cobro", "Unidades"),
                new KeyValuePair<string, string>("clavevale", clave),
                new KeyValuePair<string, string>("unidades", unidades.ToString()),
                new KeyValuePair<string, string>("undsato", check.IsChecked.ToString()),
                new KeyValuePair<string, string>("app", "aplicacion"),
                });

            try
            {
                var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/prefinalizar", content);

                if (response.IsSuccessStatusCode)
                {
                    string resul = response.Content.ReadAsStringAsync().Result;
                    string[] respuesta = resul.Split(new char[] { '_' });

                    if (respuesta[0] == "Clave incorrecta")
                    {
                        btnokunidades.IsEnabled = true;
                        grillacobrar.IsVisible = false;
                        await DisplayAlert("Contraseña incorrecta", "La contraseña ingresada no corresponde a la del vale solicitado", "Aceptar");
                    }
                    else if (respuesta[0] == "Falla icon")
                    {
                        btnokunidades.IsEnabled = true;
                        grillacobrar.IsVisible = false;
                        await DisplayAlert("Error integración", respuesta[1], "Aceptar");
                    }
                    else if (respuesta[0] == "Exceso")
                    {
                        btnokunidades.IsEnabled = true;
                        grillacobrar.IsVisible = false;
                        await DisplayAlert("Exceso de unidades", respuesta[1], "Aceptar");
                    }
                    else if (respuesta[0] == "Finalizado")
                    {
                        grillacobrar.IsVisible = false;
                        await Navigation.PopModalAsync();
                        WeakReferenceMessenger.Default.Send(new SincronizarMessage("2_1"));
                        //MessagingCenter.Send("2_1", "sincronizar");
                        Preferences.Set("sincronizando", "0");
                        Preferences.Remove("servicio");
                        Preferences.Remove("estado");
                        await DisplayAlert("Servicio finalizado", "Este servicio ya fue finalizado", "Aceptar");

                        ((FlyoutPage)((NavigationPage)Application.Current.MainPage).CurrentPage).Detail = new NavigationPage(new HomeDetail());
                    }
                    else if (respuesta[0] == "Cobro")
                    {
                        btnokunidades.IsEnabled = true;
                        grillacobrar.IsVisible = false;
                        await DisplayAlert("Advertencia", respuesta[1], "Aceptar");
                    }
                    else
                    {
                        grillacobrar.IsVisible = false;
                        bool decision = await DisplayAlert("Finalizar Servicio", "El servicio se finalizará por " + int.Parse(resul).ToString("C", nfi) + " ¿Está seguro de finalizarlo con ese valor?", "Aceptar", "Cancelar");

                        if (decision)
                        {
                            btnokunidades.IsEnabled = false;
                            grillacobrar.IsVisible = true;
                            fechas.Add(DateTime.Now);
                            Finservicio(content);
                        }
                        else
                        {
                            btnokunidades.IsEnabled = true;
                        }
                    }
                }
                else
                {
                    btnokunidades.IsEnabled = true;
                    grillacobrar.IsVisible = false;
                    await DisplayAlert("Error", response.ReasonPhrase, "Aceptar");
                }

            }
            catch (Exception ex)
            {
                btnokunidades.IsEnabled = true;
                grillacobrar.IsVisible = false;
                await DisplayAlert("Problemas de conexión", ex.Message, "Aceptar");
            }
        }
        else
        {
            grillacobrar.IsVisible = false;
            await DisplayAlert("Campos obligatorios", "Debe ingresar la contraseña del vale y las unidades del servicio", "Aceptar");
        }
    }
    private async void Finservicio(FormUrlEncodedContent content)
    {
        if (fechas.Count > 1)
        {
            TimeSpan span = DateTime.Now.Subtract((DateTime)fechas[0]);
            if (span.Seconds < 10)
            {
                return;
            }
        }

        try
        {
            var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/finalizar", content);

            if (response.IsSuccessStatusCode)
            {
                string resul = response.Content.ReadAsStringAsync().Result;
                string[] respuesta = resul.Split(new char[] { '_' });

                if (respuesta[0] == "Clave incorrecta")
                {
                    btnokunidades.IsEnabled = true;
                    grillacobrar.IsVisible = false;
                    await DisplayAlert("Contraseña incorrecta", "La contraseña ingresada no corresponde a la del vale solicitado", "Aceptar");
                }
                else if (respuesta[0] == "Falla icon")
                {
                    btnokunidades.IsEnabled = true;
                    grillacobrar.IsVisible = false;
                    await DisplayAlert("Error integración", respuesta[1], "Aceptar");
                }
                else if (respuesta[0] == "Exceso")
                {
                    btnokunidades.IsEnabled = true;
                    grillacobrar.IsVisible = false;
                    await DisplayAlert("Exceso de unidades", respuesta[1], "Aceptar");
                }
                else if (respuesta[0] == "Finalizado")
                {
                    grillacobrar.IsVisible = false;
                    await Navigation.PopModalAsync();
                    WeakReferenceMessenger.Default.Send(new SincronizarMessage("2_1"));
                    //MessagingCenter.Send("2_1", "sincronizar");
                    Preferences.Set("sincronizando", "0");
                    Preferences.Remove("servicio");
                    Preferences.Remove("estado");
                    await DisplayAlert("Servicio finalizado", "Este servicio ya fue finalizado", "Aceptar");

                    ((FlyoutPage)((NavigationPage)Application.Current.MainPage).CurrentPage).Detail = new NavigationPage(new HomeDetail());
                }
                else
                {
                    grillacobrar.IsVisible = false;
                    WeakReferenceMessenger.Default.Send(new SincronizarMessage("2_1"));
                    //MessagingCenter.Send("2_1", "sincronizar");
                    Preferences.Set("sincronizando", "0");

                    Preferences.Remove("estado");
                    Preferences.Remove("servicio");
                    Preferences.Set("sms", true);

                    int servicio = Preferences.Get("servicio", 0);
                    await DisplayAlert("Servicio Finalizado", "El valor del servicio fue: " + int.Parse(resul).ToString("C", nfi), "Aceptar");
                    ((FlyoutPage)((NavigationPage)Application.Current.MainPage).CurrentPage).Detail = new NavigationPage(new Calificar(servicio));
                    await Navigation.PopModalAsync();
                }
            }
            else
            {
                btnokunidades.IsEnabled = true;
                grillacobrar.IsVisible = false;
                await DisplayAlert("Error", response.ReasonPhrase, "Aceptar");
            }

        }
        catch (Exception)
        {
            btnokunidades.IsEnabled = true;
            grillacobrar.IsVisible = false;
            await DisplayAlert("Problemas de conexión", "No se ha podido consultar el valor del servicio en el servidor. Intente de nuevo", "Aceptar");
        }
    }

    private async void btnnounidades_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void btnmedios_Clicked(object sender, EventArgs e)
    {
        string cobro = await DisplayActionSheet("Método de cobro", "Cancelar", null, new string[] { "Unidades", "Horas", "Ruta" });
        await Navigation.PopModalAsync();

        if (cobro == "Unidades")
        {
            await ((FlyoutPage)((NavigationPage)Application.Current.MainPage).CurrentPage).Detail.Navigation.PushModalAsync(new Unidades(servicio));
        }
        else if (cobro == "Horas")
        {
            await ((FlyoutPage)((NavigationPage)Application.Current.MainPage).CurrentPage).Detail.Navigation.PushModalAsync(new Horas(servicio));
        }
        else if (cobro == "Ruta")
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

                await ((FlyoutPage)((NavigationPage)Application.Current.MainPage).CurrentPage).Detail.Navigation.PushModalAsync(new Rutas(servicio, rutas));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Problemas de conexión", "No se ha podido cargar las rutas habilitadas del cliente. Intente de nuevo. " + ex.Message, "Aceptar");
            }
        }
    }
}