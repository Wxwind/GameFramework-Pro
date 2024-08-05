using CommandLine;
using System;
using System.Collections.Generic;

namespace Tool
{
    public enum AppType
    {
        GameTool,
        ExportLubanConfig,
        ExportLocalization,
        ExportAllExcel,
        
    }

    public class Options
    {
        public static Options Instance;

        [Option("AppType", Required = false, Default = AppType.GameTool, HelpText = "AppType enum")]
        public AppType AppType { get; set; }


        // 自定义（可以用来自定义参数）
        [Option("Customs", Required = false, Default = "")]
        public string Customs { get; set; }
    }
}