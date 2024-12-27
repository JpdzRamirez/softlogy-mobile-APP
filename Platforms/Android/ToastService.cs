using Android.Widget;
using TaxistasMaui.Interfaces;

namespace TaxistasMaui.Platforms.Droid
{
    public class ToastService : IToastService
    {
        public void ShowToast(string mensaje)
        {
            #pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
            Toast.MakeText(Android.App.Application.Context, mensaje, ToastLength.Short).Show();
            #pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.
        }
    }
}
