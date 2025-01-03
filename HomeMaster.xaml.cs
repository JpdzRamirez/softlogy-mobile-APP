namespace SoftlogyMaui;

public partial class HomeMaster : ContentPage
{
    public HomeMaster()
    {
        InitializeComponent();
        lblcuenta.Text = Preferences.Get("cuenta", "").ToString();
        lblemail.Text = Preferences.Get("email", "").ToString();

        string base64 = Preferences.Get("foto", "").ToString();

        if (base64 == "nofoto")
        {
            imgfoto.Source = ImageSource.FromResource("SoftlogyMaui.Recursos.foto.png", typeof(HomeMaster).Assembly);
        }
        else
        {
            byte[] data = Convert.FromBase64String(base64);
            imgfoto.Source = ImageSource.FromStream(() => new MemoryStream(data));
        }
    }
}