//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using UnityEditor;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Editor
{
    [CustomEditor(typeof(ResourceComponent))]
    internal sealed class ResourceComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty m_MinUnloadUnusedAssetsInterval;
        private SerializedProperty m_MaxUnloadUnusedAssetsInterval;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            var t = (ResourceComponent)target;

            var minUnloadUnusedAssetsInterval = EditorGUILayout.Slider("Min Unload Unused Assets Interval", m_MinUnloadUnusedAssetsInterval.floatValue, 0f, 3600f);
            if (minUnloadUnusedAssetsInterval != m_MinUnloadUnusedAssetsInterval.floatValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.MinUnloadUnusedAssetsInterval = minUnloadUnusedAssetsInterval;
                }
                else
                {
                    m_MinUnloadUnusedAssetsInterval.floatValue = minUnloadUnusedAssetsInterval;
                }
            }

            var maxUnloadUnusedAssetsInterval = EditorGUILayout.Slider("Max Unload Unused Assets Interval", m_MaxUnloadUnusedAssetsInterval.floatValue, 0f, 3600f);
            if (maxUnloadUnusedAssetsInterval != m_MaxUnloadUnusedAssetsInterval.floatValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.MaxUnloadUnusedAssetsInterval = maxUnloadUnusedAssetsInterval;
                }
                else
                {
                    m_MaxUnloadUnusedAssetsInterval.floatValue = maxUnloadUnusedAssetsInterval;
                }
            }


            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Unload Unused Assets",
                    Utility.Text.Format("{0:F2} / {1:F2}", t.LastUnloadUnusedAssetsOperationElapseSeconds, t.MaxUnloadUnusedAssetsInterval));
                EditorGUILayout.LabelField("Game Version", t.GameVersion ?? "<Unknwon>");
            }

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();

            RefreshTypeNames();
        }

        private void OnEnable()
        {
            m_MinUnloadUnusedAssetsInterval = serializedObject.FindProperty("m_MinUnloadUnusedAssetsInterval");
            m_MaxUnloadUnusedAssetsInterval = serializedObject.FindProperty("m_MaxUnloadUnusedAssetsInterval");

            RefreshTypeNames();
        }


        private void RefreshTypeNames()
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}