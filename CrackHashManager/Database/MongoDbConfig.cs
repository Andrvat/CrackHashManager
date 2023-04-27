namespace Manager.Database;

public class MongoDbConfig: IMongoDbConfig
{
    public string Database { get; set; }
    public string ConnectionString { get; set; }

    public MongoDbConfig(IConfiguration config)
    {
        if (config != null)
        {
            Database = config["MongoDb:Database"];
            ConnectionString = config["MongoDb:MongoDbUri"];
        }
    }
}
