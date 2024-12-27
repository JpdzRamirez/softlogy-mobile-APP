using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TaxistasMaui.Modelos
{
    public class SincronizarMessage : ValueChangedMessage<string>
    {
        public SincronizarMessage(string value) : base(value)
        {
        }
    }
}
