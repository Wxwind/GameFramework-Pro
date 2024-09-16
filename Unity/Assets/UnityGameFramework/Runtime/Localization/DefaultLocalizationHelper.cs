using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityGameFramework.Localization;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 默认本地化辅助器。
    /// </summary>
    public class DefaultLocalizationHelper : ILocalizationHelper
    {
        private static readonly string[] ColumnSplitSeparator = { "\t" };
        private static readonly string   BytesAssetExtension  = ".bytes";
        private const           int      ColumnCount          = 4;

        private ResourceComponent m_ResourceComponent;

        public DefaultLocalizationHelper()
        {
            m_ResourceComponent = GameEntry.GetComponent<ResourceComponent>();
            if (m_ResourceComponent == null)
            {
                Log.Fatal("Resource component is invalid.");
            }
        }

        /// <summary>
        /// 获取系统语言。
        /// </summary>
        public Language SystemLanguage
        {
            get
            {
                switch (Application.systemLanguage)
                {
                    case UnityEngine.SystemLanguage.Afrikaans:          return Language.Afrikaans;
                    case UnityEngine.SystemLanguage.Arabic:             return Language.Arabic;
                    case UnityEngine.SystemLanguage.Basque:             return Language.Basque;
                    case UnityEngine.SystemLanguage.Belarusian:         return Language.Belarusian;
                    case UnityEngine.SystemLanguage.Bulgarian:          return Language.Bulgarian;
                    case UnityEngine.SystemLanguage.Catalan:            return Language.Catalan;
                    case UnityEngine.SystemLanguage.Chinese:            return Language.ChineseSimplified;
                    case UnityEngine.SystemLanguage.ChineseSimplified:  return Language.ChineseSimplified;
                    case UnityEngine.SystemLanguage.ChineseTraditional: return Language.ChineseTraditional;
                    case UnityEngine.SystemLanguage.Czech:              return Language.Czech;
                    case UnityEngine.SystemLanguage.Danish:             return Language.Danish;
                    case UnityEngine.SystemLanguage.Dutch:              return Language.Dutch;
                    case UnityEngine.SystemLanguage.English:            return Language.English;
                    case UnityEngine.SystemLanguage.Estonian:           return Language.Estonian;
                    case UnityEngine.SystemLanguage.Faroese:            return Language.Faroese;
                    case UnityEngine.SystemLanguage.Finnish:            return Language.Finnish;
                    case UnityEngine.SystemLanguage.French:             return Language.French;
                    case UnityEngine.SystemLanguage.German:             return Language.German;
                    case UnityEngine.SystemLanguage.Greek:              return Language.Greek;
                    case UnityEngine.SystemLanguage.Hebrew:             return Language.Hebrew;
                    case UnityEngine.SystemLanguage.Hungarian:          return Language.Hungarian;
                    case UnityEngine.SystemLanguage.Icelandic:          return Language.Icelandic;
                    case UnityEngine.SystemLanguage.Indonesian:         return Language.Indonesian;
                    case UnityEngine.SystemLanguage.Italian:            return Language.Italian;
                    case UnityEngine.SystemLanguage.Japanese:           return Language.Japanese;
                    case UnityEngine.SystemLanguage.Korean:             return Language.Korean;
                    case UnityEngine.SystemLanguage.Latvian:            return Language.Latvian;
                    case UnityEngine.SystemLanguage.Lithuanian:         return Language.Lithuanian;
                    case UnityEngine.SystemLanguage.Norwegian:          return Language.Norwegian;
                    case UnityEngine.SystemLanguage.Polish:             return Language.Polish;
                    case UnityEngine.SystemLanguage.Portuguese:         return Language.PortuguesePortugal;
                    case UnityEngine.SystemLanguage.Romanian:           return Language.Romanian;
                    case UnityEngine.SystemLanguage.Russian:            return Language.Russian;
                    case UnityEngine.SystemLanguage.SerboCroatian:      return Language.SerboCroatian;
                    case UnityEngine.SystemLanguage.Slovak:             return Language.Slovak;
                    case UnityEngine.SystemLanguage.Slovenian:          return Language.Slovenian;
                    case UnityEngine.SystemLanguage.Spanish:            return Language.Spanish;
                    case UnityEngine.SystemLanguage.Swedish:            return Language.Swedish;
                    case UnityEngine.SystemLanguage.Thai:               return Language.Thai;
                    case UnityEngine.SystemLanguage.Turkish:            return Language.Turkish;
                    case UnityEngine.SystemLanguage.Ukrainian:          return Language.Ukrainian;
                    case UnityEngine.SystemLanguage.Unknown:            return Language.Unspecified;
                    case UnityEngine.SystemLanguage.Vietnamese:         return Language.Vietnamese;
                    default:                                            return Language.Unspecified;
                }
            }
        }

        /// <summary>
        /// 读取字典。
        /// </summary>
        /// <param name="localizationManager">本地化管理器。</param>
        /// <param name="dictionaryAssetName">字典资源名称。</param>
        /// <param name="dictionaryAsset">字典资源。</param>
        /// <returns>是否读取字典成功。</returns>
        public void ReadData(ILocalizationManager localizationManager, string dictionaryAssetName,
            TextAsset dictionaryAsset)
        {
            if (dictionaryAsset != null)
            {
                if (dictionaryAssetName.EndsWith(BytesAssetExtension, StringComparison.Ordinal))
                {
                    localizationManager.ParseData(dictionaryAsset.bytes);
                }
                else
                {
                    localizationManager.ParseData(dictionaryAsset.text);
                }
            }

            else throw new GameFrameworkException($"Dictionary asset '{dictionaryAssetName}' is invalid.");
        }

        /// <summary>
        /// 读取字典。
        /// </summary>
        /// <param name="localizationManager">本地化管理器。</param>
        /// <param name="dictionaryAssetName">字典资源名称。</param>
        /// <param name="dictionaryBytes">字典二进制流。</param>
        /// <param name="startIndex">字典二进制流的起始位置。</param>
        /// <param name="length">字典二进制流的长度。</param>
        /// <returns>是否读取字典成功。</returns>
        public virtual void ReadData(ILocalizationManager localizationManager, string dictionaryAssetName,
            byte[] dictionaryBytes, int startIndex, int length)
        {
            if (dictionaryAssetName.EndsWith(BytesAssetExtension, StringComparison.Ordinal))
            {
                localizationManager.ParseData(dictionaryBytes, startIndex, length);
            }
            else
            {
                localizationManager.ParseData(Utility.Converter.GetString(dictionaryBytes, startIndex, length));
            }
        }

        /// <summary>
        /// 解析字典。
        /// </summary>
        /// <param name="localizationManager">本地化管理器。</param>
        /// <param name="dictionaryString">要解析的字典字符串。</param>
        /// <returns>是否解析字典成功。</returns>
        public virtual void ParseData(ILocalizationManager localizationManager, string dictionaryString)
        {
            try
            {
                var position = 0;
                string dictionaryLineString = null;
                while ((dictionaryLineString = dictionaryString.ReadLine(ref position)) != null)
                {
                    if (dictionaryLineString[0] == '#')
                    {
                        continue;
                    }

                    var splitedLine = dictionaryLineString.Split(ColumnSplitSeparator, StringSplitOptions.None);
                    if (splitedLine.Length != ColumnCount)
                    {
                        throw new GameFrameworkException($"Can not parse dictionary line string '{dictionaryLineString}' which column count is invalid.");
                    }

                    var dictionaryKey = splitedLine[1];
                    var dictionaryValue = splitedLine[3];
                    if (!localizationManager.AddRawString(dictionaryKey, dictionaryValue))
                    {
                        throw new GameFrameworkException($"Can not add raw string with dictionary key '{dictionaryKey}' which may be invalid or duplicate.");
                    }
                }
            }
            catch (Exception exception)
            {
                throw new GameFrameworkException("Can not parse dictionary string with exception.", exception);
            }
        }

        /// <summary>
        /// 解析字典。
        /// </summary>
        /// <param name="localizationManager">本地化管理器。</param>
        /// <param name="dictionaryBytes">要解析的字典二进制流。</param>
        /// <param name="startIndex">字典二进制流的起始位置。</param>
        /// <param name="length">字典二进制流的长度。</param>
        /// <returns>是否解析字典成功。</returns>
        public virtual void ParseData(ILocalizationManager localizationManager, byte[] dictionaryBytes, int startIndex,
            int length)
        {
            try
            {
                using (var memoryStream = new MemoryStream(dictionaryBytes, startIndex, length, false))
                {
                    using (var binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                    {
                        while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                        {
                            var dictionaryKey = binaryReader.ReadString();
                            var dictionaryValue = binaryReader.ReadString();
                            if (!localizationManager.AddRawString(dictionaryKey, dictionaryValue))
                            {
                                throw new GameFrameworkException($"Can not add raw string with dictionary key '{dictionaryKey}' which may be invalid or duplicate.");
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw new GameFrameworkException("Can not parse dictionary bytes.", exception);
            }
        }

        /// <summary>
        /// 释放字典资源。
        /// </summary>
        /// <param name="localizationManager">本地化管理器。</param>
        /// <param name="dictionaryAsset">要释放的字典资源。</param>
        public void ReleaseDataAsset(ILocalizationManager localizationManager, TextAsset dictionaryAsset)
        {
            m_ResourceComponent.UnloadAsset(dictionaryAsset);
        }
    }
}