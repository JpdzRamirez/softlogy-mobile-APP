using Newtonsoft.Json;
using System.Text.RegularExpressions;
using TaxistasMaui.Modelos;

namespace TaxistasMaui;

public partial class Mensajes : ContentPage
{
    HttpClientHandler httpHandler = new HttpClientHandler
    {
        AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
    };
    private HttpClient clientehttp;
    public static bool sincronizar = true;
    Color estilo;

    public Mensajes()
    {
        InitializeComponent();
        clientehttp = new HttpClient(httpHandler);
        Preferences.Set("mapa", false);
        /*if (Preferences.Get("estilo", "null") == "1")
        {
            stacksms.BackgroundColor = Colors.Black;
            estilo = Colors.White;
        }
        else
        {
            stacksms.BackgroundColor = Colors.White;
            estilo = Colors.Black;
        }*/
        CargarChat();
    }

    protected override void OnDisappearing()
    {
        sincronizar = false;
        Preferences.Set("sms", true);
        //Home.sms = true;
    }

    private async void CargarChat()
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("idtaxista", Preferences.Get("idtaxista", 0).ToString()),
            new KeyValuePair<string, string>("app", "aplicacion"),
        });

        try
        {
            var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/chat_historial", content);
            Mensaje[]? historial = JsonConvert.DeserializeObject<Mensaje[]>(response.Content.ReadAsStringAsync().Result);
            if(historial != null && historial.Length > 0)
            {
                for (int i = historial.Length - 1; i >= 0; i--)
                {
                    var label = new Label();
                    string[] res = MakeLink(historial[i].texto);
                    label.Text = res[0] + ".  (" + historial[i].fecha + ")";
                    label.FontSize = 22;
                    label.FontAttributes = FontAttributes.Bold;
                    label.WidthRequest = 250;
                    label.TextColor = estilo;
                    label.Margin = new Thickness(0, 0, 0, 10);
                    if (res[1] == "1")
                    {
                        label.TextType = TextType.Html;
                        label.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await Launcher.OpenAsync(new Uri(res[2]))) });
                    }

                    if (historial[i].sentido == "Enviado")
                    {
                        label.BackgroundColor = Colors.LightGreen;
                        label.HorizontalOptions = LayoutOptions.EndAndExpand;
                        label.HorizontalTextAlignment = TextAlignment.End;
                    }
                    else
                    {
                        label.BackgroundColor = Colors.LightBlue;
                        label.HorizontalOptions = LayoutOptions.StartAndExpand;
                        label.HorizontalTextAlignment = TextAlignment.Start;
                    }
                    stack.Children.Add(label);
                }

                await scroll.ScrollToAsync(0, stack.Children[stack.Children.Count - 1].AnchorY, false);
            }  
        }
        catch (Exception)
        {
            await DisplayAlert("Error de conexión", "No se pudo cargar los mensajes.", "Aceptar");
        }
    }

    protected string[] MakeLink(string txt)
    {
        Regex regx = new Regex(@"((http|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)", RegexOptions.IgnoreCase);
        MatchCollection mactches = regx.Matches(txt);
        string[] res = new string[3];
        res[1] = "0";
        if (mactches.Count > 0)
        {
            res[1] = "1";
            res[2] = mactches[0].Value;
            txt = txt.Replace(res[2], "<a href='" + res[2] + "'>" + res[2] + "</a>");
        }

        res[0] = txt;

        return res;
    }

    private async void Btnenviar_Clicked(object sender, EventArgs e)
    {
        btnenviar.IsEnabled = false;
        if (!string.IsNullOrEmpty(txttexto.Text))
        {
            var content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("idtaxista", Preferences.Get("idtaxista", 0).ToString()),
                    new KeyValuePair<string, string>("texto", txttexto.Text),
                    new KeyValuePair<string, string>("placa", Preferences.Get("placataxista", "").ToString()),
                    new KeyValuePair<string, string>("app", "aplicacion"),
                });

            try
            {
                var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/nuevo_mensaje", content);

                if (response.Content.ReadAsStringAsync().Result == "OK")
                {
                    App.playerplop.Play();

                    var label = new Label();
                    label.Text = txttexto.Text;
                    label.TextColor = estilo;
                    label.FontSize = 22;
                    label.FontAttributes = FontAttributes.Bold;
                    label.WidthRequest = 250;
                    label.Margin = new Thickness(0, 0, 0, 10);
                    label.BackgroundColor = Colors.LightBlue;
                    label.HorizontalOptions = LayoutOptions.EndAndExpand;
                    label.HorizontalTextAlignment = TextAlignment.End;

                    txttexto.Text = "";

                    stack.Children.Add(label);

                    await scroll.ScrollToAsync(0, label.Y, true);

                    btnenviar.IsEnabled = true;
                }
                else
                {
                    btnenviar.IsEnabled = true;
                    await DisplayAlert("Error de conexión", "No se pudo enviar el mensaje.", "Aceptar");
                }
            }
            catch (Exception ex)
            {
                btnenviar.IsEnabled = true;
                await DisplayAlert("Error de conexión", "No se pudo enviar el mensaje. " + ex.Message, "Aceptar");
            }
        }
        else
        {
            btnenviar.IsEnabled = true;
            DependencyService.Get<Interfaces.IToastService>().ShowToast("Escribe un mensaje primero");
        }
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        bool toogle = ((Application.Current.MainPage as NavigationPage).CurrentPage as FlyoutPage).IsPresented;
        ((Application.Current.MainPage as NavigationPage).CurrentPage as FlyoutPage).IsPresented = !toogle;
    }
}