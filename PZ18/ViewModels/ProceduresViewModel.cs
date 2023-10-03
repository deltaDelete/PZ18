using System;
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
    private ObservableCollection<Procedure> _items = new();
    private int _selectedSearchColumn;
    private List<Procedure> _itemsFull;
    private bool _isSortByDescending = false;
    private int _take = 10;
    private int _skip = 0;
    private int _currentPage;
    private List<Procedure> _filtered;

    #region Notifying Properties

    public ObservableCollection<Procedure> Items {
        get => _items;
        set {
            if (Equals(value, _items)) return;
            _items = value;
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

    public int Take {
        get => _take;
        set => SetField(ref _take, value);
    }

    public int Skip {
        get => _skip;
        set {
            if (value >= _itemsFull.Count) {
                return;
            }

            if (!SetField(ref _skip, value)) {
                return;
            };

            CurrentPage = (int)Math.Ceiling(value / (double)Take);
        }
    }

    public int CurrentPage {
        get => _currentPage;
        set {
            if (!SetField(ref _currentPage, value)) {
                return;   
            }
            
            TakeFirstCommand.RaiseCanExecuteChanged(null, EventArgs.Empty);
            TakePrevCommand.RaiseCanExecuteChanged(null, EventArgs.Empty);
            TakeNextCommand.RaiseCanExecuteChanged(null, EventArgs.Empty);
            TakeLastCommand.RaiseCanExecuteChanged(null, EventArgs.Empty);
        }
    }

    public int TotalPages => (int)Math.Ceiling(Filtered.Count / (double)Take);

    public List<Procedure> Filtered {
        get => _filtered;
        set {
            if (SetField(ref _filtered, value)) {
                TakeFirst();
            }
        }
    }

    #endregion

    public ICommand EditItemCommand { get; }
    public ICommand RemoveItemCommand { get; }
    public ICommand NewItemCommand { get; }
    public Command TakeNextCommand { get; }
    public Command TakePrevCommand { get; }
    public Command TakeFirstCommand { get; }
    public Command TakeLastCommand { get; }
    
    public ProceduresViewModel(Window view) {
        _view = view;
        EditItemCommand = new AsyncCommand<Procedure>(EditItem);
        RemoveItemCommand = new AsyncCommand<Procedure>(RemoveItem);
        NewItemCommand = new AsyncCommand(NewItem);
        TakeNextCommand = new Command(TakeNext, () => CurrentPage + 1 < TotalPages);
        TakePrevCommand = new Command(TakePrev, () => CurrentPage + 1 > 1);
        TakeFirstCommand = new Command(TakeFirst, () => CurrentPage + 1 > 1);
        TakeLastCommand = new Command(TakeLast, () => CurrentPage + 1 < TotalPages);
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
            ? new ObservableCollection<Procedure>(_itemsFull)
            : SelectedSearchColumn switch {
                1 => _itemsFull
                    .Where(it => it.ProcedureId.ToString().Contains(SearchQuery)),
                2 => _itemsFull
                    .Where(it => it.ProcedureName.ToLower().Contains(SearchQuery.ToLower())),
                3 => _itemsFull
                    .Where(it => it.BasePrice.ToString().Contains(SearchQuery.ToLower())),
                _ => _itemsFull
                    .Where(it =>
                        it.ProcedureId.ToString().Contains(SearchQuery.ToLower()) ||
                        it.ProcedureName.ToLower().Contains(SearchQuery.ToLower()) ||
                        it.BasePrice.ToString().Contains(SearchQuery)
                    )
            };

        Filtered = SelectedSearchColumn switch {
            2 => IsSortByDescending
                ? filtered.OrderByDescending(it => it.ProcedureName).ToList()
                : filtered.OrderBy(it => it.ProcedureName).ToList(),
            3 => IsSortByDescending
                ? filtered.OrderByDescending(it => it.BasePrice).ToList()
                : filtered.OrderBy(it => it.BasePrice).ToList(),
            _ => IsSortByDescending
                ? filtered.OrderByDescending(it => it.ProcedureId).ToList()
                : filtered.OrderBy(it => it.ProcedureId).ToList()
        };
    }

    private async void GetDataFromDb() {
        await using var db = new MyDatabase();
        var users = db.GetAsync<Procedure>();
        var list = await users.ToListAsync();
        _itemsFull = list;
        Filtered = _itemsFull;
    }
    
    private async Task RemoveItem(Procedure? arg) {
        if (arg is null) return;
        new ConfirmationDialog(
            "Вы собираетесь удалить строку",
            $"Процедура: {arg.ProcedureName}",
            async dialog => {
                await using var db = new MyDatabase();
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
                await using var db = new MyDatabase();
                await db.UpdateAsync(procedure.ProcedureId, procedure);
                GetDataFromDb();
            }
        ).ShowDialog(_view);
    }
    
    
    private async Task NewItem() {
        await new EditProcedureDialog(
            new Procedure(),
            async procedure => {
                await using var db = new MyDatabase();
                await db.InsertAsync(procedure);
                GetDataFromDb();
            }
        ).ShowDialog(_view);
    }
    
    private void TakeNext() {
        Skip += Take;
        Items = new ObservableCollection<Procedure>(
            Filtered.Skip(Skip).Take(Take)
        );
    }

    private void TakePrev() {
        Skip -= Take;
        Items = new ObservableCollection<Procedure>(
            Filtered.Skip(Skip).Take(Take)
        );
    }

    private void TakeFirst() {
        Skip = 0;
        Items = new ObservableCollection<Procedure>(
            Filtered.Take(Take)
        );
    }
    
    private void TakeLast() {
        Skip = Filtered.Count - Take;
        Items = new ObservableCollection<Procedure>(
            Filtered.TakeLast(Take)
        );
    }
}