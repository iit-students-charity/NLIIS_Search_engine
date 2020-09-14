using System;
using System.Linq;
using Contracts;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Access
{
    public static class MongoDBConnector
    {
        private static IMongoClient _mongoClient;
        private static IMongoDatabase _database;

        public static void CreateSession(string db)
        {
            var connectionString = $@"mongodb://lcoalhost:27017/{db}?safe=true";
            _mongoClient = new MongoClient(connectionString);
            
            _database = _mongoClient.GetDatabase(db);
        }

        public static T Add<T>(T document)
            where T : IDocument
        {
            _database.GetCollection<T>(nameof(T)).InsertOne(document);

            return document;
        }
        
        public static void Remove<T>(ObjectId documentId)
            where T : IDocument
        {
            _database.GetCollection<T>(nameof(T)).DeleteOne(document => document.Id == documentId);
        }

        public static IQueryable<T> GetAll<T>()
        {
            return _database.GetCollection<T>(nameof(T)).AsQueryable();
        }
    }
}