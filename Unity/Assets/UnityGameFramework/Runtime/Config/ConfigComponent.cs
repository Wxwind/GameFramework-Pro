﻿using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GFPro.Config;
using UnityEngine;

namespace GFPro
{
    /// <summary>
    /// 全局配置组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Config")]
    public sealed partial class ConfigComponent : GameFrameworkComponent, IConfigComponent
    {
        private readonly Dictionary<string, ConfigData> m_ConfigDatas = new();
        private          DataProvider<IConfigComponent> m_DataProvider;
        private          IConfigHelper                  m_ConfigHelper;

        /// <summary>
        /// 获取全局配置项数量。
        /// </summary>
        public int Count => m_ConfigDatas.Count;


        private void Start()
        {
            m_DataProvider = new DataProvider<IConfigComponent>(this);
            m_ConfigHelper = new DefaultConfigHelper();
            m_DataProvider.SetDataProviderHelper(m_ConfigHelper);
            m_DataProvider.SetResourceManager(GameEntry.GetComponent<ResourceComponent>());
            transform.SetParent(transform);
            transform.localScale = Vector3.one;
        }


        /// <summary>
        /// 读取全局配置。
        /// </summary>
        /// <param name="configAssetName">全局配置资源名称。</param>
        public async UniTask ReadData(string configAssetName)
        {
            await m_DataProvider.ReadData(configAssetName);
        }


        /// <summary>
        /// 解析全局配置。
        /// </summary>
        /// <param name="configString">要解析的全局配置字符串。</param>
        public void ParseData(string configString)
        {
            m_DataProvider.ParseData(configString);
        }


        /// <summary>
        /// 解析全局配置。
        /// </summary>
        /// <param name="configBytes">要解析的全局配置二进制流。</param>
        public void ParseData(byte[] configBytes)
        {
            m_DataProvider.ParseData(configBytes);
        }


        /// <summary>
        /// 解析全局配置。
        /// </summary>
        /// <param name="configBytes">要解析的全局配置二进制流。</param>
        /// <param name="startIndex">全局配置二进制流的起始位置。</param>
        /// <param name="length">全局配置二进制流的长度。</param>
        public void ParseData(byte[] configBytes, int startIndex, int length)
        {
            m_DataProvider.ParseData(configBytes, startIndex, length);
        }


        /// <summary>
        /// 检查是否存在指定全局配置项。
        /// </summary>
        /// <param name="configName">要检查全局配置项的名称。</param>
        /// <returns>指定的全局配置项是否存在。</returns>
        public bool HasConfig(string configName)
        {
            return GetConfigData(configName).HasValue;
        }

        /// <summary>
        /// 从指定全局配置项中读取布尔值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <returns>读取的布尔值。</returns>
        public bool GetBool(string configName)
        {
            var configData = GetConfigData(configName);
            if (!configData.HasValue)
            {
                throw new GameFrameworkException(Utility.Text.Format("Config name '{0}' is not exist.", configName));
            }

            return configData.Value.BoolValue;
        }

        /// <summary>
        /// 从指定全局配置项中读取布尔值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <param name="defaultValue">当指定的全局配置项不存在时，返回此默认值。</param>
        /// <returns>读取的布尔值。</returns>
        public bool GetBool(string configName, bool defaultValue)
        {
            var configData = GetConfigData(configName);
            return configData.HasValue ? configData.Value.BoolValue : defaultValue;
        }

        /// <summary>
        /// 从指定全局配置项中读取整数值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <returns>读取的整数值。</returns>
        public int GetInt(string configName)
        {
            var configData = GetConfigData(configName);
            if (!configData.HasValue)
            {
                throw new GameFrameworkException(Utility.Text.Format("Config name '{0}' is not exist.", configName));
            }

            return configData.Value.IntValue;
        }

        /// <summary>
        /// 从指定全局配置项中读取整数值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <param name="defaultValue">当指定的全局配置项不存在时，返回此默认值。</param>
        /// <returns>读取的整数值。</returns>
        public int GetInt(string configName, int defaultValue)
        {
            var configData = GetConfigData(configName);
            return configData.HasValue ? configData.Value.IntValue : defaultValue;
        }

        /// <summary>
        /// 从指定全局配置项中读取浮点数值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <returns>读取的浮点数值。</returns>
        public float GetFloat(string configName)
        {
            var configData = GetConfigData(configName);
            if (!configData.HasValue)
            {
                throw new GameFrameworkException(Utility.Text.Format("Config name '{0}' is not exist.", configName));
            }

            return configData.Value.FloatValue;
        }

        /// <summary>
        /// 从指定全局配置项中读取浮点数值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <param name="defaultValue">当指定的全局配置项不存在时，返回此默认值。</param>
        /// <returns>读取的浮点数值。</returns>
        public float GetFloat(string configName, float defaultValue)
        {
            var configData = GetConfigData(configName);
            return configData.HasValue ? configData.Value.FloatValue : defaultValue;
        }

        /// <summary>
        /// 从指定全局配置项中读取字符串值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <returns>读取的字符串值。</returns>
        public string GetString(string configName)
        {
            var configData = GetConfigData(configName);
            if (!configData.HasValue)
            {
                throw new GameFrameworkException(Utility.Text.Format("Config name '{0}' is not exist.", configName));
            }

            return configData.Value.StringValue;
        }

        /// <summary>
        /// 从指定全局配置项中读取字符串值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <param name="defaultValue">当指定的全局配置项不存在时，返回此默认值。</param>
        /// <returns>读取的字符串值。</returns>
        public string GetString(string configName, string defaultValue)
        {
            var configData = GetConfigData(configName);
            return configData.HasValue ? configData.Value.StringValue : defaultValue;
        }

        /// <summary>
        /// 增加指定全局配置项。
        /// </summary>
        /// <param name="configName">要增加全局配置项的名称。</param>
        /// <param name="boolValue">全局配置项布尔值。</param>
        /// <param name="intValue">全局配置项整数值。</param>
        /// <param name="floatValue">全局配置项浮点数值。</param>
        /// <param name="stringValue">全局配置项字符串值。</param>
        /// <returns>是否增加全局配置项成功。</returns>
        public bool AddConfig(string configName, bool boolValue, int intValue, float floatValue, string stringValue)
        {
            if (HasConfig(configName))
            {
                return false;
            }

            m_ConfigDatas.Add(configName, new ConfigData(boolValue, intValue, floatValue, stringValue));
            return true;
        }

        public bool AddConfig(string configName, string configValue)
        {
            var boolValue = false;
            bool.TryParse(configValue, out boolValue);

            var intValue = 0;
            int.TryParse(configValue, out intValue);

            var floatValue = 0f;
            float.TryParse(configValue, out floatValue);

            return AddConfig(configName, boolValue, intValue, floatValue, configValue);
        }


        /// <summary>
        /// 移除指定全局配置项。
        /// </summary>
        /// <param name="configName">要移除全局配置项的名称。</param>
        /// <returns>是否移除全局配置项成功。</returns>
        public bool RemoveConfig(string configName)
        {
            if (!HasConfig(configName))
            {
                return false;
            }

            return m_ConfigDatas.Remove(configName);
        }

        /// <summary>
        /// 清空所有全局配置项。
        /// </summary>
        public void RemoveAllConfigs()
        {
            m_ConfigDatas.Clear();
        }

        private ConfigData? GetConfigData(string configName)
        {
            if (string.IsNullOrEmpty(configName))
            {
                throw new GameFrameworkException("Config name is invalid.");
            }

            var configData = default(ConfigData);
            if (m_ConfigDatas.TryGetValue(configName, out configData))
            {
                return configData;
            }

            return null;
        }
    }
}