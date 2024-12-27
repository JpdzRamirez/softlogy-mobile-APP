using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TaxistasMaui.Modelos
{
    public class UbicarMessage : ValueChangedMessage<string>
    {
        public UbicarMessage(string value) : base(value)
        {
        }
    }
}
