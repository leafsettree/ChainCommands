using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Jw
{
public class UnityLogService : ILogService
{
    public void Log(string s, Object obj = null)
    {
        Debug.Log(s, obj);
    }

    public void LogWarning(string s, Object obj = null)
    {
        Debug.LogWarning(s, obj);
    }

    public void LogError(string s, Object obj = null)
    {
        Debug.LogError(s, obj);
    }

    public void LogException(Exception ex, Object obj = null)
    {
        Debug.LogException(ex, obj);
    }
}
}