namespace PZ17.Models;

using System;
using MySqlConnector;

public class DbTypeAttribute : Attribute {
    public DbTypeAttribute(MySqlDbType type) {
        DbType = type;
    }

    public MySqlDbType DbType { get; }
}