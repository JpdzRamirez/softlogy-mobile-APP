using Plugin.Maui.Audio;
using TaxistasMaui.Platforms.Droid;

namespace TaxistasMaui
{
    public partial class App : Application
    {
        //public static string url = "https://crm.apptaxcenter.com/";
        public static string url = "http://192.168.1.12:8080/";
        public static bool fondo = false;
        public static string? latitud;
        public static string? longitud;
        public static IAudioPlayer? playernoti;
        public static IAudioPlayer? playerplop;
        public static float versiondroid = 58;

        public App()
        {
            InitializeComponent();
            #if ANDROID
                DependencyService.Register<ToastService>();
                DependencyService.Register<VolumeService>();
                DependencyService.Register<OpenAppService>();
                DependencyService.Register<BatteryService>();
            #endif

            CargarSonidos();

            if (!Preferences.ContainsKey("estilo"))
            {
                Preferences.Set("estilo", "0");
            }

            MainPage = new NavigationPage(new MainPage());
        }

        private async void CargarSonidos()
        {
            playernoti = AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("noti.mp3"));
            playerplop = AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("plop.mp3"));
            playernoti.Volume = 1;
            playerplop.Volume = 1;
        }

        protected override void OnStart()
        {
            Preferences.Set("fondo", "0");
        }

        protected override void OnSleep()
        {
            Preferences.Set("fondo", "1");
        }

        protected override void OnResume()
        {
            Preferences.Set("fondo", "0");
        }
    }
}
