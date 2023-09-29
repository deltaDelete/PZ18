using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Microsoft.VisualBasic.CompilerServices;
using MySqlConnector;
using PZ17.Models;
using PZ18.ViewModels.dialogs;

namespace PZ17.ViewModels;

public class ClientsViewModel : ViewModelBase {
    private readonly Window _view;
    private string _searchQuery = string.Empty;
    private ObservableCollection<Client> _clients = new();
    private List<Client> _clientsFull;
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
    
    public string SearchQuery {
        get => _searchQuery;
        set {
            if (value == _searchQuery) return;
            _searchQuery = value;
            RaisePropertyChanged();
        }
    }

    public ObservableCollection<Client> Clients {
        get => _clients;
        set {
            if (Equals(value, _clients)) return;
            _clients = value;
            RaisePropertyChanged();
        }
    }

    #endregion

    public ICommand EditItemCommand { get; }
    public ICommand RemoveItemCommand { get; }
    public ICommand NewItemCommand { get; }

    public ClientsViewModel(Window view) {
        _view = view;
        EditItemCommand = new AsyncCommand<Client>(EditItem);
        RemoveItemCommand = new AsyncCommand<Client>(RemoveItem);
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
            ? new ObservableCollection<Client>(_clientsFull)
            : SelectedSearchColumn switch {
                1 => _clientsFull
                    .Where(it => it.ClientId.ToString().Contains(SearchQuery)),
                2 => _clientsFull
                    .Where(it => it.LastName.ToLower().Contains(SearchQuery.ToLower())),
                3 => _clientsFull
                    .Where(it => it.FirstName.ToLower().Contains(SearchQuery.ToLower())),
                4 => _clientsFull
                    .Where(it => it.Gender!.Name.ToLower().Contains(SearchQuery.ToLower())),
                _ => _clientsFull
                    .Where(it =>
                        it.LastName.ToLower().Contains(SearchQuery.ToLower()) ||
                        it.FirstName.ToLower().Contains(SearchQuery.ToLower()) ||
                        it.ClientId.ToString().Contains(SearchQuery) ||
                        it.Gender!.Name.ToLower().Contains(SearchQuery.ToLower())
                    )
            };

        Clients = SelectedSearchColumn switch {
            2 => new(IsSortByDescending
                ? filtered.OrderByDescending(it => it.LastName)
                : filtered.OrderBy(it => it.LastName)),
            3 => new(IsSortByDescending
                ? filtered.OrderByDescending(it => it.FirstName)
                : filtered.OrderBy(it => it.FirstName)),
            4 => new(IsSortByDescending
                ? filtered.OrderByDescending(it => it.Gender!.Name)
                : filtered.OrderBy(it => it.Gender!.Name)),
            _ => new(IsSortByDescending
                ? filtered.OrderByDescending(it => it.ClientId)
                : filtered.OrderBy(it => it.ClientId))
        };
    }

    private async void GetDataFromDb() {
        await using var db = new MyDatabase();
        var users = db.GetAsync<Client>();
        var list = await users.ToListAsync();
        list = list.Select(it => {
            it.Gender = it.Gender = db.GetById<Gender>(it.GenderId);
            return it;
        }).ToList();
        _clientsFull = list;
        Clients = new ObservableCollection<Client>(_clientsFull);
    }
    
    private async Task RemoveItem(Client? arg) {
        if (arg is null) return;
        new ConfirmationDialog(
            "Вы собираетесь удалить строку",
            $"Пользователь: {arg.LastName} {arg.FirstName}",
            async dialog => {
                await using var db = new MyDatabase();
                await db.RemoveAsync(arg);
                GetDataFromDb();
            },
            dialog => {}
        ).ShowDialog(_view);
    }

    private async Task EditItem(Client? arg) {
        if (arg is null) return;
        await new EditClientDialog(
            arg, 
            async client => {
                await using var db = new MyDatabase();
                client.GenderId = client.Gender!.GenderId;
                await db.UpdateAsync(client.ClientId, client);
            }
            ).ShowDialog(_view);
        GetDataFromDb();
    }

    private async Task NewItem() {
        await new EditClientDialog(
            new Client(),
            async client => {
                await using var db = new MyDatabase();
                client.GenderId = client.Gender!.GenderId;
                await db.InsertAsync(client);
                GetDataFromDb();
            }
        ).ShowDialog(_view);
    }
}