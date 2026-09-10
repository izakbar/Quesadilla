using EVEStandard.Models;
using Quesadilla.Classes.Queues;
using Quesadilla.Classes.ZKill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Quesadilla.Classes.SolarSystems
{
    public class SolarSystem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string RegionName { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
		public double DotlanX { get; set; }
		public double DotlanY { get; set; }
		public double X2D { get; set; }
		public double Y2D { get; set; }
        public bool MatchNameRegex(string logLine) => this._nameRegex.IsMatch(logLine);
        public List<SolarSystemConnectionData> ConnectedTo { get; set; }
        public List<PlanetInfo> Planets { get; set; }
        public bool Incursion { get; set; }
        public bool IceField { get; set; }
		public double Security { get; set; }
        public Boolean UniHole {  get; set; }

        private DateTime AlertTime { get; set; }
		private int? NPCKillsDeltaLastHour { get; set; }
		private Regex _nameRegex;
		private Regex NameRegex => this._nameRegex;
		private List<AlertQueueItem> Alerts { get; set; }
		public int? CountAlerts(bool faction, bool decloak, bool kill)
		{
			int? count = 0;
			if (Alerts == null) { return null;}
			count = Alerts.Count(o =>
				o.isFaction == faction &&
				o.isDecloak == decloak &&
				((o.Kill == null && !kill) || (o.Kill != null && kill))); 
            if (count == 0) return null;
			return count;
		}
		public List<AlertQueueItem> GetAlerts(bool faction, bool decloak, bool kill)
		{
			if (Alerts == null) Alerts = new List<AlertQueueItem>();
			return Alerts.Where(o => o.isFaction == faction && o.isDecloak == decloak && (o.Kill is null && !kill || (!(o.Kill is null) && kill))).ToList();
		}
        public List<ZKillKill> GetKills()
        {
            if (Kills == null) { Kills = new List<ZKillKill>(); }
            return Kills;
        }
		public void AddAlert(AlertQueueItem a)
        {
			if (Alerts == null) Alerts = new List<AlertQueueItem>();
			Alerts.Add(a); 
        }
		public void RemoveAlert(AlertQueueItem a)
		{
            if (Alerts == null) return;
			Alerts.Remove(a);
		}
		private Vector3 Xyz { get; set; }
		private List<ZKillKill> Kills { get; set; }
        public int? CountKills()
        {
            if (Kills == null) return null;
            return Kills.Count();
		}
        public void AddKill(ZKillKill kill)
        {
            if ( Kills == null ) {  Kills = new List<ZKillKill>();}
            Kills.Add(kill);
        }
        public void RemoveKill(ZKillKill kill)
        {
            if (Kills == null) return;
            Kills.Remove(kill);
        }
        public int? GetNPCKillsDeltaLastHour()
        {
            return NPCKillsDeltaLastHour;
		}
        public void SetNPCKillsDeltaLastHour(int? kills)
        {
			NPCKillsDeltaLastHour = kills;

		}
        public void SetAlertTime(DateTime alertTime)
        {
            AlertTime = alertTime;
        }
		public void SetNameRegex()
        {
            string str = "";
            foreach (char c in Name)
                str = !char.IsLetter(c) ? str + c.ToString() : str + "[" + c.ToString().ToLower() + c.ToString().ToUpper() + "]";
            _nameRegex = new Regex("\\b" + str + "\\b", RegexOptions.Compiled);
        }
    }
}
