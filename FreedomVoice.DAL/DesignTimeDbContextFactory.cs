using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreedomVoice.DAL
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FreedomVoiceContext>
    {
        public FreedomVoiceContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<FreedomVoiceContext>();
            var connectionString = "FreedomVoice.db";
            builder.UseSqlite(connectionString);
            return new FreedomVoiceContext(connectionString);
        }
    }
}
