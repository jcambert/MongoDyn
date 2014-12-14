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

        static IdGenerator()
        {
            //var url = new MongoUrl(Dynamic.GetConnString());
            var db = Dynamic.Db; // MongoServer.Create(url).GetDatabase(url.DatabaseName);
            NextIdTable = db.GetCollection(LastId);
        }

        /// <summary>
        /// Restart from zero the counter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal static void ResetCounter<T>()
        {
            ResetCounter(typeof(T));
        }

        /// <summary>
        /// Restart from zero the counter.
        /// </summary>
        /// <param name="t"></param>
        internal static void ResetCounter(Type t)
        {
            var docName = t.Name;
            NextIdTable.FindAndRemove(Query.EQ(Dynamic.ID_FIELD, BsonValue.Create(docName)), SortBy.Null);
        }

        internal static int GetNextIdFor(Type type)
        {
            var docName = type.Name;
            var f = NextIdTable.FindAndModify(Query.EQ(Dynamic.ID_FIELD, docName), SortBy.Null, Update.Inc(LastId, 1), true, true);
            return f.ModifiedDocument[LastId].AsInt32;
        }
    }
}
