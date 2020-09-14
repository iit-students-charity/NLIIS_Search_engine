using System;
using MongoDB.Bson;

namespace Contracts
{
    public class TextDocument : IDocument
    {
        public ObjectId Id { get; set; }
        
        public string Title { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}