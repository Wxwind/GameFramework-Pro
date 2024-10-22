﻿using System;
using Cysharp.Threading.Tasks;
using GFPro.Resource;
using UnityEngine;

namespace GFPro
{
    /// <summary>
    /// 数据提供者。
    /// </summary>
    /// <typeparam name="T">数据提供者的持有者的类型。</typeparam>
    internal sealed class DataProvider<T> : IDataProvider
    {
        private readonly T                      m_Owner;
        private          IResourceComponent     m_ResourceComponent;
        private          IDataProviderHelper<T> m_DataProviderHelper;


        /// <summary>
        /// 初始化数据提供者的新实例。
        /// </summary>
        /// <param name="owner">数据提供者的持有者。</param>
        public DataProvider(T owner)
        {
            m_Owner = owner;
            m_ResourceComponent = null;
            m_DataProviderHelper = null;
        }

        /// <summary>
        /// 读取数据。
        /// </summary>
        /// <param name="dataAssetName">内容资源名称。</param>
        public async UniTask ReadData(string dataAssetName)
        {
            if (m_ResourceComponent == null) throw new GameFrameworkException("You must set resource manager first.");

            if (m_DataProviderHelper == null) throw new GameFrameworkException("You must set data provider helper first.");

            var textAsset = await m_ResourceComponent.LoadAssetAsync<TextAsset>(dataAssetName);
            try
            {
                m_DataProviderHelper.ReadData(m_Owner, dataAssetName, textAsset);
            }
            catch (Exception exception)
            {
                throw new GameFrameworkException(Utility.Text.Format(
                    "Load data failure in data provider helper, data asset name '{0}'.", dataAssetName), exception);
            }
            finally
            {
                m_DataProviderHelper.ReleaseDataAsset(m_Owner, textAsset);
            }
        }


        /// <summary>
        /// 解析内容。
        /// </summary>
        /// <param name="dataString">要解析的内容字符串。</param>
        /// <returns>是否解析内容成功。</returns>
        public void ParseData(string dataString)
        {
            if (m_DataProviderHelper == null) throw new GameFrameworkException("You must set data helper first.");

            if (dataString == null) throw new GameFrameworkException("Data string is invalid.");

            m_DataProviderHelper.ParseData(m_Owner, dataString);
        }


        /// <summary>
        /// 解析内容。
        /// </summary>
        /// <param name="dataBytes">要解析的内容二进制流。</param>
        /// <returns>是否解析内容成功。</returns>
        public void ParseData(byte[] dataBytes)
        {
            if (dataBytes == null) throw new GameFrameworkException("Data bytes is invalid.");

            ParseData(dataBytes, 0, dataBytes.Length);
        }


        /// <summary>
        /// 解析内容。
        /// </summary>
        /// <param name="dataBytes">要解析的内容二进制流。</param>
        /// <param name="startIndex">内容二进制流的起始位置。</param>
        /// <param name="length">内容二进制流的长度。</param>
        /// <returns>是否解析内容成功。</returns>
        public void ParseData(byte[] dataBytes, int startIndex, int length)
        {
            if (m_DataProviderHelper == null) throw new GameFrameworkException("You must set data helper first.");

            if (dataBytes == null) throw new GameFrameworkException("Data bytes is invalid.");

            if (startIndex < 0 || length < 0 || startIndex + length > dataBytes.Length) throw new GameFrameworkException("Start index or length is invalid.");

            try
            {
                m_DataProviderHelper.ParseData(m_Owner, dataBytes, startIndex, length);
            }
            catch (Exception exception)
            {
                if (exception is GameFrameworkException) throw;

                throw new GameFrameworkException(
                    Utility.Text.Format("Can not parse data bytes with exception '{0}'.", exception), exception);
            }
        }

        /// <summary>
        /// 设置资源管理器。
        /// </summary>
        /// <param name="resourceComponent">资源管理器。</param>
        internal void SetResourceManager(IResourceComponent resourceComponent)
        {
            if (resourceComponent == null) throw new GameFrameworkException("Resource manager is invalid.");

            m_ResourceComponent = resourceComponent;
        }

        /// <summary>
        /// 设置数据提供者辅助器。
        /// </summary>
        /// <param name="dataProviderHelper">数据提供者辅助器。</param>
        internal void SetDataProviderHelper(IDataProviderHelper<T> dataProviderHelper)
        {
            if (dataProviderHelper == null) throw new GameFrameworkException("Data provider helper is invalid.");

            m_DataProviderHelper = dataProviderHelper;
        }
    }
}