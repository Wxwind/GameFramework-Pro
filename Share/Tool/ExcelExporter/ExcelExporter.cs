using System.IO;

namespace Tool
{
    public static partial class ExcelExporter
    {
        private static readonly string s_LocalizationExcelFile = Path.GetFullPath($"{Define.WorkDir}/../Config/Excel/Localization.xlsx");

        public static void ExportAll()
        {
            ExcelExporter_Luban.DoExport();
            ExcelExporter_Localization.DoExport();
        }

        public static void ExportLocalization()
        {
            ExcelExporter_Localization.DoExport(); 
        }

        public static void ExportLubanConfig()
        {
            ExcelExporter_Luban.DoExport();
        }
    }
}