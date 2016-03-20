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
        public void InsertContact(Contact contact)
        {
            BsonDocument contactDocument = new BsonDocument
            {
                { "left", contact.From},
                { "right", contact.To},
                { "displayName", contact.DisplayName }
            };
            IMongoCollection<BsonDocument> messageCollection = MongoDbAccess.GetContactsCollection();
            messageCollection.InsertOne(contactDocument);
        }

        public void UpdateContact(Contact contact)
        {
            IMongoCollection<BsonDocument> contactsCollection = MongoDbAccess.GetContactsCollection();
            FilterDefinitionBuilder<BsonDocument> builder = Builders<BsonDocument>.Filter;
            FilterDefinition<BsonDocument> filter = builder.Eq("left", contact.From) &
                                                    builder.Eq("right", contact.To);
            UpdateDefinition<BsonDocument> update = Builders<BsonDocument>.Update.Set("displayName", contact.DisplayName);
            UpdateResult result = contactsCollection.UpdateOne(filter, update);
            if (!result.IsAcknowledged)
            {
                throw new UnsuccessfulQueryException("Unable to update contact: " + contact);
            }
        }

        public List<Contact> GetContacts(string username)
        {
            List<Contact> contacts = new List<Contact>();

            FilterDefinitionBuilder<BsonDocument> builder = Builders<BsonDocument>.Filter;
            FilterDefinition<BsonDocument> filter = builder.Eq("left", username);
            SortDefinition<BsonDocument> sort = Builders<BsonDocument>.Sort.Ascending("left");
            IMongoCollection<BsonDocument> contactsCollection = MongoDbAccess.GetContactsCollection();

            using (var cursor = contactsCollection.Find(filter).Sort(sort).ToCursor())
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
                    To = doc["right"].AsString, 
                    DisplayName = doc["displayName"].AsString
                });
        }
    }
}
