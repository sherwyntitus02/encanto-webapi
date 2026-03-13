namespace EncantoWebAPI.Config
{
    /// <summary>
    /// MongoDB configuration settings.
    /// The ConnectionString is automatically injected from Azure Key Vault via GetConnectionString("DefaultConnection").
    /// The DatabaseName is read from appsettings.json.
    /// </summary>
    public class MongoDbSettings
    {
        public required string ConnectionString { get; set; }
        public required string DatabaseName { get; set; }
    }
}
