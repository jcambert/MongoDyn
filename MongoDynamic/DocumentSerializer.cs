using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                if (document[prop.Name].GetType() != typeof(BsonNull) )
                {
                    bsonWriter.WriteName(prop.Name);
                    BsonSerializer.Serialize(bsonWriter, prop.PropertyType, document[prop.Name]);
                }
            }


            foreach (var prop in propsType)
            {
                
                if (!Dynamic.Audit && prop.Name.Equals("LastChanges")) continue;

                try
                {
                    if (document[prop.Name].GetType() == typeof(BsonNull)) continue;
                }
                catch (Exception ex)
                {
                    continue;
                }

                try
                {
                    
                    bsonWriter.WriteName(prop.Name);
                    BsonSerializer.Serialize(bsonWriter, prop.PropertyType, prop.GetValue(value, null));
                }
                catch (Exception ex)
                {
                    bsonWriter.WriteNull();
                    continue;
                }
            }

            if (key != null && !document.IsEmbedded())
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
            object value = null;
            bsonReader.ReadStartDocument();

            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var name = bsonReader.ReadName();
                var btype = bsonReader.CurrentBsonType;

                if (!Dynamic.Audit && name.Equals("LastChanges")) continue;


                switch (btype)
                {
                    case BsonType.Array:
                        value = BsonSerializer.Deserialize(bsonReader, typeof(List<object>));
                        break;
                    case BsonType.Binary:
                        if (Debugger.IsAttached) Debugger.Break();
                        break;
                    case BsonType.Boolean:
                        value = BsonSerializer.Deserialize(bsonReader, typeof(Boolean));
                        break;
                    case BsonType.DateTime:
                        value = BsonSerializer.Deserialize(bsonReader, typeof(DateTime));
                        break;
                    case BsonType.Document:
                        if (Debugger.IsAttached) Debugger.Break();
                        break;
                    case BsonType.Double:
                        value = BsonSerializer.Deserialize(bsonReader, typeof(double));
                        break;
                    case BsonType.EndOfDocument:
                        if (Debugger.IsAttached) Debugger.Break();
                        break;
                    case BsonType.Int32:
                        value = BsonSerializer.Deserialize(bsonReader, typeof(Int32));
                        break;
                    case BsonType.Int64:
                        value = BsonSerializer.Deserialize(bsonReader, typeof(Int64));
                        break;
                    case BsonType.JavaScript:
                        if (Debugger.IsAttached) Debugger.Break();
                        break;
                    case BsonType.JavaScriptWithScope:
                        if (Debugger.IsAttached) Debugger.Break();
                        break;
                    case BsonType.MaxKey:
                        if (Debugger.IsAttached) Debugger.Break();
                        break;
                    case BsonType.MinKey:
                        if (Debugger.IsAttached) Debugger.Break();
                        break;
                    case BsonType.Null:
                        if (Debugger.IsAttached) Debugger.Break();
                        bsonReader.ReadNull();
                        value = null;
                        break;
                    case BsonType.ObjectId:
                        value = BsonSerializer.Deserialize(bsonReader, typeof(Int32));
                        break;
                    case BsonType.RegularExpression:
                        if (Debugger.IsAttached) Debugger.Break();
                        break;
                    case BsonType.String:
                        value = BsonSerializer.Deserialize(bsonReader, typeof(string));
                        break;
                    case BsonType.Symbol:
                        if (Debugger.IsAttached) Debugger.Break();
                        break;
                    case BsonType.Timestamp:

                        value = BsonSerializer.Deserialize(bsonReader, typeof(DateTime));
                        break;
                    case BsonType.Undefined:
                        if (Debugger.IsAttached) Debugger.Break();
                        break;
                    default:
                        break;
                }

                if (value != null)
               

                    obj[name] = BsonValue.Create(value);

            }

            bsonReader.ReadEndDocument();

            return obj;
        }
    }
}
