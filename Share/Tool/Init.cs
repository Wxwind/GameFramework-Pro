using System;
using System.Reflection;
using CommandLine;

namespace Tool
{
    internal static class Init
    {
        private static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => { Console.Error.WriteLine(e.ExceptionObject.ToString()); };

            try
            {
                // 命令行参数
                Parser.Default.ParseArguments<Options>(args)
                    .WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
                    .WithParsed((o) => Options.Instance = o);


                Console.WriteLine($"server start........................ ");
                switch (Options.Instance.AppType)
                {
                    case AppType.ExcelExporter:
                    {
                        //Options: Customs
                        //GB2312: 使用GB2312编码解决中文乱码
                        //Json: luban导出json
                        //Check: 只检查，不导出
                        //ShowCmd: 显示cmd
                        ExcelExporter.ExportAll();
                        return 0;
                    }
                    case AppType.LocalizationExporter:
                    {
                        ExcelExporter.ExportLocalization();
                        return 0;
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }

            return 1;
        }
    }
}