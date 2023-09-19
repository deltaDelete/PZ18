using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using PZ17.Models;

namespace PZ17.ViewModels; 

public class ProceduresClientsWindowViewModel : ViewModelBase {
    private ObservableCollection<ProcedureClient> _proceduresClients = new();
    private string _searchQuery = string.Empty;

    public ObservableCollection<ProcedureClient> ProceduresClients
    {
        get => _proceduresClients;
        set
        {
            if (Equals(value, _proceduresClients)) return;
            _proceduresClients = value;
            RaisePropertyChanged();
        }
    }
    
    public string SearchQuery {
        get => _searchQuery;
        set {
            if (value == _searchQuery) return;
            _searchQuery = value;
            RaisePropertyChanged();
        }
    }

    public ProceduresClientsWindowViewModel() {
        GetDataFromDb();
        PropertyChanged += OnSearchChanged;
    }
    
    private void OnSearchChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName != nameof(SearchQuery)) {
            return;
        }

        if (SearchQuery == "") {
            GetDataFromDb();
            return;
        }
        
        ProceduresClients = new(ProceduresClients.Where(
            it => it.Date.ToShortDateString().Contains(SearchQuery.ToLower())
                  || it.Price.ToString(CultureInfo.CurrentCulture).Contains(SearchQuery.ToLower())
                  || it.Client!.ToString()!.ToLower().Contains(SearchQuery.ToLower())
                  || it.Procedure!.ToString()!.ToLower().Contains(SearchQuery.ToLower())
        ));
    }

    private async void GetDataFromDb() {
        var db = new Database();

        var list = await db.GetAsync<ProcedureClient>().ToListAsync();
        var joined = list.Select(
            it =>
            {
                it.Procedure = db.GetById<Procedure>(it.ProcedureId);
                it.Client = db.GetById<Client>(it.ClientId);
                return it;
            }
        );
        ProceduresClients = new ObservableCollection<ProcedureClient>(joined);
    }
}