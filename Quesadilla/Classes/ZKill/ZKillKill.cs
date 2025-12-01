using Newtonsoft.Json;
using Quesadilla.Classes.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.ZKill
{
    public class ZKillKill : IEquatable<ZKillKill>
    {
        public long ID { get; set; }
        public System.DateTimeOffset Timestamp { get; set; }
        public System.DateTime TimeAdded { get; set; }
        public bool Expired { get; set; }
        public long SolarSystemId { get; set; }

        public string SolarSystemName { get; set; }
        public string RegionName { get; set; }

        public long? VictimAllianceId { get; set; }
        public long? VictimCharacterId { get; set; }
        public string VictimCharacterName { get; set; }
        public long? VictimCorporationId { get; set; }
        public long? VictimShipTypeId { get; set; }
        public string VictimShipName { get; set; }
        public bool NPC {  get; set; }
        public long? KillerCharacterId { get; set; }
        public long? KillerCorporationId { get; set; }
        public long? KillerAllianceId { get; set; }
        public long? KillerShipTypeId { get; set; }
        public long? KillerWeaponTypeId { get; set; }

        public string TotalValue { get; set; }

        public string href { get; set; }
        public string Context {  get; set; }
        public bool Alert { get; set; }
        public string ToFeedHtml()
        {
            return 
            "<div class=zkill><div class=zkill-ship><a href=https://zkillboard.com/kill/" + ID + "><img width=98% src=https://images.evetech.net/types/" + VictimShipTypeId.ToString() + "/render></a>" +
                "</div>" +
                "<div class=zkill-value>" + TotalValue + "</div>" +
                "<div class=zkill-victim>{zkillVictim}</div>" +
                "</div>";
        }

        public Guid ZKillGUID { get; set; }
        public ZKillKill()
        {
            ZKillGUID = Guid.NewGuid();
        }

        public bool Equals(ZKillKill other)
        {
            return this.ZKillGUID.Equals(other.ZKillGUID);
        }
    }
}
