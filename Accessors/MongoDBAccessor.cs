using EncantoWebAPI.Models;
using EncantoWebAPI.Models.Auth;
using EncantoWebAPI.Models.Events;
using EncantoWebAPI.Models.Profiles;
using EncantoWebAPI.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EncantoWebAPI.Accessors
{
    /// <summary>
    /// MongoDB accessor for data access operations.
    /// This class is deprecated. Use MongoDbService with dependency injection instead.
    /// </summary>
    [Obsolete("Use MongoDbService with dependency injection instead.")]
    public class MongoDBAccessor
    {
        private readonly IMongoDatabase _database;
        private readonly MongoDBSettings _settings;

        public MongoDBAccessor(MongoDbService mongoDbService, IConfiguration configuration)
        {
            _settings = configuration.GetSection("MongoDBSettings").Get<MongoDBSettings>();
            if (_settings == null)
            {
                throw new InvalidOperationException("MongoDBSettings configuration not found.");
            }

            // Use MongoDbService to get the database
            _database = mongoDbService.GetType()
                .GetProperty("Database", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(mongoDbService) as IMongoDatabase
                ?? throw new InvalidOperationException("Unable to retrieve MongoDB database from MongoDbService.");
        }

        // Collections

        public IMongoCollection<UserProfile> Users =>
            _database.GetCollection<UserProfile>(_settings.UsersCollectionName);

        public IMongoCollection<EventDetails> Events =>
            _database.GetCollection<EventDetails>(_settings.EventsCollectionName);

        public IMongoCollection<Address> Addresses =>
            _database.GetCollection<Address>(_settings.AddressCollectionName);

        public IMongoCollection<OccupationDetails> OccupationDetails =>
            _database.GetCollection<OccupationDetails>(_settings.OccupationDetailsCollectionName);

        public IMongoCollection<LoginCredential> LoginCredentials =>
            _database.GetCollection<LoginCredential>(_settings.LoginCredentialsCollectionName);

        public IMongoCollection<SessionDetails> SessionDetails =>
            _database.GetCollection<SessionDetails>(_settings.SessionsCollectionName);

        public IMongoCollection<EventFeedback> EventFeedbacks =>
            _database.GetCollection<EventFeedback>(_settings.FeedbacksCollectionName);


        public bool TestConnection()
        {
            try
            {
                var command = new BsonDocument("ping", 1);
                _database.RunCommand<BsonDocument>(command);
                return true; // Connection works
            }
            catch
            {
                return false; // Something went wrong
            }
        }

    }
}
