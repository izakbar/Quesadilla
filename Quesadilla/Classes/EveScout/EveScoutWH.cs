using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Quesadilla.Classes.EveScout
{
    public class EveScoutWH
    {
        /*
        {
            "id": "4424",
            "created_at": "2024-01-24T18:29:09.000Z",
            "created_by_id": 2116384429,
            "created_by_name": "Azamex",
            "updated_at": "2024-01-24T18:31:26.000Z",
            "updated_by_id": 2116384429,
            "updated_by_name": "Azamex",
            "completed_at": "2024-01-24T18:31:26.000Z",
            "completed_by_id": 2116384429,
            "completed_by_name": "Azamex",
            "completed": true,
            "wh_exits_outward": false,
            "wh_type": "F135",
            "max_ship_size": "large",
            "expires_at": "2024-01-25T11:29:09.000Z",
            "remaining_hours": 0,
            "signature_type": "wormhole",
            "out_system_id": 31000005,
            "out_system_name": "Thera",
            "out_signature": "OEA-078",
            "in_system_id": 31002253,
            "in_system_class": "c5",
            "in_system_name": "J100109",
            "in_region_id": 11000028,
            "in_region_name": "E-R00028",
            "in_signature": "SPP-328"
        }
         */
        [JsonPropertyName("id")]
        public string Id {  get; set; }
        [JsonPropertyName("created_at")]
        public DateTime CreatedTime { get; set; }
        [JsonPropertyName("wh_exits_outward")]
        public bool Outward { get; set; }
        [JsonPropertyName("wh_type")]
        public string WormholeType { get; set; }
        [JsonPropertyName("max_ship_size")]
        public string MaxShipSize { get; set; }
        [JsonPropertyName("out_system_id")]
        public long FromSystemId { get; set; }
        [JsonPropertyName("out_signature")]
        public string FromSignature { get; set; }
        [JsonPropertyName("in_system_id")]
        public long ToSystemId { get; set; }
        [JsonPropertyName("in_system_class")]
        public string ToSystemClass { get; set; }
        [JsonPropertyName("in_signature")]
        public string ToSignature { get; set; }

        public string Connector {  get; set; }
    }
}
