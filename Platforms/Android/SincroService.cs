using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Platform;
using TaxistasMaui.Modelos;

namespace TaxistasMaui.Platforms.Droid
{
    [Service(ForegroundServiceType = ForegroundService.TypeLocation)]
    public class SincroService : Service
    {
        CancellationTokenSource? cts;

        public override IBinder? OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            cts = new CancellationTokenSource();

            startForeground();

            Task.Run(() =>
            {
                try
                {
                    var sinc = new Sincronizador();
                    #pragma warning disable CS8604 // Posible argumento de referencia nulo
                    sinc.RunSincro(cts.Token, intent.GetStringExtra("funcion")).Wait();
                    #pragma warning restore CS8604 // Posible argumento de referencia nulo
                }
                catch (System.OperationCanceledException)
                {
                }
                finally
                {
                    if (cts.IsCancellationRequested)
                    {
                        WeakReferenceMessenger.Default.Send(new SincronizarMessage("2"));
                        //MessagingCenter.Send("2", "sincronizar");
                    }
                }


            }, cts.Token);

            return StartCommandResult.NotSticky;
        }

        public override void OnDestroy()
        {
            if (cts != null)
            {
                cts.Token.ThrowIfCancellationRequested();
                cts.Cancel();

                StopForeground(true);
            }

            base.OnDestroy();
        }

        public override void OnTaskRemoved(Intent? rootIntent)
        {
            WeakReferenceMessenger.Default.Send(new SincronizarMessage("2_1"));

            base.OnTaskRemoved(rootIntent);
        }

        private void startForeground()
        {
            var channelid = "";
            Notification notification;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                channelid = createNotificationChannel("sincro1", "Sincronizacion");

                #pragma warning disable CA1416 // Validar la compatibilidad de la plataforma
                Notification.Builder notificationBuilder = new Notification.Builder(this, channelid);
                #pragma warning restore CA1416 // Validar la compatibilidad de la plataforma
                notification = notificationBuilder.SetOngoing(true)
                    .SetSmallIcon(Resource.Mipmap.ic_taxiseguro)
                    .SetContentTitle("Taxiseguro")
                    .SetContentText("Sincronizando...")
                    .SetShowWhen(false)
                    .SetCategory(Notification.CategoryService)
                    .Build();
            }
            else
            {
                #pragma warning disable CA1422 // Validar la compatibilidad de la plataforma
                Notification.Builder notificationBuilder = new Notification.Builder(this);
                #pragma warning restore CA1422 // Validar la compatibilidad de la plataforma
                notification = notificationBuilder.SetOngoing(true)
                    .SetSmallIcon(Resource.Mipmap.ic_taxiseguro)
                    .SetContentTitle("Taxiseguro")
                    .SetContentText("Sincronizando...")
                    .SetShowWhen(false)
                    .SetCategory(Notification.CategoryService)
                    .Build();
            }

            if(Build.VERSION.SdkInt < BuildVersionCodes.UpsideDownCake)
            {
                StartForeground(101, notification);
            }
            else
            {
                StartForeground(101, notification, ForegroundService.TypeLocation);
            }
        }

        private string createNotificationChannel(string channelid, string channelname)
        {
            Color color = Color.FromRgb(0, 0, 255);
            #pragma warning disable CA1416 // Validar la compatibilidad de la plataforma
            var chan = new NotificationChannel(channelid, channelname, NotificationImportance.None)
            {
                LightColor = color.ToPlatform(),
                LockscreenVisibility = NotificationVisibility.Private
            };
            
            var service = GetSystemService(Context.NotificationService) as NotificationManager;
            #pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
            service.CreateNotificationChannel(chan);
            #pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.
            #pragma warning restore CA1416 // Validar la compatibilidad de la plataforma
            return channelid;
        }

        
    }
}
