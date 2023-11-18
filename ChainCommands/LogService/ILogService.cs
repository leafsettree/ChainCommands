using System;
using Object = UnityEngine.Object;

namespace Jw
{
public interface ILogService
{
    void Log(string s, Object obj = null);
    void LogWarning(string s, Object obj = null);
    void LogError(string s, Object obj = null);
    void LogException(Exception ex, Object obj = null);
}
}