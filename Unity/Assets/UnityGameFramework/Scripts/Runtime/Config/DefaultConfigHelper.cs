using System;
using System.IO;
using System.Text;
using GameFramework;
using GameFramework.Config;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 默认全局配置辅助器。
    /// </summary>
    public class DefaultConfigHelper : ConfigHelperBase
    {
        private static readonly string[] ColumnSplitSeparator = new string[] { "\t" };
        private static readonly string   BytesAssetExtension  = ".bytes";
        private const           int      ColumnCount          = 4;

        private ResourceComponent m_ResourceComponent = null;

        /// <summary>
        /// 读取全局配置。
        /// </summary>
        /// <param name="configManager">全局配置管理器。</param>
        /// <param name="configAssetName">全局配置资源名称。</param>
        /// <param name="configAsset">全局配置资源。</param>
        /// <returns>是否读取全局配置成功。</returns>
        public override bool ReadData(IConfigManager configManager, string configAssetName, TextAsset configAsset)
        {
            if (configAsset != null)
            {
                if (configAssetName.EndsWith(BytesAssetExtension, StringComparison.Ordinal))
                {
                    return configManager.ParseData(configAsset.bytes);
                }
                else
                {
                    return configManager.ParseData(configAsset.text);
                }
            }

            Log.Warning("Config asset '{0}' is invalid.", configAssetName);
            return false;
        }

        /// <summary>
        /// 读取全局配置。
        /// </summary>
        /// <param name="configManager">全局配置管理器。</param>
        /// <param name="configAssetName">全局配置资源名称。</param>
        /// <param name="configBytes">全局配置二进制流。</param>
        /// <param name="startIndex">全局配置二进制流的起始位置。</param>
        /// <param name="length">全局配置二进制流的长度。</param>
        /// <returns>是否读取全局配置成功。</returns>
        public override bool ReadData(IConfigManager configManager, string configAssetName, byte[] configBytes,
            int startIndex, int length)
        {
            if (configAssetName.EndsWith(BytesAssetExtension, StringComparison.Ordinal))
            {
                return configManager.ParseData(configBytes, startIndex, length);
            }
            else
            {
                return configManager.ParseData(Utility.Converter.GetString(configBytes, startIndex, length));
            }
        }

        /// <summary>
        /// 解析全局配置。
        /// </summary>
        /// <param name="configManager">全局配置管理器。</param>
        /// <param name="configString">要解析的全局配置字符串。</param>
        /// <returns>是否解析全局配置成功。</returns>
        public override bool ParseData(IConfigManager configManager, string configString)
        {
            try
            {
                var position = 0;
                string configLineString = null;
                while ((configLineString = configString.ReadLine(ref position)) != null)
                {
                    if (configLineString[0] == '#')
                    {
                        continue;
                    }

                    var splitedLine = configLineString.Split(ColumnSplitSeparator, StringSplitOptions.None);
                    if (splitedLine.Length != ColumnCount)
                    {
                        Log.Warning("Can not parse config line string '{0}' which column count is invalid.",
                            configLineString);
                        return false;
                    }

                    var configName = splitedLine[1];
                    var configValue = splitedLine[3];
                    if (!configManager.AddConfig(configName, configValue))
                    {
                        Log.Warning("Can not add config with config name '{0}' which may be invalid or duplicate.",
                            configName);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                Log.Warning("Can not parse config string with exception '{0}'.", exception);
                return false;
            }
        }

        /// <summary>
        /// 解析全局配置。
        /// </summary>
        /// <param name="configManager">全局配置管理器。</param>
        /// <param name="configBytes">要解析的全局配置二进制流。</param>
        /// <param name="startIndex">全局配置二进制流的起始位置。</param>
        /// <param name="length">全局配置二进制流的长度。</param>
        /// <returns>是否解析全局配置成功。</returns>
        public override bool ParseData(IConfigManager configManager, byte[] configBytes, int startIndex, int length)
        {
            try
            {
                using (var memoryStream = new MemoryStream(configBytes, startIndex, length, false))
                {
                    using (var binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                    {
                        while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                        {
                            var configName = binaryReader.ReadString();
                            var configValue = binaryReader.ReadString();
                            if (!configManager.AddConfig(configName, configValue))
                            {
                                Log.Warning(
                                    "Can not add config with config name '{0}' which may be invalid or duplicate.",
                                    configName);
                                return false;
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                Log.Warning("Can not parse config bytes with exception '{0}'.", exception);
                return false;
            }
        }

        /// <summary>
        /// 释放全局配置资源。
        /// </summary>
        /// <param name="configManager">全局配置管理器。</param>
        /// <param name="configAsset">要释放的全局配置资源。</param>
        public override void ReleaseDataAsset(IConfigManager configManager, TextAsset configAsset)
        {
            m_ResourceComponent.UnloadAsset(configAsset);
        }

        private void Start()
        {
            m_ResourceComponent = GameEntry.GetComponent<ResourceComponent>();
            if (m_ResourceComponent == null)
            {
                Log.Fatal("Resource component is invalid.");
                return;
            }
        }
    }
}