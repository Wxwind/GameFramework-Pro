using UnityEditor;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Editor
{
    [CustomEditor(typeof(SoundComponent))]
    internal sealed class SoundComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty m_InstanceRoot;
        private SerializedProperty m_AudioMixer;
        private SerializedProperty m_SoundGroups;


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            var t = (SoundComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                EditorGUILayout.PropertyField(m_InstanceRoot);
                EditorGUILayout.PropertyField(m_AudioMixer);
                EditorGUILayout.PropertyField(m_SoundGroups, true);
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Sound Group Count", t.SoundGroupCount.ToString());
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
            m_InstanceRoot = serializedObject.FindProperty("m_InstanceRoot");
            m_AudioMixer = serializedObject.FindProperty("m_AudioMixer");
            m_SoundGroups = serializedObject.FindProperty("m_SoundGroups");


            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}