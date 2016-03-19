using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using CommunicatorCore.Classes;

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
    }
}
