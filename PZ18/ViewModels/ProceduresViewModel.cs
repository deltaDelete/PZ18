using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using PZ17.Models;
using PZ18.ViewModels.dialogs;

namespace PZ17.ViewModels;

public class ProceduresViewModel : ViewModelBase {
    private readonly Window _view;
    private string _searchQuery = "";
    private ObservableCollection<Procedure> _procedures = new();
    private int _selectedSearchColumn;
    private bool _isSortByDescending = false;
    private List<Procedure> _proceduresFull;

    #region Notifying Properties

    public ObservableCollection<Procedure> Procedures {
        get => _procedures;
        set {
            if (Equals(value, _procedures)) return;
            _procedures = value;
            RaisePropertyChanged();
        }
    }

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
    
    public ProceduresViewModel(Window view) {
        _view = view;
        EditItemCommand = new AsyncCommand<Procedure>(EditItem);
        RemoveItemCommand = new AsyncCommand<Procedure>(RemoveItem);
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
            ? new ObservableCollection<Procedure>(_proceduresFull)
            : SelectedSearchColumn switch {
                1 => _proceduresFull
                    .Where(it => it.ProcedureId.ToString().Contains(SearchQuery)),
                2 => _proceduresFull
                    .Where(it => it.ProcedureName.ToLower().Contains(SearchQuery.ToLower())),
                3 => _proceduresFull
                    .Where(it => it.BasePrice.ToString().Contains(SearchQuery.ToLower())),
                _ => _proceduresFull
                    .Where(it =>
                        it.ProcedureId.ToString().Contains(SearchQuery.ToLower()) ||
                        it.ProcedureName.ToLower().Contains(SearchQuery.ToLower()) ||
                        it.BasePrice.ToString().Contains(SearchQuery)
                    )
            };

        Procedures = SelectedSearchColumn switch {
            2 => new(IsSortByDescending
                ? filtered.OrderByDescending(it => it.ProcedureName)
                : filtered.OrderBy(it => it.ProcedureName)),
            3 => new(IsSortByDescending
                ? filtered.OrderByDescending(it => it.BasePrice)
                : filtered.OrderBy(it => it.BasePrice)),
            _ => new(IsSortByDescending
                ? filtered.OrderByDescending(it => it.ProcedureId)
                : filtered.OrderBy(it => it.ProcedureId))
        };
    }

    private async void GetDataFromDb() {
        await using var db = new Database();
        var users = db.GetAsync<Procedure>();
        var list = await users.ToListAsync();
        _proceduresFull = list;
        Procedures = new(_proceduresFull);
    }
    
    private async Task RemoveItem(Procedure? arg) {
        if (arg is null) return;
        new ConfirmationDialog(
            "Вы собираетесь удалить строку",
            $"Процедура: {arg.ProcedureName}",
            async dialog => {
                await using var db = new Database();
                await db.RemoveAsync(arg);
                GetDataFromDb();
            },
            dialog => {}
        ).ShowDialog(_view);
    }

    private async Task EditItem(Procedure? arg) {
        if (arg is null) return;
        await new EditProcedureDialog(
            arg,
            async procedure => {
                await using var db = new Database();
                await db.UpdateAsync(procedure.ProcedureId, procedure);
            }
        ).ShowDialog(_view);
        GetDataFromDb();
    }
    
    
    private async Task NewItem() {
        await new EditProcedureDialog(
            new Procedure(),
            async procedure => {
                await using var db = new Database();
                await db.InsertAsync(procedure);
                GetDataFromDb();
            }
        ).ShowDialog(_view);
    }
}