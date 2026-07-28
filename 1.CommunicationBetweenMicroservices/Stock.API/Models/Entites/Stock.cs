using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Stock.API.Models.Entites;

public class Stock
{
    [BsonId]
    [BsonRepresentation((MongoDB.Bson.BsonType)MongoDB.Bson.GuidRepresentation.CSharpLegacy)]
    [BsonElement(Order = 0)]
    public Guid Id {get;set;}
    [BsonRepresentation((MongoDB.Bson.BsonType)MongoDB.Bson.GuidRepresentation.CSharpLegacy)]
    [BsonElement(Order = 1)]
    public Guid ProductId {get;set;}
    [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]
    [BsonElement(Order = 2)]
    public int Quantity {get;set;}

}
