using System;

namespace Quesadilla.Classes.Tick
{
	public class TickItem
	{
		public string _name { get; set; }

		private int _value;
		private int _maxValue;

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

		public void Execute()
		{
			_callback?.Invoke();
		}
	}
}