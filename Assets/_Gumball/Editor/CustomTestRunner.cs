using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Gumball.Editor
{
    public static class CustomTestRunner
    {
        
        public class ResultHandler : ICallbacks
        {
            public ITestResultAdaptor result { get; private set; }
 
            public void RunFinished(ITestResultAdaptor result)
            {
                this.result = result;
            }
        
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void TestFinished(ITestResultAdaptor result) { }
            public void TestStarted(ITestAdaptor test) { }
        }

        public enum Status
        {
            INCOMPLETE,
            FAILED,
            PASSED
        }
        
        private static TestRunnerApi testRunner;
        private static Action onCompleteTest;
        private static Dictionary<TestMode, ResultHandler> testResults;

        public static Status CurrentStatus { get; private set; }

        [InitializeOnLoadMethod]
        private static void Initialise()
        {
            if (testRunner == null)
                testRunner = ScriptableObject.CreateInstance<TestRunnerApi>();
        }
        
        public static void RunTests(Action onComplete)
        {
            testResults = new Dictionary<TestMode, ResultHandler>
            {
                { TestMode.EditMode, new ResultHandler() },
                { TestMode.PlayMode, new ResultHandler() }
            };
            
            CurrentStatus = Status.INCOMPLETE;
            RunTest(TestMode.EditMode, () =>
            {
                if (testResults[TestMode.EditMode].result.TestStatus == TestStatus.Failed)
                {
                    OnFailed();
                    return;
                }
                RunTest(TestMode.PlayMode, () =>
                {
                    if (testResults[TestMode.EditMode].result.TestStatus == TestStatus.Failed)
                        OnFailed();
                    else OnPassed();
                    onComplete?.Invoke();
                });
            });
        }

        private static void RunTest(TestMode mode, Action onComplete = null)
        {
            onCompleteTest = onComplete;
            
            Filter filter = new Filter
            {
                testMode = mode
            };
            testRunner.RegisterCallbacks(testResults[mode]);
            testRunner.Execute(new ExecutionSettings(filter)
            {
                runSynchronously = false
            });
            
            if (mode == TestMode.EditMode)
                EditorApplication.update += CheckEditModeTestStatus;
            if (mode == TestMode.PlayMode)
                EditorApplication.update += CheckPlayModeTestStatus;
        }
     
        private static void CheckTestStatus(TestMode mode)
        {
            if (testResults[mode].result != null)
            {
                onCompleteTest?.Invoke();
                
                if (mode == TestMode.EditMode)
                    EditorApplication.update -= CheckEditModeTestStatus;
                if (mode == TestMode.PlayMode)
                    EditorApplication.update -= CheckPlayModeTestStatus;
                
                testRunner.UnregisterCallbacks(testResults[mode]);
            }
        }
        
        private static void OnFailed()
        {
            CurrentStatus = Status.FAILED;
            Debug.LogError("Failed tests! Check the TestRunner for results.");
        }

        private static void OnPassed()
        {
            CurrentStatus = Status.PASSED;
            Debug.Log("Passed all tests.");
        }

        private static void CheckEditModeTestStatus() => CheckTestStatus(TestMode.EditMode);
        private static void CheckPlayModeTestStatus() => CheckTestStatus(TestMode.PlayMode);
    }
}
