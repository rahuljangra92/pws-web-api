using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PWSWebApi.Domains.DBContext
{
    public class DataClasses1DataContext : DbContext
    {
        private string _connectionString;

        public DataClasses1DataContext(DbContextOptions<DataClasses1DataContext> options) : base(options)
        {
        }

        public DataClasses1DataContext(string connectionString)
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

        internal IQueryable<dynamic> usp_AccountID(int v1, string v2)
        {
            throw new NotImplementedException();
        }

        public DbSet<ApiLog> ApiLogs { get; set; }
        public DbSet<RptModel_tmp> RptModel_tmps { get; internal set; }
        public DbSet<ReportParamMonitorPDF> ReportParamMonitorPDFs { get; set; }
        public DbSet<ReportBook> ReportBooks { get; set; }
        public virtual DbSet<SiteMonitor> SiteMonitors { get; set; }
        public virtual DbSet<PWSClient> PWSClients { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Sec> Secs { get; set; }
        public virtual DbSet<New_Accounts_tmp> New_Accounts_tmps { get; set; }
        public virtual DbSet<New_Account> New_Accounts { get; set; }
        public virtual DbSet<Perf_Static_tmp> Perf_Static_tmps { get; set; }
        public virtual DbSet<Perf_Static> Perf_Statics { get; set; }
        public virtual DbSet<usp_SpecLot_Assign_SaveResult> usp_SpecLot_Assign_SaveResults { get; set; }
        public virtual DbSet<MappingSecurity> MappingSecurities { get; set; }
        public virtual DbSet<GridColumnsDetail> GridColumnsDetails { get; set; }
        public virtual DbSet<PerfAttrib> PerfAttribs { get; set; }
        public DbSet<Usp_GridColumnsResult> UspGridColumnsResults { get; set; }
        public DbSet<PwsConfigs> PwsConfigs { get; internal set; }



        //creating SP to use in OperationController , LN:677 , var data = context.usp_GridColumns().AsQueryable();
        public List<Usp_GridColumnsResult> usp_GridColumns()
        {
            return this.UspGridColumnsResults.FromSqlRaw("EXEC DD.usp_GridColumns").ToList(); //// Need to check with DB Code, wheather SP exists or not
        }

        public usp_SpecLot_Assign_SaveResult usp_SpecLot_Assign_Save(int userId, int transId, int newEditTransId)
        {
            var result = this.usp_SpecLot_Assign_SaveResults
                        .FromSqlInterpolated($"EXEC usp_SpecLot_Assign_Save {userId}, {transId}, {newEditTransId}")
                        .ToList();

            return result.FirstOrDefault(); // Need to check with DB Code, wheather SP exists or not
        }
    }
}

