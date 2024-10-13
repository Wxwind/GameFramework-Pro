﻿using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GFPro
{
    /// <summary>
    /// 场景组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Scene")]
    public sealed class SceneComponent : Entity, IAwake, IDestroy
    {
        private ISceneManager m_SceneManager;

        private Camera                            m_MainCamera;
        private UnityEngine.SceneManagement.Scene m_GameFrameworkScene;


        /// <summary>
        /// 获取当前场景主摄像机。
        /// </summary>
        public Camera MainCamera => m_MainCamera;


        public void Awake()
        {
            m_SceneManager = new SceneManager();
            if (m_SceneManager == null)
            {
                Log.Error("Scene manager is invalid.");
                return;
            }

            m_GameFrameworkScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(0);
            if (!m_GameFrameworkScene.IsValid())
            {
                Log.Error("Game Framework scene is invalid.");
            }

            m_SceneManager.SetResourceManager(ResourceComponent.Instance);
        }


        public void Destroy()
        {
            m_SceneManager.Shutdown();
        }

        /// <summary>
        /// 获取场景名称。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景名称。</returns>
        public static string GetSceneName(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                Log.Error("Scene asset name is invalid.");
                return null;
            }

            var sceneNamePosition = sceneAssetName.LastIndexOf('/');
            if (sceneNamePosition + 1 >= sceneAssetName.Length)
            {
                Log.Error($"Scene asset name '{sceneAssetName}' is invalid.");
                return null;
            }

            var sceneName = sceneAssetName.Substring(sceneNamePosition + 1);
            sceneNamePosition = sceneName.LastIndexOf(".unity");
            if (sceneNamePosition > 0)
            {
                sceneName = sceneName.Substring(0, sceneNamePosition);
            }

            return sceneName;
        }

        /// <summary>
        /// 获取场景是否已加载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否已加载。</returns>
        public bool SceneIsLoaded(string sceneAssetName)
        {
            return m_SceneManager.SceneIsLoaded(sceneAssetName);
        }

        /// <summary>
        /// 获取已加载场景的资源名称。
        /// </summary>
        /// <returns>已加载场景的资源名称。</returns>
        public string[] GetLoadedSceneAssetNames()
        {
            return m_SceneManager.GetLoadedSceneAssetNames();
        }

        /// <summary>
        /// 获取已加载场景的资源名称。
        /// </summary>
        /// <param name="results">已加载场景的资源名称。</param>
        public void GetLoadedSceneAssetNames(List<string> results)
        {
            m_SceneManager.GetLoadedSceneAssetNames(results);
        }

        /// <summary>
        /// 获取场景是否正在加载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否正在加载。</returns>
        public bool SceneIsLoading(string sceneAssetName)
        {
            return m_SceneManager.SceneIsLoading(sceneAssetName);
        }

        /// <summary>
        /// 获取正在加载场景的资源名称。
        /// </summary>
        /// <returns>正在加载场景的资源名称。</returns>
        public string[] GetLoadingSceneAssetNames()
        {
            return m_SceneManager.GetLoadingSceneAssetNames();
        }

        /// <summary>
        /// 获取正在加载场景的资源名称。
        /// </summary>
        /// <param name="results">正在加载场景的资源名称。</param>
        public void GetLoadingSceneAssetNames(List<string> results)
        {
            m_SceneManager.GetLoadingSceneAssetNames(results);
        }

        /// <summary>
        /// 获取场景是否正在卸载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否正在卸载。</returns>
        public bool SceneIsUnloading(string sceneAssetName)
        {
            return m_SceneManager.SceneIsUnloading(sceneAssetName);
        }

        /// <summary>
        /// 获取正在卸载场景的资源名称。
        /// </summary>
        /// <returns>正在卸载场景的资源名称。</returns>
        public string[] GetUnloadingSceneAssetNames()
        {
            return m_SceneManager.GetUnloadingSceneAssetNames();
        }

        /// <summary>
        /// 获取正在卸载场景的资源名称。
        /// </summary>
        /// <param name="results">正在卸载场景的资源名称。</param>
        public void GetUnloadingSceneAssetNames(List<string> results)
        {
            m_SceneManager.GetUnloadingSceneAssetNames(results);
        }

        /// <summary>
        /// 检查场景资源是否存在。
        /// </summary>
        /// <param name="sceneAssetName">要检查场景资源的名称。</param>
        /// <returns>场景资源是否存在。</returns>
        public bool HasScene(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                Log.Error("Scene asset name is invalid.");
                return false;
            }

            if (!sceneAssetName.StartsWith("Assets/", StringComparison.Ordinal) ||
                !sceneAssetName.EndsWith(".unity", StringComparison.Ordinal))
            {
                Log.Error($"Scene asset name '{sceneAssetName}' is invalid.");
                return false;
            }

            return m_SceneManager.HasScene(sceneAssetName);
        }


        public async UniTask LoadSceneAsync(string sceneAssetName, string packageName = "",
            LoadSceneMode sceneMode = LoadSceneMode.Additive, bool suspendLoad = false, uint priority = 100,
            Action<float> progress = null)
        {
            await m_SceneManager.LoadSceneAsync(sceneAssetName, packageName, sceneMode, suspendLoad, priority,
                progress);
            SetActiveScene(m_GameFrameworkScene);
        }


        /// <summary>
        /// 卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="progress">加载场景回调</param>
        public async UniTask UnloadScene(string sceneAssetName, Action<float> progress = null)
        {
            await m_SceneManager.UnloadScene(sceneAssetName, progress);
        }

        /// <summary>
        /// 刷新当前场景主摄像机。
        /// </summary>
        public void RefreshMainCamera()
        {
            m_MainCamera = Camera.main;
        }

        private void SetActiveScene(UnityEngine.SceneManagement.Scene activeScene)
        {
            var lastActiveScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (lastActiveScene != activeScene)
            {
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(activeScene);
            }

            RefreshMainCamera();
        }
    }
}