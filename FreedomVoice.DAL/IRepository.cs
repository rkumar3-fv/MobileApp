using System.Collections.Generic;
using System.Linq;

namespace FreedomVoice.DAL
{
    public partial interface IRepository<T> where T : class
    {
        /// <summary>
        /// Insert entity without saving
        /// </summary>
        /// <param name="entity">Entity</param>
        void InsertWithoutSaving(T entity);

        /// <summary>
        /// Insert entities without saving
        /// </summary>
        /// <param name="entities">Entities</param>
        void InsertWithoutSaving(IEnumerable<T> entities);

        /// <summary>
        /// Insert entity
        /// </summary>
        /// <param name="entity">Entity</param>
        void Insert(T entity);

        /// <summary>
        /// Insert entities
        /// </summary>
        /// <param name="entities">Entities</param>
        void Insert(IEnumerable<T> entities);

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">Entity</param>
        void Update(T entity);

        /// <summary>
        /// Update entities
        /// </summary>
        /// <param name="entities">Entities</param>
        void Update(IEnumerable<T> entities);

        /// <summary>
        /// Delete entity
        /// </summary>
        /// <param name="entity">Entity</param>
        void Delete(T entity);

        /// <summary>
        /// Delete entities
        /// </summary>
        /// <param name="entities">Entities</param>
        void Delete(IEnumerable<T> entities);

        /// <summary>
        /// Remove entity without save
        /// </summary>
        /// <param name="entity">Entity</param>
        void RemoveWithoutSave(T entity);

        /// <summary>
        /// Remove entities without save
        /// </summary>
        /// <param name="entities">Entities</param>
        void RemoveWithoutSave(IEnumerable<T> entities);

        /// <summary>
        /// Saving
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Gets a table
        /// </summary>
        IQueryable<T> Table { get; }

        /// <summary>
        /// Gets a cached table
        /// </summary>
        IQueryable<T> LocalTable { get; }

        /// <summary>
        /// Gets a table with "no tracking" enabled (EF feature) Use it only when you load record(s) only for read-only operations
        /// </summary>
        IQueryable<T> TableNoTracking { get; }

        int ExecuteNativeCommand(string sql, bool doNotEnsureTransaction = false, int? timeout = null,
            params object[] parameters);

    }
}
