namespace SoftlogyMaui;

public partial class Privacidad : ContentPage
{
	public Privacidad()
	{
		InitializeComponent();
        Preferences.Set("mapa", false);
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        bool toogle = ((Application.Current.MainPage as NavigationPage).CurrentPage as FlyoutPage).IsPresented;
        ((Application.Current.MainPage as NavigationPage).CurrentPage as FlyoutPage).IsPresented = !toogle;
    }
}