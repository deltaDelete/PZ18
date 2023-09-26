using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PZ17;
using PZ17.Models;

namespace PZ18.ViewModels.dialogs; 

public partial class EditProcedureClientDialog : Window {
    private readonly Action<ProcedureClient> _confirmAction;

    public EditProcedureClientDialog(ProcedureClient pc, Action<ProcedureClient> confirmAction) {
        _confirmAction = confirmAction;
        InitializeComponent();
        DataContext = pc;
        InitializeBoxes();
    }

    private async void InitializeBoxes() {
        await using var db = new Database();
        ProcedureBox.ItemsSource = await db.GetAsync<Procedure>().ToListAsync();
        ProcedureBox.SelectedIndex = (DataContext as ProcedureClient).ProcedureId - 1;
        ClientBox.ItemsSource = await db.GetAsync<Client>().ToListAsync();
        ClientBox.SelectedIndex = (DataContext as ProcedureClient).ClientId - 1;
    }

    private void CancelClick(object? sender, RoutedEventArgs e) {
        Close();
    }

    private async void ConfirmClick(object? sender, RoutedEventArgs e) {
        _confirmAction.Invoke((DataContext as ProcedureClient)!);
        Close();
    }
}