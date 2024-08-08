using CommandLine;

namespace Tool
{
    internal static class Init
    {
        private static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => { Log.Error(e.ExceptionObject.ToString()); };

            try
            {
                // 命令行参数
                Parser.Default.ParseArguments<Options>(args)
                    .WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
                    .WithParsed((o) => Options.Instance = o);


                Console.WriteLine($"server start........................ ");
                switch (Options.Instance.AppType)
                {
                    case AppType.ExportAllExcel:
                    {
                        //Options: Customs
                        //GB2312: 使用GB2312编码解决中文乱码
                        //Json: luban导出json
                        //Check: 只检查，不导出
                        //ShowCmd: 显示cmd
                        ExcelExporter.ExportAll();
                        return 0;
                    }
                    case AppType.ExportLocalization:
                    {
                        ExcelExporter.ExportLocalization();
                        return 0;
                    }
                    case AppType.ExportLubanDataTable:
                        ExcelExporter.ExportLubanConfig();
                        return 0;
                    default:
                        Log.Error($"Unknown AppType: {Options.Instance.AppType}");
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }

            return 1;
        }
    }
}