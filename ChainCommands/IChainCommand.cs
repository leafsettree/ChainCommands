using System;
using Cysharp.Threading.Tasks;

namespace Jw
{
public interface IChainCommand : IDisposable
{
    object SignalObj {get; set;}
    UniTask Execute();
}
}
