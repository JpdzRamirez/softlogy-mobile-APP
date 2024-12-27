using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using static Android.OS.PowerManager;
using Android.Provider;
using TaxistasMaui.Platforms.Droid;
using CommunityToolkit.Mvvm.Messaging;
using TaxistasMaui.Modelos;

namespace TaxistasMaui
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public const int codigo = 101;
        private WakeLock? wakeLock;
        Intent? intent;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);

            Window?.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);

            intent = new Intent(this, typeof(SincroService));

            WeakReferenceMessenger.Default.Register<SincronizarMessage>(this, (r, m) =>
            {
                string[] msj = m.Value.Split('_');
                intent.PutExtra("funcion", msj[1]);
                if (msj[0] == "1")
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        #pragma warning disable CA1416 // Validar la compatibilidad de la plataforma
                        StartForegroundService(intent);
                        #pragma warning restore CA1416 // Validar la compatibilidad de la plataforma
                    }
                    else
                    {
                       StartService(intent);
                    }
                }
                else
                {
                    StopService(intent);
                }
            });

            /*MessagingCenter.Subscribe<string>(this, "sincronizar", message =>
            {
                
            });*/

            #pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
            PowerManager? powerManager = GetSystemService(PowerService) as PowerManager;
            wakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial, "My Lock");
            wakeLock.Acquire();

            Preferences.Set("escala", Resources.Configuration.FontScale);
            #pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.
            Preferences.Set("iddroid", Settings.Secure.GetString(Android.App.Application.Context.ContentResolver, Settings.Secure.AndroidId));
        }
    }
}
