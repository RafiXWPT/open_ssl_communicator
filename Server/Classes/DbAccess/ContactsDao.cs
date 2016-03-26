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
    public class ContactsDao
    {

        public void DeleteContact(Contact contact)
        {
            IMongoCollection<BsonDocument> cotactsCollection = MongoDbAccess.GetContactsCollection();
            FilterDefinitionBuilder<BsonDocument> builder = Builders<BsonDocument>.Filter;
            FilterDefinition<BsonDocument> filter = builder.Eq("left", contact.From) &
                                                    builder.Eq("right", contact.CipheredTo);
            DeleteResult result = cotactsCollection.DeleteOne(filter);
            if (!result.IsAcknowledged)
            {
                throw new UnsuccessfulQueryException("Unable to delete contact: " + contact);
            }
        }

        public void UpsertContact(Contact contact)
        {
            IMongoCollection<BsonDocument> contactsCollection = MongoDbAccess.GetContactsCollection();
            FilterDefinitionBuilder<BsonDocument> builder = Builders<BsonDocument>.Filter;
            FilterDefinition<BsonDocument> filter = builder.Eq("left", contact.From) &
                                                    builder.Eq("right", contact.CipheredTo);
            UpdateDefinition<BsonDocument> update = Builders<BsonDocument>.Update
                .Set("left", contact.From)
                .Set("right", contact.CipheredTo)
                .Set("displayName", contact.DisplayName)
                .Set("checksum", contact.ContactChecksum);
            UpdateResult result = contactsCollection.UpdateOne(filter, update, new UpdateOptions { IsUpsert = true});
            if (!result.IsAcknowledged)
            {
                throw new UnsuccessfulQueryException("Unable to update contact: " + contact);
            }
        }

        public bool CheckIfAlreadyExist(Contact contact)
        {
            IMongoCollection<BsonDocument> contactsCollection = MongoDbAccess.GetContactsCollection();
            FilterDefinitionBuilder<BsonDocument> builder = Builders<BsonDocument>.Filter;
            FilterDefinition<BsonDocument> filter = builder.Eq("left", contact.From) &
                                                    builder.Eq("right", contact.CipheredTo);
            List<BsonDocument> result = contactsCollection.Find(filter).ToList();
            return result.Any();
        }


        public List<Contact> GetContacts(string username)
        {
            List<Contact> contacts = new List<Contact>();

            FilterDefinitionBuilder<BsonDocument> builder = Builders<BsonDocument>.Filter;
            FilterDefinition<BsonDocument> filter = builder.Eq("left", username);
            IMongoCollection<BsonDocument> contactsCollection = MongoDbAccess.GetContactsCollection();

            using (var cursor = contactsCollection.Find(filter).ToCursor())
            {
                while (cursor.MoveNext())
                {
                    contacts.AddRange(ProcessContactsBatch(cursor.Current));
                }
            }
            return contacts;
        }

        private IEnumerable<Contact> ProcessContactsBatch(IEnumerable<BsonDocument> contactsBatch)
        {
            return contactsBatch.ToList().ConvertAll(doc => new Contact {
                    From = doc["left"].AsString,
                    CipheredTo = doc["right"].AsString, 
                    DisplayName = doc["displayName"].AsString,
                    ContactChecksum = doc["checksum"].AsString
                });
        }

    }
}
