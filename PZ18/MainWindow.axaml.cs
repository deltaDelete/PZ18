using Avalonia.Controls;
using PZ17.ViewModels;

namespace PZ18;

public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}