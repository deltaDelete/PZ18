using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PZ18.Models;

namespace PZ18.Views.Dialogs; 

public partial class EditProcedureDialog : Window {
    private Action<Procedure> _confirmAction;

    public EditProcedureDialog(Procedure procedure, Action<Procedure> confirmAction) {
        _confirmAction = confirmAction;
        InitializeComponent();
        DataContext = procedure;
    }

    private void CancelClick(object? sender, RoutedEventArgs e) {
        Close();
    }

    private async void ConfirmClick(object? sender, RoutedEventArgs e) {
        _confirmAction.Invoke((DataContext as Procedure)!);
        Close();
    }
}