using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using PZ17.Models;

namespace PZ17.ViewModels;

public class ProceduresWindowViewModel : ViewModelBase {
    private string _searchQuery = "";
    private ObservableCollection<Procedure> _procedures = new();

    public ObservableCollection<Procedure> Procedures {
        get => _procedures;
        set {
            if (Equals(value, _procedures)) return;
            _procedures = value;
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

    public ProceduresWindowViewModel() {
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

        Procedures = new(Procedures.Where(
                it => it.ProcedureName.ToLower().Contains(SearchQuery.ToLower())
            )
        );
    }

    private async void GetDataFromDb() {
        await using var db = new Database();
        var procedures = db.GetAsync<Procedure>();
        Procedures = new ObservableCollection<Procedure>(await procedures.ToListAsync());
    }
}