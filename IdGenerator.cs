using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDyn
{
    internal static class IdGenerator
    {
        private static readonly MongoCollection NextIdTable;
        private const string LastId = "LastId";
        private const string Id = "_id";

        static IdGenerator()
        {
            var url = new MongoUrl(Dynamic.GetConnString());
            var db = MongoServer.Create(url).GetDatabase(url.DatabaseName);
            NextIdTable = db.GetCollection(LastId);
        }

        /// <summary>
        /// Restart from zero the counter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal static void ResetCounter<T>()
        {
            var docName = typeof(T).Name;
            NextIdTable.FindAndRemove(Query.EQ(Id, BsonValue.Create(docName)), SortBy.Null);
        }

        internal static int GetNextIdFor(Type type)
        {
            var docName = type.Name;
            var f = NextIdTable.FindAndModify(Query.EQ(Id, docName), SortBy.Null, Update.Inc(LastId, 1), true, true);
            return f.ModifiedDocument[LastId].AsInt32;
        }
    }
}
