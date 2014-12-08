using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MongoDyn
{
    public static class Dynamic
    {
        private static string _defaultConnectionstringName = "MongoServerSettings";


        private static readonly Dictionary<string, object> Repositorories = new Dictionary<string, object>();

        public static bool NotifyPropertyChanged { get; set; }

        public static bool Audit { get; set; }

        public static string ConnectionStringOrName
        {
            get { return _defaultConnectionstringName; }
            set
            {
                Contract.Requires(value != null && value.Trim().Length > 0, "ConnectionStringOrName must not be nul nor empty");
                _defaultConnectionstringName = value;
            }
        }

        public static MongoDatabase Db
        {
            get
            {
                var url = new MongoUrl(GetConnString());
                return MongoServer.Create(url).GetDatabase(url.DatabaseName);
            }
        }


        public static string GetConnString()
        {
            return ConfigurationManager.ConnectionStrings[_defaultConnectionstringName].ConnectionString;
        }

        [Pure]
        public static DynamicCollection<TKey, TModel> GetCollection<TKey, TModel>()
          
            where TModel : class
        {
            if (typeof(TModel).GetProperties().Where(p=>Attribute.IsDefined(p,typeof(KeyAttribute))).Count() != 1)
                throw new DocumentException("Model must have exactly one Key Attribute");

     
            
            var model = typeof(TModel).Name;
            object value;
            if (!Repositorories.TryGetValue(model, out value))
            {
                PropertyInfo key = typeof(TModel).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(KeyAttribute))).First();

                PropertyInfo[] p_indexes = typeof(TModel).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(IndexAttribute))).ToArray();
                Dictionary<string, Index> l_indexes = new Dictionary<string, Index>();

                foreach (var p_index in p_indexes)
                {
                    var temp_indexes = p_index.GetCustomAttributes<IndexAttribute>(true).ToList();

                    if (!l_indexes.ContainsKey(p_index.Name))
                        l_indexes[p_index.Name] = new Index(p_index.Name, temp_indexes[0].IsUnique);
                    l_indexes[p_index.Name].AddFields((from t in p_indexes select t.Name).ToList());



                }

                foreach (var _key in l_indexes.Keys)
                    Helper.SetIndex<TModel>(l_indexes[_key].Name, l_indexes[_key].IsUnique, l_indexes[_key].Fields.ToArray());

                var repo = new DynamicCollection<TKey, TModel>(key, NotifyPropertyChanged, Audit);

                value = repo;
                Repositorories[model] = value;
                repo.PrepareEagerLoad();
            }
            return (DynamicCollection<TKey, TModel>)value;

        }

        internal static DynamicCollectionBase<TKey, TModel> BuildRepository<TKey, TModel>(Type type)
            where TModel : class
        {
            var mi = typeof(Dynamic).GetMethod("GetCollection");
            var constr = mi.GetGenericMethodDefinition().MakeGenericMethod(type);
            var result = constr.Invoke(null, null);
            return (DynamicCollectionBase<TKey, TModel>)result;
        }


    }
}
