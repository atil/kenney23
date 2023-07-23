using System;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CoroutineStarter : MonoBehaviour
{
    private static CoroutineStarter _slave;
    private static bool _isInited;

    static CoroutineStarter()
    {
        Init();
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
#endif 
    }

#if UNITY_EDITOR
    private static void EditorApplicationOnplayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            _isInited = false;
        }
    }
#endif

    private static void Init()
    {
        _slave = new GameObject("CoroutineStarter").AddComponent<CoroutineStarter>();
        DontDestroyOnLoad(_slave.gameObject);
        _isInited = true;
    }

    public static Coroutine Run(IEnumerator function)
    {
        if (!_isInited)
        {
            Init();
        }
        return _slave.StartCoroutine(function);
    }

    public static Coroutine RunDelayed(float delay, Action function)
    {
        return _slave.StartCoroutine(DelayedActionCoroutine(delay, function));
    }

    private static IEnumerator DelayedActionCoroutine(float delay, Action function)
    {
        yield return new WaitForSeconds(delay);
        function();
    }

    public static void Stop(Coroutine function)
    {
        if (function != null)
        {
            _slave.StopCoroutine(function);
        }
    }

    public static void Stop(IEnumerator function)
    {
        if (function != null)
        {
            _slave.StopCoroutine(function);
        }
    }
}