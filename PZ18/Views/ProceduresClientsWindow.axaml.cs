using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PZ18.ViewModels;

namespace PZ18.Views; 

public partial class ProceduresClientsWindow : Window {
    public ProceduresClientsWindow() {
        InitializeComponent();
        DataContext = new ProceduresClientsViewModel(this);
    }
}