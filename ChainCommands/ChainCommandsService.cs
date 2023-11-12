using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity;

namespace Jw;

public class ChainCommandsService
{
	public ChainCommandsService (IUnityContainer di, ILogService logger)
	{
		_di = di;
		_logger = logger;
	}

	private Dictionary<Type, List<Type>> _signalToCommandChain = new();
	private IUnityContainer _di;
	private Dictionary<Type,Stack<IChainSignal>> _signalPools = new();
	private Dictionary<Type,Stack<IChainCommand>> _commandPools = new();
	private readonly ILogService _logger;

	public T GetSignal<T>() where T : class, IChainSignal, new()
	{
		var t = typeof(T);
		if (_signalPools.TryGetValue(t, out var stack) && stack.Count > 0)
		{
			var signal = (T)stack.Pop();
			_di.BuildUp(signal);
			return signal;
		}
		return (T)_di.Resolve(t);
	}

	private IChainCommand GetOrCreateCommand(Type t)
	{
		if (_commandPools.TryGetValue(t, out var stack) && stack.Count > 0)
		{
			var command = stack.Pop();
			_di.BuildUp(t, command);
			return command;
		}
		return (IChainCommand)_di.Resolve(t);
	}

	private void PoolCommand<T>(T command, Type commandT) where T : class, IChainCommand
	{
		Stack<IChainCommand> stack;
		if (!_commandPools.TryGetValue(commandT, out stack!))
		{
			stack = new Stack<IChainCommand>();
			_commandPools[commandT] = stack;
		}
		stack.Push(command);
    }

	private void PoolSignal<T>(T signal) where T : class, IChainSignal
	{
		var t = typeof(T);
		Stack<IChainSignal> stack;
		if (!_signalPools.TryGetValue(t, out stack!))
		{
			stack = new Stack<IChainSignal>();
			_signalPools[t] = stack;
		}
		stack.Push(signal);
    }

	public void Register<T>(List<Type> commandTypes) where T : class, IChainSignal
	{
		var tData = typeof(T);

		var nonCommandsCount = commandTypes.RemoveAll(t=>!typeof(IChainCommand).IsAssignableFrom(t));
		if (nonCommandsCount > 0)
		{
			_logger.LogError($"{tData.FullName} has {nonCommandsCount} non IChainCommand.");
		}

		if (!_signalToCommandChain.TryAdd(tData, commandTypes))
		{
			Console.WriteLine($"Already registered: {tData.FullName}");
		}
	}

	public async Task Run<T>(T? signal) where T : class, IChainSignal
	{
		var tData = typeof(T);
		if (!_signalToCommandChain.TryGetValue(tData, out var commandTypes))
		{
			return;
		}

		for (var i = 0; i < commandTypes.Count; i++)
		{
			var t = commandTypes[i];
			var instance = GetOrCreateCommand(t);
			instance.SignalObj = signal;
			try
			{
				await instance.Execute();
			}
			catch(Exception ex)
			{
				_logger.LogError($"Exception: {ex.Message}\n{ex.StackTrace}");
			}
			instance.Dispose();
			instance.SignalObj = null;
			PoolCommand(instance, t);
		}
		signal.Dispose();
		PoolSignal(signal);
	}
}