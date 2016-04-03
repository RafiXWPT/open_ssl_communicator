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
    public class MessageDao
    {
        public void InsertMessage(Message message)
        {
            BsonDocument messageDocument = createBsonDocument(message);
            IMongoCollection<BsonDocument> messageCollection = MongoDbAccess.GetMessagesCollection();
            messageCollection.InsertOne(messageDocument);
        }

        private BsonDocument createBsonDocument(Message message)
        {
            return new BsonDocument
            {
                { "from", message.MessageSender},
                { "to", message.MessageDestination},
                { "content", message.MessageCipheredContent },
                { "date", message.MessageDate },
                { "checksum", message.Checksum }
            };
        }

        public void InsertMessages(List<Message> messages)
        {
            List<BsonDocument> messageBatch = new List<BsonDocument>();
            messages.ForEach(message => messageBatch.Add( createBsonDocument(message)));
            IMongoCollection<BsonDocument> messageCollection = MongoDbAccess.GetMessagesCollection();

            messageCollection.InsertMany(messageBatch);
        }

        public List<Message> GetMessages(Contact contactDto)
        {
            List<Message> messages = new List<Message>();

            FilterDefinitionBuilder<BsonDocument> builder = Builders<BsonDocument>.Filter;
            FilterDefinition<BsonDocument> filter = (builder.Eq("from", contactDto.From) & builder.Eq("to", contactDto.To))
                                                    |
                                                    (builder.Eq("from", contactDto.To) & builder.Eq("to", contactDto.From));

            SortDefinition<BsonDocument> sort = Builders<BsonDocument>.Sort.Descending("date");
            IMongoCollection<BsonDocument> messagesCollection = MongoDbAccess.GetMessagesCollection();

            using (var cursor = messagesCollection.Find(filter).Sort(sort).ToCursor())
            {
                while (cursor.MoveNext())
                {
                    messages.AddRange(ProcessMessageBatch(cursor.Current));
                }
            }
            
            return messages;
        }

        private IEnumerable<Message> ProcessMessageBatch(IEnumerable<BsonDocument> messagesBatch)
        {
            return messagesBatch.ToList().ConvertAll(document => new Message
            {
                MessageSender = document["from"].AsString,
                MessageDestination = document["to"].AsString,
                MessageCipheredContent = document["content"].AsString,
                MessageDate = document["date"].ToUniversalTime(),
                Checksum = document["checksum"].AsString
            });
        }


    }
}
