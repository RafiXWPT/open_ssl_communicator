using System;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;

namespace Server.Classes.DbAccess
{
    public class MongoDbAccess
    {
        private const string ConnectionString = "mongodb://localhost/";
        private const string DbName = "bsk";

        private static readonly IMongoDatabase _database;
        private static readonly IMongoClient _client;

        static MongoDbAccess()
        {
            _client = new MongoClient(ConnectionString);
            _database = _client.GetDatabase(DbName);
        }

        public static IMongoCollection<BsonDocument> GetUsersCollection()
        {
            return _database.GetCollection<BsonDocument>("users");
        }

        public static IMongoCollection<BsonDocument> GetMessagesCollection()
        {
            return _database.GetCollection<BsonDocument>("messages");
        }

        public static IMongoCollection<BsonDocument> GetContactsCollection()
        {
            return _database.GetCollection<BsonDocument>("contacts");
        }

        public static bool IsServerAlive()
        {
            // StackOverflow slightly modified solution
            try
            {
                var databases = _client.ListDatabases();
                databases.MoveNext(); // Force MongoDB to connect to the database.
                return _client.Cluster.Description.State == ClusterState.Connected;
            }
            catch (TimeoutException e)
            {
                return false;
            } 
        }
    }
}
