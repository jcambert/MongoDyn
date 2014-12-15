using ImpromptuInterface;
using ImpromptuInterface.Dynamic;
using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
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

    public abstract class DynamicCollection : IDynamicCollection
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(DynamicCollection));
        protected readonly bool NotifyEnabled;
        internal readonly MongoCollection<Document> Collection;
        protected internal MethodInfo GetByKeyMethodInfo;
        protected readonly List<string> DocumentKeys = new List<string>();
        protected bool _eagerLoad;
        protected readonly PropertyInfo[] _properties;
        //protected readonly Dictionary<string, DynamicCollection> ChildCollections = new Dictionary<string, DynamicCollection>();
        //protected readonly Dictionary<string, PropertyInfo> ChildMethods = new Dictionary<string, PropertyInfo>();
        protected readonly Dictionary<string, Child> Childs = new Dictionary<string, Child>();
        protected readonly Dictionary<string, DynamicCollection> FkCollections = new Dictionary<string, DynamicCollection>();

        /// <summary>
        /// Cache property that will be used to eager load collections.
        /// </summary>
        internal MethodInfo CollectionQueryMethodInfo;


        public Func<object> NestIdGenerator;

        #region ctor
        internal DynamicCollection(Type collectionType, string collectionName, PropertyInfo key, bool notifyEnabled = false, bool audit = true,bool eagerLoad=false)
        {
            Contract.Requires(collectionName != null && collectionName.Trim().Length > 0, "Collection Name must be non null nor empty");
            Contract.Requires(key != null, "key must not be null");

            NotifyEnabled = notifyEnabled;
            Collection = Dynamic.Db.GetCollection<Document>(collectionName);
            CollectionName = collectionName;
            CollectionType = collectionType;
            Key = key;
            KeyType = key.PropertyType;
            Audit = audit;

            _properties = collectionType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            _eagerLoad = eagerLoad;
        }
        #endregion

        #region properties

        internal PropertyInfo Key { get; private set; }

        internal bool Audit { get; private set; }

        public string CollectionName
        {
            get;
            private set;
        }

        public Type CollectionType
        {
            get;
            private set;
        }

        public Type KeyType
        {
            get;
            private set;
        }

        internal object GetKey(object entity)
        {
            return Key.GetGetMethod().Invoke(entity, new object[] { });
        }

        internal void SetKey(object entity, object id)
        {
            Key.GetSetMethod().Invoke(entity, new object[] { id });
        }
        #endregion


        protected virtual void initialize()
        {
            logger.Debug("initialize()");

        }

        internal void PrepareEagerLoad()
        {
            logger.Debug("PrepareEagerLoad()");
            if (!_eagerLoad)
                return;

            foreach (var propertyInfo in _properties.Where(p => p.CanWrite && p.PropertyType.IsInterface && p.GetCustomAttributes(typeof(ForeignKeyAttribute), true).Count() > 0))
            {
                var type = propertyInfo.PropertyType;
                if (type.IsGenericType)
                {

                    if (type.GetGenericTypeDefinition() == typeof(IList<>).GetGenericTypeDefinition())
                    {
                        var argu = type.GetGenericArguments().First();
                        var repo = Dynamic.BuildRepository(argu);


                        Child child = new Child(this, propertyInfo.Name, repo, propertyInfo);

                        Childs[argu.Name] = child;
                    }
                }
                else
                {
                    var repo = Dynamic.BuildRepository(type);
                    FkCollections.Add(propertyInfo.Name, repo);
                }
            }
        }

        internal Document GetBsonDocumentById(object id)
        {
            logger.Debug(string.Format("GetBsonDocumentById({0})", id.ToString()));
            Document dynamicDocument = Collection.FindOneById(BsonValue.Create(id));
            return dynamicDocument;
        }

        protected void Update(object entity, object keyValue)
        {
            logger.Debug(string.Format("Update({0},{1})", entity.ToString(), keyValue.ToString()));
            Document doc = BuildDocument(entity, keyValue);
            Collection.Save(doc);
        }

        protected object BuildKey(object entity, object id)
        {
            var entityKey = GetKey(entity);

            if (KeyType == typeof(int))
            {
                if (((int)entityKey) == 0)
                {
                    id = id ?? IdGenerator.GetNextIdFor(CollectionType);
                    SetKey(entity, id);
                }
                else
                    id = entityKey;
            }
            else
            {
                if (entityKey == null)
                {
                    if (NestIdGenerator == null) throw new ArgumentNullException("A Id generator must be specified (NestIdGenerator)  for non int Key");
                    id = NestIdGenerator();
                    SetKey(entity, id);
                }
                else
                    id = entityKey;
            }

            return id;
        }

        internal Document BuildDocument(object entity, object id, Child relation = null)
        {

            id = BuildKey(entity, id);

            logger.Debug(string.Format("BuildDocument({0},{1})", entity.ToString(), id.ToString()));

            var document = new Document(id, Audit);
            document.BaseType = CollectionType;
            foreach (var name in DocumentKeys)
            {
                if (relation != null && relation.Relation == RelationShip.Embeddded && relation.ForeignKey.ForeignKeyName == name) continue;
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

            foreach (KeyValuePair<string, Child> pair in Childs)
            {

                List<BsonValue> values = new List<BsonValue>();



                var pValue = Impromptu.InvokeGet(entity, pair.Value.Name);

                foreach (var item in pValue)
                {

                    if (pair.Value.Relation == RelationShip.Embeddded)
                    {
                        var k = pair.Value.Collection.GetKey(item);
                        var doc = pair.Value.Collection.BuildDocument(item, k, pair.Value);
                        ((Document)doc).Embed();
                        var p = BsonValue.Create(doc);
                        values.Add(p);
                    }
                    else
                    {
                        var k = pair.Value.Collection.GetKey(item);
                        k = pair.Value.Collection.BuildKey(item, k);

                        var p = BsonValue.Create(k);
                        values.Add(p);
                    }

                }
                if (values.Count > 0)
                    document[pair.Value.Name] = BsonArray.Create(values);

            }

            return document;
        }

        protected static bool IsDefaultValue(object keyValue)
        {
            logger.Debug(string.Format("IsDefaultValue({0})", keyValue.ToString()));
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


        public long Count
        {
            get
            {
                logger.Debug("Count");
                return Collection.Count();
            }
        }

        public void DeleteAll(bool resetCounter)
        {
            logger.Debug(string.Format("DeleteAll({0})", resetCounter));
            Collection.Drop();
            if (resetCounter)
            {
                IdGenerator.ResetCounter(CollectionType);
            }
        }
    }

    public sealed class DynamicCollection<TKey, TModel> : DynamicCollection, IDynamicCollection<TKey, TModel>

        where TModel : class
    {


        public event EventHandler<ModelEventArgs<TModel>> onBeforeSave = delegate { };
        public event EventHandler<ModelEventArgs<TModel>> onAfterSave = delegate { };

        static readonly ILog logger = LogManager.GetLogger(typeof(DynamicCollection<,>));





        #region ctor

        internal DynamicCollection(Type collectionType, string collectionName, PropertyInfo key, bool notifyEnabled = false, bool audit = true,bool eagerLoad=false)
            : base(collectionType, collectionName, key, notifyEnabled, audit,eagerLoad)
        {
            Contract.Requires(typeof(TKey).Equals(key.PropertyType), "TKey and property info key must be the same type");


            initialize();

        }




        #endregion




        protected override void initialize()
        {
            base.initialize();

            logger.Debug("initialize()");
            CollectionQueryMethodInfo = Helper.GetMethodInfo<DynamicCollection<TKey, TModel>>(x => x.CollectionQuery(null, 0, null));

            GetByKeyMethodInfo = Helper.GetMethodInfo<DynamicCollection<TKey, TModel>>(x => x.GetByKey(default(TKey)));

            //_eagerLoad = Helper.IsEagerLoadEnabled<TModel>();

          

            foreach (var propertyInfo in _properties.Where(propertyInfo => propertyInfo.CanWrite /*&& propertyInfo.PropertyType.IsInterface/* && propertyInfo.GetCustomAttributes<ForeignKeyAttribute>().Count()==0*/))
            {
                DocumentKeys.Add(propertyInfo.Name);
            }

            DocumentKeys.RemoveAll(p => p == Key.Name);

            var indexes = Helper.GetIndexes<TModel>();

            foreach (var tuple in indexes)
            {
                Collection.CreateIndex(tuple.Item1, tuple.Item2);
            }
        }



        private TModel CastSingle(Document document)
        {
            // logger.Debug(string.Format("CastSingle({0})", document.ToString()));
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

            if (Childs.Count > 0)
            {
                TryGetChildCollections(entity);
            }

            return entity;
        }

        private IEnumerable<TModel> CastMany(IEnumerable<Document> documents)
        {
            logger.Debug(string.Format("CastMany(documents)"));
            return documents.Select(CastSingle).AsEnumerable();
        }


        void TryGetFKs(TModel entity)
        {
            logger.Debug(string.Format("TryGetFKs({0})", entity.ToString()));
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
            logger.Debug(string.Format("TryGetChildCollections({0})", entity.ToString()));
            foreach (var keyValuePair in Childs)
            {
                var queryCollectionInfo = Helper.GetDefForChildCollection<TModel>(keyValuePair.Key);

                if (queryCollectionInfo.DetailTable == null)
                    continue;

                //id atual
                var currentId = Impromptu.InvokeGet(entity, Key.Name);

                var mInfo = keyValuePair.Value.Collection.CollectionQueryMethodInfo;
                // GetMethod("CollectionQuery");

                var result = mInfo.Invoke(keyValuePair.Value, new object[] { queryCollectionInfo.DetailKey, ExpressionType.Equal, currentId });

                Impromptu.InvokeSet(entity, queryCollectionInfo.MasterProperty, result);
            }
        }




        private Document Insert(object entity)
        {
            logger.Debug(string.Format("Insert({0})", entity.ToString()));
            Document newDocument = BuildDocument(entity, null);


            Collection.Insert(newDocument);
            return newDocument;
        }

        #region IDynamicCollection


        public bool DeleteByKey(TKey key)
        {
            logger.Debug(string.Format("DeleteByKey({0})", key.ToString()));
            FindAndRemoveArgs fama = new FindAndRemoveArgs();
            fama.Query = Query.EQ("_id", BsonValue.Create(key));
            fama.SortBy = SortBy.Null;
            var result = Collection.FindAndRemove(fama);
            return result.Ok;
        }

        public TModel GetByKey(TKey key)
        {
            logger.Debug(string.Format("GetByKey({0})", key.ToString()));
            Document document = GetBsonDocumentById(key);
            var entity = document == null
                       ? null
                       : CastSingle(document);

            return entity;
        }

        public TModel GetFirstOrDefault(System.Linq.Expressions.Expression<Func<TModel, bool>> id)
        {
            logger.Debug(string.Format("GetFirstOrDefault({0})", id.ToString()));
            return id == null
              ? CastSingle(Collection.FindOne())
              : CustomQuery(id).FirstOrDefault();
        }

        public IEnumerable<TModel> All()
        {
            logger.Debug("All()");
            var documents = Collection.FindAllAs<Document>();
            return CastMany(documents);
        }

        public IEnumerable<TModel> CollectionQuery(string memberName, System.Linq.Expressions.ExpressionType op, object value)
        {
            logger.Debug(string.Format("CollectionQuery({0},{1},{2})", memberName.ToString(), op, value));
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
            logger.Debug(string.Format("CustomQuery({0})", expression));
            if (expression == null)
                return All();

            var query = QueryBuilder.BuildQuery(expression, Key.Name);

            return query == null
                ? Enumerable.Empty<TModel>()
                : CastMany(Collection.FindAs<Document>(query));
        }

        public TModel New()
        {
            logger.Debug("New()");
            TModel result = NotifyEnabled
                       ? new ImpromptuDictionary().ActLike<TModel>(typeof(INotifyPropertyChanged))
                       : ImpromptuDictionary.Create<TModel>();

            foreach (KeyValuePair<string, Child> pair in Childs)
            {
                PropertyInfo p = pair.Value.Property;

                ImpromptuList child = new ImpromptuList();
                MethodInfo act = child.GetType().GetMethod("ActLike");
                MethodInfo generic = act.MakeGenericMethod(p.PropertyType);
                var o = generic.Invoke(child, new object[] { new Type[] { } });

                p.GetSetMethod().Invoke(result, new object[] { o });

                child.CollectionChanged += (sender, e) =>
                {
                    if (Debugger.IsAttached) Debugger.Break();
                    // e.NewItems[0].
                };
            }
            return result;
        }



        public void UpsertDynamic(IDictionary<string, object> item)
        {
            logger.Debug(string.Format("UpsertDynamic({0})", item));
            Upsert(item.ActLike<TModel>());
        }

        public void UpsertImpromptu(dynamic item)
        {
            logger.Debug(string.Format("UpsertImpromptu({0})", item));
            Upsert(new ImpromptuDictionary(item).ActLike<TModel>());
        }

        public void Upsert(TModel item)
        {
            logger.Debug(string.Format("Upsert({0})", item));
            try
            {
                dynamic entity = item;
                var keyValue = Impromptu.InvokeGet(entity, Key.Name);

                if (IsDefaultValue(keyValue))
                {
                    onBeforeSave(this, new ModelEventArgs<TModel>(item));
                    Document document = Insert(entity);
                    Impromptu.InvokeSet(item, Key.Name, document.GetKey);
                    onAfterSave(this, new ModelEventArgs<TModel>(item));
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
            logger.Debug(string.Format("Delete({0})", item));
            var keyValue = Impromptu.InvokeGet(item, Key.Name);
            return DeleteByKey(keyValue);
        }


        #endregion


        #region Properties





        #endregion

        public IQueryable<TModel> AsQueryable
        {
            get { return Collection.AsQueryable<TModel>(); }
        }

        public void BatchInsert(IEnumerable<TModel> items)
        {
            logger.Debug("InsertMany(items)");

            var array = items.ToArray();
            List<TModel> materializedItems = array.Where(r => r != null).ToList();
            logger.Info("InsertMany called for " + materializedItems.Count + "non-null items");
            foreach (TModel record in materializedItems)
            {
                onBeforeSave(this, new ModelEventArgs<TModel>(record));
            }
            if (materializedItems.Count > 0)
            {
                Collection.InsertBatch(materializedItems);
            }
            foreach (TModel record in materializedItems)
            {
                onAfterSave(this, new ModelEventArgs<TModel>(record));
            }

        }




        public IEnumerable<TModel> GetManyByKeys(IEnumerable<TKey> keys)
        {

            var array = keys.ToArray();
            logger.Debug("GetManyByIds(ListOfIds.Length=" + array.Length + ")");
            var q = Query.In(Dynamic.ID_FIELD, new BsonArray(array));
            return (IEnumerable<TModel>)Collection.Find(q);
        }
    }
}
