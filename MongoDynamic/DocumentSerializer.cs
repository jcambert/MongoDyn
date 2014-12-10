using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MongoDyn
{
    internal class DocumentSerializer : IBsonSerializer
    {






        public IBsonSerializationOptions GetDefaultSerializationOptions()
        {
            throw new NotImplementedException();
        }

        public void Serialize(MongoDB.Bson.IO.BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
        {
            if (nominalType == typeof(DocumentSerializer))
                throw new ArgumentException("Cannot serialize anything but self");
            Document document = value as Document;
            var elts = document.Elements;
            var propsDoc = document.BaseType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanWrite & p.GetCustomAttributes<KeyAttribute>().Count() == 0);
            var propsType = nominalType.GetProperties(BindingFlags.Instance | BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly).Where(p => p.CanWrite);
            var key = document.BaseType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanWrite & p.GetCustomAttributes<KeyAttribute>().Count() == 1).FirstOrDefault();

            bsonWriter.WriteStartDocument();

            foreach (var prop in propsDoc)
            {
                
                bsonWriter.WriteName(prop.Name);
                BsonSerializer.Serialize(bsonWriter, prop.PropertyType, document[prop.Name]);
            }


            foreach (var prop in propsType)
            {
                if (!Dynamic.Audit && prop.Name.Equals("LastChanges")) continue;
                bsonWriter.WriteName(prop.Name);
                BsonSerializer.Serialize(bsonWriter, prop.PropertyType, prop.GetValue(value, null));
            }

            if (key != null)
            {
                bsonWriter.WriteName("_id");
                BsonSerializer.Serialize(bsonWriter, key.PropertyType, document["_id"]);
            }
            bsonWriter.WriteEndDocument();

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
            var obj = Activator.CreateInstance(actualType);

            bsonReader.ReadStartDocument();

            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var name = bsonReader.ReadName();

                var field = actualType.GetField(name);
                if (field != null)
                {
                    var value = BsonSerializer.Deserialize(bsonReader, field.FieldType);
                    field.SetValue(obj, value);
                }

                var prop = actualType.GetProperty(name);
                if (prop != null)
                {
                    var value = BsonSerializer.Deserialize(bsonReader, prop.PropertyType);
                    prop.SetValue(obj, value, null);
                }
            }

            bsonReader.ReadEndDocument();

            return obj;
        }


        public object Deserialize(MongoDB.Bson.IO.BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
        {
            if (nominalType == typeof(DocumentSerializer)) throw new ArgumentException("Cannot deserialize anything but self");
            Document obj = Activator.CreateInstance(nominalType) as Document;
            object value=null;
            bsonReader.ReadStartDocument();

            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var name = bsonReader.ReadName();
                var btype = bsonReader.CurrentBsonType;

                if (!Dynamic.Audit && name.Equals("LastChanges")) continue;
                
                
                switch (btype)
                {
                    case BsonType.Array:
                        value = BsonSerializer.Deserialize(bsonReader,typeof(Array) );
                        break;
                    case BsonType.Binary:
                        break;
                    case BsonType.Boolean:
                        value = BsonSerializer.Deserialize(bsonReader, typeof(Boolean));
                        break;
                    case BsonType.DateTime:
                        value = BsonSerializer.Deserialize(bsonReader, typeof(DateTime));
                        break;
                    case BsonType.Document:
                        break;
                    case BsonType.Double:
                        value = BsonSerializer.Deserialize(bsonReader, typeof(double));
                        break;
                    case BsonType.EndOfDocument:
                        break;
                    case BsonType.Int32:
                        value = BsonSerializer.Deserialize(bsonReader, typeof(Int32));
                        break;
                    case BsonType.Int64:
                        value = BsonSerializer.Deserialize(bsonReader, typeof(Int64));
                        break;
                    case BsonType.JavaScript:
                        break;
                    case BsonType.JavaScriptWithScope:
                        break;
                    case BsonType.MaxKey:
                        break;
                    case BsonType.MinKey:
                        break;
                    case BsonType.Null:
                        break;
                    case BsonType.ObjectId:
                        value = BsonSerializer.Deserialize(bsonReader, typeof(Int32));
                        break;
                    case BsonType.RegularExpression:
                        break;
                    case BsonType.String:
                        value = BsonSerializer.Deserialize(bsonReader, typeof(string));
                        break;
                    case BsonType.Symbol:
                        break;
                    case BsonType.Timestamp:
                        value = BsonSerializer.Deserialize(bsonReader, typeof(DateTime));
                        break;
                    case BsonType.Undefined:
                        break;
                    default:
                        break;
                }

                

                obj[name] = BsonValue.Create(value);

            }

            bsonReader.ReadEndDocument();

            return obj;
        }
    }
}
