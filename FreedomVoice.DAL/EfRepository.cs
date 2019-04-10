using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace FreedomVoice.DAL
{
    public partial class EfRepository<T> : IRepository<T> where T : class
    {
        #region Fields

        private readonly IDbContext _context;
        private DbSet<T> _entities;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="context">Object context</param>
        public EfRepository(IDbContext context)
        {
            this._context = context;
        }

        #endregion

        #region Methods


        /// <summary>
        /// Insert entity without saving
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void InsertWithoutSaving(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException("entity");

                var validationContext = new ValidationContext(entity);
                Validator.ValidateObject(entity, validationContext);

                this.Entities.Add(entity);
            }
            catch (Exception ex)
            {
                var a = 0;
            }
        }

        /// <summary>
        /// Insert entities without saving
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual void InsertWithoutSaving(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException("entities");

            foreach (var entity in entities)
            {
                var validationContext = new ValidationContext(entity);
                Validator.ValidateObject(entity, validationContext);
            }

            foreach (var entity in entities)
                this.Entities.Add(entity);
        }

        /// <summary>
        /// Insert entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Insert(T entity)
        {
            InsertWithoutSaving(entity);
            this._context.SaveChanges();
        }

        /// <summary>
        /// Insert entities
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual void Insert(IEnumerable<T> entities)
        {
            InsertWithoutSaving(entities);
            this._context.SaveChanges();
        }

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            var validationContext = new ValidationContext(entity);
            Validator.ValidateObject(entity, validationContext);

            this._context.SaveChanges();

        }

        /// <summary>
        /// Update entities
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual void Update(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException("entities");

            foreach (var entity in entities)
            {
                var validationContext = new ValidationContext(entity);
                Validator.ValidateObject(entity, validationContext);
            }

            this._context.SaveChanges();
        }

        /// <summary>
        /// Delete entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Delete(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            this.Entities.Remove(entity);

            this._context.SaveChanges();
        }

        /// <summary>
        /// Remove entity without save
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void RemoveWithoutSave(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            this.Entities.Remove(entity);
        }

        /// <summary>
        /// Delete entities
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual void Delete(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException("entities");

            foreach (var entity in entities)
                this.Entities.Remove(entity);

            this._context.SaveChanges();
        }

        public int ExecuteNativeCommand(string sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters)
        {
            return this._context.ExecuteSqlCommand(sql, doNotEnsureTransaction, timeout, parameters);
        }

        /// <summary>
        /// Remove entities without save
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual void RemoveWithoutSave(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException("entities");

            foreach (var entity in entities)
                this.Entities.Remove(entity);
        }

        /// <summary>
        /// Saving
        /// </summary>
        public virtual void SaveChanges()
        {
            try
            {
                this._context.SaveChanges();
            }
            catch(Exception ex)
            {
                var a = 0;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a table
        /// </summary>
        public virtual IQueryable<T> Table => this.Entities;

        /// <summary>
        /// Gets a cached table
        /// </summary>
        public virtual IQueryable<T> LocalTable => this.Entities.Local.AsQueryable();

        /// <summary>
        /// Gets a table with "no tracking" enabled (EF feature) Use it only when you load record(s) only for read-only operations
        /// </summary>
        public virtual IQueryable<T> TableNoTracking => this.Entities.AsNoTracking();

        /// <summary>
        /// Entities
        /// </summary>
        protected virtual DbSet<T> Entities => _entities ?? (_entities = _context.Set<T>());
        #endregion
    }
}
