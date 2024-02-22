using Microsoft.EntityFrameworkCore;

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
        public virtual DbSet<Load_Index> Load_Indexes { get; set; }
        public virtual DbSet<SecuritySearchResult> SecuritySearchResults { get; set; }
        public virtual DbSet<Load_EOD> Load_EODs { get; set; }
        public virtual DbSet<Load_CorpAction> Load_CorpActions { get; set; }
        public virtual DbSet<ProcessHistory> ProcessHistories { get; set; }
        public virtual DbSet<Intermediate_GetIdentifier> Intermediate_GetIdentifiers { get; set; }
    }
}

