using System;
using System.Linq;
using System.Runtime.Remoting.Metadata;
using System.Threading.Tasks;
using EVEStandard;
using EVEStandard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quesadilla;
using Quesadilla.Classes.ZKill;
using Serilog.Core;

namespace Quesadilla.Controllers
{
    public class OBSController : Controller
    {

        [Route("[controller]")]

        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Welcome To Quesadilla");
        }
        [Route("[controller]/zkillfeed")]
        public ActionResult zkillfeed(int? limit)
        {
            string zkillfeed = "<meta http-equiv=\"refresh\" content=\"3\">\r\n";
            int count = 0;
            if (Program.q != null && Program.q.OBSZKills != null && Program.q.OBSZKills.Count > 0)
            {
                zkillfeed += "<div class=zkill>";
                foreach (ZKillKill k in Program.q.OBSZKills.Reverse())
                {
                    if (limit != null && count++ >= limit)
                    {
                        break;
                    }
                    var zkillVictim = Program.q._universe.GetCharacterName((int)k.VictimCharacterId);
                    string kf = $"<div class=zkillkill><div class=zkill-ship><a href=https://zkillboard.com/kill/{k.ID}><img class=\"rounded-corners\" width=98% src=https://images.evetech.net/types/{k.VictimShipTypeId}/render></a>" +
                "</div>" +
                $"<div class=zkill-value>{k.TotalValue}</div>" +
                $"<div class=zkill-victim>{zkillVictim}</div>" +
                "</div>";
                    zkillfeed += kf;
                }
                zkillfeed += "</div>";
            }

            zkillfeed += @"<style>
.zkillkill {
  display: inline-grid;
  position: relative;
  background-color: transparent;
  width:100px
}
.zkill-ship {
  width:100px;
}
.zkill-victim {
  color: Gold;
  position: absolute;
  bottom: 7px;
  font-size: 11px;
  text-align: center; width: 100%;
}
.zkill-value {
  color: LimeGreen;
  position: absolute;
  top: 3px;
  left: 3px;
  font-size: 18px;
}
img.rounded-corners {
  border-radius: 10%;
}
.zkill {
  border: 3px solid transparent;
  border-radius: 15px;
  outline: 2px solid Gold;

  display: inline-block;
  position: relative;
}
</style>";

            return new ContentResult
            {
                ContentType = "text/html",
                Content = zkillfeed,
            };
        }
    }
}
