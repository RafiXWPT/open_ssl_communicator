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

        public void InsertUser(UserPasswordData userPasswordData, string token)
        {
            BsonDocument userDocument = new BsonDocument
            {
                { "_id" , userPasswordData.Username},
                { "password" , userPasswordData.HashedPassword},
                { "token" , token }
            };
            IMongoCollection<BsonDocument> usersCollection = MongoDbAccess.GetUsersCollection();
            usersCollection.InsertOne(userDocument);
        }

        public void UpdatePassword(UserPasswordData userPasswordData)
        {
            IMongoCollection<BsonDocument> usersCollection = MongoDbAccess.GetUsersCollection();
            FilterDefinitionBuilder<BsonDocument> builder = Builders<BsonDocument>.Filter;
            FilterDefinition<BsonDocument> filter = builder.Eq("_id", userPasswordData.Username);
            UpdateDefinition<BsonDocument> update = Builders<BsonDocument>.Update
                .Set("password", userPasswordData.HashedPassword);

            UpdateResult result = usersCollection.UpdateOne(filter, update);
            if (!result.IsAcknowledged)
            {
                throw new UnsuccessfulQueryException("Unable to update password: " + userPasswordData.Username);
            }
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

        public bool ValidateUserToken(UserTokenDto userTokenDto)
        {
            IMongoCollection<BsonDocument> usersCollection = MongoDbAccess.GetUsersCollection();
            FilterDefinitionBuilder<BsonDocument> builder = Builders<BsonDocument>.Filter;
            FilterDefinition<BsonDocument> filter = builder.Eq("_id", userTokenDto.Username) &
                         builder.Eq("token", userTokenDto.HashedToken);
            var result = usersCollection.Find(filter).ToList();
            return result.Any();
        }

        public List<User> LoadUsers()
        {
            List<User> users = new List<User>();
            IMongoCollection<BsonDocument> usersCollection = MongoDbAccess.GetUsersCollection();
            using (var cursor = usersCollection.Find(new BsonDocument()).ToCursor())
            {
                while (cursor.MoveNext())
                {
                    var batch = cursor.Current;
                    users.AddRange(batch.ToList().ConvertAll(document => new User(document["_id"].AsString, false)));
                }
            }
            return users;
        } 

        public void RemoveUser(UserPasswordData userPasswordData)
        {
            IMongoCollection<BsonDocument> usersCollection = MongoDbAccess.GetUsersCollection();
            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("_id", userPasswordData.Username);
            DeleteResult result = usersCollection.DeleteOne(filter);
            if (!result.IsAcknowledged)
            {
                throw new UnsuccessfulQueryException("User " + userPasswordData.Username + " was not deleted successfully");
            }
        }

    }
}
