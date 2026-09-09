using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.ImageGeneration
{

	public class AnimationManager
	{
		private readonly ConcurrentDictionary<AnimationType, AnimationState> _animations = new ConcurrentDictionary<AnimationType, AnimationState>();

		public AnimationManager()
		{
			Register(AnimationType.Kills,state: 0,direction: -1,maximum: 36,pulse: false);
			Register(AnimationType.Intel,state: 1,direction: 1,maximum: 18,pulse: false);
			Register(AnimationType.Alts,state: 2,direction: -1,maximum: 12,pulse: false);
			Register(AnimationType.UniHoles,state: 3,direction: -1,maximum: 18,pulse: true);
			Register(AnimationType.Combat,state: 3,direction: -1,maximum: 36,pulse: true);
			Register(AnimationType.Incursion,state: 1,direction: 1,maximum: 8,pulse: true);
			Register(AnimationType.SecLimit,state: 3,direction: -1,maximum: 8,pulse: true);
			Register(AnimationType.StormElectric,state: 3,direction: -1,maximum: 10,pulse: true);
			Register(AnimationType.StormGamma,state: 6,direction: -1,maximum: 10,pulse: true);
			Register(AnimationType.StormPlasma,state: 9,direction: -1,maximum: 10,pulse: true);
			Register(AnimationType.StormExotic, state: 0, direction: -1, maximum: 10, pulse: true);
			Register(AnimationType.Skyhooks, state: 5, direction: -1, maximum: 12, pulse: false);
		}

		private void Register(
			AnimationType type,
			int state,
			int direction,
			int maximum,
			bool pulse)
		{
			_animations[type] = new AnimationState(
				state,
				direction,
				maximum,
				pulse);
		}

		public AnimationState this[AnimationType type]
			=> _animations[type];
	}
}





