using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PZ17;
using PZ17.Models;

namespace PZ18.ViewModels.dialogs; 

public partial class EditProcedureDialog : Window {
    public EditProcedureDialog(Procedure procedure) {
        InitializeComponent();
        DataContext = procedure;
    }

    private void CancelClick(object? sender, RoutedEventArgs e) {
        Close();
    }

    private async void ConfirmClick(object? sender, RoutedEventArgs e) {
        await using var db = new Database();
        Procedure procedure = (DataContext as Procedure)!;
        await db.UpdateAsync(procedure.ProcedureId, procedure);
        Close();
    }
}