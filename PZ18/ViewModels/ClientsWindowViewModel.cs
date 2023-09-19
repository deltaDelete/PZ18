using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.VisualBasic.CompilerServices;
using PZ17.Models;

namespace PZ17.ViewModels;

public class ClientsWindowViewModel : ViewModelBase {
    private string _searchQuery = string.Empty;
    private ObservableCollection<Client> _clients = new();
    private List<Client> _clientsFull = new List<Client>();
    private int _selectedSearchColumn;
    private bool _isSortByDescending = false;

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

    public ObservableCollection<Client> Clients {
        get => _clients;
        set {
            if (Equals(value, _clients)) return;
            _clients = value;
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

    public ClientsWindowViewModel() {
        GetDataFromDb();
        PropertyChanged += OnSearchChanged;
    }

    private void OnSearchChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName != nameof(SearchQuery)
            && e.PropertyName != nameof(SelectedSearchColumn)
            && e.PropertyName != nameof(IsSortByDescending)) {
            return;
        }

        IEnumerable<Client> filtered = SearchQuery == ""
            ? new ObservableCollection<Client>(_clientsFull)
            : SelectedSearchColumn switch {
                1 => _clientsFull
                    .Where(it => it.ClientId.ToString().Contains(SearchQuery)),
                2 => _clientsFull
                    .Where(it => it.LastName.ToLower().Contains(SearchQuery.ToLower())),
                3 => _clientsFull
                    .Where(it => it.FirstName.ToLower().Contains(SearchQuery.ToLower())),
                4 => _clientsFull
                    .Where(it => it.Gender.Name.ToLower().Contains(SearchQuery.ToLower())),
                _ => _clientsFull
                    .Where(it =>
                        it.LastName.ToLower().Contains(SearchQuery.ToLower()) ||
                        it.FirstName.ToLower().Contains(SearchQuery.ToLower()) ||
                        it.ClientId.ToString().Contains(SearchQuery) ||
                        it.Gender.Name.ToLower().Contains(SearchQuery.ToLower())
                    )
            };

        switch (SelectedSearchColumn) {
            case 2:
                Clients = new(
                    IsSortByDescending
                        ? filtered.OrderByDescending(it => it.LastName)
                        : filtered.OrderBy(it => it.LastName)
                );
                break;
            case 3:
                Clients = new(
                    IsSortByDescending
                        ? filtered.OrderByDescending(it => it.FirstName)
                        : filtered.OrderBy(it => it.FirstName)
                );
                break;
            case 4:
                Clients = new(
                    IsSortByDescending
                        ? filtered.OrderByDescending(it => it.Gender.Name)
                        : filtered.OrderBy(it => it.Gender.Name)
                );
                break;
            default:
                Clients = new(
                    IsSortByDescending
                        ? filtered.OrderByDescending(it => it.ClientId)
                        : filtered.OrderBy(it => it.ClientId)
                );
                break;
        }
    }

    private async void GetDataFromDb() {
        await using var db = new Database();
        var users = db.GetAsync<Client>();
        var list = await users.ToListAsync();
        list = list.Select(it => {
            it.Gender = it.Gender = db.GetById<Gender>(it.GenderId);
            return it;
        }).ToList();
        _clientsFull.AddRange(list);
        Clients = new ObservableCollection<Client>(_clientsFull);
    }
}