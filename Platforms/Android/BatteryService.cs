using Android.Content;
using Android.OS;
using TaxistasMaui.Interfaces;
using Android.Provider;

namespace TaxistasMaui.Platforms.Droid
{
    public class BatteryService : IBatteryService
    {
        public void AbrirOptimizacion()
        {
            Context ctx = Android.App.Application.Context;
            Intent i = new Intent();
            i.SetAction(Settings.ActionApplicationDetailsSettings);
            #pragma warning disable CS8600 // Se va a convertir un literal nulo o un posible valor nulo en un tipo que no acepta valores NULL
            Android.Net.Uri uri = Android.Net.Uri.FromParts("package", ctx.PackageName, null);
            #pragma warning restore CS8600 // Se va a convertir un literal nulo o un posible valor nulo en un tipo que no acepta valores NULL
            i.SetData(uri);
            i.SetFlags(ActivityFlags.NewTask);
            ctx.StartActivity(i);
        }

        public bool RevisarOptimizacion()
        {
            Context ctx = Android.App.Application.Context;
            PowerManager? powerManager = ctx.GetSystemService(Context.PowerService) as PowerManager;
            bool resp = true;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                if(powerManager != null)
                {
                    if (powerManager.IsIgnoringBatteryOptimizations(ctx.PackageName))
                    {
                        resp = true;
                    }
                    else
                    {
                        resp = false;
                    }
                }
            }

            return resp;
        }
    }
}
