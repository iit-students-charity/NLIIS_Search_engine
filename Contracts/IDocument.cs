using MongoDB.Bson;

namespace Contracts
{
    public interface IDocument
    {
        ObjectId Id { get; set; }
    }
}