using System;
using System.Threading;

namespace Quesadilla.Classes.Tick
{
	public class TickItem
	{
		public string Name { get; }

		private int _value;
		private readonly int _maxValue;

		private int _running;
		private readonly Action _callback;

		public TickItem(
			string name,
			int initialValue,
			int maxValue,
			Action callback)
		{
			Name = name;
			_value = initialValue;
			_maxValue = maxValue;
			_callback = callback;

			_running = 0;
		}

		public void Increment()
		{
			Interlocked.Increment(ref _value);
		}

		public bool IsTriggered()
		{
			while (true)
			{
				int current = Volatile.Read(ref _value);

				if (current < _maxValue)
					return false;

				if (Interlocked.CompareExchange(ref _value, 0, current) == current)
					return true;
			}
		}


		public bool TryBeginExecution()
		{
			return Interlocked.Exchange(ref _running, 1) == 0;
		}

		public void EndExecution()
		{
			Volatile.Write(ref _running, 0);
		}

		public void Execute()
		{
			_callback?.Invoke();
		}
	}
}