using System.Data;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using ExcelDataReader;
using GFPro.Localization;
using Luban;

namespace Tool
{
    public static partial class ExcelExporter
    {
        public static class ExcelExporter_LocalizationBuiltin
        {
            private static readonly string s_LocalizationBuiltinExcelFile = Path.GetFullPath($"{Define.WorkDir}/../Config/Excel/Localization_Builtin.xlsx");
            private static readonly string s_LocalizationOutPutDir        = Path.GetFullPath("../Unity/Assets/Resources/Localization");

            private struct LanguageTableInfo
            {
                public int      columnIndex;
                public Language language;
            }

            private struct KeyTableInfo
            {
                public int    rowIndex;
                public string key;
            }

            public static void DoExport()
            {
                var isCheck = Options.Instance.Customs.Contains("Check", StringComparison.OrdinalIgnoreCase);
                if (isCheck)
                    return;
                Log.Info("Start Export Localization Excel ...");
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var resultDict = new SortedDictionary<Language, SortedDictionary<string, string>>();
                using (var stream = new FileStream(s_LocalizationBuiltinExcelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var dataSet = reader.AsDataSet();
                        foreach (DataTable table in dataSet.Tables)
                        {
                            if (!table.Rows[0][0].ToString().StartsWith("##")) continue;
                            var startRowIndex = 0;
                            for (var rowIndex = 1; rowIndex < table.Rows.Count; rowIndex++)
                            {
                                if (table.Rows[rowIndex][0].ToString().StartsWith("##")) continue;
                                startRowIndex = rowIndex;
                                break;
                            }

                            if (startRowIndex == 0) continue;
                            //生成key和Language的信息
                            var keyTableInfos = new List<KeyTableInfo>();
                            var languageTableInfos = new List<LanguageTableInfo>();
                            var firstDataRow = table.Rows[0];
                            for (var columnIndex = 1; columnIndex < table.Columns.Count; columnIndex++)
                            {
                                var cellValue = firstDataRow[columnIndex].ToString();
                                if (cellValue.StartsWith("##")) continue;
                                if (cellValue == "key")
                                {
                                    for (var keyRowIndex = startRowIndex; keyRowIndex < table.Rows.Count; keyRowIndex++)
                                        keyTableInfos.Add(new KeyTableInfo()
                                        {
                                            key = table.Rows[keyRowIndex][columnIndex].ToString(),
                                            rowIndex = keyRowIndex
                                        });
                                }
                                else
                                {
                                    if (Enum.TryParse(cellValue, out Language language))
                                    {
                                        foreach (var languageTableInfo in languageTableInfos)
                                            if (languageTableInfo.language == language)
                                                throw new Exception(GetErrorString(table, columnIndex, 0, $"Language {cellValue} is duplicate!"));
                                        languageTableInfos.Add(new LanguageTableInfo()
                                        {
                                            columnIndex = columnIndex,
                                            language = language
                                        });
                                    }
                                    else
                                    {
                                        throw new Exception(GetErrorString(table, columnIndex, 0,
                                            $"Language {cellValue} is not exit, please get right Language string from GFPro.Localization.Language.cs!"));
                                    }
                                }
                            }

                            foreach (var keyTableInfo in keyTableInfos)
                            foreach (var languageTableInfo in languageTableInfos)
                            {
                                if (!resultDict.TryGetValue(languageTableInfo.language, out var dict))
                                {
                                    dict = new SortedDictionary<string, string>();
                                    resultDict.Add(languageTableInfo.language, dict);
                                }

                                if (dict.ContainsKey(keyTableInfo.key))
                                    throw new Exception(GetErrorString(table, languageTableInfo.columnIndex, keyTableInfo.rowIndex,
                                        $"Language key {keyTableInfo.key} is duplicate!"));
                                dict.Add(keyTableInfo.key, table.Rows[keyTableInfo.rowIndex][languageTableInfo.columnIndex].ToString());
                            }
                        }
                    }
                }

                var useJson = Options.Instance.Customs.Contains("Json", StringComparison.OrdinalIgnoreCase);
                if (Directory.Exists(s_LocalizationOutPutDir)) LubanFileHelper.ClearDirectory(s_LocalizationOutPutDir);
                else Directory.CreateDirectory(s_LocalizationOutPutDir);

                //多语言写入配置文件
                foreach (var pair in resultDict)
                {
                    var language = pair.Key;
                    var dict = pair.Value;
                    var resFullPath = Path.GetFullPath($"{s_LocalizationOutPutDir}/{language.ToString()}");

                    var jsonFileFullPath = Path.GetFullPath($"{resFullPath}.json");
                    var bytesFileFullPath = Path.GetFullPath($"{resFullPath}.bytes");
                    if (useJson)
                    {
                        var memoryStream = new MemoryStream();
                        var jsonWriter = new Utf8JsonWriter(memoryStream, new JsonWriterOptions()
                        {
                            Indented = true,
                            SkipValidation = false,
                            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        });
                        jsonWriter.WriteStartObject();
                        foreach (var kv in dict)
                        {
                            jsonWriter.WritePropertyName(kv.Key);
                            jsonWriter.WriteStringValue(kv.Value);
                        }

                        jsonWriter.WriteEndObject();
                        jsonWriter.Flush();
                        var bytes = new byte[memoryStream.Length];
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        _ = memoryStream.Read(bytes, 0, bytes.Length);
                        File.WriteAllBytes(jsonFileFullPath, bytes);
                    }
                    else
                    {
                        var byteBuf = new ByteBuf();
                        foreach (var kv in dict)
                        {
                            byteBuf.WriteString(kv.Key);
                            byteBuf.WriteString(kv.Value);
                        }

                        File.WriteAllBytes(bytesFileFullPath, byteBuf.CopyData());
                    }

                    Log.Info($"Gen {language} Success!");
                }
            }


            private static string GetErrorString(DataTable table, int columnIndex, int rowIndex, string errorMsg)
            {
                string ToAlphaString(int column)
                {
                    var h = column / 26;
                    var n = column % 26;
                    return $"{(h > 0 ? ((char)('A' + h - 1)).ToString() : "")}{(char)('A' + n)}";
                }

                var error = $@"
=======================================================================
    解析失败!

        文件:        {s_LocalizationBuiltinExcelFile}
        错误位置:    sheet:{table.TableName} [{ToAlphaString(columnIndex)}:{rowIndex + 1}] {table.Rows[rowIndex][columnIndex]}
        Err:         {errorMsg}
        字段:        {table.Rows[0][columnIndex]}

=======================================================================
";
                return error;
            }
        }
    }
}