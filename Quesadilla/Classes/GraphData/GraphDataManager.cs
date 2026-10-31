using Quesadilla.Classes.Logfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.WebPages;

namespace Quesadilla.Classes.GraphData
{
	public class GraphDataManager
	{
		private readonly Dictionary<string, List<CombatLine>> Combat = new Dictionary<string, List<CombatLine>>();
		private readonly Dictionary<string, List<IskLine>> ISK = new Dictionary<string, List<IskLine>>();
		private readonly Dictionary<string, List<MiningLine>> Mining = new Dictionary<string, List<MiningLine>>();

		private readonly object _combatLineLock = new object();
		private readonly object _iskLineLock = new object();
		private readonly object _miningLineLock = new object();

		public void AddMining(string characterName, MiningLine line)
		{
			lock (_miningLineLock)
			{
				if (!Mining.TryGetValue(characterName, out var miningLines))
				{
					miningLines = new List<MiningLine>();
					Mining.Add(characterName, miningLines);
				}

				miningLines.Add(line);
			}
		}
		public void AddISK(string characterName, IskLine line)
		{
			lock (_iskLineLock)
			{
				if (!ISK.TryGetValue(characterName, out var iskLines))
				{
					iskLines = new List<IskLine>();
					ISK.Add(characterName, iskLines);
				}

				iskLines.Add(line);
			}
		}
		public void AddCombat(string characterName, CombatLine line)
		{
			lock (_combatLineLock)
			{
				if (!Combat.TryGetValue(characterName, out var combatLines))
				{
					combatLines = new List<CombatLine>();
					Combat.Add(characterName, combatLines);
				}

				combatLines.Add(line);
			}
		}
		public List<string> GetMiningDataKeys()
		{
			lock (_miningLineLock)
				return Mining.Keys.ToList();
		}

		public List<MiningLine> GetMiningDataLines(string characterName)
		{
			lock (_miningLineLock)
			{
				if (!Mining.TryGetValue(characterName, out var lines))
					return new List<MiningLine>();

				return lines.ToList();
			}
		}

		public List<string> GetCombatDataKeys()
		{
			lock (_combatLineLock)
				return Combat.Keys.ToList();
		}

		public List<CombatLine> GetCombatDataLines(string characterName)
		{
			lock (_combatLineLock)
			{
				if (!Combat.TryGetValue(characterName, out var lines))
					return new List<CombatLine>();

				return lines.ToList();
			}
		}

		public List<string> GetIskDataKeys()
		{
			lock (_iskLineLock)
				return ISK.Keys.ToList();
		}
		public List<IskLine> GetIskDataLines(string characterName)
		{
			lock (_iskLineLock)
			{
				if (!ISK.TryGetValue(characterName, out var lines))
					return new List<IskLine>();

				return lines.ToList();
			}
		}

		public string GetIskLastHour(string toon)
		{
			var ret = "";
			double isk = 0;

			//var diffInSeconds = (DateTime.Now - a.Entry.TimeAdded).TotalSeconds;
			var expireTime = DateTime.Now.AddSeconds(-3600);
			var keys = new HashSet<string>(GetIskDataKeys());
			foreach (var t in Program.CharacterManager.Characters)
			{
				if ((!toon.IsEmpty() && t.CharacterName != toon) || !keys.Contains(t.CharacterName)) continue;
				foreach (var i in GetIskDataLines(t.CharacterName).Where(o => o.TimeAdded >= expireTime))
				{
					isk += i.IskAmount;
				}
			}

			if (isk > 0)
			{
				ret = Math.Truncate(isk).ToString("0,000 ISK");
			}

			return ret;
		}
		public string GetMiningLastHour(string toon)
		{
			var ret = "";
			double units = 0;
			double units3Wasted = 0;

			//var diffInSeconds = (DateTime.Now - a.Entry.TimeAdded).TotalSeconds;
			var expireTime = DateTime.Now.AddSeconds(-3600);

			var keys = new HashSet<string>(GetMiningDataKeys());
			foreach (var t in Program.CharacterManager.Characters)
			{
				if ((!toon.IsEmpty() && t.CharacterName != toon) || !keys.Contains(t.CharacterName)) continue;
				foreach (var m in GetMiningDataLines(t.CharacterName).Where(o => o.TimeAdded >= expireTime))
				{
					units += m.MiningAmount;
					units3Wasted += m.WastageAmount;
				}
			}

			if (units > 0)
			{
				ret = ((int)Math.Truncate(units)).ToString("N0") + " - " + ((int)Math.Truncate(units3Wasted)).ToString("N0") + " ";
			}

			return ret;
		}
		public Dictionary<string, List<MiningDetails>> GetMiningLastHourDetail(string toon, bool itemTotalsOnly)
		{
			Dictionary<string, List<MiningDetails>> miningDetails = new Dictionary<string, List<MiningDetails>>();

			//var diffInSeconds = (DateTime.Now - a.Entry.TimeAdded).TotalSeconds;
			var expireTime = DateTime.Now.AddSeconds(-3600);

			var keys = new HashSet<string>(GetMiningDataKeys());
			foreach (var t in Program.CharacterManager.Characters)
			{
				if ((!toon.IsEmpty() && t.CharacterName != toon) || !keys.Contains(t.CharacterName)) continue;
				if (!miningDetails.ContainsKey(t.CharacterName))
				{
					miningDetails[t.CharacterName] = new List<MiningDetails>();
				}
				foreach (var m in GetMiningDataLines(t.CharacterName).Where(o => o.TimeAdded >= expireTime))
				{
					MiningDetails md = miningDetails[t.CharacterName].Find(o => o.MiningItem == m.MiningItem);
					if (md == null)
					{
						md = new MiningDetails()
						{
							MiningItem = m.MiningItem,
							MiningAmount = m.MiningAmount,
							WastageAmount = m.WastageAmount,
						};
						miningDetails[t.CharacterName].Add(md);
					}
					else
					{
						md.MiningAmount += m.MiningAmount;
						md.WastageAmount += m.WastageAmount;
					}
				}
			}

			if (itemTotalsOnly)
			{
				return GetMiningItemSummary(miningDetails);
			}
			else
			{
				return miningDetails;
			}
		}

		public Dictionary<string, List<MiningDetails>> GetMiningItemSummary(Dictionary<string, List<MiningDetails>> miningDetail)
		{
			Dictionary<string, List<MiningDetails>> miningSummary = new Dictionary<string, List<MiningDetails>>();

			if (miningDetail.Count == 0) return miningSummary;

			miningSummary["Summary"] = new List<MiningDetails>();
			foreach (var detail in miningDetail)
			{
				foreach (var detailItem in detail.Value)
				{
					MiningDetails miningSummaryItem = miningSummary["Summary"].Find(o => o.MiningItem == detailItem.MiningItem);
					if (miningSummaryItem == null)
					{
						miningSummaryItem = new MiningDetails()
						{
							MiningItem = detailItem.MiningItem,
							MiningAmount = detailItem.MiningAmount,
							WastageAmount = detailItem.WastageAmount,
						};
						miningSummary["Summary"].Add(miningSummaryItem);
					}
					else
					{
						miningSummaryItem.MiningAmount += detailItem.MiningAmount;
						miningSummaryItem.WastageAmount += detailItem.WastageAmount;
					}
				}
			}
			return miningSummary;
		}
		public void ExpireMining()
		{
			foreach (var k in GetMiningDataKeys())
			{
				lock (_miningLineLock)
				{
					if (!Mining.TryGetValue(k, out var lines)) continue;
					while (lines.Count > 0)
					{
						var ts = DateTime.Now - lines[0].TimeAdded;
						if (ts.TotalSeconds <= Program.MainConfiguration.ExpireMiningSeconds) break;
						lines.RemoveAt(0);
					}
					if (lines.Count == 0) Mining.Remove(k);
				}
			}
		}

		public void ExpireISK()
		{
			foreach (var k in GetIskDataKeys())
			{
				lock (_iskLineLock)
				{
					if (!ISK.TryGetValue(k, out var lines)) continue;
					while (lines.Count > 0)
					{
						var ts = DateTime.Now - lines[0].TimeAdded;
						if (ts.TotalSeconds <= Program.MainConfiguration.ExpireISKSeconds) break;
						lines.RemoveAt(0);
					}
					if (lines.Count == 0) ISK.Remove(k);
				}
			}
		}

		public void ExpireCombat()
		{
			foreach (var k in GetCombatDataKeys())
			{
				lock (_combatLineLock)
				{
					if (!Combat.TryGetValue(k, out var lines)) continue;
					while (lines.Count > 0)
					{
						var ts = DateTime.Now - lines[0].TimeAdded;
						if (ts.TotalSeconds <= Program.MainConfiguration.ExpireCombatSeconds) break;
						lines.RemoveAt(0);
					}
					if (lines.Count == 0) Combat.Remove(k);
				}
			}
		}
	}
}