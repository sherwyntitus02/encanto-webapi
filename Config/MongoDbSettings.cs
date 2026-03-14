namespace EncantoWebAPI.Config
{
    /// <summary>
    /// MongoDB configuration settings.
    /// 
    /// Configuration Sources:
    /// - Local Development: Read from User Secrets (secrets.json)
    /// - Azure Deployment: Read from Environment Variables
    /// 
    /// Sensitive data (ConnectionString) is stored in:
    /// - secrets.json for local development
    /// - Environment variables in Azure App Service for production
    /// </summary>
    public class MongoDbSettings
    {
        public required string ConnectionString { get; set; }
        public required string DatabaseName { get; set; }
    }
}
