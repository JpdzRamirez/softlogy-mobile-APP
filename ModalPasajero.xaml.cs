using System.Collections.ObjectModel;
using SoftlogyMaui.Interfaces;
using SoftlogyMaui.Modelos;

namespace SoftlogyMaui;

public partial class ModalPasajero : ContentPage
{

    public ModalPasajero(Pasajero[] pasajeros)
    {
        InitializeComponent();

        /*if (Preferences.Get("estilo", "null") == "1")
        {

            ContentMain.BackgroundColor = Colors.Black;
            StackMain.BackgroundColor = Colors.Black;
            LblListadoPasajeros.BackgroundColor = Colors.DimGray;
            lstpasajeros.BackgroundColor = Colors.Black;
        }
        else
        {
            StackMain.BackgroundColor = Colors.White;
        }*/

        ListarPasajeros(pasajeros);
    }
    private void ListarPasajeros(Pasajero[] pasajeros)
    {
        ObservableCollection<Pasajero> itemList = new ObservableCollection<Pasajero>();
        foreach (var pasajero in pasajeros)
        {

            itemList.Add(new Pasajero(pasajero.nombre,pasajero.direccion, pasajero.celulares));

        }
        lstpasajeros.ItemsSource = itemList;
    }

    private void lstpasajeros_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        try
        {
            Pasajero pasajero = (Pasajero)lstpasajeros.SelectedItem;
            if (pasajero != null)
            {
                if (pasajero.celulares != null)
                {
                    PhoneDialer.Open(pasajero.celulares.Split('-')[0]);
                }
            }
        }
        catch (Exception ex)
        {
            DependencyService.Get<IToastService>().ShowToast(ex.Message);
        }
    }
}