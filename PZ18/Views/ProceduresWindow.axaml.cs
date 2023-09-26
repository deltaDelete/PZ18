using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PZ17.ViewModels;

namespace PZ18.Views; 

public partial class ProceduresWindow : Window {
    public ProceduresWindow() {
        InitializeComponent();
        DataContext = new ProceduresViewModel(this);
    }
}