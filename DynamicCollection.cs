using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MongoDyn
{
    public class DynamicCollection<TKey, TModel> : DynamicCollectionBase<TKey, TModel>
        where TKey : class
        where TModel : class
    {

        protected readonly PropertyInfo[] _properties = typeof(TModel).GetProperties(BindingFlags.Instance | BindingFlags.Public);

        private  bool _eagerLoad;

        internal DynamicCollection(PropertyInfo key, bool notifyPropertyChanged = false, bool audit = false)
            : this(typeof(TModel).Name, key, notifyPropertyChanged, audit)
        {
        }

        internal DynamicCollection(string collectionName, PropertyInfo key, bool notifyEnabled = false, bool audit = false)
            : base(collectionName, key, notifyEnabled, audit)
        {
            initialize();
        }

        private void initialize()
        {
            CollectionQueryMethodInfo = Helper.GetMethodInfo<DynamicCollection<TKey,TModel>>(x => x.CollectionQuery(null, 0, null));

            GetByKeyMethodInfo = Helper.GetMethodInfo<DynamicCollection<TKey,TModel>>(x => x.GetByKey(null));

            _eagerLoad = Helper.IsEagerLoadEnabled<TModel>();

            foreach (var propertyInfo in _properties.Where(propertyInfo => propertyInfo.CanWrite && !propertyInfo.PropertyType.IsInterface))
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

        internal void PrepareEagerLoad()
        {
            if (!_eagerLoad)
                return;

            foreach (var propertyInfo in _properties.Where(p => p.CanWrite && p.PropertyType.IsInterface))
            {
                var type = propertyInfo.PropertyType;
                if (type.IsGenericType)
                {
                    if (type.GetGenericTypeDefinition() == typeof(IEnumerable<>).GetGenericTypeDefinition())
                    {
                        var argu = type.GetGenericArguments().First();
                        var repo = Dynamic.BuildRepository<TKey,TModel>(argu);
                        ChildCollections.Add(argu.Name, repo);
                    }
                }
                else
                {
                    var repo = Dynamic.BuildRepository<TKey,TModel>(type);
                    FkCollections.Add(propertyInfo.Name, repo);
                }
            }
        }
    }
}
