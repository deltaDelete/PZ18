using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using PZ17.Models;
using PZ18.ViewModels.dialogs;

namespace PZ17.ViewModels; 

public class ProceduresClientsViewModel : ViewModelBase {
    private readonly Window _view;
    private ObservableCollection<ProcedureClient> _proceduresClients = new();
    private string _searchQuery = string.Empty;
    private List<ProcedureClient> _proceduresClientsFull;
    private int _selectedSearchColumn;
    private bool _isSortByDescending = false;

    #region Notifying Properties

    public int SelectedSearchColumn {
        get => _selectedSearchColumn;
        set {
            RaisePropertyChanging();
            if (value == _selectedSearchColumn) return;
            _selectedSearchColumn = value;
            RaisePropertyChanged();
        }
    }

    public bool IsSortByDescending {
        get => _isSortByDescending;
        set => SetField(ref _isSortByDescending, value);
    }
    
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

    #endregion

    public ICommand EditItemCommand { get; }
    public ICommand RemoveItemCommand { get; }
    public ICommand NewItemCommand { get; }
    
    public ProceduresClientsViewModel(Window view) {
        _view = view;
        EditItemCommand = new AsyncCommand<ProcedureClient>(EditItem);
        RemoveItemCommand = new AsyncCommand<ProcedureClient>(RemoveItem);
        NewItemCommand = new AsyncCommand(NewItem);
        GetDataFromDb();
        PropertyChanged += OnSearchChanged;
    }
    
    private void OnSearchChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName != nameof(SearchQuery)
            && e.PropertyName != nameof(SelectedSearchColumn)
            && e.PropertyName != nameof(IsSortByDescending)) {
            return;
        }

        var filtered = SearchQuery == ""
            ? new ObservableCollection<ProcedureClient>(_proceduresClientsFull)
            : SelectedSearchColumn switch {
                1 => _proceduresClientsFull
                    .Where(it => it.Id.ToString().Contains(SearchQuery)),
                2 => _proceduresClientsFull
                    .Where(it => $"{it.Client.LastName} {it.Client.FirstName}".ToLower().Contains(SearchQuery.ToLower())),
                3 => _proceduresClientsFull
                    .Where(it => it.Procedure.ProcedureName.ToLower().Contains(SearchQuery.ToLower())),
                4 => _proceduresClientsFull
                    .Where(it => it.Date.ToString(CultureInfo.InvariantCulture).Contains(SearchQuery.ToLower())),
                5 => _proceduresClientsFull
                    .Where(it => it.Price.ToString(CultureInfo.InvariantCulture).Contains(SearchQuery.ToLower())),
                _ => _proceduresClientsFull
                    .Where(it =>
                        it.Procedure.ProcedureName.ToLower().Contains(SearchQuery.ToLower()) ||
                        it.Date.ToString(CultureInfo.InvariantCulture).Contains(SearchQuery.ToLower()) ||
                        $"{it.Client.LastName} {it.Client.FirstName}".ToLower().Contains(SearchQuery.ToLower()) ||
                        it.Id.ToString().Contains(SearchQuery) ||
                        it.Price.ToString(CultureInfo.InvariantCulture).Contains(SearchQuery.ToLower())
                    )
            };

        ProceduresClients = SelectedSearchColumn switch {
            2 => new(IsSortByDescending
                ? filtered.OrderByDescending(it => $"{it.Client.LastName} {it.Client.FirstName}")
                : filtered.OrderBy(it => $"{it.Client.LastName} {it.Client.FirstName}")),
            3 => new(IsSortByDescending
                ? filtered.OrderByDescending(it => it.Procedure.ProcedureName)
                : filtered.OrderBy(it => it.Procedure.ProcedureName)),
            4 => new(IsSortByDescending
                ? filtered.OrderByDescending(it => it.Date)
                : filtered.OrderBy(it => it.Date)),
            5 => new(IsSortByDescending
                ? filtered.OrderByDescending(it => it.Price)
                : filtered.OrderBy(it => it.Price)),
            _ => new(IsSortByDescending
                ? filtered.OrderByDescending(it => it.Id)
                : filtered.OrderBy(it => it.Id))
        };
    }

    private async void GetDataFromDb() {
        await using var db = new Database();
        var list = await db.GetAsync<ProcedureClient>().ToListAsync();
        list = list.Select(
            it =>
            {
                it.Procedure = db.GetById<Procedure>(it.ProcedureId);
                it.Client = db.GetById<Client>(it.ClientId);
                return it;
            }
        ).ToList();
        _proceduresClientsFull = list;
        ProceduresClients = new ObservableCollection<ProcedureClient>(list);
    }
    
    private async Task RemoveItem(ProcedureClient? arg) {
        if (arg is null) return;
        new ConfirmationDialog(
            "Вы собираетесь удалить процедуру",
            string.Empty,
            async dialog => {
                await using var db = new Database();
                await db.RemoveAsync(arg);
                GetDataFromDb();
            },
            dialog => {}
        ).ShowDialog(_view);
        GetDataFromDb();
    }

    private async Task EditItem(ProcedureClient? arg) {
        if (arg is null) return;
        await new EditProcedureClientDialog(
            arg, 
            async procedureClient => {
                await using var db = new Database();
                procedureClient.ProcedureId = procedureClient.Procedure!.ProcedureId;
                procedureClient.ClientId = procedureClient.Client!.ClientId;
                await db.UpdateAsync(procedureClient.ProcedureId, procedureClient);
            }
        ).ShowDialog(_view);
        GetDataFromDb();
    }

    private async Task NewItem() {
        await new EditProcedureClientDialog(
            new ProcedureClient() {
                ClientId = 1,
                ProcedureId = 1
            },
            async procedureClient => {
                await using var db = new Database();
                procedureClient.ProcedureId = procedureClient.Procedure!.ProcedureId;
                procedureClient.ClientId = procedureClient.Client!.ClientId;
                await db.InsertAsync(procedureClient);
            }
        ).ShowDialog(_view);
        GetDataFromDb();
    }
}