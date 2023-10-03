using DbTools;
using MySqlConnector;

namespace PZ18;

public class MyDatabase : Database {
    private static readonly MySqlConnectionStringBuilder ConnectionStringBuilder = new() {
        // Server = "10.10.1.24",
        // Database = "pro1_2",
        // UserID = "user_01",
        // Password = "user01pro"
        Server = "localhost",
        Database = "pz17",
        UserID = "dev",
        Password = "devPassword"
    };
    
    public MyDatabase() : base(ConnectionStringBuilder) { }
}