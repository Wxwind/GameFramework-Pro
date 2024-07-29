using System;

namespace GameFramework.Resource
{
    /// <summary>
    ///     加载资源回调函数集。
    /// </summary>
    public sealed class LoadAssetCallbacks
    {
        /// <summary>
        ///     获取加载资源成功回调函数。
        /// </summary>
        public LoadAssetSuccessCallback LoadAssetSuccessCallback { get; }

        /// <summary>
        ///     获取加载资源失败回调函数。
        /// </summary>
        public LoadAssetFailureCallback LoadAssetFailureCallback { get; }


        public LoadSceneUpdateCallback LoadAssetUpdateCallback { get; }


        /// <summary>
        ///     初始化加载资源回调函数集的新实例。
        /// </summary>
        /// <param name="loadAssetSuccessCallback">加载资源成功回调函数。</param>
        /// <param name="loadAssetFailureCallback">加载资源失败回调函数。</param>
        /// <param name="loadAssetUpdateCallback">加载资源更新回调函数。</param>
        public LoadAssetCallbacks(LoadAssetSuccessCallback loadAssetSuccessCallback,
            LoadAssetFailureCallback loadAssetFailureCallback = null,
            LoadSceneUpdateCallback loadAssetUpdateCallback = null
        )

        {
            if (loadAssetSuccessCallback == null)
                throw new GameFrameworkException("Load asset success callback is invalid.");

            LoadAssetSuccessCallback = loadAssetSuccessCallback;
            LoadAssetFailureCallback = loadAssetFailureCallback;
            LoadAssetUpdateCallback = loadAssetUpdateCallback;
        }
    }
}