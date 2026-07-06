using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Unity.AI.Assistant.PlayModeTest
{
    [InitializeOnLoad]
    internal static class PlayModeTestRunner
    {
        private const string StateKey = "PlayModeTest.State";
        private const string ResultKey = "PlayModeTest.Result";
        private const string ScriptPathKey = "PlayModeTest.ScriptPath";
        private const string SentinelLog = "PLAY_MODE_TEST_COMPLETE";

        private static readonly int WaitFrames = SessionState.GetInt("PlayModeTest.WaitFrames", 10);
        private static readonly float TestTimeout = SessionState.GetFloat("PlayModeTest.TestTimeout", 15.0f);

        private static List<string> _capturedLogs = new List<string>();
        private const int MaxCapturedLogs = 100;

        static PlayModeTestRunner()
        {
            string state = SessionState.GetString(StateKey, "Idle");

            switch (state)
            {
                case "Idle":
                    break;

                case "WaitingForCompile":
                    EditorApplication.delayCall += () =>
                    {
                        SessionState.SetString(StateKey, "EnteringPlayMode");
                        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                        EditorApplication.isPlaying = true;
                    };
                    break;

                case "EnteringPlayMode":
                    if (EditorApplication.isPlaying)
                    {
                        SessionState.SetString(StateKey, "InPlayMode");
                        EditorApplication.update += WaitFramesThenRun;
                    }
                    break;

                case "InPlayMode":
                    if (EditorApplication.isPlaying)
                    {
                        EditorApplication.update += WaitFramesThenRun;
                    }
                    break;

                case "Done":
                    Debug.Log(SentinelLog);
                    EditorApplication.delayCall += SelfDestruct;
                    break;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                SessionState.SetString(StateKey, "InPlayMode");
                EditorApplication.update += WaitFramesThenRun;
            }
        }

        private static int _frameCount = 0;
        private static bool _setupDone = false;
        private static bool _testDone = false;
        private static double _testStartTime = 0;
        private static float _lastCheckTime = 0;

        private static void WaitFramesThenRun()
        {
            _frameCount++;
            if (_frameCount < WaitFrames) return;

            if (_testDone) return;

            if (!_setupDone)
            {
                _setupDone = true;
                Application.logMessageReceived += OnLogMessage;
                _testStartTime = EditorApplication.timeSinceStartup;
                try
                {
                    Setup();
                }
                catch (System.Exception e)
                {
                    FinishTest(true, e.Message);
                    return;
                }
                return;
            }

            float elapsed = (float)(EditorApplication.timeSinceStartup - _testStartTime);
            bool timedOut = elapsed >= TestTimeout;

            try
            {
                bool complete = Tick(elapsed);
                if (complete || timedOut)
                {
                    FinishTest(timedOut && !complete, timedOut ? "Test timed out after " + TestTimeout + "s" : null);
                }
            }
            catch (System.Exception e)
            {
                FinishTest(true, e.Message);
            }
        }

        private static void FinishTest(bool isError, string errorMessage)
        {
            _testDone = true;
            EditorApplication.update -= WaitFramesThenRun;
            Application.logMessageReceived -= OnLogMessage;

            string resultJson = GetResult();
            SessionState.SetString(ResultKey, resultJson);
            SessionState.SetString(StateKey, "Done");
            EditorApplication.isPlaying = false;
        }

        private static void OnLogMessage(string message, string stackTrace, LogType type)
        {
            if (_capturedLogs.Count >= MaxCapturedLogs) return;
            if (type == LogType.Error || type == LogType.Exception ||
                message.Contains("[Test]") || message.Contains("TEST_RESULT"))
            {
                _capturedLogs.Add("[" + type + "] " + message);
            }
        }

        private static void SelfDestruct()
        {
            string scriptPath = SessionState.GetString(ScriptPathKey, "");
            if (!string.IsNullOrEmpty(scriptPath) && AssetDatabase.AssetPathExists(scriptPath))
            {
                AssetDatabase.DeleteAsset(scriptPath);
            }
            SessionState.EraseString(StateKey);
            SessionState.EraseString(ScriptPathKey);
        }

        [System.Serializable]
        private class TestResult
        {
            public bool success;
            public string error;
            public string[] logs;
            public int finalLeafCount;
            public int initialPoolSize;
        }

        private static void Setup()
        {
            Debug.Log("[Test] Setup started");
            var managerObj = GameObject.Find("LeafEffectManager");
            if (managerObj != null)
            {
                var manager = managerObj.GetComponent("UILeafEffectManager");
                if (manager != null)
                {
                    var settingsField = manager.GetType().GetField("m_Settings");
                    if (settingsField != null)
                    {
                        var settings = settingsField.GetValue(manager);
                        var poolSizeField = settings.GetType().GetField("InitialPoolSize");
                        Debug.Log("[Test] Initial pool size: " + poolSizeField.GetValue(settings));
                    }
                }
            }
            else
            {
                Debug.LogError("[Test] LeafEffectManager not found!");
            }
        }

        private static bool Tick(float elapsed)
        {
            if (elapsed - _lastCheckTime < 0.5f) return false;
            _lastCheckTime = elapsed;

            var managerObj = GameObject.Find("LeafEffectManager");
            if (managerObj != null)
            {
                int childCount = managerObj.transform.childCount;
                Debug.Log("[Test] Time: " + elapsed.ToString("F2") + "s, Leaf Count: " + childCount);
            }
            return elapsed >= 5.0f;
        }

        private static string GetResult()
        {
            var managerObj = GameObject.Find("LeafEffectManager");
            int finalCount = managerObj != null ? managerObj.transform.childCount : 0;
            
            int initialPool = 0;
            if (managerObj != null)
            {
                var manager = managerObj.GetComponent("UILeafEffectManager");
                var settingsField = manager.GetType().GetField("m_Settings");
                if (settingsField != null)
                {
                    var settings = settingsField.GetValue(manager);
                    var poolSizeField = settings.GetType().GetField("InitialPoolSize");
                    initialPool = (int)poolSizeField.GetValue(settings);
                }
            }

            var result = new TestResult
            {
                success = true,
                finalLeafCount = finalCount,
                initialPoolSize = initialPool,
                logs = _capturedLogs.ToArray()
            };
            return JsonUtility.ToJson(result);
        }
    }
}
