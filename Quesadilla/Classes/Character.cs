using Quesadilla.Classes.EveScout;
using Quesadilla.Classes.SolarSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes
{
    public class Character
    {
        public string CharacterName {  get; set; }
        public string Location { get; set; }
        public int LocationId { get; set; }
        public bool IsClientActive { get; set; }
        public bool IsCurrentlActiveClient { get; set; }
        public DateTime LastFactionAlert { get; set; }

        public EveScoutWH ClosestTurnur { get; set; }
        public PathInfo ClosestTurnurPath { get; set; }
        public EveScoutWH ClosestThera { get; set; }
        public PathInfo ClosestTheraPath { get; set; }
        public bool InAbyss {  get; set; }

        public Character()
        {
            LastFactionAlert = DateTime.Now;
        }
    }
}
