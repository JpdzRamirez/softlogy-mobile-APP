using Newtonsoft.Json;
using SoftlogyMaui.Interfaces;
using SoftlogyMaui.Modelos;

namespace SoftlogyMaui
{
    public partial class MainPage : ContentPage
    {
        HttpClientHandler httpHandler = new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        };
        private HttpClient clientehttp;
        public static string? placat;
        float versactual;
        string plataforma;

        public MainPage()
        {
            InitializeComponent();
            versactual = App.versiondroid;
            plataforma = "1";
            clientehttp = new HttpClient(httpHandler);
            imgtaxi.Source = ImageSource.FromFile("softlogy.png");

            /*if (Preferences.Get("estilo", "null") == "1")
            {
                stackmain.BackgroundColor = Colors.Black;
                lblrecordar.TextColor = Colors.White;
            }
            else
            {
                stackmain.BackgroundColor = Colors.White;
                lblrecordar.TextColor = Colors.Black;
            }*/

            //if (Preferences.Get("sesion", "no") == "si")
            //{
            //    Saltarlogin();
            //}
        }

        private async void Btnentrar_Clicked(object sender, EventArgs e)
        {
            string username, password;

            username = txtusername.Text;
            password = txtpassword.Text;

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                grillalogin.IsVisible = true;
                var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("ussername", username),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("plataforma", plataforma),
                new KeyValuePair<string, string>("iddroid", Preferences.Get("iddroid", "").ToString()),
                new KeyValuePair<string, string>("app", "aplicacion"),
            });
                try
                {
                    var response = await clientehttp.PostAsync(App.url + "microservicios/softlogy/login", content);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {

                        CuentaC? usuario = JsonConvert.DeserializeObject<CuentaC>(response.Content.ReadAsStringAsync().Result);

                        if (usuario?.version > versactual)
                        {
                            grillalogin.IsVisible = false;
                            await DisplayAlert("Versión antigua", "Es necesario actualizar la aplicación", "Aceptar");
                            DependencyService.Get<IOpenAppService>().Abrir("actualizar");
                        }
                        else
                        {
                            if (usuario?.password != null)
                            {
                                if (usuario.is_active == "0")
                                {
                                    grillalogin.IsVisible = false;
                                    await DisplayAlert("Usuario bloqueado", "Su usuario se encuentra Inactivo", "Aceptar");
                                }
                                else
                                {
                                    Preferences.Set("idusuario", usuario.id);
                                    Preferences.Set("cuenta", usuario.name);
                                    Preferences.Set("nombre", usuario.realname);
                                    Preferences.Set("apellido", usuario.firstname);
                                    Preferences.Set("telefono", usuario.phone);
                                    Preferences.Set("celular", usuario.mobile);
                                    Preferences.Set("password", usuario.password);
                                    Preferences.Set("estado", usuario.is_active);
                                    Preferences.Set("tickets", usuario.tickets);
                                    Preferences.Set("sesion", usuario.token);
                                    if (usuario.picture == null)
                                            {
                                                Preferences.Set("foto", "nofoto");
                                            }
                                            else
                                            {
                                                Preferences.Set("foto", usuario.picture);
                                            }

                                            if (checklogin.IsChecked)
                                            {
                                                Preferences.Set("sesion", "si");
                                                Preferences.Set("documento", usuario.name);
                                                Preferences.Set("pass", usuario.password);
                                            }

                                            grillalogin.IsVisible = false;
                                            Navigation.InsertPageBefore(new Home(), Navigation.NavigationStack[Navigation.NavigationStack.Count - 1]);
                                            await Navigation.PopAsync();
                                }
                            }
                            else
                            {
                                grillalogin.IsVisible = false;
                                await DisplayAlert("Credenciales incorrectas", "Ingresar su usuario y contraseña", "Aceptar");

                            }
                        }
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        grillalogin.IsVisible = false;
                        btnentrar.IsEnabled = true;
                        await DisplayAlert("Error", "El usuario ingresado no es válido.", "Aceptar");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        await DisplayAlert("Error", "Usuario no encontrado.", "Aceptar");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        await DisplayAlert("Error", "Credenciales inválidas.", "Aceptar");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        await DisplayAlert("Error", "El usuario está inactivo.", "Aceptar");
                    }
                    else
                    {
                        grillalogin.IsVisible = false;
                        await DisplayAlert("Error", "No ha sido posible conectarse. " + response.ReasonPhrase, "Aceptar");
                    }
                }
                catch (Exception ex)
                {
                    grillalogin.IsVisible = false;
                    if (ex is JsonException)
                    {
                        await DisplayAlert("Credenciales incorrectas", "Ingresar su usuario y contraseña", "Aceptar");
                    }
                    else
                    {
                        await DisplayAlert("Error", "No ha sido posible conectarse. " + ex.Message, "Aceptar");
                    }
                }
            }
            else
            {
                txtusername.PlaceholderColor = Colors.Red;
                txtpassword.PlaceholderColor = Colors.Red;
                await DisplayAlert("Completar campos", "Ingrese su usuario y contraseña", "Aceptar");
            }
        }
            private async void OnEntryFocused(object sender, FocusEventArgs e)
            {
                if (sender is Entry entry)
                {
                    if (entry == txtusername)
                    {
                        await lblusername.TranslateTo(0, -40, 150, Easing.CubicIn);
                        await lblusername.FadeTo(1, 150);
                        lblusername.FontSize = 22;
                    }
                    if (entry == txtpassword)
                    {
                        await lblpassword.TranslateTo(0, -40, 150, Easing.CubicIn);
                        await lblpassword.FadeTo(1, 150);
                        lblpassword.FontSize = 22;
                    }
                // Agregar lógica para otros `Entry` si es necesario 
            }
        }

            private async void OnEntryUnfocused(object sender, FocusEventArgs e)
            {
                if (sender is Entry entry)
                {
                    if (entry == txtusername && string.IsNullOrEmpty(entry.Text))
                    {
                        await lblusername.TranslateTo(0, 0, 150, Easing.CubicOut);
                        await lblusername.FadeTo(0.7, 150);
                        lblusername.FontSize = 22;
                    }
                    if (entry == txtpassword)
                    {
                        await lblpassword.TranslateTo(0, 0, 150, Easing.CubicOut);
                        await lblpassword.FadeTo(0.7, 150);
                        lblpassword.FontSize = 22;
                    }
                // Agregar lógica para otros `Entry` si es necesario
            }
            }

        //private async void Saltarlogin()
        //{
        //    string documento, clave;

        //    documento = Preferences.Get("documento", "12345");
        //    clave = Preferences.Get("pass", "0000");

        //    if (documento != "12345" && clave != "0000")
        //    {

        //        grillalogin.IsVisible = true;
        //        btnentrar.IsEnabled = false;
        //        var content = new FormUrlEncodedContent(new[]
        //        {
        //            new KeyValuePair<string, string>("documento", documento),
        //            new KeyValuePair<string, string>("password",  clave),
        //            new KeyValuePair<string, string>("plataforma", plataforma),
        //            new KeyValuePair<string, string>("app", "aplicacion"),
        //        });

        //        try
        //        {
        //            var response = await clientehttp.PostAsync(App.url + "aplicaciones/taxista/login", content);

        //            if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //            {
        //                CuentaC? taxista = JsonConvert.DeserializeObject<CuentaC>(response.Content.ReadAsStringAsync().Result);

        //                if (taxista?.version > versactual)
        //                {
        //                    grillalogin.IsVisible = false;
        //                    await DisplayAlert("Versión antigua", "Es necesario actualizar la aplicación", "Aceptar");
        //                    DependencyService.Get<IOpenAppService>().Abrir("actualizar");
        //                }
        //                else
        //                {
        //                    if (taxista?.password != null && taxista?.password == clave)
        //                    {
        //                        if (taxista.estado == "Bloqueado")
        //                        {
        //                            grillalogin.IsVisible = false;
        //                            btnentrar.IsEnabled = true;
        //                            await DisplayAlert("Usuario bloqueado", "Su usuario se encuentra bloqueado. Será habilitado hasta: " + taxista.fechabloqueo, "Aceptar");
        //                        }
        //                        else if (taxista.estado == "Inactivo")
        //                        {
        //                            grillalogin.IsVisible = false;
        //                            btnentrar.IsEnabled = true;
        //                            await DisplayAlert("Usuario inactivo", "Su usuario se encuentra inactivo", "Aceptar");
        //                        }
        //                        else if (taxista.estado == "Singleton")
        //                        {
        //                            grillalogin.IsVisible = false;
        //                            btnentrar.IsEnabled = true;
        //                            await DisplayAlert("Sesión ocupada", "Su cuenta está operando en otro dispositivo en este momento.", "Aceptar");
        //                        }
        //                        else
        //                        {
        //                            Preferences.Set("placas", taxista.placas);
        //                            string[] placas = taxista.placas.Split(new char[] { '_' });
        //                            if (placas.Length > 0)
        //                            {
        //                                if (!string.IsNullOrEmpty(placas[0]))
        //                                {
        //                                    Preferences.Set("ocupadas", taxista.ocupadas);
        //                                    Preferences.Set("placataxista", taxista.placa);
        //                                    Preferences.Set("idtaxista", taxista.id);
        //                                    Preferences.Set("doctaxista", taxista.identificacion);
        //                                    Preferences.Set("passtaxista", taxista.password);
        //                                    Preferences.Set("nombretaxista", taxista.nombre);
        //                                    Preferences.Set("saldortaxista", taxista.saldo);
        //                                    Preferences.Set("saldovtaxista", taxista.saldovales);
        //                                    Preferences.Set("estadotaxista", taxista.estado);
        //                                    Preferences.Set("bloqueotaxista", taxista.fechabloqueo);
        //                                    if (taxista.foto == null)
        //                                    {
        //                                        Preferences.Set("foto", "nofoto");
        //                                    }
        //                                    else
        //                                    {
        //                                        Preferences.Set("foto", taxista.foto);
        //                                    }

        //                                    if (checklogin.IsChecked)
        //                                    {
        //                                        Preferences.Set("sesion", "si");
        //                                        Preferences.Set("documento", taxista.identificacion);
        //                                        Preferences.Set("pass", taxista.password);
        //                                    }

        //                                    grillalogin.IsVisible = false;
        //                                    Navigation.InsertPageBefore(new Home(), Navigation.NavigationStack[Navigation.NavigationStack.Count - 1]);
        //                                    await Navigation.PopAsync();
        //                                }
        //                                else
        //                                {
        //                                    grillalogin.IsVisible = false;
        //                                    btnentrar.IsEnabled = true;
        //                                    await DisplayAlert("Sin vehículo", "Ninguno de los vehículos asignados están disponibles", "Aceptar");
        //                                }
        //                            }
        //                            else
        //                            {
        //                                grillalogin.IsVisible = false;
        //                                btnentrar.IsEnabled = true;
        //                                await DisplayAlert("Sin vehículo", "Ninguno de los vehículos asignados están disponibles", "Aceptar");
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        grillalogin.IsVisible = false;
        //                        btnentrar.IsEnabled = true;
        //                        await DisplayAlert("Credenciales incorrectas", "Ingresar su usuario y contraseña", "Aceptar");

        //                    }
        //                }
        //            }
        //            else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
        //            {
        //                grillalogin.IsVisible = false;
        //                btnentrar.IsEnabled = true;
        //                await DisplayAlert("Error", "El usuario ingresado no es válido.", "Aceptar");
        //            }
        //            else
        //            {
        //                grillalogin.IsVisible = false;
        //                btnentrar.IsEnabled = true;
        //                await DisplayAlert("Error", "No ha sido posible conectarse.", "Aceptar");
        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            grillalogin.IsVisible = false;
        //            btnentrar.IsEnabled = true;
        //            if (ex is JsonException)
        //            {
        //                await DisplayAlert("Credenciales incorrectas", "Ingresar su usuario y contraseña", "Aceptar");
        //            }
        //            else
        //            {
        //                await DisplayAlert("Error", "No ha sido posible conectarse. " + ex.Message, "Aceptar");
        //            }
        //        }
        //    }
        //}

        private void btnprivacidad_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new Privacidad());
        }
    }
}
