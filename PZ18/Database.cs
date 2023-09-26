using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MySqlConnector;
using PZ17.Models;

namespace PZ17;

public class Database : IDisposable, IAsyncDisposable {
    // public static readonly MySqlConnectionStringBuilder ConnectionStringBuilder = new() {
    //     Server = "10.10.1.24",
    //     Database = "pro1_2",
    //     UserID = "user_01",
    //     Password = "user01pro"
    // };
    public static readonly MySqlConnectionStringBuilder ConnectionStringBuilder = new() {
        Server = "localhost",
        Database = "pz17",
        UserID = "dev",
        Password = "devPassword"
    };

    private MySqlConnection _connection;

    public Database() {
        _connection = new MySqlConnection(ConnectionStringBuilder.ConnectionString);
        _connection.Open();
    }

    /// <summary>
    /// Крутой Generic метод для аннотированных атрибутом Column классов
    /// </summary>
    /// <typeparam name="T">Тип получаемых объектов</typeparam>
    /// <returns>Все объекты таблицы преобразованные в тип T</returns>
    public IEnumerable<T> Get<T>() where T : new() {
        var columns = GetColumns<T>().ToList();
        var tableInfo = GetTableName<T>();
        if (tableInfo is null) throw new Exception($"Тип {nameof(T)} не имеет атрибута Table");

        if (_connection.State != ConnectionState.Open) _connection.Open();
        using var cmd = new MySqlCommand($"select * from `{tableInfo.Name}`", _connection);
        var reader = cmd.ExecuteReader();

        while (reader.Read()) {
            var obj = new T();
            foreach (var column in columns) {
                if (column.ColumnAttribute.Name is null) {
                    throw new Exception($"Атрибут Column свойства {column.Property.Name} типа {nameof(T)} " +
                                        "не имеет заданного имени");
                }

                column.Property.SetValue(obj, reader.GetValue(column.ColumnAttribute.Name));
            }

            yield return obj;
        }
    }

    public T? GetById<T>(int id) where T : new() {
        var columns = GetColumns<T>().ToList();
        var tableInfo = GetTableName<T>();
        if (tableInfo is null) throw new Exception($"Тип {nameof(T)} не имеет атрибута Table");

        var keys = columns
            .Where(it => it.Property.GetCustomAttribute<KeyAttribute>() is not null)
            .Select(it => it.Property)
            .ToList();
        if (!keys.Any()) {
            throw new Exception($"Тип {nameof(T)} не содержит свойства с атрибутом Key");
        }

        var primaryKeyAttribute = keys.First().GetCustomAttribute<ColumnAttribute>();
        if (primaryKeyAttribute is null) {
            throw new Exception($"Свойство {keys.First().Name} типа {nameof(T)} не имеет атрибута Column");
        }

        var primaryKey = primaryKeyAttribute.Name;

        using var cmd = new MySqlCommand($"select * from `{tableInfo.Name}` where `{primaryKey}` = {id}", _connection);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) {
            var obj = new T();
            foreach (var column in columns) {
                if (column.ColumnAttribute.Name is null) {
                    throw new Exception($"Атрибут Column свойства {column.Property.Name} типа {nameof(T)} " +
                                        "не имеет заданного имени");
                }

                column.Property.SetValue(obj, reader.GetValue(column.ColumnAttribute.Name));
            }

            return obj;
        }

        cmd.Cancel();
        return default;
    }

    static Func<CustomAttributeData, bool> typeChecker = p => p.AttributeType == typeof(ColumnAttribute);

    private static IEnumerable<ColumnInfo> GetColumns<T>() {
        return typeof(T)
            .GetProperties()
            .Where(it => it.CustomAttributes.Any(typeChecker))
            .Select(
                it => new ColumnInfo(
                    it, 
                    it.GetCustomAttribute<ColumnAttribute>()!, 
                    it.GetCustomAttribute<DbTypeAttribute>()!.DbType)
            );
    }

    private static TableInfo? GetTableName<T>() {
        IEnumerable<TableInfo?> info = typeof(T)
            .GetCustomAttributes<TableAttribute>()
            .Select(it => new TableInfo(typeof(T), it.Name));
        var tableInfos = info.ToList();
        return tableInfos.Any() ? tableInfos.First() : null;
    }


    public static IEnumerable<ForeignKeyInfo> GetForeignKeys<T>() {
        var props = typeof(T)
            .GetProperties()
            .Where(
                it => it.GetCustomAttribute<ForeignKeyAttribute>() is not null
                      && it.GetCustomAttribute<ColumnAttribute>() is not null)
            .ToList();
        var info = props.Select(
            it => new Database.ForeignKeyInfo(
                it.GetCustomAttribute<ColumnAttribute>()!.Name!,
                it.GetCustomAttribute<ForeignKeyAttribute>()!.Name));
        return info;
    }

    public static ColumnInfo GetPrimaryKey<T>() {
        var prop = typeof(T)
            .GetProperties()
            .FirstOrDefault(it => it.GetCustomAttribute<KeyAttribute>() is not null
                                  && it.GetCustomAttribute<ColumnAttribute>() is not null);
        return new ColumnInfo(
            prop,
            prop.GetCustomAttribute<ColumnAttribute>(),
            prop.GetCustomAttribute<DbTypeAttribute>().DbType
        );
    }

    public record ColumnInfo(PropertyInfo Property, ColumnAttribute ColumnAttribute, MySqlDbType DbType) {
        public string ParameterName => $"@{ColumnAttribute.Name}";
    };

    public record TableInfo(Type Type, String Name);

    public record ForeignKeyInfo(string Column, string ForeignColumn);

    public void Dispose() {
        _connection.Dispose();
    }

    #region Async

    public async ValueTask DisposeAsync() {
        await _connection.DisposeAsync();
    }

    public async Task<T?> GetByIdAsync<T>(int id) where T : new() {
        var columns = GetColumns<T>().ToList();
        var tableInfo = GetTableName<T>();
        if (tableInfo is null) throw new Exception($"Тип {nameof(T)} не имеет атрибута Table");

        var keys = columns
            .Where(it => it.Property.GetCustomAttribute<KeyAttribute>() is not null)
            .Select(it => it.Property)
            .ToList();
        if (!keys.Any()) {
            throw new Exception($"Тип {nameof(T)} не содержит свойства с атрибутом Key");
        }

        var primaryKeyAttribute = keys.First().GetCustomAttribute<ColumnAttribute>();
        if (primaryKeyAttribute is null) {
            throw new Exception($"Свойство {keys.First().Name} типа {nameof(T)} не имеет атрибута Column");
        }

        var primaryKey = primaryKeyAttribute.Name;

        await using var cmd =
            new MySqlCommand($"select * from `{tableInfo.Name}` where `{primaryKey}` = {id}", _connection);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync()) {
            var obj = new T();
            foreach (var column in columns) {
                if (column.ColumnAttribute.Name is null) {
                    throw new Exception($"Атрибут Column свойства {column.Property.Name} типа {nameof(T)} " +
                                        "не имеет заданного имени");
                }

                column.Property.SetValue(obj, reader.GetValue(column.ColumnAttribute.Name));
            }

            return obj;
        }

        return default;
    }

    public async IAsyncEnumerable<T> GetAsync<T>() where T : new() {
        var columns = GetColumns<T>().ToList();
        var tableInfo = GetTableName<T>();
        if (tableInfo is null) throw new Exception($"Тип {nameof(T)} не имеет атрибута Table");

        if (_connection.State != ConnectionState.Open) _connection.Open();
        await using var cmd = new MySqlCommand($"select * from `{tableInfo.Name}`", _connection);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (reader.Read()) {
            var obj = new T();
            foreach (var column in columns) {
                if (column.ColumnAttribute.Name is null) {
                    throw new Exception($"Атрибут Column свойства {column.Property.Name} типа {nameof(T)} " +
                                        "не имеет заданного имени");
                }

                column.Property.SetValue(obj, reader.GetValue(column.ColumnAttribute.Name));
            }

            yield return obj;
        }
    }

    public async Task InsertAsync<T>(T obj) where T : new() {
        var columns = GetColumns<T>()
            .Where(it => it.Property.GetCustomAttribute<KeyAttribute>() is null)
            .ToList();
        var columnStr = String.Join(',', columns.Select(it => it.ColumnAttribute.Name!));
        var valuesStr = String.Join(',', columns.Select(it => '@' + it.ColumnAttribute.Name!));

        var tableInfo = GetTableName<T>();
        if (tableInfo is null) throw new Exception($"Тип {nameof(T)} не имеет атрибута Table");

        if (_connection.State != ConnectionState.Open) _connection.Open();
        await using var cmd = new MySqlCommand(
            $"""
             insert into `{tableInfo.Name}` ({columnStr})
             values ({valuesStr});
             """
            , _connection);
        foreach (var columnInfo in columns) {
            cmd.Parameters.Add(columnInfo.ParameterName, columnInfo.DbType);
            cmd.Parameters[columnInfo.ParameterName].Value = columnInfo.Property.GetValue(obj);
        }

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync<T>(int id, T obj) where T : new() {
        var columns = GetColumns<T>()
            .Where(it => it.Property.GetCustomAttribute<KeyAttribute>() is null)
            .ToList();
        var keyName = GetPrimaryKey<T>().ColumnAttribute.Name;
        if (keyName is null) throw new Exception($"Тип {nameof(T)} не имеет атрибута Key");
        var setters = string.Join(",\n",
            columns.Select(it => $"{it.ColumnAttribute.Name} = {it.ParameterName}"));

        var tableInfo = GetTableName<T>();
        if (tableInfo is null) throw new Exception($"Тип {nameof(T)} не имеет атрибута Table");

        if (_connection.State != ConnectionState.Open) _connection.Open();
        await using var cmd = new MySqlCommand(
            $"""
             update `{tableInfo.Name}`
             set
                 {setters}
             where {keyName} = {id};
             """
            , _connection);
        foreach (var columnInfo in columns) {
            cmd.Parameters.Add(columnInfo.ParameterName, columnInfo.DbType);
            cmd.Parameters[columnInfo.ParameterName].Value = columnInfo.Property.GetValue(obj);
        }

        await cmd.ExecuteNonQueryAsync();
    }

    #endregion

    public async Task RemoveAsync<T>(T obj) {
        var key = GetPrimaryKey<T>();
        if (key is null) throw new Exception($"Тип {nameof(T)} не имеет атрибута Key");

        var tableInfo = GetTableName<T>();
        if (tableInfo is null) throw new Exception($"Тип {nameof(T)} не имеет атрибута Table");

        if (_connection.State != ConnectionState.Open) _connection.Open();
        await using var cmd = new MySqlCommand(
            $"""
             delete from `{tableInfo.Name}`
             where {key.ColumnAttribute.Name} = {key.Property.GetValue(obj)};
             """
            , _connection);

        await cmd.ExecuteNonQueryAsync();
    }
}