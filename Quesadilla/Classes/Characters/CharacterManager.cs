using Quesadilla.Classes.EveScout;
using Quesadilla.Classes.SolarSystems;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quesadilla.Classes.Characters
{
	public class CharacterManager
	{
		private readonly List<Character> _characters = new List<Character>();
		private readonly object _lock = new object();
		public IReadOnlyCollection<Character> Characters
		{
			get
			{
				lock (_lock)
				{
					return _characters.ToArray();
				}
			}
		}
		public Character GetOrCreate(string characterName)
		{
			lock (_lock)
			{
				var character = _characters.FirstOrDefault(c => c.CharacterName == characterName);
				if (character == null)
				{
					character = new Character
					{
						CharacterName = characterName,
						InAbyss = false
					};
					_characters.Add(character);
				}
				return character;
			}
		}

		public Character Get(string characterName)
		{
			lock (_lock)
				return _characters.FirstOrDefault(c => c.CharacterName == characterName);
		}

		public bool Exists(string characterName)
		{
			lock (_lock)
				return _characters.Any(c => c.CharacterName == characterName);
		}

		public List<Character> IsActive()
		{
			lock (_lock)
			{
				return _characters
					.Where(c => c.IsClientActive)
					.ToList();
			}
		}

		public List<Character> CharactersWithLocation()
		{
			lock (_lock)
			{
				return _characters
				.Where(c => c.LocationId > 0)
				.ToList();
			}
		}
		public List<Character> CharactersInLocation(int system)
		{
			lock (_lock)
			{
				return _characters
				.Where(c => c.LocationId == system)
				.ToList();
			}
		}
		public List<Character> CharactersNamed(string characterName)
		{
			lock (_lock)
			{
				return _characters
				.Where(c => c.CharacterName == characterName)
				.ToList();
			}
		}
		public void UpdateLocation(string characterName, int systemId, string systemName)
		{
			var character = GetOrCreate(characterName);

			lock (_lock)
			{
				character.LocationId = systemId;
				character.Location = systemName;
				character.InAbyss = false;
			}
		}
		public void SetInAbyss(string characterName)
		{
			var character = GetOrCreate(characterName);
			lock (_lock)
				character.InAbyss = true;
		}
		public void SetLastFactionAlert(string characterName, DateTime factionAlertTime)
		{
			var character = GetOrCreate(characterName);
			lock (_lock)
				character.LastFactionAlert = factionAlertTime;
		}
		public void SetIsActive(string characterName, bool active)
		{
			var character = GetOrCreate(characterName);
			lock (_lock)
				character.IsClientActive = active;
		}
		public void SetCurrentlyActiveClient(string characterName)
		{
			lock (_lock)
			{
				foreach (var character in _characters)
				{
					character.IsCurrentlActiveClient = false;
				}

				var current = _characters.FirstOrDefault(
					c => c.CharacterName == characterName);

				if (current != null)
				{
					current.IsCurrentlActiveClient = true;
				}
			}
		}

		public void SetClosestThera(string character, EveScoutWH whInfo, PathInfo path)
		{
			var c = GetOrCreate(character);
			lock (_lock)
			{
				c.ClosestThera = whInfo;
				c.ClosestTheraPath = path;
			}
		}
		public void SetClosestTurnur(string character, EveScoutWH whInfo, PathInfo path)
		{
			var c = GetOrCreate(character);
			lock (_lock)
			{
				c.ClosestTurnur = whInfo;
				c.ClosestTurnurPath = path;
			}
		}
	}
}