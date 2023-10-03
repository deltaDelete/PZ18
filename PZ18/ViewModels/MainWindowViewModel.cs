using PZ18.Views;

namespace PZ18.ViewModels;

public class MainWindowViewModel : ViewModelBase {
    public Command OpenClientsCommand { get; }
    public Command OpenProceduresCommand { get; }
    public Command OpenProceduresClientsCommand { get; }

    public MainWindowViewModel() {
        OpenClientsCommand = new Command(OpenClients);
        OpenProceduresCommand = new Command(OpenProcedures);
        OpenProceduresClientsCommand = new Command(OpenProceduresClients);
    }

    private void OpenClients() {
        new ClientsWindow().ShowDialog(
            App.Current.GetMainWindow()
        );
    }

    private void OpenProcedures() {
        new ProceduresWindow().ShowDialog(
            App.Current.GetMainWindow()
        );
    }

    private void OpenProceduresClients() {
        new ProceduresClientsWindow().ShowDialog(
            App.Current.GetMainWindow()
        );
    }
}