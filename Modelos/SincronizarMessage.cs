using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SoftlogyMaui.Modelos
{
    public class SincronizarMessage : ValueChangedMessage<string>
    {
        public SincronizarMessage(string value) : base(value)
        {
        }
    }
}
