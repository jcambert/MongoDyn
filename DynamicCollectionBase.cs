using ImpromptuInterface;
using ImpromptuInterface.Dynamic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MongoDyn
{
    public abstract class DynamicCollectionBase<TKey, TModel> : IDynamicCollection<TKey, TModel>
       
        where TModel : class
    {

        protected readonly bool NotifyEnabled;

        internal readonly MongoCollection<Document> Collection;

        /// <summary>
        /// Cache property that will be used to eager load collections.
        /// </summary>
        internal MethodInfo CollectionQueryMethodInfo;

        /// <summary>
        /// Cache property that will be used to eager load foreignKeys
        /// </summary>

        internal MethodInfo GetByKeyMethodInfo;


        protected readonly List<string> DocumentKeys = new List<string>();

        internal readonly Dictionary<string, DynamicCollectionBase<TKey, TModel>> FkCollections = new Dictionary<string, DynamicCollectionBase<TKey, TModel>>();

        internal readonly Dictionary<string, DynamicCollectionBase<TKey, TModel>> ChildCollections = new Dictionary<string, DynamicCollectionBase<TKey, TModel>>();

        #region ctor

        internal DynamicCollectionBase(string collectionName, PropertyInfo key, bool notifyEnabled = false, bool audit = false)
        {
            Contract.Requires(collectionName != null && collectionName.Trim().Length > 0, "Collection Name must be non null nor empty");
            Contract.Requires(key != null, "key must not be null");
            Contract.Requires(typeof(TKey).Equals(key.PropertyType), "TKey and property info key must be the same type");
            NotifyEnabled = notifyEnabled;
            Collection = Dynamic.Db.GetCollection<Document>(collectionName);
            Key = key;
            KeyType = typeof(TKey);
            Audit = audit;
        }

        #endregion



        internal Document GetBsonDocumentById(object id)
        {
            Document dynamicDocument = Collection.FindOneById(BsonValue.Create(id));
            return dynamicDocument ;
        }

        protected virtual void Update(object entity, object keyValue)
        {
            Document doc = BuildDocument(entity, keyValue);
            Collection.Save(doc);
        }

        internal Document BuildDocument(object entity, object id)
        {
            var document = new Document(id, Audit);
            foreach (var name in DocumentKeys)
            {
                var pValue = Impromptu.InvokeGet(entity, name);
                try
                {
                    var bsonValue = BsonValue.Create(pValue);
                    document[name] = bsonValue;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    document[name] = BsonNull.Value;
                    continue;
                }
            }

            return document;
        }

        private TModel CastSingle(Document document)
        {
            if (document == null)
                return null;

            dynamic item = new ImpromptuDictionary(document.ToDictionary());
            var keyValue = document.GetKey;
            Impromptu.InvokeSetIndex(item, Key.Name, keyValue);

            var entity = NotifyEnabled
                       ? item.ActLike<TModel>(typeof(INotifyPropertyChanged))
                       : item.ActLike<TModel>();

            if (FkCollections.Count > 0)
            {
                TryGetFKs(entity);
            }

            if (ChildCollections.Count > 0)
            {
                TryGetChildCollections(entity);
            }

            return entity;
        }

        private IEnumerable<TModel> CastMany(IEnumerable<Document> documents)
        {
            return documents.Select(CastSingle).AsEnumerable();
        }


        void TryGetFKs(TModel entity)
        {
            foreach (var keyValuePair in FkCollections)
            {
                var qInfo = Helper.GetDefForFK<TModel>(keyValuePair.Key);

                if (qInfo.SimpleProperty == null)
                    continue;

                var fkValue = Impromptu.InvokeGet(entity, qInfo.SimpleProperty);

                if (fkValue == null)
                    continue;

                var mInfo = keyValuePair.Value.GetByKeyMethodInfo;

                var result = mInfo.Invoke(keyValuePair.Value, new object[] { fkValue });
                if (result == null)
                    continue;

                Impromptu.InvokeSet(entity, qInfo.ComplexPropertyName, result);
            }
        }

        void TryGetChildCollections(TModel entity)
        {
            foreach (var keyValuePair in ChildCollections)
            {
                var queryCollectionInfo = Helper.GetDefForChildCollection<TModel>(keyValuePair.Key);

                if (queryCollectionInfo.DetailTable == null)
                    continue;

                //id atual
                var currentId = Impromptu.InvokeGet(entity, Key.Name);

                var mInfo = keyValuePair.Value.CollectionQueryMethodInfo;
                // GetMethod("CollectionQuery");

                var result = mInfo.Invoke(keyValuePair.Value, new object[] { queryCollectionInfo.DetailKey, ExpressionType.Equal, currentId });

                Impromptu.InvokeSet(entity, queryCollectionInfo.MasterProperty, result);
            }
        }

        protected static bool IsDefaultValue(object keyValue)
        {
            if (keyValue == null)
                return true;

            var bsonValue = BsonValue.Create(keyValue);

            switch (bsonValue.BsonType)
            {
                case BsonType.Int32:
                    return Convert.ToInt32(keyValue) == 0;
                default:
                    return false;
            }
        }


        private Document Insert(object entity)
        {
            Document newDocument = KeyType == typeof(int)
                ? BuildDocument(entity, IdGenerator.GetNextIdFor(typeof(TModel)))
                : BuildDocument(entity, null);

            Collection.Insert(newDocument);
            return newDocument;
        }

        #region IDynamicCollection
        public long Count
        {
            get { return Collection.Count(); }
        }

        public bool DeleteByKey(TKey key)
        {
            FindAndRemoveArgs fama = new FindAndRemoveArgs();
            fama.Query = Query.EQ("_id", BsonValue.Create(key));
            fama.SortBy = SortBy.Null;
            var result = Collection.FindAndRemove(fama);
            return result.Ok;
        }

        public TModel GetByKey(TKey key)
        {
            Document document =  GetBsonDocumentById(key);
            var entity = document == null
                       ? null
                       : CastSingle(document);

            return entity;
        }

        public TModel GetFirstOrDefault(System.Linq.Expressions.Expression<Func<TModel, bool>> id)
        {
            return id == null
              ? CastSingle(Collection.FindOne())
              : CustomQuery(id).FirstOrDefault();
        }

        public IEnumerable<TModel> All()
        {
            var documents = Collection.FindAllAs<Document>();
            return CastMany(documents);
        }

        public IEnumerable<TModel> CollectionQuery(string memberName, System.Linq.Expressions.ExpressionType op, object value)
        {
            var bsonValue = BsonValue.Create(value);

            IMongoQuery query = null;
            switch (op)
            {
                case ExpressionType.Equal: query = Query.EQ(memberName, bsonValue);
                    break;
                case ExpressionType.GreaterThan:
                    query = Query.GT(memberName, bsonValue);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    query = Query.GTE(memberName, bsonValue);
                    break;
                case ExpressionType.LessThan:
                    query = Query.LT(memberName, bsonValue);
                    break;
                case ExpressionType.LessThanOrEqual:
                    query = Query.LTE(memberName, bsonValue);
                    break;
            }
            return CastMany(Collection.FindAs<Document>(query));
        }

        public IEnumerable<TModel> CustomQuery(System.Linq.Expressions.Expression<Func<TModel, bool>> expression)
        {
            if (expression == null)
                return All();

            var query = QueryBuilder.BuildQuery(expression, Key.Name);

            return query == null
                ? Enumerable.Empty<TModel>()
                : CastMany(Collection.FindAs<Document>(query));
        }

        public TModel New()
        {
            return NotifyEnabled
                       ? new ImpromptuDictionary().ActLike<TModel>(typeof(INotifyPropertyChanged))
                       : ImpromptuDictionary.Create<TModel>();
        }

        public void UpsertDynamic(IDictionary<string, object> item)
        {
            Upsert(item.ActLike<TModel>());
        }

        public void UpsertImpromptu(dynamic item)
        {
            Upsert(new ImpromptuDictionary(item).ActLike<TModel>());
        }

        public void Upsert(TModel item)
        {
            try
            {
                dynamic entity = item;
                var keyValue = Impromptu.InvokeGet(entity, Key.Name);

                if (IsDefaultValue(keyValue))
                {
                    Document document = Insert(entity);
                    Impromptu.InvokeSet(item, Key.Name, document.GetKey);
                }
                else Update(entity, keyValue);
            }
            catch (MongoSafeModeException mongoSafeModeException)
            {
                Debug.WriteLine(mongoSafeModeException);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                throw;
            }
        }

        public bool Delete(TModel item)
        {
            var keyValue = Impromptu.InvokeGet(item, Key.Name);
            return DeleteByKey(keyValue);
        }

        public void DeleteAll(bool resetCounter)
        {
            Collection.Drop();
            if (resetCounter)
            {
                IdGenerator.ResetCounter<TModel>();
            }
        }
        #endregion


        #region Properties
        protected PropertyInfo Key { get; private set; }

        protected bool Audit { get; private set; }

        protected Type KeyType { get; private set; }
        #endregion
    }
}
