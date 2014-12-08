using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDyn
{
    internal class Document : BsonDocument
    {
        public Document()
            : this(null, false)
        {
        }

        public Document(object value, bool audit)
        {
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
    }
}
