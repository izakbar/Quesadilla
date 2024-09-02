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
        public ActionResult zkillfeed(int? limit = 10, bool bigbois = false, string feedtype = "all")
        {
            string output = "<meta http-equiv=\"refresh\" content=\"3\">\r\n";
            int count = 0;

            string ZKillOutput = "";
            if (Program.ZKillHandler.ZKillKills != null && Program.ZKillHandler.ZKillKills.Count > 0)
            {
                ZKillOutput += "<div class=zkill>";

                foreach (ZKillKill k in Program.ZKillHandler.ZKillKills.Reverse() )
                {
                    // check this matches criteria 
                    if (feedtype.Equals("alerts") && !k.Alert) continue;
                    if (bigbois && !k.TotalValue.Contains("b")) continue;

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
                    ZKillOutput += kf;
                }
                ZKillOutput += "</div>";
            }

            if (count > 0) output += ZKillOutput;

            var css = Program.MainConfiguration.CSSData.Where(o => o.Name == "OBS.ZkillList").First().CSS;
            output += $"<style>{css}</style>";

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

                var css = Program.MainConfiguration.CSSData.Where(o => o.Name == "OBS.LatestAlert").First().CSS;
                output += $"<style>{css}</style>";
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
                string isk = Program.GetIskLastHour(toon);
                if (!isk.IsNullOrEmpty())
                {
                    output += $"<div class=iskcontainer><div class=iskperhour>{isk}</div></div>";

                    var css = Program.MainConfiguration.CSSData.Where(o => o.Name == "OBS.ISK").First().CSS;
                    output += $"<style>{css}</style>";
                }
            }
            return new ContentResult
            {
                ContentType = "text/html",
                Content = output,
            };
        }
        [HttpGet("[controller]/miningPerHour")]
        public IActionResult miningPerHour(string toon)
        {
            string output = "<meta http-equiv=\"refresh\" content=\"10\">\r\n";
            if (Program.q != null)
            {
                string mining = Program.GetMiningLastHour(toon);
                if (!mining.IsNullOrEmpty())
                {
                    output += $"<div class=miningcontainer><div class=miningperhour>{mining}</div></div>";

                    var css = Program.MainConfiguration.CSSData.Where(o => o.Name == "OBS.Mining").First().CSS;
                    output += $"<style>{css}</style>";
                }
            }
            return new ContentResult
            {
                ContentType = "text/html",
                Content = output,
            };
        }
        [HttpGet("[controller]/miningPerHourTable")]
        public IActionResult miningPerHourTable(string toon, bool totalsOnly, bool hideWastage = false)
        {
            string output = "<meta http-equiv=\"refresh\" content=\"10\">\r\n";
            if (Program.q != null)
            {
                var mining = Program.GetMiningLastHourDetail(toon, totalsOnly);
                if (!mining.IsNullOrEmpty())
                {
                    output += $"<div class=miningcontainer><div class=miningperhour>";

                    foreach (var m in mining) {
                        output += $"<div class=MiningRow><div class=miningCharacter>{m.Key}</div></div>";
                        foreach (var o in m.Value)
                        {
                            output += $"<div class=miningRow><div class=miningItem>{o.MiningItem}</div><div class=miningAmount>{o.MiningAmount}</div>";
                            if ( hideWastage == false ) output += $"<div class=miningWastage>{o.WastageAmount}</div>";
                            output += "</div>";
                        }
                        //< div class=miningperhour>{mining}</div>
                    }
                    
                    output += "</div></div>";

                    var css = Program.MainConfiguration.CSSData.Where(o => o.Name == "OBS.MiningTable").First().CSS;
                    output += $"<style>{css}</style>";
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
