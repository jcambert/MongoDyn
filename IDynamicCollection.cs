using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MongoDyn
{
    /// <summary>
    /// Wrap Mongo collection documents
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    [ContractClass(typeof(IDynamicCollectionContract<,>))]
    public interface IDynamicCollection<TKey, TModel>
        where TKey : class
        where TModel : class
    {
        /// <summary>
        /// Return collection elements count
        /// </summary>
        long Count { get; }


        /// <summary>
        /// Delete a document by its Id
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool DeleteByKey(TKey key);

        /// <summary>
        /// Get a document by its key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        TModel GetByKey(TKey key);


        /// <summary>
        /// Get the first document, or default if nothing
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TModel GetFirstOrDefault(Expression<Func<TModel, bool>> id);

        /// <summary>
        /// return all documents
        /// </summary>
        /// <returns></returns>
        IEnumerable<TModel> All();



        IEnumerable<TModel> CollectionQuery(string memberName, ExpressionType op, object value);


        IEnumerable<TModel> CustomQuery(Expression<Func<TModel, bool>> expression);

        /// <summary>
        /// return new document
        /// </summary>
        /// <returns></returns>
        TModel New();

        void UpsertDynamic(IDictionary<string, object> item);


        void UpsertImpromptu(dynamic item);

        void Upsert(TModel item);


        /// <summary>
        /// Delete a document
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool Delete(TModel item);

        /// <summary>
        /// Delete all documents
        /// </summary>
        /// <param name="resetCounter"></param>
        void DeleteAll(bool resetCounter);
    }

    [ContractClassFor(typeof(IDynamicCollection<,>))]
    internal abstract class IDynamicCollectionContract<TKey, TModel> : IDynamicCollection<TKey, TModel>
        where TKey : class
        where TModel : class
    {


        public long Count
        {
            get {
                Contract.Ensures(Contract.Result<long>() > -1);
                throw new NotImplementedException();
            }
        }

        public bool DeleteByKey(TKey key)
        {
            Contract.Requires(key != null, "key must not be null");
            throw new NotImplementedException();
        }

        public TModel GetByKey(TKey key)
        {
            Contract.Requires(key != null, "key must not be null");
            throw new NotImplementedException();
        }

        public TModel GetFirstOrDefault(Expression<Func<TModel, bool>> id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TModel> All()
        {
            Contract.Ensures(Contract.Result<IEnumerable<TModel>>() != null);
            throw new NotImplementedException();
        }

        public IEnumerable<TModel> CollectionQuery(string memberName, ExpressionType op, object value)
        {
            Contract.Requires(memberName != null, "memberName must not be null");
            Contract.Requires(value != null, "Value must not be null");
            Contract.Ensures(Contract.Result<IEnumerable<TModel>>() != null);
            throw new NotImplementedException();
        }

        public IEnumerable<TModel> CustomQuery(Expression<Func<TModel, bool>> expression)
        {
            Contract.Requires(expression != null, "expression must not be null");
            Contract.Ensures(Contract.Result<IEnumerable<TModel>>() != null);
            throw new NotImplementedException();
        }

        public TModel New()
        {
            Contract.Ensures(Contract.Result<TModel>() != null);
            throw new NotImplementedException();
        }

        public void UpsertDynamic(IDictionary<string, object> items)
        {
            Contract.Requires(items != null, "items must not be null");
            throw new NotImplementedException();
        }

        public void UpsertImpromptu(dynamic item)
        {
            
            throw new NotImplementedException();
        }

        public void Upsert(TModel item)
        {
            Contract.Requires(item != null, "item must not be null");
            throw new NotImplementedException();
        }

        public bool Delete(TModel item)
        {
            Contract.Requires(item != null, "item must not be null");
            throw new NotImplementedException();
        }

        public void DeleteAll(bool resetCounter)
        {
            throw new NotImplementedException();
        }
    }
}
