using Annapurnaworld.entity;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Annapurnaworld.service
{
    /// <summary>
    /// Interface for courier delievry hub service.
    /// </summary>
    public interface IShipmentDetailsService
    {
        /// <summary>
        /// Get Latest Shippment records.
        /// </summary>
        /// <returns>IEnumerable of shippment records<OrderDeliveredDetails></returns>
        Task<List<PackageShipmentDetails>> GetPackageShipmentDetails();

        /// <summary>
        /// Get latest order delivery details by agent delivery status.
        /// </summary>
        /// <param name="param">comma sepearated delivery hub value.</param>
        /// <returns>key value data </returns>
        Task<(IEnumerable<string>, List<Dictionary<string, object>>)> GetPackageShipmentDetailsByAgentWithShipmentStatus(DateTime dateRange, string deliveryHubName);

        /// <summary>
        /// Upload shippment file for the current day.
        /// </summary>
        /// <param name="stream">file stream</param>
        /// <returns>Task</returns>
        Task UploadOrderDeliveredDetailsAsync(IFormFile request, DateOnly currentDate);

        /// <summary>
        /// Get shipment total records count by date range.
        /// </summary>
        /// <param name="dateRange">date range</param>
        /// <returns></returns>
        Task<int> GetShippmentTotalCountBasedOnDateAndRunsheetGrandTotal(DateTime dateRange);

        /// <summary>
        /// Get shipment total records count by date range and status delivered.
        /// </summary>
        /// <param name="dateRange">date range</param>
        /// <returns>record count</returns>
        Task<int> GetShippmentTotalCountBasedOnDateAndRunsheetAndStatusDelivered(DateTime dateRange);

        /// <summary>
        /// Get shipment records count based on sheettype => runsheet.
        /// </summary>
        /// <param name="dateRange">date range</param>
        /// <returns></returns>
        Task<int> GetShippmentTotalCountBasedOnDateAndSheetTypePickupSheetGrandTotal(DateTime dateRange);

        /// <summary>
        /// Get shipment records count based on sheettype => PickupSheet.
        /// </summary>
        /// <param name="dateRange">date range</param>
        /// <returns></returns>
        Task<int> GetShippmentTotalCountBasedOnDateAndSheetTypePickupSheet(DateTime dateRange);


        /// <summary>
        /// Download files based on the filetype and daterange..
        /// </summary>
        /// <param name="fileType">file type</param>
        /// <param name="daterange">date range</param>
        /// <returns>list of files</returns>
        Task<List<string>> Download(string fileType, DateTime fromDate, DateTime toDate);
    }
}
