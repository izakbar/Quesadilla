using System;
using System.Linq;
using System.Threading.Tasks;
using EVEStandard;
using EVEStandard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quesadilla;
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
            if (Program.q != null && Program.q.Kills != null)
            {
                foreach (var k in Program.q.Kills.Reverse())
                {
                    if (limit != null && count++ >= limit)
                    {
                        break;
                    }
                    var zkillVictim = Program.q._universe.GetCharacterName((int)k.VictimCharacterId);
                    string kf = $"<div class=zkill><div class=zkill-ship><a href=https://zkillboard.com/kill/{k.ID}><img width=98% src=https://images.evetech.net/types/{k.VictimShipTypeId}/render></a>" +
                "</div>" +
                $"<div class=zkill-value>{k.TotalValue}</div>" +
                $"<div class=zkill-victim>{zkillVictim}</div>" +
                "</div>";
                    zkillfeed += kf;
                }
            }

            zkillfeed += "<style>\r\n" +
                ".zkill {\r\n    display: inline-grid;\r\n   position: relative;\r\nwidth:100px\r\n}\r\n" +
                ".zkill-ship {\r\n  background-color:red\r\n  width:100px}\r\n" +
                ".zkill-victim {\r\n  color: Gold;\r\n  position: absolute;\r\n  bottom: 7px;\r\n  font-size: 11px;\r\n    text-align: center; width: 100%;\r\n   \r\n}\r\n" +
                ".zkill-value {\r\n  color: LimeGreen;\r\n  position: absolute;\r\n  top: 3px;\r\n  left: 3px;\r\nfont-size: 18px;\r\n}\r\n" +
                "</style>";

            return new ContentResult
            {
                ContentType = "text/html",
                Content = zkillfeed,
            };
        }
    }
}
