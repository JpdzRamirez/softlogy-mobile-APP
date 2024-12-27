using Newtonsoft.Json;
using System.Net.Http.Headers;
using TaxistasMaui.Interfaces;
using TaxistasMaui.Modelos;
using Plugin.Media;
using Plugin.Media.Abstractions;
using CommunityToolkit.Mvvm.Messaging;

namespace TaxistasMaui;

public partial class Cuenta : ContentPage
{
    HttpClientHandler httpHandler = new HttpClientHandler
    {
        AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
    };
    private HttpClient clientehttp;
    bool imagen = false;
    Stream? stream;
    public Cuenta()
    {
        InitializeComponent();
        clientehttp = new HttpClient(httpHandler);
        Preferences.Set("mapa", false);
        //Home.mapa = false;
        /*if (Preferences.Get("estilo", "null") == "1")
        {
            stackcuenta.BackgroundColor = Colors.Black;
            lblcelular.TextColor = Colors.White;
            lbldireccion.TextColor = Colors.White;
            lblfijo.TextColor = Colors.White;
            lblemail.TextColor = Colors.White;
            lblpass.TextColor = Colors.White;
        }
        else
        {
            stackcuenta.BackgroundColor = Colors.White;
            lblcelular.TextColor = Colors.Black;
            lbldireccion.TextColor = Colors.Black;
            lblfijo.TextColor = Colors.Black;
            lblemail.TextColor = Colors.Black;
            lblpass.TextColor = Colors.Black;
        }*/
        CargarDatos();

        CheckAndRequestStoragePermission();
    }

    private async void CargarDatos()
    {
        var content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("idtaxista", Preferences.Get("idtaxista", 0).ToString()),
                    new KeyValuePair<string, string>("app", "aplicacion"),
                });
        try
        {
            var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/informacion", content);

            Conductor? conductor = JsonConvert.DeserializeObject<Conductor>(response.Content.ReadAsStringAsync().Result);
            if(conductor != null)
            {
                txtcelular.Text = conductor.celular;
                txtdireccion.Text = conductor.direccion;
                txtfijo.Text = conductor.telefono;
                txtemail.Text = conductor.email;

                imgfoto.Source = null;

                if (conductor.foto != null)
                {
                    string base64 = conductor.foto;
                    byte[] data = Convert.FromBase64String(base64);

                    imgfoto.Source = ImageSource.FromStream(() => new MemoryStream(data));
                }
                else
                {
                    imgfoto.Source = ImageSource.FromResource("TaxistasMaui.Recursos.foto.png", typeof(Cuenta).Assembly);
                }
            }  
        }
        catch (Exception ex)
        {
            await DisplayAlert("Problemas de conexión", "No ha sido posible cargar los datos de su cuenta " + ex.Message, "Aceptar");
        }
    }

    private async void Btnguardar_Clicked(object sender, EventArgs e)
    {
        btnguardar.IsEnabled = false;
        grilla.IsVisible = true;
        MultipartFormDataContent contentmultipart = new MultipartFormDataContent();

        contentmultipart.Add(new StringContent(Preferences.Get("idtaxista", 0).ToString()), "idtaxista");

        if (!string.IsNullOrEmpty(txtcelular.Text))
        {
            contentmultipart.Add(new StringContent(txtcelular.Text), "celular");
        }
        if (!string.IsNullOrEmpty(txtfijo.Text))
        {
            contentmultipart.Add(new StringContent(txtfijo.Text), "telefono");
        }
        if (!string.IsNullOrEmpty(txtemail.Text))
        {
            contentmultipart.Add(new StringContent(txtemail.Text), "email");
        }
        if (!string.IsNullOrEmpty(txtdireccion.Text))
        {
            contentmultipart.Add(new StringContent(txtdireccion.Text), "direccion");
        }
        if (!string.IsNullOrEmpty(txtclave.Text))
        {
            contentmultipart.Add(new StringContent(txtclave.Text), "clave");
        }
        contentmultipart.Add(new StringContent("aplicacion"), "app");

        if (imagen && stream != null)
        {
            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            var imageContent = new ByteArrayContent(memoryStream.ToArray());
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
            contentmultipart.Add(imageContent, "foto", "fototaxista.jpg");
        }

        try
        {
            var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/update_datos", contentmultipart);
            if (imagen && stream != null)
            {
                stream.Close();
            }

            grilla.IsVisible = false;
            if (response.IsSuccessStatusCode)
            {
                DependencyService.Get<IToastService>().ShowToast("Datos actualizados correctamente");
                ((FlyoutPage)((NavigationPage)Application.Current.MainPage).CurrentPage).Detail = new NavigationPage(new Cuenta());
            }
            else
            {
                btnguardar.IsEnabled = true;
                await DisplayAlert("Problemas de conexión", "No ha sido posible actualizar sus datos " + response.StatusCode, "Aceptar");
            }
        }
        catch (Exception ex)
        {
            btnguardar.IsEnabled = true;
            await DisplayAlert("Problemas de conexión", "No ha sido posible actualizar sus datos." + ex.Message, "Aceptar");
        }
    }

    private async void Changefoto_Clicked(object sender, EventArgs e)
    {

        changefoto.IsEnabled = false;

        await CrossMedia.Current.Initialize();

        if (!CrossMedia.Current.IsPickPhotoSupported)
        {
            await DisplayAlert("No soportado", "Tu dispositivo no permite la selección de fotos", "Aceptar");
            return;
        }

        var mediaoptions = new PickMediaOptions()
        {
            CompressionQuality = 50
        };

        var foto = await CrossMedia.Current.PickPhotoAsync(mediaoptions);

        if (foto != null)
        {
            stream = foto.GetStream();
            imgfoto.Source = ImageSource.FromStream(() => foto.GetStream());
            imagen = true;
        }

        changefoto.IsEnabled = true;
    }

    public async void CheckAndRequestStoragePermission()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
        if (status != PermissionStatus.Granted)
        {
            bool resp = await DisplayAlert("Permiso de almacenamiento", "Esta aplicación requiere acceso al almacenamiento para habilitar la selección de una foto para su cuenta", "Aceptar", "Cancelar");
            if (resp)
            {
                status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            }

            if (status == PermissionStatus.Denied)
            {
                DependencyService.Get<IToastService>().ShowToast("Sin acceso al almacenamiento no podrá seleccionar una de sus fotos");
            }
        }
    }

    private async void Btneliminar_Clicked(object sender, EventArgs e)
    {
        if (Preferences.ContainsKey("servicio"))
        {
            int serv = Preferences.Get("servicio", 0);
            if (serv != 0)
            {
                await DisplayAlert("Servicio en curso", "Asegurese de finalizar el servicio que tiene en curso antes de borrar su cuenta", "Aceptar");
                return;
            }
        }

        grilla.IsVisible = true;
        var respuesta = await DisplayAlert("Eliminar Cuenta", "¿Está seguro de eliminar su cuenta? ", "Aceptar", "Cancelar");
        if (respuesta)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("idtaxista", Preferences.Get("idtaxista", 0).ToString()),
                new KeyValuePair<string, string>("app", "aplicacion"),
            });

            try
            {
                var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/eliminarCuenta", content);

                grilla.IsVisible = false;
                if (response.IsSuccessStatusCode)
                {
                    if (response.Content.ReadAsStringAsync().Result == "Eliminado")
                    {
                        WeakReferenceMessenger.Default.Send(new SincronizarMessage("2_1"));
                        //MessagingCenter.Send("2_1", "sincronizar");
                        Preferences.Set("salir", false);
                        HomeDetail.parar = false;
                        Sincronizador.limite = 1;
                        if (Preferences.ContainsKey("sesion"))
                        {
                            Preferences.Remove("sesion");
                        }

                        await DisplayAlert("Cuenta borrada", "Su cuenta fue borrada, se redirigirá al inicio de la aplicación", "Aceptar");
                        Navigation.InsertPageBefore(new MainPage(), Navigation.NavigationStack[Navigation.NavigationStack.Count - 1]);
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        await DisplayAlert("Error", "No ha sido posible borrar los datos de su cuenta", "Aceptar");
                    }
                }
                else
                {
                    await DisplayAlert("Problemas de conexión", "No ha sido posible borrar los datos de su cuenta " + response.ReasonPhrase, "Aceptar");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "No ha sido posible borrar los datos de su cuenta " + ex.Message, "Aceptar");
            }
        }
        else
        {
            grilla.IsVisible = false;
        }
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        bool toogle = ((Application.Current.MainPage as NavigationPage).CurrentPage as FlyoutPage).IsPresented;
        ((Application.Current.MainPage as NavigationPage).CurrentPage as FlyoutPage).IsPresented = !toogle;
    }
}