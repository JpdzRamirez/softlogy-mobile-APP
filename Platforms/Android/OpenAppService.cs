using Android.Content;
using SoftlogyMaui.Interfaces;

namespace SoftlogyMaui.Platforms.Droid
{
    public class OpenAppService : IOpenAppService
    {
        public void Abrir(string accion)
        {
            Context ctx = Android.App.Application.Context;
            if (accion == "abrir")
            {
                Intent? intent = new Intent(ctx, typeof(MainActivity));
                if (intent != null)
                {
                    intent.SetAction(Intent.ActionMain);
                    intent.AddCategory(Intent.CategoryLauncher);
                    intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.LaunchedFromHistory | ActivityFlags.BroughtToFront | ActivityFlags.ReorderToFront);
                    ctx.StartActivity(intent);
                }
                else
                {
                    intent = ctx.PackageManager?.GetLaunchIntentForPackage(ctx.PackageName);
                    intent.SetPackage(null);
                    intent.SetAction(Intent.ActionMain);
                    intent.AddCategory(Intent.CategoryLauncher);
                    intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.LaunchedFromHistory | ActivityFlags.BroughtToFront | ActivityFlags.ReorderToFront);
                    ctx.StartActivity(intent);
                }
            }
            else
            {
                Intent i = new Intent(Intent.ActionView);
                i.AddFlags(ActivityFlags.NewTask);
                try
                {
                    i.SetData(Android.Net.Uri.Parse("market://details?id=" + ctx.PackageName));
                    ctx.StartActivity(i);
                }
                catch (Exception)
                {
                    i.SetData(Android.Net.Uri.Parse("market://details?id=" + ctx.PackageName));
                    ctx.StartActivity(i);
                }
            }
        }


    }
}
