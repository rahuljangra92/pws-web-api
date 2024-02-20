using Microsoft.EntityFrameworkCore;
using PWSWebApi.Domains;

namespace PWSWebApi.Domains.DBContext
{
    public class PWSRecDataContext : DbContext
    {
        private string _connectionString;

        public PWSRecDataContext(DbContextOptions<PWSRecDataContext> options) : base(options)
        {
        }

        public PWSRecDataContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                if (!string.IsNullOrEmpty(_connectionString))
                {
                    optionsBuilder.UseSqlServer(_connectionString);
                }
                else
                {
                    // You can provide a default connection string here if needed
                }
            }
        }

        public DbSet<CashProProcessHistory> CashProProcessHistory { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public virtual DbSet<NewEditTrans> NewEditTrans { get; set; }
        public virtual DbSet<NewEditTrans_tmp> NewEditTrans_tmps { get; set; }

    }
}

