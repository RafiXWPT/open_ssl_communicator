using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicatorCore.Classes;
using CommunicatorCore.Classes.Exceptions;
using CommunicatorCore.Classes.Model;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Server.Classes.DbAccess
{
    public class UsersDao
    {
        public bool IsUserExist(string username)
        {
            IMongoCollection<BsonDocument> usersCollection = MongoDbAccess.GetUsersCollection();
            FilterDefinitionBuilder<BsonDocument> builder = Builders<BsonDocument>.Filter;
            FilterDefinition<BsonDocument> filter = builder.Eq("_id", username);
            List<BsonDocument> result = usersCollection.Find(filter).ToList();
            return result.Any();

        }

        public void InsertUser(UserPasswordData userPasswordData)
        {
            BsonDocument userDocument = new BsonDocument
            {
                {"_id", userPasswordData.Username},
                {"password", userPasswordData.HashedPassword}
            };
            IMongoCollection<BsonDocument> usersCollection = MongoDbAccess.GetUsersCollection();
            usersCollection.InsertOne(userDocument);
        }

        public bool ValidateUserCredentials(UserPasswordData userPasswordData)
        {
            IMongoCollection<BsonDocument> usersCollection = MongoDbAccess.GetUsersCollection();
            FilterDefinitionBuilder<BsonDocument> builder = Builders<BsonDocument>.Filter;
            FilterDefinition<BsonDocument> filter = builder.Eq("_id", userPasswordData.Username) &
                         builder.Eq("password", userPasswordData.HashedPassword);
            var result = usersCollection.Find(filter).ToList();
            return result.Any();
        }

        public void RemoveUser(UserPasswordData userPasswordData)
        {
            IMongoCollection<BsonDocument> usersCollection = MongoDbAccess.GetUsersCollection();
            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("_id", userPasswordData.Username);
            DeleteResult result = usersCollection.DeleteOne(filter);
            if (!result.IsAcknowledged)
            {
                throw new UnsuccessfulQueryException("User was not deleted successfully");
            }
        }

    }
}
