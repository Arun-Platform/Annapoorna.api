using Annapurnaworld.entity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Annapurnaworld.data
{
    /// <summary>
    /// Database layer for db operations.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        private readonly string connectionString = string.Empty;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor to initialze the data.
        /// </summary>
        /// <param name="dbContextOptions">dbcontext options</param>
        /// <param name="configuration">application configurations</param>
        public ApplicationDbContext(DbContextOptions dbContextOptions, IConfiguration configuration) : base(dbContextOptions)
        {
            _configuration = configuration;
            this.connectionString = _configuration.GetConnectionString("dbConn");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderShipmentMetaData>().HasKey(x => x.KeyId);
            modelBuilder.Entity<OrderShippmentStatus>().HasKey(x => x.Id);
            modelBuilder.Entity<DeliveryHub>().HasKey(x => x.Id);

            modelBuilder.Entity<User>().HasKey(x => x.Id);
            modelBuilder.Entity<User>().Property(x => x.Id).ValueGeneratedNever();

            modelBuilder.Entity<Address>().HasKey(x => x.Id);
            modelBuilder.Entity<Address>().Property(x => x.Id).ValueGeneratedNever();

            modelBuilder.Entity<Role>().HasKey(x => x.Id);
            modelBuilder.Entity<Role>().Property(x => x.Id).ValueGeneratedNever();

            modelBuilder.Entity<SubRole>().HasKey(x => x.Id);
            modelBuilder.Entity<SubRole>().Property(x => x.Id).ValueGeneratedNever();

            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            base.OnModelCreating(modelBuilder);
        }
        public DbSet<OrderShippmentStatus> OrderShippmentStatuss { get; set; }
        public DbSet<PackageShipmentDetails> PackageShipmentDetails { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<SubRole> SubRoles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<DeliveryHub> DeliveryHubs { get; set; }
        public DbSet<OrderShipmentMetaData> OrderShipmentMetaDatas { get; set; }

        /// <summary>
        /// Get datatbale based on stored procedure.
        /// </summary>
        /// <param name="storedProcedureName">stored procedure name</param>
        /// <param name="dateRange">date range in which data needs to be fetched.</param>
        /// <param name="deliveryHubName">deliveryhub name</param>
        /// <returns>datatable</returns>
        public async Task<DataTable> GetDataTableFromStoredProcedureAsync(string storedProcedureName, DateTime dateRange, string deliveryHubName)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    using (var command = new SqlCommand(storedProcedureName, connection))
                    {
                        command.CommandTimeout = 120;
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@deliveryHubName", deliveryHubName));
                        command.Parameters.Add(new SqlParameter("@dateRange", dateRange));
                        using (var adapter = new SqlDataAdapter(command))
                        {
                            var dataTable = new DataTable();
                            await Task.Run(() => adapter.Fill(dataTable));
                            return dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Execute stored procedure with sql parameter collections.
        /// </summary>
        /// <param name="storedProcedureName">stored procedure name</param>
        /// <param name="sqlParameterCollection">parameterlists</param>
        /// <returns>datatable</returns>
        public async Task<DataTable> ExecuteStoredProcedure(string storedProcedureName, SqlParameter[] sqlParameterCollection)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    using (var command = new SqlCommand(storedProcedureName, connection))
                    {
                        command.CommandTimeout = 120;
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddRange(sqlParameterCollection);
                        using (var adapter = new SqlDataAdapter(command))
                        {
                            var dataTable = new DataTable();
                            await Task.Run(() => adapter.Fill(dataTable));
                            return dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Execute Sql Query
        /// </summary>
        /// <param name="sqlQuery">SQL Query</param>
        /// <returns>Datatable</returns>
        public async Task<DataTable> ExecuteSqlQuery(string sqlQuery)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    using (var command = new SqlCommand(sqlQuery, connection))
                    {
                        command.CommandTimeout = 120;
                        using (var adapter = new SqlDataAdapter(command))
                        {
                            var dataTable = new DataTable();
                            await Task.Run(() => adapter.Fill(dataTable));
                            return dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Execute scalar which returns counts
        /// </summary>
        /// <param name="sqlQuery">sql query</param>
        /// <returns>int</returns>
        public async Task<int> ExecuteSqlQueryScalar(string sqlQuery)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(sqlQuery, connection))
                    {
                        command.CommandTimeout = 120;
                        object result = await command.ExecuteScalarAsync();

                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Get table records count from stored procedure
        /// </summary>
        /// <param name="storedProcedureName">storec procedure name</param>
        /// <param name="whereClause">where clause</param>
        /// <returns>int</returns>
        public async Task<int> GetRecordCountFromStoredProcedureAsync(string storedProcedureName, string whereClause)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(storedProcedureName, connection))
                {
                    command.Parameters.Add(new SqlParameter("@WhereClause ", whereClause));
                    command.CommandType = CommandType.StoredProcedure;
                    var result = await command.ExecuteScalarAsync();

                    return Convert.ToInt32(result);
                }
            }
        }

        public void ExecuteBulk<T>(List<T> list)
        { 
        }
    }
}
