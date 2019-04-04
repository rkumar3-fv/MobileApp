using FreedomVoice.DAL.DbEntities;
using FreedomVoice.DAL.Mapping;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace FreedomVoice.DAL
{
    public class FreedomVoiceContext : DbContext, IDbContext
    {
        private readonly string _databasePath;
        
        public FreedomVoiceContext(string databasePath)
        {
            _databasePath = databasePath;
        }

        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Phone> Phones { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={_databasePath}");
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ConversationMap());
            modelBuilder.ApplyConfiguration(new PhoneMap());
            modelBuilder.ApplyConfiguration(new MessageMap());
        }
        public DbSet<TEntity> Set<TEntity>() where TEntity : class => base.Set<TEntity>();

        public IList<TEntity> ExecuteStoredProcedureList<TEntity>(string commandText, params object[] parameters) where TEntity : BaseEntity, new()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TElement> SqlQuery<TElement>(string sql, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public int ExecuteSqlCommand(string sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters)
        {
            return this.Database.ExecuteSqlCommand(sql, parameters);
        }

        public void Detach(object entity)
        {
            throw new NotImplementedException();
        }

        public bool ProxyCreationEnabled { get; set; }
        public bool AutoDetectChangesEnabled { get; set; }
    }
}
