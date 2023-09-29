using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PZ17;
using PZ17.Models;

namespace PZ18.ViewModels.dialogs; 

public partial class EditClientDialog : Window {
    private readonly Action<Client> _confirmAction;

    public EditClientDialog(Client client, Action<Client> confirmAction) {
        _confirmAction = confirmAction;
        InitializeComponent();
        DataContext = client;
        InitializeGenderBox();
    }

    private async void InitializeGenderBox() {
        await using var db = new MyDatabase();
        GenderBox.ItemsSource = await db.GetAsync<Gender>().ToListAsync();
        GenderBox.SelectedIndex = (DataContext as Client)!.GenderId - 1;
    }

    private void CancelClick(object? sender, RoutedEventArgs e) {
        Close();
    }

    private async void ConfirmClick(object? sender, RoutedEventArgs e) {
        _confirmAction.Invoke((DataContext as Client)!);
        Close();
    }
}