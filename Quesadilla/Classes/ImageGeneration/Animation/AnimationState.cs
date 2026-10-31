namespace Quesadilla.Classes
{
	public class AnimationState
	{
		private int _state = 0;
		private int _direction = 1;
		private int _maximum = 3;
		private bool _pulse = true;

		public AnimationState(
			int state = 0,
			int direction = 1,
			int maximum = 3,
			bool pulse = true)
		{
			_state = state;
			_direction = direction;
			_maximum = maximum;
			_pulse = pulse;
		}

		public void Advance()
		{
			_state += _direction;

			if (_pulse)
			{
				if (_state > _maximum)
				{
					// Hold maximum for an extra frame
					_state = _maximum;
					_direction = -1;
				}
				else if (_state < 0)
				{
					// Hold minimum for an extra frame
					_state = 0;
					_direction = 1;
				}
			}
			else
			{
				if (_state > _maximum)
				{
					_state = 0;
				}
				else if (_state < 0)
				{
					_state = _maximum;
				}
			}
		}

		public int Value()
		{
			return _state;
		}

		public int GetNextValue()
		{
			Advance();
			return _state;
		}
	}
}