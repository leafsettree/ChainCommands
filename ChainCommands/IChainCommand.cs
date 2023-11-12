using System;
using System.Threading.Tasks;

namespace Jw;

public interface IChainCommand : IDisposable
{
    object? SignalObj {get; set;}
    Task Execute();
}