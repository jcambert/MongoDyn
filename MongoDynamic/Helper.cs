using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MongoDyn
{
    internal  static class Helper
    {
        private static readonly Dictionary<Type, string> KeyNames = new Dictionary<Type, string>();
        private static readonly Dictionary<Type, List<Tuple<IMongoIndexKeys, IMongoIndexOptions>>> Indexes
            = new Dictionary<Type, List<Tuple<IMongoIndexKeys, IMongoIndexOptions>>>();

        private static readonly List<ForeignKeyDef> ForeignKeyDefs = new List<ForeignKeyDef>();
        private static readonly List<ChildCollectionDef> ChildCollectionDefs = new List<ChildCollectionDef>();


        public static MethodInfo GetMethodInfo<T>(Expression<Action<T>> expression)
        {
            Contract.Requires(expression != null, "Expression must not be null");

            var methodCall = expression.Body as MethodCallExpression;
            if (methodCall == null)
            {
                throw new ArgumentException("expression");
            }
            var method = methodCall.Method;
            return method;
        }


        /// <summary>
        /// Struct used as storage for definitions of a child collection that will be eager loaded.
        /// </summary>
        internal struct ChildCollectionDef
        {
            public string MasterTable { get; set; }
            public string MasterProperty { get; set; }
            public string DetailTable { get; set; }
            public string DetailKey { get; set; }
        }

        /// <summary>
        /// Struct used as storage for definitions os FK's that will be eager loaded
        /// </summary>
        internal struct ForeignKeyDef
        {
            public string CurrentTable { get; set; }
            public string ComplexPropertyName { get; set; }
            public string SimpleProperty { get; set; }

            public string OtherTable { get; set; }
        }
        public static void LoadCollection<T, T2>(Expression<Func<T, IEnumerable<T2>>> collectionExpr, Expression<Func<T2, object>> keyExpr)
        {
            var qcInfo = new ChildCollectionDef
            {
                MasterTable = typeof(T).Name,
                MasterProperty = collectionExpr.GetMemberName(),
                DetailTable = typeof(T2).Name,
                DetailKey = keyExpr.GetMemberName()
            };

            ChildCollectionDefs.Add(qcInfo);
        }

        public static void LoadFK<TTable, TOtherTable>(Expression<Func<TTable, TOtherTable>> complexProperty,
            Expression<Func<TTable, object>> simpleProperty)
        {
            Contract.Requires(complexProperty != null);
            var qInfo = new ForeignKeyDef
            {
                CurrentTable = typeof(TTable).Name,
                ComplexPropertyName = complexProperty.GetMemberName(),
                OtherTable = typeof(TOtherTable).Name,
                SimpleProperty = simpleProperty.GetMemberName()
            };

            ForeignKeyDefs.Add(qInfo);
        }

        internal static bool IsEagerLoadEnabled<TModel>()
        {
            var compareStr = typeof(TModel).Name;

            return ForeignKeyDefs.Any(q => q.CurrentTable == compareStr) || ChildCollectionDefs.Any(q => q.MasterTable == compareStr);
        }

        internal static ForeignKeyDef GetDefForFK<TModel>(string complexPropertyName)
        {
            return ForeignKeyDefs
                .Where(q => q.CurrentTable == typeof(TModel).Name && q.ComplexPropertyName == complexPropertyName)
                .FirstOrDefault();
        }

        internal static ChildCollectionDef GetDefForChildCollection<TModel>(string childCollection)
        {
            return ChildCollectionDefs.Where(q => q.MasterTable == typeof(TModel).Name &&
                q.DetailTable == childCollection).FirstOrDefault();
        }


        #region GetKey/SetKey names

        public static void setKeyName<TModel>(PropertyInfo p){
            KeyNames[typeof(TModel)] = p.Name;
        }

        /// <summary>
        /// Configure the property that will be used as Key in document
        /// </summary>
        public static void SetKeyName<TModel>(Expression<Func<TModel, object>> expression)
        {
            var memberName = expression.GetMemberName();
            KeyNames[typeof(TModel)] = memberName;
        }

        internal static bool TryGetKeyName<TModel>(out string keyName)
        {
            if (KeyNames.ContainsKey(typeof(TModel)))
            {
                keyName = KeyNames[typeof(TModel)];
                return true;
            }
            keyName = string.Empty;
            return false;
        }

        #endregion



        #region Get/Set Indexes

        /// <summary>
        /// Configures indexes that will be enforced during Dynamic Collection instantiation
        /// </summary>
        public static void SetIndex<TModel>(string indexName, bool isUnique, params Expression<Func<TModel, object>>[] expressions)
        {
            var fieldNames = expressions.Select(Extensions.GetMemberName).ToList();
            SetIndex<TModel>(indexName, isUnique, fieldNames.ToArray());
        }

        /// <summary>
        /// Configures indexes that will be enforced during Dynamic Collection instantiation
        /// </summary>
        public static void SetIndex<TModel>(string indexName, bool isUnique, params string[] fieldNames)
        {
            IMongoIndexKeys ib = new IndexKeysBuilder().Ascending(fieldNames);
            IMongoIndexOptions io = new IndexOptionsBuilder().SetUnique(isUnique).SetName(indexName);

            List<Tuple<IMongoIndexKeys, IMongoIndexOptions>> list;
            if (!Indexes.TryGetValue(typeof(TModel), out list))
            {
                list = new List<Tuple<IMongoIndexKeys, IMongoIndexOptions>>();
                Indexes[typeof(TModel)] = list;
            }
            list.Add(new Tuple<IMongoIndexKeys, IMongoIndexOptions>(ib, io));
        }

        internal static List<Tuple<IMongoIndexKeys, IMongoIndexOptions>> GetIndexes<TModel>()
        {
            List<Tuple<IMongoIndexKeys, IMongoIndexOptions>> list;
            return Indexes.TryGetValue(typeof(TModel), out list)
                ? list
                : new List<Tuple<IMongoIndexKeys, IMongoIndexOptions>>();
        }

        #endregion
    }
}
