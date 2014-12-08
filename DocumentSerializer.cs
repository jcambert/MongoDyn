using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDyn
{
    internal class DocumentSerializer : HashSet<Document>,IBsonSerializer
    {

          private class EntityComparer : IEqualityComparer<Document>
    {
        public bool Equals(Document x, Document y) { return x.Id.Equals(y.Id); }
        public int GetHashCode(Document obj) { return obj.Id.GetHashCode(); }
    }
 
    public DocumentSerializer()
        : base(new EntityComparer())
    {
    }

    public DocumentSerializer(IEnumerable<Document> values)
        : base (values, new EntityComparer())
    {
    }

        

        public object Deserialize(MongoDB.Bson.IO.BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
        {
            if (nominalType == typeof(DocumentSerializer)) throw new ArgumentException("Cannot deserialize anything but self");
            ArraySerializer<Document> ser = new ArraySerializer<Document>();
            return new DocumentSerializer((Document[])ser.Deserialize(bsonReader,nominalType,options));
        }

        public IBsonSerializationOptions GetDefaultSerializationOptions()
        {
            throw new NotImplementedException();
        }

        public void Serialize(MongoDB.Bson.IO.BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
        {
            if (nominalType == typeof(DocumentSerializer))
                throw new ArgumentException("Cannot serialize anything but self");
            //BsonSerializer.Serialize<Document>(bsonWriter, value as Document);
            //ArraySerializer<Document> ser = new ArraySerializer<Document>();
            //ser.Serialize(bsonWriter, nominalType, value,options);
        }


        public bool GetDocumentId(out object id, out Type idNominalType, out IIdGenerator idGenerator)
        {
            id = null;
            idGenerator = null;
            idNominalType = typeof(Document);
            return false;
        }

      /*  public void Serialize(MongoDB.Bson.IO.BsonWriter bsonWriter, Type nominalType, IBsonSerializationOptions options)
        {
            if (nominalType == typeof(DocumentSerializer))
                throw new ArgumentException("Cannot serialize anything but self");
            ArraySerializer<Document> ser = new ArraySerializer<Document>();
            ser.Serialize(bsonWriter,nominalType,value, options);
        }
        */
        public void SetDocumentId(object id)
        {
            return;
        }

        public object Deserialize(MongoDB.Bson.IO.BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
        {
            if (nominalType == typeof(DocumentSerializer)) throw new ArgumentException("Cannot deserialize anything but self");
            //ArraySerializer<Document> ser = new ArraySerializer<Document>();
            //return new DocumentSerializer((Document[])ser.Deserialize(bsonReader, nominalType, options));
            return BsonSerializer.Deserialize<Document>(bsonReader);
        }
    }
}
