using Annapurnaworld.data;
using Annapurnaworld.entity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Printing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Annapurnaworld.common;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Transactions;
using System.IO.Compression;
using Microsoft.Data.SqlClient;
using ClosedXML.Excel;

namespace Annapurnaworld.service
{
    /// <summary>
    /// Class for shipment details service.
    /// </summary>
    public class ShipmentDetailsService : IShipmentDetailsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string connectionString = string.Empty;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ShipmentDetailsService> _logger;

        /// <summary>
        /// Contruction to initialize the objects.
        /// </summary>
        /// <param name="context"></param>
        public ShipmentDetailsService(ApplicationDbContext context, ILogger<ShipmentDetailsService> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            this.connectionString = configuration.GetConnectionString("dbConn");
        }

        /// <summary>
        /// Get Latest Shippment records.
        /// </summary>
        /// <returns>IEnumerable of shippment records<OrderDeliveredDetails></returns>
        public async Task<List<PackageShipmentDetails>> GetPackageShipmentDetails()
        {
            var shipmentDetails = new List<PackageShipmentDetails>();
            try
            {
                var latestRecord = _context.OrderShipmentMetaDatas.OrderByDescending(r => r.FileDate).FirstOrDefault();
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads\\", latestRecord.FileName);
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var headerRow = worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column];
                    var headers = headerRow.Select(cell => cell.Text).ToList();
                    var values = new List<string>();
                    var rowCount = worksheet.Dimension.Rows;
                    for (int row = 2; row <= rowCount; row++) // Start from 2 to skip header row
                    {
                        var shipmentDetail = new PackageShipmentDetails();
                        var properties = shipmentDetail.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        for (int j = 1; j <= headers.Count; j++)
                        {
                            var cellValue = worksheet.Cells[row, j].GetValue<string>();
                            var property = properties.FirstOrDefault(p => p.Name.Equals(headers[j - 1], StringComparison.OrdinalIgnoreCase));
                            var convertedValue = ConvertValue(cellValue, property.PropertyType);
                            property.SetValue(shipmentDetail, convertedValue);
                        }
                        shipmentDetails.Add(shipmentDetail);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return shipmentDetails;
        }

        /// <summary>
        /// Get latest order delivery details by agent delivery status.
        /// </summary>
        /// <param name="param">delivery hub name.</param>
        /// <returns>key value data </returns>
        public async Task<(IEnumerable<string>, List<Dictionary<string, object>>)> GetPackageShipmentDetailsByAgentWithShipmentStatus(DateTime dateRange, string deliveryHubName)
        {
            List<Dictionary<string, object>> shipmentDetailsHub = null;
            List<string> deliverHubs = null;
            try
            {
                deliverHubs = _context.DeliveryHubs.AsNoTracking().ToList().Select(x => x.Name).ToList();
                if (deliveryHubName is null)
                {
                    deliveryHubName = deliverHubs.First();
                }
                DataTable dataTable = await _context.GetDataTableFromStoredProcedureAsync(common.Constants.STOREDPROCEDURE_GETLATESTSHIPMENTSTATUSBYAGENTS, dateRange, deliveryHubName + "_tbl");
                shipmentDetailsHub = ConvertDataTableToList(dataTable);
            }
            catch (Exception ex)
            {
                throw;
            }
            return (deliverHubs, shipmentDetailsHub);
        }

        /// <summary>
        /// Get shipment total records count by date range.
        /// </summary>
        /// <param name="dateRange">date range</param>
        /// <returns></returns>
        public async Task<int> GetShippmentTotalCountBasedOnDateAndRunsheetGrandTotal(DateTime dateRange)
        {
            int recordCount = 0;
            try
            {
                var currentDate = dateRange.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string query = string.Format(common.Constants.QUERY_GETSHIPPMENT_TOTAL_COUNT_BASED_ONDATE_AND_RUNSHEET, currentDate);
                recordCount = await _context.ExecuteSqlQueryScalar(query);
            }
            catch (Exception ex)
            {
                throw;
            }
            return recordCount;
        }

        /// <summary>
        /// Get shipment total records count by date range and status delivered.
        /// </summary>
        /// <param name="dateRange">date range</param>
        /// <returns>record count</returns>
        public async Task<int> GetShippmentTotalCountBasedOnDateAndRunsheetAndStatusDelivered(DateTime dateRange)
        {
            int recordCount = 0;
            try
            {
                var convertedDate = dateRange.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string statusCommaString = string.Join(",", common.Constants.SheetTypeRunsheetStatuses);
                string query = string.Format(common.Constants.QUERY_GETSHIPPMENT_TOTALCOUNT_BASEDON_DATE_AND_RUNSHEET_STATUS_DELIVERED, convertedDate, statusCommaString);
                recordCount = await _context.ExecuteSqlQueryScalar(query);
            }
            catch (Exception ex)
            {
                throw;
            }
            return recordCount;
        }

        /// <summary>
        /// Get latest order delivery details by agent delivery status.
        /// </summary>
        /// <param name="param">comma sepearated delivery hub value.</param>
        /// <returns>key value data </returns>
        public async Task<int> GetShippmentTotalCountBasedOnDateAndSheetTypePickupSheetGrandTotal(DateTime dateRange)
        {
            int recordCount = 0;
            try
            {
                var convertedDate = dateRange.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string query = string.Format(common.Constants.QUERY_GETSHIPPMENT_TOTAL_COUNT_BASEDON_DATE_AND_SHEETTYPE_PICKUPSHEET, convertedDate);
                recordCount = await _context.ExecuteSqlQueryScalar(query);
            }
            catch (Exception ex)
            {
                throw;
            }
            return recordCount;
        }

        /// <summary>
        /// Get latest order delivery details by agent delivery status.
        /// </summary>
        /// <param name="param">comma sepearated delivery hub value.</param>
        /// <returns>key value data </returns>
        public async Task<int> GetShippmentTotalCountBasedOnDateAndSheetTypePickupSheet(DateTime dateRange)
        {
            int recordCount = 0;
            try
            {
                var convertedDate = dateRange.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string query = string.Format(common.Constants.QUERY_GETSHIPPMENT_TOTALCOUNT_BASED_ON_DATE_AND_SHEET_TYPE_PickupSheet, convertedDate);
                recordCount = await _context.ExecuteSqlQueryScalar(query);
            }
            catch (Exception ex)
            {
                throw;
            }
            return recordCount;
        }

        /// <summary>
        /// Upload order Shippment details file for the current day.
        /// </summary>
        /// <param name="stream">file stream</param>
        /// <param name="currentDate">currentDate</param>
        /// <returns>Task</returns>
        public async Task UploadOrderDeliveredDetailsAsync(IFormFile request, DateOnly currentDate)
        {
            _logger.LogInformation("Start : UploadOrderDeliveredDetailsAsync");
            SqlTransaction transaction = null;
            try
            {
                var packageShipmentDetails = new List<PackageShipmentDetails>();
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                using (var package = new ExcelPackage(request.OpenReadStream()))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var headerRow = worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column];
                    var headers = headerRow.Select(cell => cell.Text).ToList();
                    var rowCount = worksheet.Dimension.Rows;
                    for (int row = 2; row <= rowCount; row++) // Start from 2 to skip header row
                    {
                        var packageShipmentDetail = new PackageShipmentDetails();
                        var properties = packageShipmentDetail.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        for (int j = 1; j <= headers.Count; j++)
                        {
                            var cellValue = worksheet.Cells[row, j].GetValue<string>();
                            var property = properties.FirstOrDefault(p => p.Name.Equals(headers[j - 1], StringComparison.OrdinalIgnoreCase));
                            var convertedValue = ConvertValue(cellValue, property.PropertyType);
                            property.SetValue(packageShipmentDetail, convertedValue);
                        }
                        packageShipmentDetails.Add(packageShipmentDetail);
                    }
                }

                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                transaction = conn.BeginTransaction();

                ProcessRecords(currentDate, connectionString, packageShipmentDetails, conn, transaction);
                await CreateTableAndUploadAsync(request, currentDate, packageShipmentDetails, conn, transaction);

                transaction.Commit();
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogInformation(string.Format("{0} Exception occured : UploadOrderDeliveredDetailsAsync", ex.Message));
                throw;
            }
            _logger.LogInformation("End : UploadOrderDeliveredDetailsAsync");
        }

        public async Task<List<string>> Download(string fileType, DateTime fromDate, DateTime toDate)
        {
            List<string> files = new List<string>();
            var fromDateTostring = fromDate.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var toDateString = toDate.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string query = string.Format("select FileName FROM [annapurnaworlddb].[dbo].[OrderShipmentMetaDatas]  with(nolock)  where FileDate  between cast('{0}' as Date) and CAST('{1}' as date) and SheetType = 'RUNSHEET' and Status is null", fromDateTostring, toDateString);
            var result = await _context.ExecuteSqlQuery(query);

            foreach (DataRow row in result.Rows)
            {
                if (row["FileName"] == DBNull.Value)
                {
                    continue;
                }
                else
                {
                    files.Add(row["FileName"].ToString());
                }
            }

            string _fileDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Uploads\\");
            string fileName = Guid.NewGuid().ToString() + ".xlsx";
            string filepath = _fileDirectory + fileName;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("MergedSheet");
                int currentRow = 1;

                foreach (var filed in files)
                {
                    var file = _fileDirectory + filed;
                    using (var tempWorkbook = new XLWorkbook(file))
                    {
                        var tempWorksheet = tempWorkbook.Worksheet(1); // Assuming data is in the first sheet
                        var range = tempWorksheet.RangeUsed();
                        int lastRow = range.RowCount();

                        // Copy headers only from the first file
                        if (currentRow == 1)
                        {
                            range.Rows().First().CopyTo(worksheet.Row(currentRow));
                            currentRow++;
                        }

                        // Copy data rows
                        for (int i = 2; i <= lastRow; i++)
                        {
                            var row = range.Row(i);
                            row.CopyTo(worksheet.Row(currentRow));
                            currentRow++;
                        }
                    }
                }
                try
                {
                    workbook.SaveAs(filepath);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return new List<string>() { filepath };
        }

        private async Task CreateTableAndUploadAsync(IFormFile file, DateOnly currentDate, List<PackageShipmentDetails> records, SqlConnection SqlConnection, SqlTransaction SqlTransaction)
        {
            string fileName = string.Format("ShipmentDetails_{0}.xlsx", currentDate);
            string tableName = string.Format("ShipmentDetails_{0}_tbl", currentDate);
            string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads\\" + fileName);


            var test1 = _context.OrderShipmentMetaDatas.FirstOrDefault(x => x.FileName == fileName && x.SheetType == "RUNSHEET" && x.Status == null);
            if (test1 == null)
            {
                OrderShipmentMetaData orderShipmentMetaData = new OrderShipmentMetaData();
                orderShipmentMetaData.FileName = fileName;
                orderShipmentMetaData.FileDate = currentDate;
                orderShipmentMetaData.RecordCount = records.Where(X => X.SheetType == "RUNSHEET").Count();
                orderShipmentMetaData.SheetType = "RUNSHEET";
                await _context.OrderShipmentMetaDatas.AddAsync(orderShipmentMetaData);
            }
            else
            {
                test1.FileDate = currentDate;
                test1.RecordCount = records.Where(X => X.SheetType == "RUNSHEET").Count();
                _context.OrderShipmentMetaDatas.Update(test1);
            }
            test1 = _context.OrderShipmentMetaDatas.FirstOrDefault(x => x.FileName == fileName && common.Constants.SheetTypeRunsheetStatuses.Contains(x.Status) && x.SheetType == "RUNSHEET");
            if (test1 == null)
            {
                OrderShipmentMetaData orderShipmentMetaData = new OrderShipmentMetaData();
                orderShipmentMetaData.FileName = fileName;
                orderShipmentMetaData.FileDate = currentDate;
                orderShipmentMetaData.SheetType = "RUNSHEET";
                orderShipmentMetaData.Status = string.Join(",", common.Constants.SheetTypeRunsheetStatuses);
                orderShipmentMetaData.RecordCount = records.Where(x => common.Constants.SheetTypeRunsheetStatuses.Contains(x.ShipmentStatus) && x.SheetType == "RUNSHEET").Count();
                await _context.OrderShipmentMetaDatas.AddAsync(orderShipmentMetaData);
            }
            else
            {
                test1.FileDate = currentDate;
                test1.RecordCount = records.Where(x => common.Constants.SheetTypeRunsheetStatuses.Contains(x.ShipmentStatus) && x.SheetType == "RUNSHEET").Count();
                await _context.OrderShipmentMetaDatas.AddAsync(test1);
            }


            test1 = _context.OrderShipmentMetaDatas.FirstOrDefault(x => x.FileName == fileName && x.SheetType == "PICKUPSHEET" && x.Status == null);
            if (test1 == null)
            {
                OrderShipmentMetaData orderShipmentMetaData = new OrderShipmentMetaData();
                orderShipmentMetaData.FileName = fileName;
                orderShipmentMetaData.FileDate = currentDate;
                orderShipmentMetaData.SheetType = "PICKUPSHEET";
                orderShipmentMetaData.RecordCount = records.Where(x => x.SheetType == "PICKUPSHEET").Count();
                await _context.OrderShipmentMetaDatas.AddAsync(orderShipmentMetaData);
            }
            else
            {
                test1.FileDate = currentDate;
                test1.RecordCount = records.Where(x => x.SheetType == "PICKUPSHEET").Count();
                await _context.OrderShipmentMetaDatas.AddAsync(test1);
            }

            test1 = _context.OrderShipmentMetaDatas.FirstOrDefault(x => x.FileName == fileName && x.Status == "PICKUP_Picked_Complete" && x.SheetType == "PICKUPSHEET");
            if (test1 == null)
            {
                OrderShipmentMetaData orderShipmentMetaData = new OrderShipmentMetaData();
                orderShipmentMetaData.FileName = fileName;
                orderShipmentMetaData.FileDate = currentDate;
                orderShipmentMetaData.SheetType = "PICKUPSHEET";
                orderShipmentMetaData.Status = "PICKUP_Picked_Complete";
                orderShipmentMetaData.RecordCount = records.Where(x => x.ShipmentStatus == "PICKUP_Picked_Complete" && x.SheetType == "PICKUPSHEET").Count();
                await _context.OrderShipmentMetaDatas.AddAsync(orderShipmentMetaData);
            }
            else
            {
                test1.FileDate = currentDate;
                test1.RecordCount = records.Where(x => x.ShipmentStatus == "PICKUP_Picked_Complete" && x.SheetType == "PICKUPSHEET").Count();
                await _context.OrderShipmentMetaDatas.AddAsync(test1);
            }

            using (var stream = new FileStream(_uploadPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Check if table exists
            bool tableExists = CheckTableExists(SqlConnection, SqlTransaction, tableName);

            // Create table if it does not exist
            if (!tableExists)
            {
                CreateTable(SqlConnection, SqlTransaction, tableName);
            }

            // Insert records into the table
            InsertRecords(SqlConnection, SqlTransaction, tableName, records);
        }

        private void ProcessRecords(DateOnly date, string connectionString, List<PackageShipmentDetails> records, SqlConnection SqlConnection, SqlTransaction SqlTransaction)
        {

            var orderShipmentStatus = _context.OrderShippmentStatuss.ToList();
            InsertShipmentStatusAsync(records.Select(x => x.DeliveryHub).Distinct()).ConfigureAwait(true);

            var recordStatus = records.Select(x => x.ShipmentStatus).Distinct();

            var recordsNeedsTobeAddedd = recordStatus.Except(orderShipmentStatus.Select(x => x.Status));

            if (orderShipmentStatus.Count() == 0)
            {
                var lis = new List<OrderShippmentStatus>();
                foreach (var item in recordStatus)
                {
                    lis.Add(new OrderShippmentStatus() { Status = item });
                }
                _context.OrderShippmentStatuss.AddRange(lis);
            }
            else if ((recordsNeedsTobeAddedd != null && recordsNeedsTobeAddedd.Count() > 0))
            {
                var lis = new List<OrderShippmentStatus>();
                foreach (var item in recordsNeedsTobeAddedd)
                {
                    lis.Add(new OrderShippmentStatus() { Status = item });
                }
                _context.OrderShippmentStatuss.AddRange(lis);
            }
            var finalStatus = orderShipmentStatus.Select(x => x.Status).Union(recordStatus);

            // Group records by table name
            var tablesGrouped = records
                .GroupBy(r => r.DeliveryHub)
                .ToDictionary(g => string.Format("{0}_tbl", g.Key), g => g.ToList());

            // Process each table
            foreach (var tableGroup in tablesGrouped)
            {
                string tableName = tableGroup.Key;
                List<PackageShipmentDetails> tableRecords = tableGroup.Value;

                var transformedData = TransformToColumnar(tableRecords);

                var statuses = transformedData.SelectMany(row => row.Keys).Distinct().ToList();

                bool tableExists = CheckTableExists(SqlConnection, SqlTransaction, tableName);

                if (!tableExists)
                {
                    CreateTableWithDynamicColumns(tableName, finalStatus.Distinct().ToList());
                }
                else if (recordsNeedsTobeAddedd != null && recordsNeedsTobeAddedd.Count() > 0)
                {
                    var stringBuilder = new StringBuilder();
                    for (int i = 0; i < recordsNeedsTobeAddedd.ToList().Count; i++)
                    {
                        if (i > 0)
                        {
                            stringBuilder.Append(", ");
                        }
                        stringBuilder.Append($"[{recordsNeedsTobeAddedd.ToList()[i]}] INT");
                    }
                    var sql = $@"
                             ALTER TABLE {tableName}
                               ADD  {stringBuilder.ToString()}
                            ";

                    var command = new SqlCommand(sql, SqlConnection, SqlTransaction);
                    command.ExecuteNonQuery();
                }

                InsertDataIntoTable(date, tableName, transformedData, statuses, SqlConnection, SqlTransaction);
            }
        }

        public List<Dictionary<string, object>> TransformToColumnar(List<PackageShipmentDetails> records)
        {
            var agentStatusCounts = new Dictionary<string, Dictionary<string, int>>();

            foreach (var record in records)
            {
                if (!agentStatusCounts.ContainsKey(record.AgentName))
                {
                    agentStatusCounts[record.AgentName] = new Dictionary<string, int>();
                }

                var statusCounts = agentStatusCounts[record.AgentName];
                if (!statusCounts.ContainsKey(record.ShipmentStatus))
                {
                    statusCounts[record.ShipmentStatus] = 0;
                }
                statusCounts[record.ShipmentStatus]++;
            }

            var allStatuses = agentStatusCounts.Values
                .SelectMany(dict => dict.Keys)
                .Distinct()
                .ToList();

            var columnarData = new List<Dictionary<string, object>>();

            foreach (var kvp in agentStatusCounts)
            {
                var agentName = kvp.Key;
                var statusCounts = kvp.Value;

                var row = new Dictionary<string, object> { { "AgentName", agentName } };
                foreach (var status in allStatuses)
                {
                    row[status] = statusCounts.ContainsKey(status) ? (object)statusCounts[status] : 0;
                }
                columnarData.Add(row);
            }

            return columnarData;
        }

        public void CreateTableWithDynamicColumns(string tableName, List<string> statuses)
        {
            var columns = string.Join(", ", statuses.Select(s => $"[{s}] INT"));


            var stringBuilder = new StringBuilder();

            for (int i = 0; i < statuses.Count; i++)
            {
                if (i > 0)
                {
                    stringBuilder.Append(", ");
                }
                stringBuilder.Append($"[{statuses[i]}] INT");
            }

            var sql = $@"
            CREATE TABLE {tableName}(
                AgentName NVARCHAR(255),
                {stringBuilder.ToString()}, CreatedDate DATETIME
            )";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(sql, connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void InsertDataIntoTable(DateOnly date, string tableName, List<Dictionary<string, object>> data, List<string> statuses, SqlConnection SqlConnection, SqlTransaction SqlTransaction)
        {
            statuses.RemoveAt(0);
            string dateTime = date.ToString("yyyy-MM-dd"); ;
            foreach (var row in data)
            {
                var agentName = row["AgentName"];
                var values = string.Join(", ", statuses.Select(s => row.ContainsKey(s) ? row[s] : 0));
                var sql = $@"
                INSERT INTO {tableName} (AgentName, {string.Join(", ", statuses)},CreatedDate)
                VALUES ('{agentName}', {values},'{dateTime}')";

                var command = new SqlCommand(sql, SqlConnection, SqlTransaction);
                command.ExecuteNonQuery();
            }
        }

        private async Task InsertShipmentStatusAsync(IEnumerable<string> values)
        {
            // Step 1: Retrieve existing values from the database
            var existingValues = await _context.DeliveryHubs
                                              .AsNoTracking()
                                              .Select(e => e.Name)
                                              .ToListAsync();

            // Step 2: Determine which values are new
            var newValues = values.Where(value => !existingValues.Contains(value))
                                  .Select(value => new DeliveryHub { Name = value })
                                  .ToList();

            // Step 3: Insert new values into the database
            if (newValues.Any())
            {
                _context.DeliveryHubs.AddRange(newValues);
            }
        }

        private bool CheckTableExists(SqlConnection SqlConnection, SqlTransaction SqlTransaction, string tableName)
        {
            string query = @"
            SELECT CASE 
                WHEN EXISTS (
                    SELECT * 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME = @TableName
                ) 
                THEN 1 
                ELSE 0 
            END";

            using (SqlCommand cmd = new SqlCommand(query, SqlConnection, SqlTransaction))
            {
                cmd.Parameters.AddWithValue("@TableName", tableName);
                return (int)cmd.ExecuteScalar() == 1;
            }
        }

        private void CreateTable(SqlConnection SqlConnection, SqlTransaction SqlTransaction, string tableName)
        {
            string createTableQuery = $@"
            SELECT  * INTO dbo.[" + tableName + "] FROM  [annapurnaworlddb].[dbo].[PackageShipmentDetails]  with(nolock) WHERE  DeliveryHub ='test122'";

            using (SqlCommand cmd = new SqlCommand(createTableQuery, SqlConnection, SqlTransaction))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private void InsertRecords(SqlConnection SqlConnection, SqlTransaction SqlTransaction, string tableName, List<PackageShipmentDetails> records)
        {
            DataTable tempTable = ConvertToDataTable(records);
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(SqlConnection, SqlBulkCopyOptions.Default, SqlTransaction))
            {
                bulkCopy.DestinationTableName = tableName;

                // Convert the list of records to DataTable
                bulkCopy.WriteToServer(tempTable);
            }
        }

        static DataTable ConvertToDataTable<T>(IEnumerable<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            // Get all the properties
            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Define columns in the DataTable
            foreach (PropertyInfo prop in props)
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            try
            {


                // Populate rows in the DataTable
                foreach (T item in items)
                {
                    DataRow row = dataTable.NewRow();
                    foreach (PropertyInfo prop in props)
                    {
                        row[prop.Name] = prop.GetValue(item, null);
                    }
                    dataTable.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return dataTable;
        }

        private static object ConvertValue(string value, Type targetType)
        {
            if (string.IsNullOrEmpty(value)) return null;

            if (targetType == typeof(string)) return value;
            if (targetType == typeof(int)) return int.Parse(value);
            if (targetType == typeof(decimal)) return decimal.Parse(value);
            if (targetType == typeof(bool)) return bool.Parse(value);
            if (targetType == typeof(DateTime)) return DateUtils.ConvertUtcToIst(DateTimeOffset.Parse(value));

            throw new NotSupportedException($"Conversion to type {targetType} is not supported.");
        }

        private List<Dictionary<string, object>> ConvertDataTableToList(DataTable dataTable)
        {
            var dataList = new List<Dictionary<string, object>>();
            DataRow totalRow = dataTable.NewRow();
            int k = 0;

            foreach (DataColumn column in dataTable.Columns)
            {
                if (k == 0)
                {
                    k++;
                    totalRow[column] = "Grand Total";
                    continue;
                }
                decimal sum = 0;
                foreach (DataRow row in dataTable.Rows)
                {
                    if (row[column] == DBNull.Value)
                    {
                        continue;
                    }
                    else
                    {
                        sum += Convert.ToDecimal(row[column]);
                    }
                }
                totalRow[column] = sum;
            }
            dataTable.Rows.Add(totalRow);
            foreach (DataRow row in dataTable.Rows)
            {
                var dataRow = new Dictionary<string, object>();
                foreach (DataColumn column in dataTable.Columns)
                {
                    dataRow[column.ColumnName] = row[column];
                }
                dataList.Add(dataRow);
            }

            return dataList;
        }

    }
}
