using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityGameFramework.Resource;

namespace UnityGameFramework.Localization
{
    /// <summary>
    /// 本地化管理器。
    /// </summary>
    internal sealed class LocalizationManager : ILocalizationManager
    {
        private readonly Dictionary<string, string>         m_Dictionary;
        private readonly DataProvider<ILocalizationManager> m_DataProvider;
        private          ILocalizationHelper                m_LocalizationHelper;
        private          Language                           m_Language;

        /// <summary>
        /// 初始化本地化管理器的新实例。
        /// </summary>
        public LocalizationManager()
        {
            m_Dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
            m_DataProvider = new DataProvider<ILocalizationManager>(this);
            m_LocalizationHelper = null;
            m_Language = Language.Unspecified;
        }

        /// <summary>
        /// 获取或设置本地化语言。
        /// </summary>
        public Language Language
        {
            get => m_Language;
            set
            {
                if (value == Language.Unspecified)
                {
                    throw new GameFrameworkException("Language is invalid.");
                }

                m_Language = value;
            }
        }

        /// <summary>
        /// 获取系统语言。
        /// </summary>
        public Language SystemLanguage
        {
            get
            {
                if (m_LocalizationHelper == null)
                {
                    throw new GameFrameworkException("You must set localization helper first.");
                }

                return m_LocalizationHelper.SystemLanguage;
            }
        }

        /// <summary>
        /// 获取字典数量。
        /// </summary>
        public int DictionaryCount => m_Dictionary.Count;


        /// <summary>
        /// 设置资源管理器。
        /// </summary>
        /// <param name="resourceComponent">资源管理器。</param>
        public void SetResourceManager(IResourceComponent resourceComponent)
        {
            m_DataProvider.SetResourceManager(resourceComponent);
        }

        /// <summary>
        /// 设置本地化数据提供者辅助器。
        /// </summary>
        /// <param name="dataProviderHelper">本地化数据提供者辅助器。</param>
        public void SetDataProviderHelper(IDataProviderHelper<ILocalizationManager> dataProviderHelper)
        {
            m_DataProvider.SetDataProviderHelper(dataProviderHelper);
        }

        /// <summary>
        /// 设置本地化辅助器。
        /// </summary>
        /// <param name="localizationHelper">本地化辅助器。</param>
        public void SetLocalizationHelper(ILocalizationHelper localizationHelper)
        {
            if (localizationHelper == null)
            {
                throw new GameFrameworkException("Localization helper is invalid.");
            }

            m_LocalizationHelper = localizationHelper;
        }

        /// <summary>
        /// 读取字典。
        /// </summary>
        /// <param name="dictionaryAssetName">字典资源名称。</param>
        public async UniTask ReadData(string dictionaryAssetName)
        {
            await m_DataProvider.ReadData(dictionaryAssetName);
        }


        /// <summary>
        /// 解析字典。
        /// </summary>
        /// <param name="dictionaryString">要解析的字典字符串。</param>
        public void ParseData(string dictionaryString)
        {
            m_DataProvider.ParseData(dictionaryString);
        }


        /// <summary>
        /// 解析字典。
        /// </summary>
        /// <param name="dictionaryBytes">要解析的字典二进制流。</param>
        public void ParseData(byte[] dictionaryBytes)
        {
            m_DataProvider.ParseData(dictionaryBytes);
        }


        /// <summary>
        /// 解析字典。
        /// </summary>
        /// <param name="dictionaryBytes">要解析的字典二进制流。</param>
        /// <param name="startIndex">字典二进制流的起始位置。</param>
        /// <param name="length">字典二进制流的长度。</param>
        public void ParseData(byte[] dictionaryBytes, int startIndex, int length)
        {
            m_DataProvider.ParseData(dictionaryBytes, startIndex, length);
        }


        /// <summary>
        /// 根据字典主键获取字典内容字符串。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <returns>要获取的字典内容字符串。</returns>
        public string GetString(string key)
        {
            var value = GetRawString(key);
            if (value == null)
            {
                return Utility.Text.Format("<NoKey>{0}", key);
            }

            return value;
        }

        public string GetString(string key, params object[] args)
        {
            var value = GetRawString(key);
            if (value == null)
            {
                return Utility.Text.Format("<NoKey>{0}", key);
            }

            try
            {
                return Utility.Text.Format(value, args);
            }
            catch (Exception exception)
            {
                return Utility.Text.Format("<Error>{0},{1},{2},{3}", key, value, string.Join(',', args.Select(a => a.ToString())), exception);
            }
        }

        /// <summary>
        /// 是否存在字典。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <returns>是否存在字典。</returns>
        public bool HasRawString(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new GameFrameworkException("Key is invalid.");
            }

            return m_Dictionary.ContainsKey(key);
        }

        /// <summary>
        /// 根据字典主键获取字典值。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <returns>字典值。</returns>
        public string GetRawString(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new GameFrameworkException("Key is invalid.");
            }

            return m_Dictionary.GetValueOrDefault(key);
        }

        /// <summary>
        /// 增加字典。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <param name="value">字典内容。</param>
        /// <returns>是否增加字典成功。</returns>
        public bool AddRawString(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new GameFrameworkException("Key is invalid.");
            }

            if (m_Dictionary.ContainsKey(key))
            {
                return false;
            }

            m_Dictionary.Add(key, value ?? string.Empty);
            return true;
        }

        /// <summary>
        /// 移除字典。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <returns>是否移除字典成功。</returns>
        public bool RemoveRawString(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new GameFrameworkException("Key is invalid.");
            }

            return m_Dictionary.Remove(key);
        }

        /// <summary>
        /// 清空所有字典。
        /// </summary>
        public void RemoveAllRawStrings()
        {
            m_Dictionary.Clear();
        }
    }
}