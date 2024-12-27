namespace TaxistasMaui;

public partial class WebPage : ContentPage
{
	public WebPage()
	{
		InitializeComponent();
        //weblogin.Source = "http://192.168.3.70:8000/autologin_conductor/" + App.Current.Properties["doctaxista"].ToString();
        weblogin.Source = "http://credit.cahors.co/autologin_conductor/" + Preferences.Get("doctaxista", "0").ToString();
        weblogin.Reload();
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        bool toogle = ((Application.Current.MainPage as NavigationPage).CurrentPage as FlyoutPage).IsPresented;
        ((Application.Current.MainPage as NavigationPage).CurrentPage as FlyoutPage).IsPresented = !toogle;
    }
}