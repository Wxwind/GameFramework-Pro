using GameFramework;
using GameFramework.Localization;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 本地化辅助器基类。
    /// </summary>
    public abstract class LocalizationHelperBase : MonoBehaviour, IDataProviderHelper<ILocalizationManager>,
        ILocalizationHelper
    {
        /// <summary>
        /// 获取系统语言。
        /// </summary>
        public abstract Language SystemLanguage { get; }

        /// <summary>
        /// 读取字典。
        /// </summary>
        /// <param name="localizationManager">本地化管理器。</param>
        /// <param name="dictionaryAssetName">字典资源名称。</param>
        /// <param name="dictionaryAsset">字典资源。</param>
        /// <returns>是否读取字典成功。</returns>
        public abstract void ReadData(ILocalizationManager localizationManager, string dictionaryAssetName,
            TextAsset dictionaryAsset);

        /// <summary>
        /// 读取字典。
        /// </summary>
        /// <param name="localizationManager">本地化管理器。</param>
        /// <param name="dictionaryAssetName">字典资源名称。</param>
        /// <param name="dictionaryBytes">字典二进制流。</param>
        /// <param name="startIndex">字典二进制流的起始位置。</param>
        /// <param name="length">字典二进制流的长度。</param>
        /// <returns>是否读取字典成功。</returns>
        public abstract void ReadData(ILocalizationManager localizationManager, string dictionaryAssetName,
            byte[] dictionaryBytes, int startIndex, int length);

        /// <summary>
        /// 解析字典。
        /// </summary>
        /// <param name="localizationManager">本地化管理器。</param>
        /// <param name="dictionaryString">要解析的字典字符串。</param>
        /// <returns>是否解析字典成功。</returns>
        public abstract void ParseData(ILocalizationManager localizationManager, string dictionaryString);

        /// <summary>
        /// 解析字典。
        /// </summary>
        /// <param name="localizationManager">本地化管理器。</param>
        /// <param name="dictionaryBytes">要解析的字典二进制流。</param>
        /// <param name="startIndex">字典二进制流的起始位置。</param>
        /// <param name="length">字典二进制流的长度。</param>
        /// <returns>是否解析字典成功。</returns>
        public abstract void ParseData(ILocalizationManager localizationManager, byte[] dictionaryBytes, int startIndex,
            int length);

        /// <summary>
        /// 释放字典资源。
        /// </summary>
        /// <param name="localizationManager">本地化管理器。</param>
        /// <param name="dictionaryAsset">要释放的字典资源。</param>
        public abstract void ReleaseDataAsset(ILocalizationManager localizationManager, TextAsset dictionaryAsset);
    }
}