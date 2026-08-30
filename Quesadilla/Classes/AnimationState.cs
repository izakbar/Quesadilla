using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes
{
    public class AnimationState
    {
        private int _state = 0;
        private int _direction = 1;
        private int _maximum = 3;
        private bool _pulse = true;
        public AnimationState(int state = 0, int direction = 1, int maximum = 3, bool pulse = true) 
        { 
            _state = state;
            _direction = direction;
            _maximum = maximum;
            _pulse = pulse;
        }
        public void Advance()
        {
            _state += _direction;

            if (_direction > 0 && (_state > _maximum))
            {
                if (_pulse == true)
                {
                    _state = _maximum - 1;
                    _direction = -1;
                } else
                {
                    _state = 0;
                }
            }
            if (_direction < 0 && (_state < 0))
            {
                if ( _pulse == true )
                {
                _state = 1;
                _direction = 1;
                } else
                {
                    _state = _maximum;
                }
            }
        }

        public int? Value()
        {
            return _state;
        }

        public int? GetNextValue()
        {
            Advance();
            return Value();
        }

    }
}
