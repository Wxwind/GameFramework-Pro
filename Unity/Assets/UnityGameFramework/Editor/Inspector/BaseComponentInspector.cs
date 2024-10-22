﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GFPro.Editor
{
    [CustomEditor(typeof(BaseComponent))]
    internal sealed class BaseComponentInspector : GameFrameworkInspector
    {
        private const           string  NoneOptionName = "<None>";
        private static readonly float[] GameSpeed      = { 0f, 0.01f, 0.1f, 0.25f, 0.5f, 1f, 1.5f, 2f, 4f, 8f };

        private static readonly string[] GameSpeedForDisplay = { "0x", "0.01x", "0.1x", "0.25x", "0.5x", "1x", "1.5x", "2x", "4x", "8x" };

        private SerializedProperty m_ResourceMode;
        private SerializedProperty m_EditorLanguage;
        private SerializedProperty m_TextHelperTypeName;
        private SerializedProperty m_VersionHelperTypeName;
        private SerializedProperty m_LogHelperTypeName;
        private SerializedProperty m_CompressionHelperTypeName;
        private SerializedProperty m_JsonHelperTypeName;
        private SerializedProperty m_FrameRate;
        private SerializedProperty m_GameSpeed;
        private SerializedProperty m_RunInBackground;
        private SerializedProperty m_NeverSleep;

        private string[] m_TextHelperTypeNames;
        private int      m_TextHelperTypeNameIndex;
        private string[] m_VersionHelperTypeNames;
        private int      m_VersionHelperTypeNameIndex;
        private string[] m_LogHelperTypeNames;
        private int      m_LogHelperTypeNameIndex;
        private string[] m_CompressionHelperTypeNames;
        private int      m_CompressionHelperTypeNameIndex;
        private string[] m_JsonHelperTypeNames;
        private int      m_JsonHelperTypeNameIndex;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            var t = (BaseComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                EditorGUILayout.PropertyField(m_ResourceMode);
                EditorGUILayout.PropertyField(m_EditorLanguage);
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Global Helpers", EditorStyles.boldLabel);

                    var textHelperSelectedIndex =
                        EditorGUILayout.Popup("Text Helper", m_TextHelperTypeNameIndex, m_TextHelperTypeNames);
                    if (textHelperSelectedIndex != m_TextHelperTypeNameIndex)
                    {
                        m_TextHelperTypeNameIndex = textHelperSelectedIndex;
                        m_TextHelperTypeName.stringValue = textHelperSelectedIndex <= 0
                            ? null
                            : m_TextHelperTypeNames[textHelperSelectedIndex];
                    }

                    var versionHelperSelectedIndex = EditorGUILayout.Popup("Version Helper",
                        m_VersionHelperTypeNameIndex, m_VersionHelperTypeNames);
                    if (versionHelperSelectedIndex != m_VersionHelperTypeNameIndex)
                    {
                        m_VersionHelperTypeNameIndex = versionHelperSelectedIndex;
                        m_VersionHelperTypeName.stringValue = versionHelperSelectedIndex <= 0
                            ? null
                            : m_VersionHelperTypeNames[versionHelperSelectedIndex];
                    }

                    var logHelperSelectedIndex =
                        EditorGUILayout.Popup("Log Helper", m_LogHelperTypeNameIndex, m_LogHelperTypeNames);
                    if (logHelperSelectedIndex != m_LogHelperTypeNameIndex)
                    {
                        m_LogHelperTypeNameIndex = logHelperSelectedIndex;
                        m_LogHelperTypeName.stringValue = logHelperSelectedIndex <= 0
                            ? null
                            : m_LogHelperTypeNames[logHelperSelectedIndex];
                    }

                    var compressionHelperSelectedIndex = EditorGUILayout.Popup("Compression Helper",
                        m_CompressionHelperTypeNameIndex, m_CompressionHelperTypeNames);
                    if (compressionHelperSelectedIndex != m_CompressionHelperTypeNameIndex)
                    {
                        m_CompressionHelperTypeNameIndex = compressionHelperSelectedIndex;
                        m_CompressionHelperTypeName.stringValue = compressionHelperSelectedIndex <= 0
                            ? null
                            : m_CompressionHelperTypeNames[compressionHelperSelectedIndex];
                    }

                    var jsonHelperSelectedIndex =
                        EditorGUILayout.Popup("JSON Helper", m_JsonHelperTypeNameIndex, m_JsonHelperTypeNames);
                    if (jsonHelperSelectedIndex != m_JsonHelperTypeNameIndex)
                    {
                        m_JsonHelperTypeNameIndex = jsonHelperSelectedIndex;
                        m_JsonHelperTypeName.stringValue = jsonHelperSelectedIndex <= 0
                            ? null
                            : m_JsonHelperTypeNames[jsonHelperSelectedIndex];
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUI.EndDisabledGroup();

            var frameRate = EditorGUILayout.IntSlider("Frame Rate", m_FrameRate.intValue, 1, 120);
            if (frameRate != m_FrameRate.intValue)
            {
                if (EditorApplication.isPlaying)
                    t.FrameRate = frameRate;
                else
                    m_FrameRate.intValue = frameRate;
            }

            EditorGUILayout.BeginVertical("box");
            {
                var gameSpeed = EditorGUILayout.Slider("Game Speed", m_GameSpeed.floatValue, 0f, 8f);
                var selectedGameSpeed =
                    GUILayout.SelectionGrid(GetSelectedGameSpeed(gameSpeed), GameSpeedForDisplay, 5);
                if (selectedGameSpeed >= 0) gameSpeed = GetGameSpeed(selectedGameSpeed);

                if (gameSpeed != m_GameSpeed.floatValue)
                {
                    if (EditorApplication.isPlaying)
                        t.GameSpeed = gameSpeed;
                    else
                        m_GameSpeed.floatValue = gameSpeed;
                }
            }
            EditorGUILayout.EndVertical();

            var runInBackground = EditorGUILayout.Toggle("Run in Background", m_RunInBackground.boolValue);
            if (runInBackground != m_RunInBackground.boolValue)
            {
                if (EditorApplication.isPlaying)
                    t.RunInBackground = runInBackground;
                else
                    m_RunInBackground.boolValue = runInBackground;
            }

            var neverSleep = EditorGUILayout.Toggle("Never Sleep", m_NeverSleep.boolValue);
            if (neverSleep != m_NeverSleep.boolValue)
            {
                if (EditorApplication.isPlaying)
                    t.NeverSleep = neverSleep;
                else
                    m_NeverSleep.boolValue = neverSleep;
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();

            RefreshTypeNames();
        }

        private void OnEnable()
        {
            m_ResourceMode = serializedObject.FindProperty("m_ResourceMode");
            m_EditorLanguage = serializedObject.FindProperty("m_EditorLanguage");
            m_TextHelperTypeName = serializedObject.FindProperty("m_TextHelperTypeName");
            m_VersionHelperTypeName = serializedObject.FindProperty("m_VersionHelperTypeName");
            m_LogHelperTypeName = serializedObject.FindProperty("m_LogHelperTypeName");
            m_CompressionHelperTypeName = serializedObject.FindProperty("m_CompressionHelperTypeName");
            m_JsonHelperTypeName = serializedObject.FindProperty("m_JsonHelperTypeName");
            m_FrameRate = serializedObject.FindProperty("m_FrameRate");
            m_GameSpeed = serializedObject.FindProperty("m_GameSpeed");
            m_RunInBackground = serializedObject.FindProperty("m_RunInBackground");
            m_NeverSleep = serializedObject.FindProperty("m_NeverSleep");

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            var textHelperTypeNames = new List<string>
            {
                NoneOptionName
            };

            textHelperTypeNames.AddRange(Type.GetRuntimeTypeNames(typeof(Utility.Text.ITextHelper)));
            m_TextHelperTypeNames = textHelperTypeNames.ToArray();
            m_TextHelperTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(m_TextHelperTypeName.stringValue))
            {
                m_TextHelperTypeNameIndex = textHelperTypeNames.IndexOf(m_TextHelperTypeName.stringValue);
                if (m_TextHelperTypeNameIndex <= 0)
                {
                    m_TextHelperTypeNameIndex = 0;
                    m_TextHelperTypeName.stringValue = null;
                }
            }

            var versionHelperTypeNames = new List<string>
            {
                NoneOptionName
            };

            versionHelperTypeNames.AddRange(Type.GetRuntimeTypeNames(typeof(Version.IVersionHelper)));
            m_VersionHelperTypeNames = versionHelperTypeNames.ToArray();
            m_VersionHelperTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(m_VersionHelperTypeName.stringValue))
            {
                m_VersionHelperTypeNameIndex = versionHelperTypeNames.IndexOf(m_VersionHelperTypeName.stringValue);
                if (m_VersionHelperTypeNameIndex <= 0)
                {
                    m_VersionHelperTypeNameIndex = 0;
                    m_VersionHelperTypeName.stringValue = null;
                }
            }

            var logHelperTypeNames = new List<string>
            {
                NoneOptionName
            };

            logHelperTypeNames.AddRange(Type.GetRuntimeTypeNames(typeof(ILogHelper)));
            m_LogHelperTypeNames = logHelperTypeNames.ToArray();
            m_LogHelperTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(m_LogHelperTypeName.stringValue))
            {
                m_LogHelperTypeNameIndex = logHelperTypeNames.IndexOf(m_LogHelperTypeName.stringValue);
                if (m_LogHelperTypeNameIndex <= 0)
                {
                    m_LogHelperTypeNameIndex = 0;
                    m_LogHelperTypeName.stringValue = null;
                }
            }

            var compressionHelperTypeNames = new List<string>
            {
                NoneOptionName
            };

            compressionHelperTypeNames.AddRange(
                Type.GetRuntimeTypeNames(typeof(Utility.Compression.ICompressionHelper)));
            m_CompressionHelperTypeNames = compressionHelperTypeNames.ToArray();
            m_CompressionHelperTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(m_CompressionHelperTypeName.stringValue))
            {
                m_CompressionHelperTypeNameIndex =
                    compressionHelperTypeNames.IndexOf(m_CompressionHelperTypeName.stringValue);
                if (m_CompressionHelperTypeNameIndex <= 0)
                {
                    m_CompressionHelperTypeNameIndex = 0;
                    m_CompressionHelperTypeName.stringValue = null;
                }
            }

            var jsonHelperTypeNames = new List<string>
            {
                NoneOptionName
            };

            jsonHelperTypeNames.AddRange(Type.GetRuntimeTypeNames(typeof(Utility.Json.IJsonHelper)));
            m_JsonHelperTypeNames = jsonHelperTypeNames.ToArray();
            m_JsonHelperTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(m_JsonHelperTypeName.stringValue))
            {
                m_JsonHelperTypeNameIndex = jsonHelperTypeNames.IndexOf(m_JsonHelperTypeName.stringValue);
                if (m_JsonHelperTypeNameIndex <= 0)
                {
                    m_JsonHelperTypeNameIndex = 0;
                    m_JsonHelperTypeName.stringValue = null;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private float GetGameSpeed(int selectedGameSpeed)
        {
            if (selectedGameSpeed < 0) return GameSpeed[0];

            if (selectedGameSpeed >= GameSpeed.Length) return GameSpeed[GameSpeed.Length - 1];

            return GameSpeed[selectedGameSpeed];
        }

        private int GetSelectedGameSpeed(float gameSpeed)
        {
            for (var i = 0; i < GameSpeed.Length; i++)
                if (Mathf.Approximately(gameSpeed, GameSpeed[i]))
                    return i;

            return -1;
        }
    }
}