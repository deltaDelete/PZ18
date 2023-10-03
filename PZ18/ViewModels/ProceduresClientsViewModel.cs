using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using PZ18.Models;
using PZ18.Views.Dialogs;

namespace PZ18.ViewModels; 

public class ProceduresClientsViewModel : ViewModelBase {
    private readonly Window _view;
    private string _searchQuery = string.Empty;
    private ObservableCollection<ProcedureClient> _items = new();
    private List<ProcedureClient> _itemsFull;
    private int _selectedSearchColumn;
    private bool _isSortByDescending = false;
    private int _take = 10;
    private int _skip = 0;
    private int _currentPage;
    private List<ProcedureClient> _filtered;

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
    
    public ObservableCollection<ProcedureClient> Items
    {
        get => _items;
        set
        {
            if (Equals(value, _items)) return;
            _items = value;
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

    public List<ProcedureClient> Filtered {
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
    
    public ProceduresClientsViewModel(Window view) {
        _view = view;
        EditItemCommand = new AsyncCommand<ProcedureClient>(EditItem);
        RemoveItemCommand = new AsyncCommand<ProcedureClient>(RemoveItem);
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
            ? new ObservableCollection<ProcedureClient>(_itemsFull)
            : SelectedSearchColumn switch {
                1 => _itemsFull
                    .Where(it => it.Id.ToString().Contains(SearchQuery)),
                2 => _itemsFull
                    .Where(it => $"{it.Client.LastName} {it.Client.FirstName}".ToLower().Contains(SearchQuery.ToLower())),
                3 => _itemsFull
                    .Where(it => it.Procedure.ProcedureName.ToLower().Contains(SearchQuery.ToLower())),
                4 => _itemsFull
                    .Where(it => it.Date.ToString(CultureInfo.InvariantCulture).Contains(SearchQuery.ToLower())),
                5 => _itemsFull
                    .Where(it => it.Price.ToString(CultureInfo.InvariantCulture).Contains(SearchQuery.ToLower())),
                _ => _itemsFull
                    .Where(it =>
                        it.Procedure.ProcedureName.ToLower().Contains(SearchQuery.ToLower()) ||
                        it.Date.ToString(CultureInfo.InvariantCulture).Contains(SearchQuery.ToLower()) ||
                        $"{it.Client.LastName} {it.Client.FirstName}".ToLower().Contains(SearchQuery.ToLower()) ||
                        it.Id.ToString().Contains(SearchQuery) ||
                        it.Price.ToString(CultureInfo.InvariantCulture).Contains(SearchQuery.ToLower())
                    )
            };

        Filtered = SelectedSearchColumn switch {
            2 => IsSortByDescending
                ? filtered.OrderByDescending(it => $"{it.Client.LastName} {it.Client.FirstName}").ToList()
                : filtered.OrderBy(it => $"{it.Client.LastName} {it.Client.FirstName}").ToList(),
            3 => IsSortByDescending
                ? filtered.OrderByDescending(it => it.Procedure.ProcedureName).ToList()
                : filtered.OrderBy(it => it.Procedure.ProcedureName).ToList(),
            4 => IsSortByDescending
                ? filtered.OrderByDescending(it => it.Date).ToList()
                : filtered.OrderBy(it => it.Date).ToList(),
            5 => IsSortByDescending
                ? filtered.OrderByDescending(it => it.Price).ToList()
                : filtered.OrderBy(it => it.Price).ToList(),
            _ => IsSortByDescending
                ? filtered.OrderByDescending(it => it.Id).ToList()
                : filtered.OrderBy(it => it.Id).ToList()
        };
    }

    private async void GetDataFromDb() {
        await using var db = new MyDatabase();
        var list = await db.GetAsync<ProcedureClient>().ToListAsync();
        list = list.Select(
            it =>
            {
                it.Procedure = db.GetById<Procedure>(it.ProcedureId);
                it.Client = db.GetById<Client>(it.ClientId);
                return it;
            }
        ).ToList();
        _itemsFull = list;
        Filtered = _itemsFull;
    }
    
    private async Task RemoveItem(ProcedureClient? arg) {
        if (arg is null) return;
        new ConfirmationDialog(
            "Вы собираетесь удалить процедуру",
            string.Empty,
            async dialog => {
                await using var db = new MyDatabase();
                await db.RemoveAsync(arg);
                GetDataFromDb();
            },
            dialog => {}
        ).ShowDialog(_view);
    }

    private async Task EditItem(ProcedureClient? arg) {
        if (arg is null) return;
        await new EditProcedureClientDialog(
            arg, 
            async procedureClient => {
                await using var db = new MyDatabase();
                procedureClient.ProcedureId = procedureClient.Procedure!.ProcedureId;
                procedureClient.ClientId = procedureClient.Client!.ClientId;
                await db.UpdateAsync(procedureClient.ProcedureId, procedureClient);
                GetDataFromDb();
            }
        ).ShowDialog(_view);
    }

    private async Task NewItem() {
        await new EditProcedureClientDialog(
            new ProcedureClient() {
                ClientId = 1,
                ProcedureId = 1
            },
            async procedureClient => {
                await using var db = new MyDatabase();
                procedureClient.ProcedureId = procedureClient.Procedure!.ProcedureId;
                procedureClient.ClientId = procedureClient.Client!.ClientId;
                await db.InsertAsync(procedureClient);
                GetDataFromDb();
            }
        ).ShowDialog(_view);
    }
    
    private void TakeNext() {
        Skip += Take;
        Items = new ObservableCollection<ProcedureClient>(
            Filtered.Skip(Skip).Take(Take)
        );
    }

    private void TakePrev() {
        Skip -= Take;
        Items = new ObservableCollection<ProcedureClient>(
            Filtered.Skip(Skip).Take(Take)
        );
    }

    private void TakeFirst() {
        Skip = 0;
        Items = new ObservableCollection<ProcedureClient>(
            Filtered.Take(Take)
        );
    }
    
    private void TakeLast() {
        Skip = Filtered.Count - Take;
        Items = new ObservableCollection<ProcedureClient>(
            Filtered.TakeLast(Take)
        );
    }
}