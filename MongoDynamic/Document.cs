using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDyn
{
    [BsonSerializer(typeof(DocumentSerializer))]
    [Serializable]
    internal class Document : BsonDocument
    {
        private bool _isEmbedded;
        public Document()
            : this(null, false)
        {
        }

        public Document(object value, bool audit)
        {
            _isEmbedded = false;
            if (value != null)
                this["_id"] = BsonValue.Create(value);
            else this["_id"] = BsonNull.Value;

            if (audit)
                LastChanges = DateTime.Now;
        }

        public DateTime LastChanges
        {
            get { return this["LastChanges"].ToUniversalTime(); }
            set { this["LastChanges"] = value; }
        }



        public object GetKey
        {
            get { return this["_id"].RawValue; }
        }

        public BsonValue Id { get { return this["_id"]; } }

        internal Type BaseType { get; set; }

        public override BsonValue this[string name]
        {
            get
            {
                return base[name];
            }
            set
            {
                base[name] = value;
            }
        }

        internal  void Embed()
        {
            _isEmbedded = true;
        }

        internal bool IsEmbedded()
        {
            return _isEmbedded;
        }
    }
}
