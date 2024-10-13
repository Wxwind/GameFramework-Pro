using System;
using System.IO;
using System.Text;
using GFPro.Config;
using UnityEngine;

namespace GFPro
{
    /// <summary>
    /// 默认全局配置辅助器。
    /// </summary>
    public class DefaultConfigHelper : IConfigHelper
    {
        private static readonly string[] ColumnSplitSeparator = { "\t" };
        private static readonly string   BytesAssetExtension  = ".bytes";
        private const           int      ColumnCount          = 4;

        private ResourceComponent m_ResourceComponent;

        public DefaultConfigHelper()
        {
            m_ResourceComponent = ResourceComponent.Instance;
            if (m_ResourceComponent == null)
            {
                Log.Error("Resource component is invalid.");
            }
        }

        /// <summary>
        /// 读取全局配置。
        /// </summary>
        /// <param name="configComponent">全局配置管理器。</param>
        /// <param name="configAssetName">全局配置资源名称。</param>
        /// <param name="configAsset">全局配置资源。</param>
        public void ReadData(IConfigComponent configComponent, string configAssetName, TextAsset configAsset)
        {
            if (configAsset != null)
            {
                if (configAssetName.EndsWith(BytesAssetExtension, StringComparison.Ordinal))
                {
                    configComponent.ParseData(configAsset.bytes);
                }
                else
                {
                    configComponent.ParseData(configAsset.text);
                }
            }
            else throw new GameFrameworkException($"Config asset '{configAssetName}' is invalid.");
        }

        /// <summary>
        /// 读取全局配置。
        /// </summary>
        /// <param name="configComponent">全局配置管理器。</param>
        /// <param name="configAssetName">全局配置资源名称。</param>
        /// <param name="configBytes">全局配置二进制流。</param>
        /// <param name="startIndex">全局配置二进制流的起始位置。</param>
        /// <param name="length">全局配置二进制流的长度。</param>
        public void ReadData(IConfigComponent configComponent, string configAssetName, byte[] configBytes,
            int startIndex, int length)
        {
            if (configAssetName.EndsWith(BytesAssetExtension, StringComparison.Ordinal))
            {
                configComponent.ParseData(configBytes, startIndex, length);
            }
            else
            {
                configComponent.ParseData(Utility.Converter.GetString(configBytes, startIndex, length));
            }
        }

        /// <summary>
        /// 解析全局配置。
        /// </summary>
        /// <param name="configComponent">全局配置管理器。</param>
        /// <param name="configString">要解析的全局配置字符串。</param>
        public void ParseData(IConfigComponent configComponent, string configString)
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
                        throw new GameFrameworkException($"Can not parse config line string '{configLineString}' which column count is invalid.");
                    }

                    var configName = splitedLine[1];
                    var configValue = splitedLine[3];
                    if (!configComponent.AddConfig(configName, configValue))
                    {
                        throw new GameFrameworkException($"Can not add config with config name '{configName}' which may be invalid or duplicate.");
                    }
                }
            }
            catch (Exception exception)
            {
                throw new GameFrameworkException("Can not parse config string with exception '{0}'.", exception);
            }
        }

        /// <summary>
        /// 解析全局配置。
        /// </summary>
        /// <param name="configComponent">全局配置管理器。</param>
        /// <param name="configBytes">要解析的全局配置二进制流。</param>
        /// <param name="startIndex">全局配置二进制流的起始位置。</param>
        /// <param name="length">全局配置二进制流的长度。</param>
        public void ParseData(IConfigComponent configComponent, byte[] configBytes, int startIndex, int length)
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
                            if (!configComponent.AddConfig(configName, configValue))
                            {
                                throw new GameFrameworkException(
                                    $"Can not add config with config name '{configName}' which may be invalid or duplicate.");
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw new GameFrameworkException("Can not parse config bytes.", exception);
            }
        }

        /// <summary>
        /// 释放全局配置资源。
        /// </summary>
        /// <param name="configComponent">全局配置管理器。</param>
        /// <param name="configAsset">要释放的全局配置资源。</param>
        public void ReleaseDataAsset(IConfigComponent configComponent, TextAsset configAsset)
        {
            m_ResourceComponent.UnloadAsset(configAsset);
        }
    }
}