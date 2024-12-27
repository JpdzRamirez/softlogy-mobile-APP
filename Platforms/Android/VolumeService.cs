using Android.Content;
using Android.Media;
using TaxistasMaui.Interfaces;

namespace TaxistasMaui.Platforms.Droid
{
    public class VolumeService : IUpVolume
    {
        public void subirVolumen()
        {
            AudioManager? am = Android.App.Application.Context.GetSystemService(Context.AudioService) as AudioManager;
            if(am != null)
            {
                var max_volume = am.GetStreamMaxVolume(Android.Media.Stream.Music);
                am.SetStreamVolume(Android.Media.Stream.Music, max_volume, VolumeNotificationFlags.AllowRingerModes);
            }
        }
    }
}
