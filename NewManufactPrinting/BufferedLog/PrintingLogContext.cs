using NewManufactPrinting.BufferedLog.Entities;
using SQLite.CodeFirst;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewManufactPrinting.BufferedLog
{
    class PrintingLogContext : DbContext
    {
        public PrintingLogContext() : base("name=printingLog")
        {

        }

        public DbSet<PrintingLog> PrintingLogs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<PrintingLogContext>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }
    }
}
