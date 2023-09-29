using DbTools;
using MySqlConnector;

namespace PZ17;

public class MyDatabase : Database {
    private static readonly MySqlConnectionStringBuilder ConnectionStringBuilder = new() {
        Server = "10.10.1.24",
        Database = "pro1_2",
        UserID = "user_01",
        Password = "user01pro"
    };
    
    public MyDatabase() : base(ConnectionStringBuilder) { }
}