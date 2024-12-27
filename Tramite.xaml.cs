namespace SoftlogyMaui;

public partial class Tramite : ContentPage
{
    HttpClientHandler httpHandler = new HttpClientHandler
    {
        AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
    };
    private HttpClient clientehttp;
    public Tramite()
    {
        InitializeComponent();
        clientehttp = new HttpClient(httpHandler);
        string[] docs = { "Certificación de Vinculación", "Constancia de Ingresos" };
        foreach (var item in docs)
        {
            picdoc.Items.Add(item);
        }
        picdoc.SelectedIndex = 0;
        cedula.Text = Preferences.Get("doctaxista", "").ToString();
        nombre.Text = Preferences.Get("nombretaxista", "").ToString();

        /*if (Preferences.Get("estilo", "null") == "1")
        {
            gridtramite.BackgroundColor = Colors.Black;
            lblsolicitud.TextColor = Colors.White;
            lblplaca.TextColor = Colors.White;
            lblcelular.TextColor = Colors.White;
            lblemail.TextColor = Colors.White;
            lbldirigida.TextColor = Colors.White;
            lblmonto.TextColor = Colors.White;
            lblcedula.TextColor = Colors.White;
            lblnombre.TextColor = Colors.White;
        }
        else
        {
            gridtramite.BackgroundColor = Colors.White;
            lblsolicitud.TextColor = Colors.Black;
            lblplaca.TextColor = Colors.Black;
            lblcelular.TextColor = Colors.Black;
            lblemail.TextColor = Colors.Black;
            lbldirigida.TextColor = Colors.Black;
            lblmonto.TextColor = Colors.Black;
            lblcedula.TextColor = Colors.Black;
            lblnombre.TextColor = Colors.Black;
        }*/
    }

    private async void btnsolicitar_Clicked(object sender, EventArgs e)
    {
        grilla.IsVisible = true;
        string solicitud = picdoc.SelectedItem.ToString();
        string placa = editplaca.Text;
        string email = editemail.Text;
        string celular = editcelular.Text;
        string monto = editmonto.Text;
        string dirigido = editdirigido.Text;
        FormUrlEncodedContent? contenido = null;

        if (!string.IsNullOrEmpty(solicitud) && !string.IsNullOrEmpty(placa) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(celular))
        {
            if (placa.Length == 7)
            {
                if (placa[3] == '-')
                {
                    if (solicitud == "Constancia de Ingresos")
                    {
                        if (!string.IsNullOrEmpty(monto) || !string.IsNullOrEmpty(dirigido))
                        {
                            contenido = new FormUrlEncodedContent(new[]
                            {
                                    new KeyValuePair<string, string>("idtaxista", Preferences.Get("idtaxista", 0).ToString()),
                                    new KeyValuePair<string, string>("solicitud", solicitud),
                                    new KeyValuePair<string, string>("placa", placa),
                                    new KeyValuePair<string, string>("monto", monto),
                                    new KeyValuePair<string, string>("dirigido", dirigido),
                                    new KeyValuePair<string, string>("email", email),
                                    new KeyValuePair<string, string>("celular", celular),
                                    new KeyValuePair<string, string>("app", "aplicacion")
                                });
                        }
                        else
                        {
                            await DisplayAlert("Completar datos", "Debe diligenciar todos los campos", "Aceptar");
                        }
                    }
                    else
                    {
                        contenido = new FormUrlEncodedContent(new[]
                        {
                                new KeyValuePair<string, string>("idtaxista", Preferences.Get("idtaxista", 0).ToString()),
                                new KeyValuePair<string, string>("solicitud", solicitud),
                                new KeyValuePair<string, string>("placa", placa),
                                new KeyValuePair<string, string>("email", email),
                                new KeyValuePair<string, string>("celular", celular),
                                new KeyValuePair<string, string>("app", "aplicacion")
                            });
                    }
                }
                else
                {
                    editplaca.Text = "";
                    await DisplayAlert("Placa incorrecta", "Escribir la placa con el siguiente formato ABC-123", "Aceptar");
                }
            }
            else
            {
                editplaca.Text = "";
                await DisplayAlert("Placa incorrecta", "Escribir la placa con el siguiente formato ABC-123", "Aceptar");
            }
        }
        else
        {
            await DisplayAlert("Completar datos", "Debe diligenciar todos los campos", "Aceptar");
        }

        if (contenido != null)
        {
            var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/tramite", contenido);
            if (response.IsSuccessStatusCode)
            {
                string[] resul = response.Content.ReadAsStringAsync().Result.Split('-');
                if (resul.Length == 2)
                {
                    if (resul[0] == "0")
                    {
                        await DisplayAlert("Solicitud realizada", "Tras la validación de datos. El documento solicitado será enviado al correo electrónico suministrado.", "Aceptar");
                        ((FlyoutPage)((NavigationPage)Application.Current.MainPage).CurrentPage).Detail = new NavigationPage(new HomeDetail());
                    }
                    else
                    {
                        await DisplayAlert("La solicitud no fue realizada", resul[1], "Aceptar");
                    }
                }
                else
                {
                    await DisplayAlert("La solicitud no fue realizada", "Bad Response", "Aceptar");
                }
            }
            else
            {
                await DisplayAlert("La solicitud no fue realizada", response.ReasonPhrase, "Aceptar");
            }
        }
        grilla.IsVisible = false;
    }

    private void picdoc_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (picdoc.SelectedItem.ToString() == "Certificación de Vinculación")
        {
            stackmonto.IsVisible = false;
        }
        else
        {
            stackmonto.IsVisible = true;
        }
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        bool toogle = ((Application.Current.MainPage as NavigationPage).CurrentPage as FlyoutPage).IsPresented;
        ((Application.Current.MainPage as NavigationPage).CurrentPage as FlyoutPage).IsPresented = !toogle;
    }
}