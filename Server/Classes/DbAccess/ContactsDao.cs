using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicatorCore.Classes;
using CommunicatorCore.Classes.Model;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Server.Classes.DbAccess
{
    public class ContactsDao
    {
        public void InsertContact(Contact message)
        {
            BsonDocument contactDocument = new BsonDocument
            {
                { "left", message.From},
                { "right", message.To}
            };
            IMongoCollection<BsonDocument> messageCollection = MongoDbAccess.GetContactsCollection();
            messageCollection.InsertOne(contactDocument);
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
                    contacts.AddRange(processContactsBatch(cursor.Current));
                }
            }

            return contacts;
        }

        private IEnumerable<Contact> processContactsBatch(IEnumerable<BsonDocument> contactsBatch)
        {
            return contactsBatch.ToList().ConvertAll(doc => new Contact(doc["left"].AsString, doc["right"].AsString));
        }
    }
}
