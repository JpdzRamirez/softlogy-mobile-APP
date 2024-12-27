using CommunityToolkit.Mvvm.Messaging;
using System.Globalization;
using SoftlogyMaui.Modelos;

namespace SoftlogyMaui.Modales;

public partial class Rutas : ContentPage
{
    HttpClientHandler httpHandler = new HttpClientHandler
    {
        AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
    };
    Servicio servicio;
    Ruta[] rutas;
    private HttpClient clientehttp;
    NumberFormatInfo nfi = new CultureInfo("es-CO", false).NumberFormat;

    public Rutas(Servicio servicio, Ruta[] rutas)
    {
        InitializeComponent();
        nfi.CurrencyDecimalDigits = 0;
        clientehttp = new HttpClient(httpHandler);
        this.servicio = servicio;
        this.rutas = rutas;
        txtrutavale.Keyboard = Keyboard.Create(KeyboardFlags.CapitalizeCharacter);

        if (servicio.valesid != 0)
        {
            lblrutavale.IsVisible = true;
            txtrutavale.IsVisible = true;
            txtrutavale.Text = servicio.passvale;
        }

        foreach (var ruta in rutas)
        {
            selectrutas.Items.Add(ruta.origen + "-" + ruta.destino);
        }

        if (rutas.Length > 0)
        {
            selectrutas.SelectedIndex = 0;
        }
    }

    private async void btnnorutas_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void btnokrutas_Clicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(txtrutavale.Text) && selectrutas.SelectedIndex != -1)
        {
            btnokrutas.IsEnabled = false;
            grillacobrar.IsVisible = true;
            string ruta = rutas[selectrutas.SelectedIndex].origen + "-" + rutas[selectrutas.SelectedIndex].destino;
            string clave = txtrutavale.Text;
            int contrato = rutas[selectrutas.SelectedIndex].contrato_vale;
            int secuencia = rutas[selectrutas.SelectedIndex].secuencia;

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("idservicio", servicio.id.ToString()),
                new KeyValuePair<string, string>("idtaxista", servicio.cuentasc_id.ToString()),
                new KeyValuePair<string, string>("cobro", "Rutas"),
                new KeyValuePair<string, string>("contrato", contrato.ToString()),
                new KeyValuePair<string, string>("secuencia", secuencia.ToString()),
                new KeyValuePair<string, string>("clavevale", clave),
                new KeyValuePair<string, string>("unidades", ruta),
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
                        btnokrutas.IsEnabled = true;
                        grillacobrar.IsVisible = false;
                        await DisplayAlert("Contraseña incorrecta", "La contraseña ingresada no corresponde a la del vale solicitado", "Aceptar");
                    }
                    else if (respuesta[0] == "Falla icon")
                    {
                        btnokrutas.IsEnabled = true;
                        grillacobrar.IsVisible = false;
                        await DisplayAlert("Error integración", respuesta[1], "Aceptar");

                    }
                    else if (respuesta[0] == "Cobro bloqueado")
                    {
                        btnokrutas.IsEnabled = true;
                        grillacobrar.IsVisible = false;
                        await DisplayAlert("Cobro bloqueado", respuesta[1], "Aceptar");
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
                        btnokrutas.IsEnabled = true;
                        grillacobrar.IsVisible = false;
                        await DisplayAlert("Advertencia", respuesta[1], "Aceptar");
                    }
                    else
                    {
                        grillacobrar.IsVisible = false;
                        bool decision = await DisplayAlert("Finalizar Servicio", "El servicio se finalizará por " + int.Parse(resul).ToString("C", nfi) + " ¿Está seguro de finalizarlo con ese valor?", "Aceptar", "Cancelar");

                        if (decision)
                        {
                            btnokrutas.IsEnabled = false;
                            grillacobrar.IsVisible = true;
                            Finservicio(content);
                        }
                        else
                        {
                            btnokrutas.IsEnabled = true;
                        }
                    }
                }
                else
                {
                    btnokrutas.IsEnabled = true;
                    grillacobrar.IsVisible = false;
                    await DisplayAlert("Error", response.ReasonPhrase, "Aceptar");
                }

            }
            catch (Exception)
            {
                btnokrutas.IsEnabled = true;
                await DisplayAlert("Problemas de conexión", "No se ha podido consultar el valor del servicio en el servidor. Intente de nuevo", "Aceptar");
            }
        }
        else
        {
            await DisplayAlert("Campos obligatorios", "Debe ingresar la contraseña del vale y las unidades del servicio", "Aceptar");
        }
    }

    private async void Finservicio(FormUrlEncodedContent content)
    {
        try
        {
            var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/finalizar", content);

            if (response.IsSuccessStatusCode)
            {
                string resul = response.Content.ReadAsStringAsync().Result;

                string[] respuesta = resul.Split(new char[] { '_' });

                if (respuesta[0] == "Clave incorrecta")
                {
                    btnokrutas.IsEnabled = true;
                    grillacobrar.IsVisible = false;
                    await DisplayAlert("Contraseña incorrecta", "La contraseña ingresada no corresponde a la del vale solicitado", "Aceptar");
                }
                else if (respuesta[0] == "Falla icon")
                {
                    btnokrutas.IsEnabled = true;
                    grillacobrar.IsVisible = false;
                    await DisplayAlert("Error integración", respuesta[1], "Aceptar");
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
                btnokrutas.IsEnabled = true;
                grillacobrar.IsVisible = false;
                await DisplayAlert("Error", response.ReasonPhrase, "Aceptar");
            }

        }
        catch (Exception)
        {
            btnokrutas.IsEnabled = true;
            await DisplayAlert("Problemas de conexión", "No se ha podido consultar el valor del servicio en el servidor. Intente de nuevo", "Aceptar");
        }
    }
}