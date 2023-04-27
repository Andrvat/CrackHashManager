namespace Manager.Database;

public interface IMongoDbConfig
{ 
    string Database { get; set; }
    string ConnectionString { get; set; }
}