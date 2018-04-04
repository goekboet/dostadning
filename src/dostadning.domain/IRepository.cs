using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dostadning.domain
{
    public interface IRepository<T, TKey> : 
        IQuery<T>, 
        IDataCommand<T, TKey>, 
        IDisposable { }

    public interface IDataCommand<T, TKey>
    {
        /// <summary>
        /// Prepare an addition of data to the underlying datastore. 
        /// </summary>
        /// <param name="t">Item to add</param>
        /// <returns>A reference to the same object that was supplied</returns>
        IRepository<T, TKey> Add(T t);

        IObservable<T> Find(TKey key);
        /// <summary>
        /// Issue a command to the underlying datastore to reflect the state of the repository object in one transaction.
        /// </summary>
        /// <returns>either the number of rows affected by the command or an error</returns>
        IObservable<int> Commit();
    }

    public interface IQuery<T>
    {
        /// <summary>
        /// Given a linq to sql-query in an IQueryable, return the result of the query on the 
        /// underlying datastore as an observable.
        /// </summary>
        /// <param name="q">A linq2sql query</param>
        /// <returns>An obseavable of the result</returns>
        IObservable<IEnumerable<T2>> Query<T2>(Func<IQueryable<T>, IQueryable<T2>> q);
    }

}
