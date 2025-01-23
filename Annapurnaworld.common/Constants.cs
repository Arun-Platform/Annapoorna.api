using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Annapurnaworld.common
{
    public class Constants
    {
        public const string STOREDPROCEDURE_GETLATESTSHIPMENTSTATUSBYAGENTS = "GetLatestShipmentStatusByAgents";

        public static string QUERY_GETSHIPPMENT_TOTAL_COUNT_BASED_ONDATE_AND_RUNSHEET = "select ISNULL(sum(RecordCount),0) FROM [annapurnaworlddb].[dbo].[OrderShipmentMetaDatas]  with(nolock)  where  FileDate  between cast('{0}' as Date) and CAST(GETDATE() as date) and SheetType ='RUNSHEET' and status is null ";

        public static string QUERY_GETSHIPPMENT_TOTALCOUNT_BASEDON_DATE_AND_RUNSHEET_STATUS_DELIVERED = "select ISNULL(sum(RecordCount),0) FROM [annapurnaworlddb].[dbo].[OrderShipmentMetaDatas] v where  FileDate between cast('{0}' as Date) and CAST(GETDATE() as date) and [Status] ='{1}'  and SheetType ='RUNSHEET'";

        public static string QUERY_GETSHIPPMENT_TOTAL_COUNT_BASEDON_DATE_AND_SHEETTYPE_PICKUPSHEET = "select  ISNULL(sum(RecordCount),0) FROM [annapurnaworlddb].[dbo].[OrderShipmentMetaDatas] v where  FileDate between cast('{0}' as Date) and CAST(GETDATE() as date) and SheetType ='PICKUPSHEET' and status is null";

        public static string QUERY_GETSHIPPMENT_TOTALCOUNT_BASED_ON_DATE_AND_SHEET_TYPE_PickupSheet = "select  ISNULL(sum(RecordCount),0) FROM [annapurnaworlddb].[dbo].[OrderShipmentMetaDatas] v where  FileDate between cast('{0}' as Date) and CAST(GETDATE() as date) and [Status] ='PICKUP_Picked_Complete' and SheetType ='PICKUPSHEET' ";


       public static readonly List<string> SheetTypeRunsheetStatuses = new List<string>
        {
            "Delivered",
            "Partially_Delivered",
            "Delivery_Update"
        };
    }
}
