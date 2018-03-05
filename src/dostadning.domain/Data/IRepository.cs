using System;
using System.Linq;
using System.Threading.Tasks;
using dostadning.domain.ourdata;
using dostadning.domain.result;

namespace dostadning.domain.ourdata
{
    public interface IRepository<T, TKey> : IDisposable 
    {
        /// <summary>
        /// Query the datastore with linq to sql
        /// </summary>
        /// <returns>An IQueryable implementention for the datastore</returns>
        IQueryable<T> Store { get; }
        /// <summary>
        /// Prepare an addition of data to the underlying datastore. 
        /// </summary>
        /// <param name="t">Item to add</param>
        /// <returns>A reference to the same object that was supplied</returns>
        T Add(T t);

        Task<Either<T>> Find(TKey key);
        /// <summary>
        /// Issue a command to the underlying datastore to reflect the state of the repository object in one transaction.
        /// </summary>
        /// <returns>either the number of rows affected by the command or an error</returns>
        Task<Either<int>> Commit();
    }

}
