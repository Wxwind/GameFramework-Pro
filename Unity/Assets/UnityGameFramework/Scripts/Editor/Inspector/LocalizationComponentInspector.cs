﻿using UnityEditor;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Editor
{
    [CustomEditor(typeof(LocalizationComponent))]
    internal sealed class LocalizationComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty m_CachedBytesSize = null;

        private HelperInfo<LocalizationHelperBase> m_LocalizationHelperInfo =
            new HelperInfo<LocalizationHelperBase>("Localization");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            LocalizationComponent t = (LocalizationComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                m_LocalizationHelperInfo.Draw();
                EditorGUILayout.PropertyField(m_CachedBytesSize);
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Language", t.Language.ToString());
                EditorGUILayout.LabelField("System Language", t.SystemLanguage.ToString());
                EditorGUILayout.LabelField("Dictionary Count", t.DictionaryCount.ToString());
                EditorGUILayout.LabelField("Cached Bytes Size", t.CachedBytesSize.ToString());
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
            m_CachedBytesSize = serializedObject.FindProperty("m_CachedBytesSize");

            m_LocalizationHelperInfo.Init(serializedObject);

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            m_LocalizationHelperInfo.Refresh();
            serializedObject.ApplyModifiedProperties();
        }
    }
}