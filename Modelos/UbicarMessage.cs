using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SoftlogyMaui.Modelos
{
    public class UbicarMessage : ValueChangedMessage<string>
    {
        public UbicarMessage(string value) : base(value)
        {
        }
    }
}
