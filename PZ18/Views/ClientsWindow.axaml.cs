using Avalonia.Controls;
using PZ18.ViewModels;

namespace PZ18.Views; 

public partial class ClientsWindow : Window {
    public ClientsWindow() {
        InitializeComponent();
        DataContext = new ClientsViewModel(this);
    }
}