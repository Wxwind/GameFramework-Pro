using System;
using Cysharp.Threading.Tasks;
using Game;
using GameFramework;
using GameFramework.Localization;
using GameFramework.Resource;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 本地化组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Localization")]
    public sealed class LocalizationComponent : GameFrameworkComponent
    {
        private ILocalizationManager m_LocalizationManager = null;

        [SerializeField] private int m_CachedBytesSize = 0;

        public Action OnLanguageChanged;

        /// <summary>
        /// 获取或设置本地化语言。
        /// </summary>
        public Language Language => m_LocalizationManager.Language;

        /// <summary>
        /// 获取系统语言。
        /// </summary>
        public Language SystemLanguage => m_LocalizationManager.SystemLanguage;

        /// <summary>
        /// 获取字典数量。
        /// </summary>
        public int DictionaryCount => m_LocalizationManager.DictionaryCount;

        /// <summary>
        /// 获取缓冲二进制流的大小。
        /// </summary>
        public int CachedBytesSize => m_LocalizationManager.CachedBytesSize;

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_LocalizationManager = GameFrameworkEntry.GetModule<ILocalizationManager>();
            if (m_LocalizationManager == null)
            {
                Log.Fatal("Localization manager is invalid.");
                return;
            }
        }

        private void Start()
        {
            var baseComponent = GameEntry.GetComponent<BaseComponent>();
            if (baseComponent == null)
            {
                Log.Fatal("Base component is invalid.");
                return;
            }

            m_LocalizationManager.SetResourceManager(GameFrameworkEntry.GetModule<IResourceManager>());

            var localizationHelper = new JsonLocalizationHelper();
            m_LocalizationManager.SetDataProviderHelper(localizationHelper);
            m_LocalizationManager.SetLocalizationHelper(localizationHelper);
            m_LocalizationManager.Language =
                baseComponent.ResourceMode == ResourceMode.EditorSimulateMode &&
                baseComponent.EditorLanguage != Language.Unspecified
                    ? baseComponent.EditorLanguage
                    : m_LocalizationManager.SystemLanguage;
            if (m_CachedBytesSize > 0)
            {
                EnsureCachedBytesSize(m_CachedBytesSize);
            }
        }


        /// <summary>
        /// 确保二进制流缓存分配足够大小的内存并缓存。
        /// </summary>
        /// <param name="ensureSize">要确保二进制流缓存分配内存的大小。</param>
        public void EnsureCachedBytesSize(int ensureSize)
        {
            m_LocalizationManager.EnsureCachedBytesSize(ensureSize);
        }

        /// <summary>
        /// 释放缓存的二进制流。
        /// </summary>
        public void FreeCachedBytes()
        {
            m_LocalizationManager.FreeCachedBytes();
        }


        /// <summary>
        /// 读取字典。
        /// </summary>
        /// <param name="dictionaryAssetName">字典资源名称。</param>
        public async UniTask ReadData(string dictionaryAssetName)
        {
            await m_LocalizationManager.ReadData(dictionaryAssetName);
        }

        /// <summary>
        /// 解析字典。
        /// </summary>
        /// <param name="dictionaryString">要解析的字典字符串。</param>
        public void ParseData(string dictionaryString)
        {
            m_LocalizationManager.ParseData(dictionaryString);
        }

        /// <summary>
        /// 解析字典。
        /// </summary>
        /// <param name="dictionaryBytes">要解析的字典二进制流。</param>
        public void ParseData(byte[] dictionaryBytes)
        {
            m_LocalizationManager.ParseData(dictionaryBytes);
        }

        /// <summary>
        /// 解析字典。
        /// </summary>
        /// <param name="dictionaryBytes">要解析的字典二进制流。</param>
        /// <param name="startIndex">字典二进制流的起始位置。</param>
        /// <param name="length">字典二进制流的长度。</param>
        public void ParseData(byte[] dictionaryBytes, int startIndex, int length)
        {
            m_LocalizationManager.ParseData(dictionaryBytes, startIndex, length);
        }

        /// <summary>
        /// 根据字典主键获取字典内容字符串。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <param name="args">字典参数。</param>
        /// <returns>要获取的字典内容字符串。</returns>
        public string GetString(string key, params object[] args)
        {
            return m_LocalizationManager.GetString(key, args);
        }

        /// <summary>
        /// 是否存在字典。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <returns>是否存在字典。</returns>
        public bool HasRawString(string key)
        {
            return m_LocalizationManager.HasRawString(key);
        }

        /// <summary>
        /// 根据字典主键获取字典值。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <returns>字典值。</returns>
        public string GetRawString(string key)
        {
            return m_LocalizationManager.GetRawString(key);
        }

        /// <summary>
        /// 移除字典。
        /// </summary>
        /// <param name="key">字典主键。</param>
        /// <returns>是否移除字典成功。</returns>
        public bool RemoveRawString(string key)
        {
            return m_LocalizationManager.RemoveRawString(key);
        }

        /// <summary>
        /// 清空所有字典。
        /// </summary>
        public void RemoveAllRawStrings()
        {
            m_LocalizationManager.RemoveAllRawStrings();
        }

        public async UniTask SetLanguage(Language language)
        {
            var dictionaryAssetName = language.ToString();
            m_LocalizationManager.Language = language;
            RemoveAllRawStrings();
            await ReadData(dictionaryAssetName);
            OnLanguageChanged?.Invoke();
        }

        public async UniTask InitAsync()
        {
            var dictionaryAssetName = Language.ToString();
            await ReadData(dictionaryAssetName);
            OnLanguageChanged?.Invoke();
        }

        public void InitBuiltinLocalization(Language language)
        {
            try
            {
                var path = $"Localization/{language}";
                var asset = Resources.Load<TextAsset>(path);
                if (asset == null)
                {
                    throw new GameFrameworkException($"{path} is invalid");
                }

                m_LocalizationManager.Language = language;
                ParseData(asset.text);
            }
            catch (Exception e)
            {
                throw new GameFrameworkException("Parse default dictionary failure.", e);
            }
        }
    }
}