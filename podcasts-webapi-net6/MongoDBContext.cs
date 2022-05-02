// https://code.msdn.microsoft.com/How-to-using-MongoDB-with-74f3e1cf

using PodcastsWebapi.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PodcastsWebapi
{
    public class MongoDBContext
    {
        public static string ConnectionString { get; set; }
        public static string DatabaseName { get; set; }
        public static bool IsSSL { get; set; }

        private IMongoDatabase _database { get; }

        public MongoDBContext()
        {
            
            if (ConnectionString == null)
            {
                //ConnectionString = "mongodb://localhost:27017";
                ConnectionString = "mongodb+srv://MyMongoPodcastDBUser:1PodcastEtVite%21@cluster0.pkthk.mongodb.net/?retryWrites=true&w=majority";
            }
            
            if (DatabaseName == null)
            {
                DatabaseName = "Podcasts";
            }
            try
            {
                MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(ConnectionString));
                if (IsSSL)
                {
                    settings.SslSettings = new SslSettings { EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 };
                }
                var mongoClient = new MongoClient(settings);
                _database = mongoClient.GetDatabase(DatabaseName);
            }
            catch (Exception ex)
            {
                throw new Exception("Can not access to db server.", ex);
            }
        }

        public IMongoCollection<Channel> Channels
        {
            get
            {
                return _database.GetCollection<Channel>("channels");
            }
        }
    }
}
