using System.Collections;
using System.Globalization;
using SoftlogyMaui.Interfaces;

namespace SoftlogyMaui;

public partial class Recargar : ContentPage
{
    HttpClientHandler httpHandler = new HttpClientHandler
    {
        AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
    };
    private HttpClient clientehttp;
    int saldor, saldov;
    bool falla = false;
    ArrayList fechas = new ArrayList();
    public Recargar()
    {
        InitializeComponent();
        clientehttp = new HttpClient(httpHandler);
        Preferences.Set("mapa", false);
        /*if (Preferences.Get("estilo", "null") == "1")
        {
            stackrecarga.BackgroundColor = Colors.Black;
            lblsaldor.TextColor = Colors.White;
            lblsaldov.TextColor = Colors.White;
            lblnota.TextColor = Colors.White;
        }
        else
        {
            stackrecarga.BackgroundColor = Colors.White;
            lblsaldor.TextColor = Colors.Black;
            lblsaldov.TextColor = Colors.Black;
            lblnota.TextColor = Colors.Black;
        }*/

        if (Preferences.ContainsKey("saldortaxista") && Preferences.ContainsKey("saldovtaxista"))
        {
            saldor = Preferences.Get("saldortaxista", 0);
            saldov = Preferences.Get("saldovtaxista", 0);
        }
        else
        {
            DependencyService.Get<IToastService>().ShowToast("No se ha podido obtener sus saldos actuales");
            saldor = 0;
            saldov = 0;
        }
        saldor = Preferences.Get("saldortaxista", 0);
        saldov = Preferences.Get("saldovtaxista", 0);
        recargas.Text = saldor.ToString("C", CultureInfo.CurrentCulture);
        vales.Text = saldov.ToString("C", CultureInfo.CurrentCulture);
    }

    private async void btnrecargar_Clicked(object sender, EventArgs e)
    {
        btnrecargar.IsEnabled = false;
        if (!string.IsNullOrEmpty(txtvalor.Text))
        {
            if (int.Parse(txtvalor.Text) >= 13600 && int.Parse(txtvalor.Text) <= 100000 && falla == false)
            {
                fechas.Add(DateTime.Now);
                bool opt = await DisplayAlert("Recarga", "Realizar recarga por " + int.Parse(txtvalor.Text).ToString("C", CultureInfo.CurrentCulture) + " ?", "Aceptar", "Cancelar");
                if (fechas.Count > 1)
                {
                    TimeSpan span = DateTime.Now.Subtract((DateTime)fechas[0]);
                    if (span.Seconds < 10)
                    {
                        return;
                    }
                }
                if (opt)
                {
                    grilla.IsVisible = true;
                    var content = new FormUrlEncodedContent(new[]
                    {
                            new KeyValuePair<string, string>("idtaxista", Preferences.Get("idtaxista", 0).ToString()),
                            new KeyValuePair<string, string>("valor", txtvalor.Text),
                            new KeyValuePair<string, string>("app", "aplicacion"),
                        });

                    try
                    {
                        var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/recargar", content);
                        if (response.IsSuccessStatusCode)
                        {
                            string resul = response.Content.ReadAsStringAsync().Result;

                            string[] respuesta = resul.Split(new char[] { '_' });

                            grilla.IsVisible = false;
                            if (respuesta[0] == "saldo")
                            {
                                await DisplayAlert("Saldo insuficiente", "El valor ingresado es mayor que su saldo en vales", "Aceptar");
                            }
                            else if (respuesta[0] == "Falla icon")
                            {
                                falla = true;
                                await DisplayAlert("Error integración", respuesta[1], "Aceptar");
                            }
                            else
                            {
                                Preferences.Set("saldortaxista", saldor + int.Parse(txtvalor.Text));
                                Preferences.Set("saldovtaxista", saldov - int.Parse(txtvalor.Text));
                                DependencyService.Get<IToastService>().ShowToast("Recarga realizada satisfactoriamente");
                                ((FlyoutPage)((NavigationPage)Application.Current.MainPage).CurrentPage).Detail = new NavigationPage(new Recargar());
                            }
                        }
                        else
                        {
                            falla = true;
                            await DisplayAlert("Error integración", response.ReasonPhrase, "Aceptar");
                        }
                    }
                    catch (Exception)
                    {
                        grilla.IsVisible = false;
                        await DisplayAlert("Error de conexión", "No fue posible realizar la recarga.", "Aceptar");
                    }
                }
            }
            else
            {
                if (falla)
                {
                    await DisplayAlert("Falla en la recarga", "Por favor intente de nuevo en unos minutos", "Aceptar");
                }
                else
                {
                    await DisplayAlert("Valor fuera de rango", "El valor ingresado debe estar entre $13.600 y $100.000.", "Aceptar");
                }
            }
        }
        else
        {
            txtvalor.PlaceholderColor = Colors.Red;
            DependencyService.Get<IToastService>().ShowToast("Ingresar valor de la recarga para continuar");
        }
        btnrecargar.IsEnabled = true;
        grilla.IsVisible = false;
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        bool toogle = ((Application.Current.MainPage as NavigationPage).CurrentPage as FlyoutPage).IsPresented;
        ((Application.Current.MainPage as NavigationPage).CurrentPage as FlyoutPage).IsPresented = !toogle;
    }
}