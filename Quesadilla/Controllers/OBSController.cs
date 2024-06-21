using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Metadata;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Web.WebPages;
using EVEStandard;
using EVEStandard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.IdentityModel.Tokens;
using Quesadilla;
using Quesadilla.Classes.ZKill;
using Serilog.Core;
using static System.Net.Mime.MediaTypeNames;

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
            string output = "<meta http-equiv=\"refresh\" content=\"3\">\r\n";
            int count = 0;
            if (Program.ZKillHandler.OBSZKills != null && Program.ZKillHandler.OBSZKills.Count > 0)
            {
                output += "<div class=zkill>";
                foreach (ZKillKill k in Program.ZKillHandler.OBSZKills.Reverse())
                {
                    if (limit != null && count++ >= limit)
                    {
                        break;
                    }
                    var zkillVictim = Program.Universe.GetCharacterName((int)k.VictimCharacterId);
                    string kf = $"<div class=zkillkill><div class=zkill-ship><a href=https://zkillboard.com/kill/{k.ID}><img class=\"rounded-corners\" width=98% src=https://images.evetech.net/types/{k.VictimShipTypeId}/render></a>" +
                "</div>" +
                $"<div class=zkill-value>{k.TotalValue}</div>" +
                $"<div class=zkill-victim>{zkillVictim}</div>" +
                "</div>";
                    output += kf;
                }
                output += "</div>";
            }

            output += @"<style>
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
                Content = output,
            };
        }
        [Route("[controller]/latestAlert")]
        public ActionResult latestAlert(int? seconds)
        {
            if (seconds is null) { seconds = Program.MainConfiguration.AlertPopupSeconds; }
            string output = "<meta http-equiv=\"refresh\" content=\"3\">\r\n";
            if (Program.q != null && Program.q.latestAlertBitmap != null)
            {
                TimeSpan timeAgo = DateTime.Now - Program.q.latestAlertTimestamp;
                if (timeAgo.Seconds > seconds)
                {
                    return new ContentResult
                    {
                        ContentType = "text/html",
                        Content = output,
                    };
                }

                output += "<img class=alert src=/OBS/latestAlertImage>";
                output += @"<style>
.alert {
  border: 3px solid transparent;
  border-radius: 15px;
  outline: 2px solid Gold;

  display: inline-block;
  position: relative;
}
</style>";
            }
            return new ContentResult
            {
                ContentType = "text/html",
                Content = output,
            };
        }

        [HttpGet("[controller]/latestAlertImage")]
        public IActionResult latestAlertImage()
        {
            if (Program.q != null && Program.q.latestAlertBitmap != null)
            {
                Byte[] b;
                b = ImageToByteArray(Program.q.latestAlertBitmap);
                return File(b, "image/jpeg");
            }
            return null;
        }
        [HttpGet("[controller]/iskPerHour")]
        public IActionResult iskPerHour(string toon)
        {
            string output = "<meta http-equiv=\"refresh\" content=\"10\">\r\n";
            if (Program.q != null)
            {
                string isk = Program.q.GetIskLastHour(toon);
                if (!isk.IsNullOrEmpty())
                {
                    output += $"<div class=iskcontainer><div class=iskperhour>{isk}</div></div>";

                    output += @"<style>
.iskcontainer {
  width: 100%;
  position: relative;
}
.iskperhour {
  border: 3px solid transparent;
  border-radius: 15px;
  outline: 2px solid Gold;

  display: inline-block;
  position: absolute;

  color: green;
  font-size: 24px;

  margin: auto;
}
</style>";
                }
            }
            return new ContentResult
            {
                ContentType = "text/html",
                Content = output,
            };                    
        }
        public byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }

    }
}
