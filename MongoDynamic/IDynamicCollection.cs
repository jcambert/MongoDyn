using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MongoDyn
{


    public interface IDynamicCollection
    {
        /// <summary>
        /// Return collection elements count
        /// </summary>
        long Count { get; }

        /// <summary>
        /// Delete all documents
        /// </summary>
        /// <param name="resetCounter"></param>
        void DeleteAll(bool resetCounter);

        /// <summary>
        /// The name of the underlying MongoDB collection name
        /// </summary>
        string CollectionName { get; }


        /// <summary>
        /// Collection Type
        /// </summary>
        Type CollectionType
        {
            get;
        }

        /// <summary>
        /// Key Type
        /// </summary>
         Type KeyType
        {
            get;
        }

    }

    /// <summary>
    /// Wrap Mongo collection documents
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    [ContractClass(typeof(IDynamicCollectionContract<,>))]
    public interface IDynamicCollection<TKey, TModel> : IDynamicCollection

        where TModel : class
    {

        /// <summary>
        /// The Linq Queryable for the collection
        /// </summary>
        IQueryable<TModel> AsQueryable { get; }




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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="op"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IEnumerable<TModel> CollectionQuery(string memberName, ExpressionType op, object value);

        /// <summary>
        /// get list af item according exrpression
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        IEnumerable<TModel> CustomQuery(Expression<Func<TModel, bool>> expression);

        /// <summary>
        /// return new document
        /// </summary>
        /// <returns></returns>
        TModel New();

        /// <summary>
        /// Upsert items
        /// </summary>
        /// <param name="item"></param>
        void UpsertDynamic(IDictionary<string, object> item);


        /// <summary>
        /// Upsert an dynamic item
        /// </summary>
        /// <param name="item"></param>
        void UpsertImpromptu(dynamic item);


        /// <summary>
        /// Upsert an item
        /// </summary>
        /// <param name="item"></param>
        void Upsert(TModel item);


        /// <summary>
        /// Delete a document
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool Delete(TModel item);




        /// <summary>
        /// Inserts many items into the collection in a single batch.
        /// </summary>
        /// <remarks>The number of items cannot be too large because there is a size-limit on messages, but it's pretty reasonable.</remarks>
        /// <returns>The number of items that were Inserted.</returns>
        void BatchInsert(IEnumerable<TModel> items);


        /// <summary>
        /// Retrieve matching records by keys
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        IEnumerable<TModel> GetManyByKeys(IEnumerable<TKey> keys);



    }

    [ContractClassFor(typeof(IDynamicCollection<,>))]
    internal abstract class IDynamicCollectionContract<TKey, TModel> : IDynamicCollection<TKey, TModel>
        where TModel : class
    {


        public long Count
        {
            get
            {
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

        public IQueryable<TModel> AsQueryable
        {
            get
            {
                Contract.Ensures(Contract.Result<IQueryable<TModel>>() != null);
                throw new NotImplementedException();
            }
        }

        public void BatchInsert(IEnumerable<TModel> items)
        {
            Contract.Requires(items != null, "items for BatchInsert cannot be null");
            throw new NotImplementedException();
        }

        public string CollectionName
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                throw new NotImplementedException();
            }
        }


        public IEnumerable<TModel> GetManyByKeys(IEnumerable<TKey> keys)
        {
            Contract.Requires(keys != null, "Keys for GetManyByKeys cannot be null");
            Contract.Ensures(Contract.Result<IEnumerable<TModel>>() != null);
            throw new NotImplementedException();
        }


        public Type CollectionType
        {
            get { throw new NotImplementedException(); }
        }

        public Type KeyType
        {
            get { throw new NotImplementedException(); }
        }
    }
}
