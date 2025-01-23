using Annapurnaworld.entity;
using Annapurnaworld.service;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO.Compression;

namespace Annapurnaworld.api.Controllers
{

    /// <summary>
    /// API controller class to handle api methods.
    /// </summary>
    [Authorize(Roles = "Admin,User")]
    [ApiController]
    [Route("api/courierDeliveryhubs")]
    public class CourierDeliveryHubController : ControllerBase
    {
        private readonly IShipmentDetailsService _shipmentDetailsService;
        private readonly ILogger<CourierDeliveryHubController> _logger;

        /// <summary>
        /// Constructor to initialize the objects.
        /// </summary>
        /// <param name="logger">logger object</param>
        /// <param name="IShipmentDetailsService"> shipmentDetailsService object</param>
        public CourierDeliveryHubController(ILogger<CourierDeliveryHubController> logger, IShipmentDetailsService shipmentDetailsService)
        {
            _logger = logger;
            _shipmentDetailsService = shipmentDetailsService;
        }

        /// <summary>
        /// Get latest packet shipment details.
        /// </summary>
        /// <returns>IEnumerable<PackageShipmentDetails></returns>
        [HttpGet]
        [Route("GetPackageShipmentDetails")]
        public async Task<IActionResult> GetPackageShipmentDetails()
        {
            _logger.LogInformation("Start : GetPackageShipmentDetails");
            List<PackageShipmentDetails> details = null;
            try
            {
                details = await _shipmentDetailsService.GetPackageShipmentDetails();
            }
            catch (Exception ex)
            {
                _logger.LogInformation(string.Format("{0} Exception occured : GetPackageShipmentDetails", ex.Message));
                throw;
            }
            _logger.LogInformation("End : GetPackageShipmentDetails");

            return Ok(details);
        }

        [HttpGet("download-zip")]
        public async Task<IActionResult> DownloadZip(string fileType, DateTime fromDate, DateTime toDate)
            {
            var files = await _shipmentDetailsService.Download(fileType, fromDate, toDate);
            var zipStream = new MemoryStream();
            using (var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                // Add the Excel file to the ZIP archive
                var zipEntry = zipArchive.CreateEntry(files.FirstOrDefault().Split("\\").Last());

                using (var entryStream = zipEntry.Open())
                using (var fileStream = new FileStream(files.FirstOrDefault(), FileMode.Open, FileAccess.Read))
                {
                    await fileStream.CopyToAsync(entryStream);
                }
            }
            zipStream.Position = 0;
            return File(zipStream, "application/zip", "file.zip");
        }

        /// <summary>
        /// Get latest order delivery details by agent delivery status.
        /// </summary>
        /// <param name="param">comma sepearated delivery hub value.</param>
        /// <returns>key value data </returns>
        [HttpGet]
        [Route("GetPackageShipmentDetailsByAgentWithShipmentStatus")]
        public async Task<IActionResult> GetPackageShipmentDetailsByAgentWithShipmentStatus(DateTime dateRange, string? deliveryHubName)
        {
            _logger.LogInformation("Start : GetPackageShipmentDetailsByAgentWithShipmentStatus");
            PackageShipmentHubResponse packageShipmentHubResponse = new PackageShipmentHubResponse();

            try
            {
                var response = await _shipmentDetailsService.GetPackageShipmentDetailsByAgentWithShipmentStatus(dateRange, deliveryHubName);
                packageShipmentHubResponse.DeliveryHubs = response.Item1;
                packageShipmentHubResponse.KeyValuePairs = response.Item2;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(string.Format("{0} Exception occured : GetPackageShipmentDetailsByAgentWithShipmentStatus", ex.Message));
                throw;
            }
            _logger.LogInformation("End : GetPackageShipmentDetailsByAgentWithShipmentStatus");

            return Ok(packageShipmentHubResponse);
        }

        /// <summary>
        /// Upload order delivered file for the current day.
        /// </summary>
        /// <returns>boolean</returns>
        [HttpPost]
        [Route("UploadOrderDeliveredFile")]
        public async Task<IActionResult> UploadOrderDeliveredFile(IFormFile file, DateOnly dateTime)
        {
            _logger.LogInformation("Start : UploadOrderDeliveredFile");

            try
            {
                await _shipmentDetailsService.UploadOrderDeliveredDetailsAsync(file, dateTime);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(string.Format("{0} Exception occured : UploadOrderDeliveredFile", ex.Message));
                throw;
            }
            _logger.LogInformation("End : UploadOrderDeliveredFile");
            return Ok(true);
        }

        /// <summary>
        /// Get shipment total count based on daterange.
        /// </summary>
        /// <param name="dateRange">date.</param>
        /// <returns>Count</returns>
        [HttpGet]
        [Route("GetShippmentTotalCountBasedOnDateAndRunsheetGrandTotal")]
        public async Task<IActionResult> GetShippmentTotalCountBasedOnDateAndRunsheetGrandTotal(DateTime dateRange)
        {
            _logger.LogInformation("Start : GetShippmentTotalCountBasedOnDateAndRunsheetGrandTotal");
            int recordCount = 0;
            try
            {
                recordCount = await _shipmentDetailsService.GetShippmentTotalCountBasedOnDateAndRunsheetGrandTotal(dateRange);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(string.Format("{0} Exception occured : GetShippmentTotalCountBasedOnDateAndRunsheetGrandTotal", ex.Message));
                throw;
            }
            _logger.LogInformation("End : GetShippmentTotalCountBasedOnDateAndRunsheetGrandTotal");

            return Ok(recordCount);
        }

        /// <summary>
        /// Get shipment total count based on daterange and sheetype runsheet.
        /// </summary>
        /// <param name="dateRange">date.</param>
        /// <returns>Count</returns>
        [HttpGet]
        [Route("GetShippmentTotalCountBasedOnDateAndRunsheetAndStatusDelivered")]
        public async Task<IActionResult> GetShippmentTotalCountBasedOnDateAndRunsheetAndStatusDelivered(DateTime dateRange)
        {
            _logger.LogInformation("Start : GetShippmentTotalCountBasedOnDateAndRunsheetAndStatusDelivered");
            int recordCount = 0;
            try
            {
                recordCount = await _shipmentDetailsService.GetShippmentTotalCountBasedOnDateAndRunsheetAndStatusDelivered(dateRange);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(string.Format("{0} Exception occured : GetShippmentTotalCountBasedOnDateAndRunsheetAndStatusDelivered", ex.Message));
                throw;
            }
            _logger.LogInformation("End : GetShippmentTotalCountBasedOnDateAndRunsheetAndStatusDelivered");

            return Ok(recordCount);
        }

        /// <summary>
        /// Get shipment total count based on daterange and sheetype pickupsheet.
        /// </summary>
        /// <param name="dateRange">date.</param>
        /// <returns>Count</returns>
        [HttpGet]
        [Route("GetShippmentTotalCountBasedOnDateAndSheetTypePickupSheetGrandTotal")]
        public async Task<IActionResult> GetShippmentTotalCountBasedOnDateAndSheetTypePickupSheetGrandTotal(DateTime dateRange)
        {
            _logger.LogInformation("Start : GetShippmentTotalCountBasedOnDateAndSheetTypePickupSheetGrandTotal");
            int recordCount = 0;
            try
            {
                recordCount = await _shipmentDetailsService.GetShippmentTotalCountBasedOnDateAndSheetTypePickupSheetGrandTotal(dateRange);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(string.Format("{0} Exception occured : GetShippmentTotalCountBasedOnDateAndSheetTypePickupSheetGrandTotal", ex.Message));
                throw;
            }
            _logger.LogInformation("End : GetShippmentTotalCountBasedOnDateAndSheetTypePickupSheetGrandTotal");

            return Ok(recordCount);

        }

        /// <summary>
        /// Get latest order delivery details by agent delivery status.
        /// </summary>
        /// <param name="param">comma sepearated delivery hub value.</param>
        /// <returns>key value data </returns>
        [HttpGet]
        [Route("GetShippmentTotalCountBasedOnDateAndSheetTypePickupSheet")]
        public async Task<IActionResult> GetShippmentTotalCountBasedOnDateAndSheetTypePickupSheet(DateTime dateRange)
        {
            _logger.LogInformation("Start : GetShippmentTotalCountBasedOnDateAndSheetTypePickupSheet");
            int recordCount = 0;
            try
            {
                recordCount = await _shipmentDetailsService.GetShippmentTotalCountBasedOnDateAndSheetTypePickupSheet(dateRange);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(string.Format("{0} Exception occured : GetShippmentTotalCountBasedOnDateAndSheetTypePickupSheet", ex.Message));
                throw;
            }
            _logger.LogInformation("End : GetShippmentTotalCountBasedOnDateAndSheetTypePickupSheet");

            return Ok(recordCount);
        }
    }
}
