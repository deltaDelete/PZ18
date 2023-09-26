using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PZ17;
using PZ17.Models;

namespace PZ18.ViewModels.dialogs; 

public partial class EditUserDialog : Window {
    public EditUserDialog(Client client) {
        InitializeComponent();
        DataContext = client;
        InitializeGenderBox();
    }

    private async void InitializeGenderBox() {
        await using var db = new Database();
        GenderBox.ItemsSource = await db.GetAsync<Gender>().ToListAsync();
        GenderBox.SelectedIndex = (DataContext as Client)!.GenderId - 1;
    }

    private void CancelClick(object? sender, RoutedEventArgs e) {
        Close();
    }

    private async void ConfirmClick(object? sender, RoutedEventArgs e) {
        await using var db = new Database();
        Client client = (DataContext as Client)!;
        client.GenderId = client.Gender!.GenderId;
        await db.UpdateAsync(client.ClientId, client);
        Close();
    }
}