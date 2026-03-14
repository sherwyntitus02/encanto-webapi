using MongoDB.Driver;

namespace EncantoWebAPI.Services
{
    /// <summary>
    /// MongoDB service for accessing the MongoDB database and collections.
    /// Registered as a singleton in the dependency injection container.
    /// </summary>
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(MongoClient mongoClient, string databaseName)
        {
            _database = mongoClient.GetDatabase(databaseName);
        }

        /// <summary>
        /// Internal property to access the MongoDB database.
        /// Used by legacy accessors during migration.
        /// </summary>
        internal IMongoDatabase Database => _database;

        /// <summary>
        /// Gets a MongoDB collection for the specified type and collection name.
        /// </summary>
        /// <typeparam name="T">The document type.</typeparam>
        /// <param name="collectionName">The name of the collection.</param>
        /// <returns>A MongoDB collection.</returns>
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }

        /// <summary>
        /// Tests the MongoDB connection.
        /// </summary>
        /// <returns>True if the connection is successful; otherwise, false.</returns>
        public bool TestConnection()
        {
            try
            {
                var command = new MongoDB.Bson.BsonDocument("ping", 1);
                _database.RunCommand<MongoDB.Bson.BsonDocument>(command);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
