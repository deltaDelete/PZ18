using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PZ17;
using PZ17.Models;

namespace PZ18.ViewModels.dialogs; 

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