using EncantoWebAPI.Models;
using EncantoWebAPI.Models.Auth;
using EncantoWebAPI.Models.Events;
using EncantoWebAPI.Models.Profiles;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EncantoWebAPI.Accessors
{
    public class MongoDBAccessor
    {
        private readonly IMongoDatabase _database;
        private readonly MongoDBSettings _settings;

        public MongoDBAccessor()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            _settings = config.GetSection("MongoDBSettings").Get<MongoDBSettings>();

            var client = new MongoClient(_settings.ConnectionURI);
            _database = client.GetDatabase(_settings.DatabaseName);
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

        public string GetConnectionInfo()
        {
            return $"Connected to: {_settings.DatabaseName} via {_settings.ConnectionURI}";
        }

    }
}
