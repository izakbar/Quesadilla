using System;
using System.Threading;

namespace Quesadilla.Classes.Tick
{
	public class TickItem
	{
		public string _name { get; private set; }

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
			_name = name;
			_value = initialValue;
			_maxValue = maxValue;
			_callback = callback;

			_running = 0;
		}

		public void Increment()
		{
			_value++;
		}

		public bool IsTriggered()
		{
			if (_value < _maxValue)
				return false;

			_value = 0;
			return true;
		}

		public bool TryBeginExecution()
		{
			return Interlocked.Exchange(ref _running, 1) == 0;
		}

		public void EndExecution()
		{
			Interlocked.Exchange(ref _running, 0);
		}

		public void Execute()
		{
			_callback?.Invoke();
		}
	}
}