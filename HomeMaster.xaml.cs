namespace TaxistasMaui;

public partial class HomeMaster : ContentPage
{
    public HomeMaster()
    {
        InitializeComponent();
        lbluser.Text = Preferences.Get("doctaxista", "").ToString();
        lblnombres.Text = Preferences.Get("nombretaxista", "").ToString();

        string base64 = Preferences.Get("foto", "").ToString();

        if (base64 == "nofoto")
        {
            imgfoto.Source = ImageSource.FromResource("TaxistasMaui.Recursos.foto.png", typeof(HomeMaster).Assembly);
        }
        else
        {
            byte[] data = Convert.FromBase64String(base64);
            imgfoto.Source = ImageSource.FromStream(() => new MemoryStream(data));
        }
    }
}