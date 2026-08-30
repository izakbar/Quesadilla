using Cyotek.Windows.Forms;
using Microsoft.AspNetCore.Mvc.Formatters.Json.Internal;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Quesadilla.Classes.Attributes;
using Quesadilla.Classes.Configuration;
using Quesadilla.Classes.SolarSystems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Quesadilla.Classes
{
    public class Config
    {
		public void ApplyConfiguration(Form form,List<ConfigurationItemLocationInformation> locations,int topAdjust,bool miningPerHourTotal,bool iskPerHourTotal,bool ignoreVisible)
		{
			if (form.InvokeRequired)
			{
				form.BeginInvoke((MethodInvoker)(() =>
					ApplyConfiguration(form,locations,topAdjust,miningPerHourTotal,iskPerHourTotal,ignoreVisible)));
				return;
			}

			foreach (ConfigurationItemLocationInformation item in locations)
			{
				var foundItem = form.Controls.Find(item.Name, true).FirstOrDefault();

				if (foundItem != null)
				{
					if (!foundItem.Name.Equals("ChatServerDown") && !ignoreVisible)
						foundItem.Visible = item.IsVisible;

					foundItem.Location =
						new Point(item.Location.X, item.Location.Y + topAdjust);

					foundItem.ForeColor = item.Color;
					foundItem.BackColor = item.BackColor;
					foundItem.Size = item.Size;
					foundItem.Font = item.Font;

					if (foundItem.Name.Equals("MiningData") && miningPerHourTotal)
						foundItem.ForeColor = item.AlternateColor;

					if (foundItem.Name.Equals("RattingData") && iskPerHourTotal)
						foundItem.ForeColor = item.AlternateColor;
				}
			}
		}
        public ConfigurationItemLocationInformation GetConfiguration(string name, List<ConfigurationItemLocationInformation> locations)
        {
            ConfigurationItemLocationInformation foundItem = locations.Find(o => o.Name == name);
            return foundItem;
		}

		private List<ConfigurationItemLocationInformation> DuplicateLocations(List<ConfigurationItemLocationInformation> locations)
		{
            List<ConfigurationItemLocationInformation> newItems = new List<ConfigurationItemLocationInformation>();
			foreach (var item in locations)
			{
				newItems.Add(new ConfigurationItemLocationInformation()
				{
					Name = item.Name,
					Location = new Point(item.Location.X, item.Location.Y),
					Color = item.Color,
					IsVisible = item.IsVisible,
					Size = new Size(item.Size.Width, item.Size.Height),
					Font = item.Font,
                    BackColor = item.BackColor,
                    AlternateColor = item.AlternateColor,
				});
			}
            return newItems;
		}

		public Config Duplicate()
        {
            Config c = new Config()
            {
                AlertPopupLocation = AlertPopupLocation,
                DPSLocation = DPSLocation,
                InitialPosition = InitialPosition,
                ZKillSound = ZKillSound,
                AlertPopupMinimapRawXY = AlertPopupMinimapRawXY,
                MinimapRawXY = MinimapRawXY,
            };

            // copy as many config items as possible
            foreach(var p in typeof(Config).GetProperties().Where(o => o.PropertyType == typeof(Color) || o.PropertyType == typeof(int) || o.PropertyType == typeof(string) || o.PropertyType == typeof(bool)))
            {
                var propertyType = typeof(Config).GetProperty(p.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.ExactBinding);
                var value = propertyType.GetValue(this);
                propertyType.SetValue(c, value);
            }

            c.ignoreStrings = new List<Regex>();
            c.ignoreSystems = new List<int>();
            c.AlertConfiguration = new List<AlertConfig>();
            c.Logfiles = new List<LogfileConfig>();
            c.CSSData = new List<CSSData>();

            foreach (var s in ignoreStrings)
            {
                c.ignoreStrings.Add(s);
            }
            foreach (var s in ignoreSystems)
            {
                c.ignoreSystems.Add(s);
            }
            foreach (var a in AlertConfiguration)
            {
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    AlertRangeType = a.AlertRangeType,
                    AlertType = a.AlertType,
                    CustomTriggerEverySeconds = a.CustomTriggerEverySeconds,
                    Enabled = a.Enabled,
                    FriendlyName = a.FriendlyName,
                    Jumps1Comparison = a.Jumps1Comparison,
                    MenuDisplay = a.MenuDisplay,
                    TextDisplay = a.TextDisplay,
                    Jumps1Number = a.Jumps1Number,
                    Jumps2Comparison = a.Jumps2Comparison,
                    SoundFile = a.SoundFile,
                    Jumps2Number = a.Jumps2Number,
                    TriggerText = a.TriggerText
                });
            }
            foreach (var l in Logfiles)
            {
                c.Logfiles.Add(l);
            }
            foreach (var css in CSSData)
            {
                c.CSSData.Add(css);
            }

            c.MainWindowItemLocations = DuplicateLocations(MainWindowItemLocations);
			c.AlertWindowItemLocations = DuplicateLocations(AlertWindowItemLocations);
            c.OverlayWindowItemLocations = DuplicateLocations(OverlayWindowItemLocations);

			c.InitialPosition = new Point(InitialPosition.X, InitialPosition.Y);
			c.MainWindowSize = new Size(MainWindowSize.Width, MainWindowSize.Height);
			c.AlertWindowSize = new Size(AlertWindowSize.Width, AlertWindowSize.Height);
			c.DPSGraphWindowSize = new Size(DPSGraphWindowSize.Width, DPSGraphWindowSize.Height);
            c.DPSGraphWindowSizeAdjust = new Point(DPSGraphWindowSizeAdjust.X, DPSGraphWindowSizeAdjust.Y);

            return (c);
        }


        public static Config Load(string filename)
        {
            Config c = null;

            try
            {
                StreamReader reader = new StreamReader(filename);
                var json = reader.ReadToEnd();
                reader.Close();
                c = JsonSerializer.Deserialize<Config>(json);
            }
            catch (Exception ex)
            {
            }

            if (c is null)
            {
                c = new Config();

                // Find/set default values from DefaultValue attribute
                foreach (PropertyDescriptor p in TypeDescriptor.GetProperties(c))
                {
                    var propertyType = typeof(Config).GetProperty(p.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.ExactBinding);
                    DefaultValueAttribute myAttribute = (DefaultValueAttribute)p.Attributes[typeof(DefaultValueAttribute)];
                    if ( myAttribute != null)
                    {
                        propertyType.SetValue(c, myAttribute.Value);
                    }
                }

                c.InitialPosition = new Point(0, 0);
				c.MainWindowSize = new Size(760, 404);
				c.AlertWindowSize = new Size(400, 180);
				c.DPSGraphWindowSize = new Size(250, 100);
                c.DPSGraphWindowSizeAdjust = new Point(10, 15);

				c.OverlayMapWindowSize = new Size(600, 350);
				c.OverlayMapWindowPosition = new Point(275, 1);

				c.AlertPopupLocation = PopupLocationType.BottomLeft;
                c.DPSLocation = PopupLocationType.TopRight;

                c.AlertPopZkillLogoSize = 120;
                c.AlertPopZkillHeight = 130;

				c.AlertPopupShowKillerLogo = true;
                c.AlertPopupShowVictimLogo = true;
                c.MinimapSecLimitValue = -0.7f;

                //c.MinimapSecLimitColour = Color.Aqua;
                //c.MinimapSecLimitAlpha = 10;
                //c.MinimapSecLimitCloudSize = 30;
                c.MinimapSecLimit = true;
			}

            if (c.ignoreSystems == null) c.ignoreSystems = new List<int>();
            if (c.ignoreStrings == null)
            {
                c.ignoreStrings = new List<Regex>();
                c.ignoreStrings.Add(new Regex("\\b" + "clr" + "\\b", RegexOptions.IgnoreCase | RegexOptions.Compiled));
                c.ignoreStrings.Add(new Regex("\\b" + "clear" + "\\b", RegexOptions.IgnoreCase | RegexOptions.Compiled));
                c.ignoreStrings.Add(new Regex("\\b" + "status" + "\\b", RegexOptions.IgnoreCase | RegexOptions.Compiled));
            }

            if (c.AlertConfiguration == null)
            {
                c.AlertConfiguration = new List<AlertConfig>();
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    FriendlyName = "All online toons <= 5 jumps",
                    Enabled = true,
                    AlertType = AlertConfigType.Range,
                    AlertRangeType = AlertConfigRangeType.AllOnlineCharacters,
                    TextDisplay = "{jumps}->{fromsystem} : {route}",
                    MenuDisplay = "{jumps} jumps from {character} in {tosystem}",
                    SoundFile = "xylophone",
                    Jumps1Comparison = AlertConfigJumpsComparison.LessThanEqual,
                    Jumps1Number = 5,
                    Jumps2Comparison = AlertConfigJumpsComparison.None,
                });
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    FriendlyName = "Izakbar (if online) <= 5 jumps",
                    Enabled = false,
                    AlertType = AlertConfigType.Range,
                    AlertRangeType = AlertConfigRangeType.OneOnlineCharacter,
                    TextDisplay = "{jumps}->{fromsystem} : {route}",
                    MenuDisplay = "{jumps} jumps from {character} in {tosystem}",
                    SoundFile = "xylophone",
                    Jumps1Comparison = AlertConfigJumpsComparison.LessThanEqual,
                    Jumps1Number = 5,
                    Jumps2Comparison = AlertConfigJumpsComparison.None,
                    TriggerText = "Izakbar",
                });
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    FriendlyName = "Home system < 5 jumps",
                    Enabled = false,
                    AlertType = AlertConfigType.Range,
                    AlertRangeType = AlertConfigRangeType.Home,
                    TextDisplay = "{jumps}->{fromsystem} : {route}",
                    MenuDisplay = "{jumps} jumps from {fromsystem} in {tosystem}",
                    SoundFile = "xylophone",
                    Jumps1Comparison = AlertConfigJumpsComparison.LessThanEqual,
                    Jumps1Number = 5,
                    Jumps2Comparison = AlertConfigJumpsComparison.None,
                });
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    FriendlyName = "1DQ1-A < 5 jumps",
                    Enabled = false,
                    AlertType = AlertConfigType.Range,
                    AlertRangeType = AlertConfigRangeType.System,
                    TextDisplay = "{jumps}->{fromsystem} : {route}",
                    MenuDisplay = "{jumps} jumps from {fromsystem} in {tosystem}",
                    SoundFile = "xylophone",
                    Jumps1Comparison = AlertConfigJumpsComparison.LessThanEqual,
                    Jumps1Number = 5,
                    Jumps2Comparison = AlertConfigJumpsComparison.None,
                    TriggerText = "1DQ1-A",
                });
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    FriendlyName = "ESS robbery",
                    Enabled = false,
                    AlertType = AlertConfigType.Text,
                    TextDisplay = "ESS robbery {text} reported by {reporter}",
                    MenuDisplay = "ESS robbery {text} reported by {reporter}",
                    SoundFile = "Boo2",
                    TriggerText = "ess",
                });
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    FriendlyName = "Skyhook robbery",
                    Enabled = false,
                    AlertType = AlertConfigType.Text,
                    TextDisplay = "Skyhook robbery {text} reported by {reporter}",
                    MenuDisplay = "Skyhook robbery {text} reported by {reporter}",
                    SoundFile = "Boo2",
                    TriggerText = "skyhook",
                });
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    FriendlyName = "Faction Spawn",
                    Enabled = true,
                    AlertType = AlertConfigType.FactionSpawn,
                    TextDisplay = "Spawn {context}",
                    MenuDisplay = "Spawn {context}",
                    SoundFile = "trumpet",
                    CustomTriggerEverySeconds = 30,
                });
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    FriendlyName = "Decloak",
                    Enabled = true,
                    AlertType = AlertConfigType.Decloak,
                    TextDisplay = "Decloak by {context}",
                    MenuDisplay = "Decloak by {context}",
                    SoundFile = "tuba",
                });
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    FriendlyName = "Out of rock",
                    Enabled = true,
                    AlertType = AlertConfigType.MiningEndedRoidPopped,
                    TextDisplay = "{character} rock depleated",
                    MenuDisplay = "rock depleated",
                    SoundFile = "pop1",
                });
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    FriendlyName = "Miner full",
                    Enabled = true,
                    AlertType = AlertConfigType.MiningEndedFull,
                    TextDisplay = "{character} cargo full",
                    MenuDisplay = "cargo full",
                    SoundFile = "full",
                });
            }

            if (c.Logfiles == null)
            {
                c.Logfiles = new List<LogfileConfig> { };
				/*
								c.Logfiles.Add(new LogfileConfig()
								{
									Enabled = true,
									FriendlyName = "Tenerifis/Immensea/Catch/Impass (CONDI Sov)",
									Logfile = "east.imperium",
									LogType = Logfile.LogFileType.ChatIntel,
								});
								c.Logfiles.Add(new LogfileConfig()
								{
									Enabled = true,
									FriendlyName = "Delve/Querious/Period Basis",
									Logfile = "west.imperium",
									LogType = Logfile.LogFileType.ChatIntel,
								});
								c.Logfiles.Add(new LogfileConfig()
								{
									Enabled = true,
									FriendlyName = "Paragon Soul, Esoteria, Feythabolis",
									Logfile = "southeast.imperium",
									LogType = Logfile.LogFileType.ChatIntel,
								});
								c.Logfiles.Add(new LogfileConfig()
								{
									Enabled = true,
									FriendlyName = "LS Aridia Intel",
									Logfile = "aridia.imperium",
									LogType = Logfile.LogFileType.ChatIntel,
								});
								c.Logfiles.Add(new LogfileConfig()
								{
									Enabled = true,
									FriendlyName = "LS Khanid Intel",
									Logfile = "khanid.imperium",
									LogType = Logfile.LogFileType.ChatIntel,
								});
								c.Logfiles.Add(new LogfileConfig()
								{
									Enabled = true,
									FriendlyName = "Pochven Intel",
									Logfile = "triangle.imperium",
									LogType = Logfile.LogFileType.ChatIntel,
								});
								c.Logfiles.Add(new LogfileConfig()
								{
									Enabled = true,
									FriendlyName = "Fountain Intel",
									Logfile = "ftn.imperium",
									LogType = Logfile.LogFileType.ChatIntel,
								});
								c.Logfiles.Add(new LogfileConfig()
								{
									Enabled = true,
									FriendlyName = "Curse Intel",
									Logfile = "curse.imperium ",
									LogType = Logfile.LogFileType.ChatIntel,
								});
								c.Logfiles.Add(new LogfileConfig()
								{
									Enabled = true,
									FriendlyName = "Querious/Catch Intel",
									Logfile = "Quer_Catch.imperium",
									LogType = Logfile.LogFileType.ChatIntel,
								});

				c.Logfiles.Add(new LogfileConfig()
				{
					Enabled = true,
					FriendlyName = "Fountain Intel",
					Logfile = "I. Ftn Intel",
					LogType = Logfile.LogFileType.ChatIntel,
				});
				c.Logfiles.Add(new LogfileConfig()
				{
					Enabled = true,
					FriendlyName = "Cloud Ring Intel",
					Logfile = "I. C Ring Intel",
					LogType = Logfile.LogFileType.ChatIntel,
				});
				c.Logfiles.Add(new LogfileConfig()
				{
					Enabled = true,
					FriendlyName = "Aridia Intel",
					Logfile = "I. Aridia Intel",
					LogType = Logfile.LogFileType.ChatIntel,
				});

				c.Logfiles.Add(new LogfileConfig()
				{
					Enabled = true,
					FriendlyName = "Delve and Querious Intel",
					Logfile = "I. Delve & Q Intel",
					LogType = Logfile.LogFileType.ChatIntel,
				});

                c.Logfiles.Add(new LogfileConfig()
				{
					Enabled = true,
					FriendlyName = "Curse Intel",
					Logfile = "I. Curse Intel",
					LogType = Logfile.LogFileType.ChatIntel,
				});
				c.Logfiles.Add(new LogfileConfig()
				{
					Enabled = true,
					FriendlyName = "OuterRing Intel",
					Logfile = "I. OR Intel",
					LogType = Logfile.LogFileType.ChatIntel,
				});
				c.Logfiles.Add(new LogfileConfig()
				{
					Enabled = true,
					FriendlyName = "Pochven Intel",
					Logfile = "I. Poch Intel",
					LogType = Logfile.LogFileType.ChatIntel,
				});


				c.Logfiles.Add(new LogfileConfig()
                {
                    Enabled = true,
                    FriendlyName = "Izakbar DEBUG",
                    Logfile = "izakbar.debug",
                    LogType = Logfile.LogFileType.ChatIntel,
                });

				*/

				c.Logfiles.Add(new LogfileConfig()
				{
					Enabled = true,
					FriendlyName = "wc.Dek+Fa+PB",
					Logfile = "wc.Dek+Fa+PB",
					LogType = Logfile.LogFileType.ChatIntel,
				});
				c.Logfiles.Add(new LogfileConfig()
				{
					Enabled = true,
					FriendlyName = "wc.Poch",
					Logfile = "wc.Poch",
					LogType = Logfile.LogFileType.ChatIntel,
				});
				c.Logfiles.Add(new LogfileConfig()
				{
					Enabled = true,
					FriendlyName = "wc.Vale",
					Logfile = "wc.Vale",
					LogType = Logfile.LogFileType.ChatIntel,
				});
				c.Logfiles.Add(new LogfileConfig()
				{
					Enabled = true,
					FriendlyName = "wc.Venal+Br+Te",
					Logfile = "wc.Venal+Br+Te",
					LogType = Logfile.LogFileType.ChatIntel,
				});
			}

			if (c.CSSData== null)
            {
                c.CSSData = new List<CSSData> { };
                c.CSSData.Add(new CSSData()
                {
                    Name = "OBS.MiningTable",
                    FriendlyName = "OBS Mining Table",
                    CSS = @"
@import url(https://fonts.googleapis.com/css?family=Roboto+Condensed);

.miningcontainer {
  width: 100%;
  position: relative;
  font-family: 'Roboto Condensed';
}
.miningperhour {
  border: 4px solid transparent;
  border-radius: 10px;
  background-color: #0a0e15;
  outline: 1px solid steelblue;

  display: inline-block;
  position: absolute;

  color: green;
  font-size: 24px;

  margin: auto;
}
.miningTable {
        display:  table;
        width:auto;
        background-color:#eee;
        border:1px solid  #666666;
        border-spacing:5px;/*cellspacing:poor IE support for  this*/
       /* border-collapse:separate;*/
}
.miningRow {
       display:table-row;
       width:auto;
 }

.miningItem {
        float:left;
        display:table-column;
        width:350px;
        color: CornflowerBlue;
        padding-left:30px;
 }
.miningCharacter {
        float:left;
        display:table-column;
        width:100%;
        padding-left:5px;
 }
.miningAmount {
        float:left;
        display:table-column;
        width:70px;
        text-align: right;
        padding-right:5px;
 }
.miningWastage {
        float:left;
        display:table-column;
        width:70px;
        color: Red;
        text-align: right;
        padding-right:5px;
 }
",
                });
                c.CSSData.Add(new CSSData()
                {
                    Name = "OBS.Mining",
                    FriendlyName = "OBS Mining Info",
                    CSS = @"

@import url(https://fonts.googleapis.com/css?family=Roboto+Condensed);

.miningcontainer {
  width: 100%;
  position: relative;
}
.miningperhour {
  border: 4px solid transparent;
  border-radius: 10px;
  background-color: #0a0e15;
  outline: 1px solid steelblue;

  display: inline-block;
  position: absolute;

  color: green;
  font-family: 'Roboto Condensed';
  font-size: 24px;

  margin: auto;
}
",
                });
                c.CSSData.Add(new CSSData()
                {
                    Name = "OBS.ISK",
                    FriendlyName = "OBS ISK Value",
                    CSS = @"
@import url(https://fonts.googleapis.com/css?family=Titan+One);

.iskcontainer {
  width: 100%;
  position: relative;
}
.iskperhour {
  border: 4px solid transparent;
  border-radius: 10px;
  background-color: #0a0e15;
  outline: 1px solid steelblue;

  display: inline-block;
  position: absolute;

  color: green;
  font-family: 'Titan One';
  font-size: 24px;

  margin: auto;
}",
                });
                c.CSSData.Add(new CSSData()
                {
                    Name = "OBS.LatestAlert",
                    FriendlyName = "OBS Latest Alert",
                    CSS = @"
.alert {
  border: 2px solid transparent;
  border-radius: 15px;
  outline: 2px solid steelblue;

  display: inline-block;
  position: relative;
}
",
                });
                c.CSSData.Add(new CSSData()
                {
                    Name = "OBS.ZkillList",
                    FriendlyName = "OBS Zkill Listing",
                    CSS = @"
@import url(https://fonts.googleapis.com/css?family=Roboto+Condensed);

img.rounded-corners {
  border-radius: 50%;
    border: 1px solid red;
}
.zkill {
  background-color: #0a0e15;
    border: 3px solid transparent;
  border-radius: 15px;
  outline: 2px solid steelblue;
  display: inline-block;
  position: relative;
}
.zkillkill {
  display: block;
  position: relative;
  background-color: #0a0e15;
  width:240px;
  height: 90px;
}

.zkillkill:not(:last-child) {
  border-bottom: 2px solid steelblue;
}


.zkill-ship {
  width:80px;
  top: 6px;
  position: absolute;
}
.zkill-victim {
  color: cadetblue;
  position: absolute;
  top: 35px;
  left: 85px;
  font-family: 'Roboto Condensed';
  font-size: 15px;
  text-align: left; width: 65%;
}
.zkill-shipname {
  color: lightgray;
  position: absolute;
  top: 50px;
  left: 85px;
  font-family: 'Roboto Condensed';
  font-size: 13px;
  text-align: left; width: 65%;
}
.zkill-value {
  color: white;
  background-color: LimeGreen;
  position: absolute;
  top: 65px;
  left: 50px;
  font-family: 'Roboto Condensed';
  font-size: 18px;
  border-radius: 20%;
}

.zkill-timestamp {
  color: lightgoldenrodyellow;
  position: absolute;
  top: 2px;
  left: 5px;
  font-family: 'Roboto Condensed';
  font-size: 15px;
  border-radius: 20%;
}
.zkill-killer {
    visibility: hidden;
}
.zkill-region {
  color: white;
  position: absolute;
  top: 5px;
  left: 83px;
  text-align: right; width: 65%;
  font-family: 'Roboto Condensed';
  font-size: 15px;
}
.zkill-system {
  color: orange;
  position: absolute;
  top: 22px;
  left: 83px;
  text-align: right; width: 65%;
  font-family: 'Roboto Condensed';
  font-size: 12px;
}

.zkill-killer-logo
{
    visibility: hidden;
}

.zkill-victim-logo
{
    visibility: hidden;
}

.zkill-victim-alliance 
{
    visibility: hidden;
}
.zkill-victim-corporation
{
    visibility: hidden;
}",
                });
            }


            if (c.MainWindowItemLocations == null)
            {
                c.MainWindowItemLocations = new List<ConfigurationItemLocationInformation>();
                c.MainWindowItemLocations.Add(new ConfigurationItemLocationInformation()
                {
                    Name = "WorkingIndicator",
                    Color = Color.Red,
					BackColor = Color.Transparent,
					IsVisible = true,
                    Location = new Point(625, -1),
                    Size = new Size(13, 15),
                    Font = new Font("Microsoft Sans Serif", 12.0f, FontStyle.Bold),
                });
                c.MainWindowItemLocations.Add(new ConfigurationItemLocationInformation()
                {
                    Name = "CurrentClient",
                    Color = Color.MediumTurquoise,
					BackColor = Color.Transparent,
					IsVisible = true,
                    Location = new Point(2, 2),
                    Size = new Size(265, 21),
                    Font = new Font("Segoe UI", 12.0f, FontStyle.Bold),
                });
                c.MainWindowItemLocations.Add(new ConfigurationItemLocationInformation()
                {
                    Name = "EveScoutStatus",
                    Color = Color.LimeGreen,
					BackColor = Color.Transparent,
					IsVisible = true,
                    Location = new Point(0, 218),
                    Size = new Size(163, 13),
                    Font = new Font("Segoe UI", 8.25f, FontStyle.Regular),
                });

                c.MainWindowItemLocations.Add(new ConfigurationItemLocationInformation()
                {
                    Name = "AlertsOutstanding",
                    Color = Color.LightCoral,
					BackColor = Color.Transparent,
					IsVisible = true,
                    Location = new Point(240, 3),
                    Size = new Size(114, 11),
                    Font = new Font("Segoe UI", 8.25f, FontStyle.Regular),
                });

                c.MainWindowItemLocations.Add(new ConfigurationItemLocationInformation()
                {
                    Name = "MiningData",
                    Color = Color.Aqua,
					BackColor = Color.Transparent,
					AlternateColor = Color.Orange,
                    IsVisible = true,
                    Location = new Point(200, 3),
                    Size = new Size(118, 11),
                    Font = new Font("Segoe UI", 8.25f, FontStyle.Bold),
                });

                c.MainWindowItemLocations.Add(new ConfigurationItemLocationInformation()
                {
                    Name = "RattingData",
                    Color = Color.Aqua,
                    AlternateColor = Color.Orange,
					BackColor = Color.Transparent,
					IsVisible = true,
                    Location = new Point(350, 3),
                    Size = new Size(118, 11),
                    Font = new Font("Segoe UI", 8.25f, FontStyle.Bold),
                });

                c.MainWindowItemLocations.Add(new ConfigurationItemLocationInformation()
                {
                    Name = "MainMinimap",
                    Color = Color.White,
					BackColor = Color.Transparent,
					AlternateColor = Color.Red,
                    IsVisible = true,
                    Location = new Point(3, 3),
                    Size = new Size(260, 200),
                    Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular),
                });

				c.MainWindowItemLocations.Add(new ConfigurationItemLocationInformation()
				{
					Name = "MouseoverSystemName",
					Color = Color.Orchid,
					BackColor = Color.Transparent,
					AlternateColor = Color.Red,
					IsVisible = true,
					Location = new Point(445, 203),
					Size = new Size(193, 11),
					Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular),
				});

				c.MainWindowItemLocations.Add(new ConfigurationItemLocationInformation()
				{
					Name = "ExtraInformation",
					Color = Color.LimeGreen,
					BackColor = Color.Transparent,
					AlternateColor = Color.Brown,
					IsVisible = true,
					Location = new Point(270, 18),
					Size = new Size(360, 170),
					Font = new Font("Segoe UI Variable", 8.25f, FontStyle.Regular),
				});

				c.MainWindowItemLocations.Add(new ConfigurationItemLocationInformation()
				{
					Name = "ChatServerDown",
					Color = Color.Red,
					BackColor = Color.Transparent,
					AlternateColor = Color.Brown,
					IsVisible = true,
					Location = new Point(471, 3),
					Size = new Size(152, 20),
					Font = new Font("Microsoft Sans Serif", 12.0f, FontStyle.Bold),
				});

                c.MainWindowItemLocations.Add(new ConfigurationItemLocationInformation()
                {
                    Name = "IntelLog",
                    Color = Color.LightGoldenrodYellow,
                    BackColor = System.Drawing.ColorTranslator.FromHtml("#0f0f11"),
					AlternateColor = Color.Brown,		
                    IsVisible = true,
                    Location = new Point(471, 3),
                    Size = new Size(152, 20),
                    Font = new Font("Segoe UI Variable", 8.25f, FontStyle.Regular),
                });
			}

            if (c.AlertWindowItemLocations == null)
            {
                c.AlertWindowItemLocations = new List<ConfigurationItemLocationInformation>();
				c.AlertWindowItemLocations.Add(new ConfigurationItemLocationInformation()
				{
					Name = "AlertText",
					Color = Color.White,
					BackColor = Color.Transparent,
					IsVisible = true,
					Location = new Point(0, 165),
					Size = new Size(400, 12),
					Font = new Font("Tahoma", 8.25f, FontStyle.Regular),
				});
				c.AlertWindowItemLocations.Add(new ConfigurationItemLocationInformation()
				{
					Name = "SystemName",
					Color = Color.Lime,
					BackColor = Color.Transparent,
					IsVisible = true,
					Location = new Point(115, 4),
					Size = new Size(280, 31),
					Font = new Font("Segoe UI Variable", 20.0f, FontStyle.Bold),
				});
				c.AlertWindowItemLocations.Add(new ConfigurationItemLocationInformation()
				{
					Name = "RegionName",
					Color = Color.Cyan,
					BackColor = Color.Transparent,
					IsVisible = true,
					Location = new Point(275, 36),
					Size = new Size(120, 13),
					Font = new Font("Segoe UI Variable", 8.0f, FontStyle.Bold),
				});
				c.AlertWindowItemLocations.Add(new ConfigurationItemLocationInformation()
				{
					Name = "ContextInfo",
					Color = Color.Yellow,
					BackColor = Color.Transparent,
					IsVisible = true,
					Location = new Point(2,47),
					Size = new Size(396, 19),
					Font = new Font("Verdana", 10.0f, FontStyle.Bold),
				});
				c.AlertWindowItemLocations.Add(new ConfigurationItemLocationInformation()
				{
					Name = "IntelLine",
					Color = Color.White,
					BackColor = Color.Transparent,
					IsVisible = true,
					Location = new Point(155,64),
					Size = new Size(240, 57),
					Font = new Font("Tahoma", 9.0f, FontStyle.Regular),
				});
				c.AlertWindowItemLocations.Add(new ConfigurationItemLocationInformation()
				{
					Name = "ShipName",
					Color = Color.Lime,
					BackColor = Color.Transparent,
					IsVisible = true,
					Location = new Point(180, 125),
					Size = new Size(150, 13),
					Font = new Font("Segoe UI Variable", 8.25f, FontStyle.Bold),
				});
				c.AlertWindowItemLocations.Add(new ConfigurationItemLocationInformation()
				{
					Name = "AlertImage",
					Color = Color.White,
					BackColor = Color.Transparent,
					IsVisible = true,
					Location = new Point(335, 95),
					Size = new Size(53, 57),
					Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold),
				});
				c.AlertWindowItemLocations.Add(new ConfigurationItemLocationInformation()
				{
					Name = "MinimapImage",
					Color = Color.White,
					BackColor = Color.Transparent,
					IsVisible = true,
					Location = new Point(34, 3),
					Size = new Size(193, 128),
					Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold),
				});
				c.AlertWindowItemLocations.Add(new ConfigurationItemLocationInformation()
				{
					Name = "AlertLogoKiller",
					Color = Color.White,
					BackColor = Color.Transparent,
					IsVisible = true,
					Location = new Point(437, 138),
					Size = new Size(34, 34),
					Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold),
				});
				c.AlertWindowItemLocations.Add(new ConfigurationItemLocationInformation()
				{
					Name = "AlertLogoVictim",
					Color = Color.White,
					BackColor = Color.Transparent,
					IsVisible = true,
					Location = new Point(516, 122),
					Size = new Size(34, 34),
					Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold),
				});
			}

			if (c.OverlayWindowItemLocations == null)
			{
				c.OverlayWindowItemLocations = new List<ConfigurationItemLocationInformation>();
				c.OverlayWindowItemLocations.Add(new ConfigurationItemLocationInformation()
				{
					Name = "RattingData",
					Color = Color.Aqua,
					AlternateColor = Color.Orange,
					BackColor = Color.Transparent,
					IsVisible = true,
					Location = new Point(440, 12),
					Size = new Size(118, 15),
					Font = new Font("Impact", 10f, FontStyle.Regular),
				});
				c.OverlayWindowItemLocations.Add(new ConfigurationItemLocationInformation()
				{
					Name = "MiningData",
					Color = Color.Aqua,
					BackColor = Color.Transparent,
					AlternateColor = Color.Orange,
					IsVisible = true,
					Location = new Point(300, 323),
					Size = new Size(118, 15),
					Font = new Font("Impact", 10f, FontStyle.Regular),
				});
				c.OverlayWindowItemLocations.Add(new ConfigurationItemLocationInformation()
				{
					Name = "EveScoutStatus",
                    Color = Color.LimeGreen,
					BackColor = Color.Transparent,
					IsVisible = true,
                    Location = new Point(1, 1),
                    Size = new Size(640, 13),
                    Font = new Font("Segoe UI Variable", 9f, FontStyle.Regular),
                });

				c.OverlayWindowItemLocations.Add(new ConfigurationItemLocationInformation()
				{
					Name = "Minimap",
					Color = Color.White,
					BackColor = Color.Transparent,
					AlternateColor = Color.Red,
					IsVisible = true,
					Location = new Point(0,0),
					Size = new Size(500, 350),
					Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular),
				});

				c.OverlayWindowItemLocations.Add(new ConfigurationItemLocationInformation()
				{
					Name = "IskPerMinuteChart",
					Color = Color.Orchid,
					BackColor = Color.Transparent,
					AlternateColor = Color.Red,
					IsVisible = true,
					Location = new Point(320, 32),
					Size = new Size(280, 80),
					Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular),
				});
				c.OverlayWindowItemLocations.Add(new ConfigurationItemLocationInformation()
				{
					Name = "MinedPerMinuteChart",
					Color = Color.Orchid,
					BackColor = Color.Transparent,
					AlternateColor = Color.Red,
					IsVisible = true,
					Location = new Point(320, 270),
					Size = new Size(280, 80),
					Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular),
				});




			}

			return (c);
        }

        public void Save(string filename)
        {
            var json = JsonSerializer.Serialize(this);
            File.WriteAllText(filename, json);
        }

		[Category("General")]
		[DisplayName("Staging system")]
		[Description("When alerts are generated relative to your home/staging system - this is the value used")]
		[DefaultValue(typeof(string), "K7D-II")]
		public string HomeSystem { get; set; }

		[Category("General")]
		[DisplayName("Output Character Locations")]
		[Description("When character locations change - output to file")]
		[DefaultValue(typeof(Boolean), "False")]
		public Boolean OutputCharacterLocationsToFile { get; set; }

		[Category("General")]
        [DisplayName("Show ZKill alerts")]
        [Description("Show ZKill events close to your alert setup")]
        [DefaultValue(typeof(bool), "true")]
        public bool ShowZKillAlerts { get; set; }

        [Category("General")]
        [DisplayName("Too many jumps away")]
        [Description("How many jumps away should be considered too far to be involved with")]
        [DefaultValue(typeof(int), "12")]
        public int IgnoreAsTooManyJumps { get; set; }

        [Category("General")]
        [DisplayName("Show Last Alerts")]
        [Description("How many unexpired alerts to show in tray menu")]
        [DefaultValue(typeof(int), "7")]
        public int ShowLastUnexpiredAlerts { get; set; }

        [Category("Tick Timing")]
        [DisplayName("Milliseconds for main tick")]
        [ReadOnly(true)]
        [Description("How many milliseconds the main tick is - smaller means more occur")]
        [DefaultValue(typeof(int), "100")]
        public int TickMiliseconds { get; set; }

        [Category("Tick Timing")]
        [ReadOnly(true)]
        [DisplayName("HowManyTicksBeforeCleardown")]
        [Description("How many ticks will occur between event cleardown checks")]
        [DefaultValue(typeof(int), "600")]
        public int HowManyTicksBeforeCleardown { get; set; }

        [Category("Tick Timing")]
        [ReadOnly(true)]
        [DisplayName("HowManyTicksBeforeAlert")]
        [Description("After an alert is put into the new alert queue - how many ticks before display")]
        [DefaultValue(typeof(int), "4")]
        public int HowManyTicksBeforeAlert { get; set; }

        [Category("Tick Timing")]
        [ReadOnly(true)]
        [DisplayName("HowManyTicksBeforeDPSGraphUpdate")]
        [Description("How many ticks between DPS graph update")]
        [DefaultValue(typeof(int), "5")]
        public int HowManyTicksBeforeDPSGraphUpdate { get; set; }

		[Category("Tick Timing")]
		[ReadOnly(true)]
		[DisplayName("How often to update EveO Location")]
		[Description("How many ticks between EveO Location update if no location change")]
		[DefaultValue(typeof(int), "100")]
		public int HowManyTicksBeforeEveOLocationUpdate { get; set; }


		[Category("Tick Timing")]
        [ReadOnly(true)]
        [DisplayName("HowManyTicksBeforeLogfileCheck")]
        [Description("How many ticks between logfile checks")]
        [DefaultValue(typeof(int), "7")]
        public int HowManyTicksBeforeLogfileCheck { get; set; }

		[Category("Tick Timing")]
		[ReadOnly(true)]
		[DisplayName("HowManyTicksRefreshProcesses")]
		[Description("How many ticks before refreshing process list")]
		[DefaultValue(typeof(int), "20")]

		public int HowManyTicksRefreshProcesses { get; set; }

		[Category("Tick Timing")]
		[ReadOnly(true)]
		[DisplayName("HowManyTicksBeforeIncursionData")]
		[Description("How many ticks before refreshing Incursion Data")]
		[DefaultValue(typeof(int), "3600")]
		public int HowManyTicksBeforeIncursionData { get; set; }

		[Category("Tick Timing")]
        [ReadOnly(true)]
        [DisplayName("HowManyTicksMapAnimation")]
        [Description("How many ticks between animation of map")]
        [DefaultValue(typeof(int), "2")]
        public int HowManyTicksMapAnimation { get; set; }

        [Category("Tick Timing")]
        [ReadOnly(true)]
        [DisplayName("HowManyTicksBeforeSwitchISKPerHour")]
        [Description("How many ticks between ISH/Hr and Total ISK/Hr")]
        [DefaultValue(typeof(int), "41")]
        public int HowManyTicksBeforeSwitchISKPerHour { get; set; }

        [Category("Tick Timing")]
        [ReadOnly(true)]
        [DisplayName("HowManyTicksBeforeSwitchMiningPerHour")]
        [Description("How many ticks between Mining/Hr and Total Mining/Hr")]
        [DefaultValue(typeof(int), "39")]
        public int HowManyTicksBeforeSwitchMiningPerHour { get; set; }

        [Category("Tick Timing")]
        [ReadOnly(true)]
        [DisplayName("HowManyTicksMiningCSVOutput")]
        [Description("How many ticks between Mining/Hr CSV output")]
        [DefaultValue(typeof(int), "500")] 
        public int HowManyTicksMiningCSVOutput { get; set; }
        
        [Category("Main window")]
        public Point InitialPosition { get; set; }

        [Category("Main window")]
        public Size MainWindowSize { get; set; }

		[Category("Alert window")]
		public Size AlertWindowSize { get; set; }

		[Category("Main window")]
        [DisplayName("Stay On Top")]
        [Description("Should the main window stay on top of other windows")]
        [DefaultValue(typeof(bool), "true")]
        public bool StayOnTop { get; set; }

        [Category("Main window")]
        [DisplayName("Text Colour")]
        [Description("The Text Foreground colour for the main window")]
        [DefaultValue(typeof(Color), "LightGoldenrodYellow")]
        public Color MainWindowTextColour { get; set; }

        [Category("Main window")]
        [DisplayName("Background Colour")]
        [Description("Background Colour for the main window")]
        [DefaultValue(typeof(Color), "#0f0f11")]
        public Color MainWindowBackgroundColour { get; set; }

        [Category("Main window")]
        [DisplayName("Show Minimap")]
        [Description("Show minimap")]
        [DefaultValue(typeof(bool), "true")]
        public bool ShowMinimap { get; set; }

        [Category("Main window")]
        [DisplayName("Show Minimap Border")]
        [Description("Show border on the minimap")]
        [DefaultValue(typeof(bool), "false")]
        public bool ShowMinimapBorder { get; set; }

		[Category("Main window")]
		[DisplayName("Show system text on minimap")]
		[Description("")]
		[DefaultValue(typeof(bool), "true")]
		public bool MinimapShowSystemText { get; set; }

		[Category("Main window")]
		[DisplayName("Show all system text on minimap")]
		[Description("")]
		[DefaultValue(typeof(bool), "false")]
		public bool MinimapShowAllSystemText { get; set; }

		[Category("Main window")]
		[DisplayName("Show connecting system text on minimap")]
		[Description("")]
		[DefaultValue(typeof(bool), "true")]
		public bool MinimapShowConnectingSystemText { get; set; }

		[Category("Main window")]
        [DisplayName("Minimap Border Colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "SteelBlue")]
        public Color MinimapBorderColour { get; set; }

        [Category("Main window")]
        [DisplayName("Minimap Normal connection colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Gainsboro")]
        public Color MinimapLineColourNormal { get; set; }

        [Category("Main window")]
        [DisplayName("Minimap Out of region connection colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "DimGray")]
        public Color MinimapLineColourOutOfRegion { get; set; }

        [Category("Main window")]
        [DisplayName("Minimap Route connection colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Tomato")]
        public Color MinimapLineColourRoute { get; set; }
        
        [Category("Main window")]
        [DisplayName("Minimap normal system colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Gainsboro")]
        public Color MinimapSystemColourNormal { get; set; }
        
        [Category("Main window")]
        [DisplayName("Minimap route system colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Pink")]
        public Color MinimapSystemColourRoute { get; set; }

        [Category("Main window")]
        [DisplayName("Minimap from system colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Red")]
        public Color MinimapSystemColourFrom { get; set; }

        [Category("Main window")]
        [DisplayName("Minimap to system colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Green")]
        public Color MinimapSystemColourTo { get; set; }

		[Category("Main window")]
		[DisplayName("Minimap system name colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "LemonChiffon")]
		public Color MinimapSystemColourText { get; set; }

		[Category("Main window")]
		[DisplayName("Minimap all system name colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "PaleGoldenrod")]
		public Color MinimapAllSystemColourText { get; set; }

		[Category("Main window")]
		[DisplayName("Minimap connecting system name colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "DarkKhaki")]
		public Color MinimapConnectingSystemColourText { get; set; }

		[Category("Main window")]
        [DisplayName("Minimap zkill indicator colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Red")]
        public Color MinimapZKillColour { get; set; }

        [Category("Main window")]
        [DisplayName("Minimap intel indicator colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Orange")]
        public Color MinimapIntelColour { get; set; }

        [Category("Main window")]
        [DisplayName("Minimap system name colour for mouseover")]
        [Description("")]
        [DefaultValue(typeof(Color), "OrangeRed")]
        public Color MinimapMouseoverSystemColour { get; set; }

        [Category("Main window")]
        [DisplayName("Minimap zoom delta")]
        [Description("How much to adjust zoom on mouse scroll")]
        [DefaultValue(typeof(int), "5")]
        public int MainWindowMouseScrollAmount { get; set; }

        [Category("Main window")]
        [DisplayName("Minimap zoom initial map scale")]
        [Description("How to scale main window minimap - default 100")]
        [DefaultValue(typeof(int), "90")]
        public int MainWindowMapScale { get; set; }
        

        [Category("Alert")]
        [DisplayName("Minimap system name colour for mouseover")]
        [Description("")]
        [DefaultValue(typeof(Color), "OrangeRed")]
        public Color AlertPopupMinimapMouseoverSystemColour { get; set; }

        [Category("Alert")]
        [DisplayName("Transparent")]
        [Description("Sill the alert show as transparent window - this does not currently work amazingly")]
        [DefaultValue(typeof(bool), "false")]
        [ReadOnly(true)]
        public bool AlertTransparent { get; set; }

		[Category("Main window")]
		[DisplayName("Minimap Alt Indicator colour")]
		[Description("Colour to use for the logged in alt indicators on the minimap")]
		[DefaultValue(typeof(Color), "Aqua")]
		public Color MinimapAltPositionColour { get; set; }

		[Category("Main window")]
		[DisplayName("Minimap Unihole Indicator colour")]
		[Description("Colour to use for Unihole indicators on the minimap")]
		[DefaultValue(typeof(Color), "Fuchsia")]
		public Color MinimapUniHoleColour { get; set; }

		[Category("Main window")]
		[DisplayName("Minimap incursion Indicator colour")]
		[Description("Colour to use for incursion indicators on the minimap")]
		[DefaultValue(typeof(Color), "DarkGreen")]
		public Color MinimapIncursionColour { get; set; }

		[Category("Main window")]
		[DisplayName("Minimap sec limit Indicator colour")]
		[Description("Colour to use for security limit indicators on the minimap")]
		[DefaultValue(typeof(Color), "DarkBlue")]
		public Color MinimapSecLimitColour { get; set; }

		[Category("Main window")]
		[DisplayName("Minimap sec limit Indicator outline colour")]
		[Description("Colour to use for security limit indicators on the minimap")]
		[DefaultValue(typeof(Color), "Blue")]
		public Color MinimapSecLimitOutlineColour { get; set; }

		[Category("Main window")]
		[DisplayName("Minimap sec limit Indicator enabled")]
		[Description("use security limit indicators on the minimap")]
		[DefaultValue(typeof(bool), "true")]
		public bool MinimapSecLimit { get; set; }

		[Category("Main window")]
		[DisplayName("Minimap seclimit Indicator alpha")]
		[Description("Alpha to use for seclimit indicators on the minimap")]
		[DefaultValue(typeof(int), "70")]
		[Range(0, 100)]
		public int MinimapSecLimitAlpha { get; set; }

		[Category("Main window")]
		[DisplayName("Minimap seclimit cloud size")]
		[Description("Size to use for seclimit indicators on the minimap")]
		[DefaultValue(typeof(int), "55")]
		[Range(0, 100)]
		public int MinimapSecLimitCloudSize { get; set; }

		[Category("Main window")]
		[DisplayName("Minimap seclimit")]
		[Description("seclimit on the minimap")]
		[DefaultValue(typeof(float), "-0.7")]
		[Range(0, 100)]
		public float MinimapSecLimitValue { get; set; }

		[Category("Main window")]
		[DisplayName("Minimap incursion Indicator outline colour")]
		[Description("Colour to use for outline for incursion indicators on the minimap")]
		[DefaultValue(typeof(Color), "Red")]
		public Color MinimapIncursionOutlineColour { get; set; }

		[Category("Main window")]
		[DisplayName("Minimap incursion Indicator alpha")]
		[Description("Alpha to use for incursion indicators on the minimap")]
		[DefaultValue(typeof(int), "60")]
        [Range(0, 100)]
		public int MinimapIncursionAlpha { get; set; }

		[Category("Main window")]
		[DisplayName("Minimap incursion cloud size")]
		[Description("Size to use for incursion indicators on the minimap")]
		[DefaultValue(typeof(int), "65")]
		[Range(0, 100)]
		public int MinimapIncursionCloudSize { get; set; }

		[Category("Main window")]
		[DisplayName("Minimap connection line gap size")]
		[Description("Size of gap at each side of connection on minimap")]
		[DefaultValue(typeof(int), "10")]
		[Range(0, 100)]
		public int MinimapConnectionGap { get; set; }

		[Category("Main window")]
		[DisplayName("Minimap Ice Indicator colour")]
		[Description("Colour to use for ice indicatos on the minimap")]
		[DefaultValue(typeof(Color), "Cyan")]
		public Color MinimapIceColour { get; set; }

		[Category("Main window")]
		[DisplayName("Minimap Mercoxit Indicator colour")]
		[Description("Colour to use for Mercoxit indicatos on the minimap")]
		[DefaultValue(typeof(Color), "IndianRed")]
		public Color MinimapMercoxitColour { get; set; }

        [Category("Main window")]
        [DisplayName("Maptype to use")]
        [Description("Which map layout to use for the Minimap")]
        [DefaultValue(typeof(MapType), "Dotlan")]
        public MapType MinimapRawXY { get; set; }

        [Category("Main window")]
        [DisplayName("Minimap Jumps")]
        [Description("How many jumps to show in the minimap away from starting system")]
        [DefaultValue(typeof(int), "3")]
        public int MinimapJumps { get; set; }

        [Category("Main window")]
        [DisplayName("Minimap show out of region systems")]
        [Description("Show systems outside current region if in configured jumps range {MinimapJumps}")]
        [DefaultValue(typeof(bool), "false")]
        public bool MinimapShowOutOfRegion { get; set; }

        [Category("Main window")]
        [DisplayName("Show Extra Information")]
        [Description("The extra information display shows zkill and alert activity recently in systems on the current minimap view")]
        [DefaultValue(typeof(bool), "false")]
        public bool MainWindowShowExtraInformation { get; set; }

        [Category("Main window")]
        [DisplayName("Show total ISK per Hour")]
        [Description("Show the total ISK per hour earned as well as per toon")]
        [DefaultValue(typeof(bool), "true")]
        public bool ShowISKTotal { get; set; }

        [Category("Main window")]
        [DisplayName("Show total Mining per Hour")]
        [Description("Show the total Mining per hour as well as per toon")]
        [DefaultValue(typeof(bool), "true")]
        public bool ShowMiningTotal { get; set; }

		[Category("Main window")]
		[DisplayName("Item Locations")]
		public List<ConfigurationItemLocationInformation> MainWindowItemLocations { get; set; }

		[Category("Alert")]
		[DisplayName("Item Locations")]
		public List<ConfigurationItemLocationInformation> AlertWindowItemLocations { get; set; }

		[Category("DPS")]
        [DisplayName("DPS Graph location")]
        [Description("Location to display DPS graph when required")]
        public PopupLocationType DPSLocation { get; set; }

        [Category("DPS")]
        [DisplayName("DPS Graph Window size")]
        [Description("Size of the DPS graph")]
        public Size DPSGraphWindowSize { get; set; }

        [Category("DPS")]
        [DisplayName("DPS Graph Window size adjustment")]
        [Description("Adjustment pixels to the DPS graph window size")]
        public Point DPSGraphWindowSizeAdjust { get; set; }

        [Category("DPS")]
        [DisplayName("Stay On Top")]
        [Description("Should the DPS Graph stay on top")]
        [DefaultValue(typeof(bool), "true")]
        public bool DPSGraphStayOnTop { get; set; }

        [Category("DPS")]
        [DisplayName("Legends")]
        [Description("Show legends on the DPS graph")]
        [DefaultValue(typeof(bool), "false")]
        public bool DPSGraphLegends { get; set; }

        [Category("DPS")]
        [DisplayName("Transparent")]
        [Description("Should the DPS graph be transparent. If so it will overlay the current eveonline window and be unclickable (clickthrough!)")]
        [DefaultValue(typeof(bool), "true")]
        public bool DPSGraphTransparent { get; set; }

        [Category("DPS")]
        [DisplayName("Show DPS")]
        [Description("")]
        [DefaultValue(typeof(bool), "true")]
        public bool DPSShowWindow { get; set; }

        [Category("DPS")]
        [DisplayName("Combine DPS")]
        [Description("Should all DPS values be combined for the graph")]
        [DefaultValue(typeof(bool), "false")]
        public bool DPSAllToons { get; set; }

        [Category("DPS")]
        [DisplayName("Save to CSV")]
        [Description("Should data be saved to CSV as graph is built")]
        [DefaultValue(typeof(bool), "false")]
        public bool DPSSaveCSV { get; set; }

        [Category("DPS")]
        [DisplayName("Combat Seconds")]
        [Description("How many seconds combat the DPS graph should show")]
        [DefaultValue(typeof(int), "300")]
        public int GraphCombatSeconds { get; set; }

        [Category("DPS")]
        [DisplayName("Combat Max Points")]
        [Description("Maximum number of data points the graph should show")]
        [DefaultValue(typeof(int), "30")]
        public int GraphCombatMaxPoints { get; set; }

        [Category("DPS")]
        [DisplayName("Combat Minimum Point Seconds")]
        [Description("Minimum number of seconds per datapoint")]
        [DefaultValue(typeof(int), "10")]
        public int GraphCombatMinPointSeconds { get; set; }

        [Category("DPS")]
        [DisplayName("Line size")]
        [Description("Width of the graph lines")]
        [DefaultValue(typeof(int), "2")]
        public int DPSGraphLineSize { get; set; }


        [Category("DPS")]
        [DisplayName("DPS In Colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Red")]
        public Color DPSGraphDPSInColour { get; set; }

        [Category("DPS")]
        [DisplayName("DPS Out Colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Green")]
        public Color DPSGraphDPSOutColour { get; set; }

        [Category("DPS")]
        [DisplayName("Neut In Colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Orange")]
        public Color DPSGraphNeutInColour { get; set; }

        [Category("DPS")]
        [DisplayName("Neut Out Colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "YellowGreen")]
        public Color DPSGraphNeutOutColour { get; set; }

        [Category("DPS")]
        [DisplayName("Logi Shields In Colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Aqua")]
        public Color DPSGraphLogiShieldsInColour { get; set; }

        [Category("DPS")]
        [DisplayName("Logi Shields Out Colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "CornflowerBlue")]
        public Color DPSGraphLogiShieldsOutColour { get; set; }

        [Category("DPS")]
        [DisplayName("Logi Armor In Colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Fuchsia")]
        public Color DPSGraphLogiArmorInColour { get; set; }

        [Category("DPS")]
        [DisplayName("Logi Armor Out Colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "DarkMagenta")]
        public Color DPSGraphLogiArmorOutColour { get; set; }

        [Category("DPS")]
        [DisplayName("Logi Hull In Colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Silver")]
        public Color DPSGraphLogiHullInColour { get; set; }

        [Category("DPS")]
        [DisplayName("Logi Hull Out Colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "SlateGray")]
        public Color DPSGraphLogiHullOutColour { get; set; }

        [Category("DPS")]
        [DisplayName("Logi Capacitor In Colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "LightYellow")]
        public Color DPSGraphLogiCapacitorInColour { get; set; }

        [Category("DPS")]
        [DisplayName("Logi Capacitor Out Colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "DarkOrange")]
        public Color DPSGraphLogiCapacitorOutColour { get; set; }

        [Category("Alert")]
        [DisplayName("Popup Seconds")]
        [Description("How long to show alerts for")]
        [DefaultValue(typeof(int), "8")]
        public int AlertPopupSeconds { get; set; }

        [Category("Alert")]
        [DisplayName("Popup Opacity")]
        [Description("")]
        [DefaultValue(typeof(int), "99")]
        [ReadOnly(true)]
        public int AlertPopupOpacity { get; set; }

        [Category("Alert")]
        [DisplayName("Opacity Step")]
        [Description("%age opacity to change at a time. smaller values means fade in/out slower")]
        [ReadOnly(true)]
        [DefaultValue(typeof(int), "5")]
        public int AlertPopupOpacityStep { get; set; }

        [Category("Alert")]
        [DisplayName("Alert Location")]
        [Description("")]
        public PopupLocationType AlertPopupLocation { get; set; }

        [Category("Alert")]
        [DisplayName("Background")]
        [Description("")]
        [DefaultValue(typeof(string), "bg_DefaultAlert")]
        [Browsable(false)]
        public string AlertPopupBackground { get; set; }

        [Category("Alert")]
        [DisplayName("Show Minimap")]
        [Description("Show the minimap in alerts")]
        [DefaultValue(typeof(bool), "true")]
        public bool AlertPopupShowMinimap { get; set; }

        [Category("Alert")]
        [DisplayName("Show Minimap Border")]
        [Description("")]
        [DefaultValue(typeof(bool), "false")]
        public bool AlertPopupShowMinimapBorder { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap Show System Text")]
		[Description("Show to/from system names in minimap")]
		[DefaultValue(typeof(bool), "true")]
		public bool AlertPopupMinimapShowSystemText { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap Show All System Text")]
		[Description("Show all system names in minimap")]
		[DefaultValue(typeof(bool), "false")]
		public bool AlertPopupMinimapShowAllSystemText { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap Show Connecting System Text")]
		[Description("Show connecting system names in minimap")]
		[DefaultValue(typeof(bool), "false")]
		public bool AlertPopupMinimapShowConnectingSystemText { get; set; }

		[Category("Alert")]
        [DisplayName("Minimap Border Colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "SteelBlue")]
        public Color AlertPopupMinimapBorderColour { get; set; }

        [Category("Alert")]
        [DisplayName("Minimap Normal connection colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Gainsboro")]
        public Color AlertPopupMinimapLineColourNormal { get; set; }

        [Category("Main window")]
        [DisplayName("Minimap Out of region connection colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "DimGray")]
        public Color AlertPopupMinimapLineColourOutOfRegion { get; set; }

        [Category("Alert")]
        [DisplayName("Minimap route connection colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Tomato")]
        public Color AlertPopupMinimapLineColourRoute { get; set; }

        [Category("Alert")]
        [DisplayName("Minimap Normal system colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Gainsboro")]
        public Color AlertPopupMinimapSystemColourNormal { get; set; }

        [Category("Alert")]
        [DisplayName("Minimap Route system colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Pink")]
        public Color AlertPopupMinimapSystemColourRoute { get; set; }

        [Category("Alert")]
        [DisplayName("Minimap From system colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Red")]
        public Color AlertPopupMinimapSystemColourFrom { get; set; }

        [Category("Alert")]
        [DisplayName("Minimap To system colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Green")]
        public Color AlertPopupMinimapSystemColourTo { get; set; }

        [Category("Alert")]
        [DisplayName("Minimap system name colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "LemonChiffon")]
        public Color AlertPopupMinimapSystemColourText { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap all system name colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "PaleGoldenrod")]
		public Color AlertPopupMinimapAllSystemColourText { get; set; }
		[Category("Alert")]
		[DisplayName("Minimap connecting system name colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "DarkKhaki")]
		public Color AlertPopupMinimapConnectingSystemColourText { get; set; }
		
        [Category("Alert")]
        [DisplayName("Minimap zkill indicator colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Red")]
        public Color AlertPopupMinimapZKillColour { get; set; }

        [Category("Alert")]
        [DisplayName("Minimap intel indicator colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Orange")]
        public Color AlertPopupMinimapIntelColour { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap logged in alt indicator colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "Aqua")]
		public Color AlertPopupMinimapAltPositionColour { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap Unihole indicator colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "Fuchsia")]
		public Color AlertPopupMinimapUniHoleColour { get; set; }

		[Category("Alert")]
        [DisplayName("Minimap Ice Indicator colour")]
        [Description("Colour to use for ice indicatos on the alert popup")]
        [DefaultValue(typeof(Color), "Cyan")]
        public Color AlertPopupMinimapIceColour { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap Mercoxit Indicator colour")]
		[Description("Colour to use for Mercoxit indicatos on the alert popup")]
		[DefaultValue(typeof(Color), "IndianRed")]
		public Color AlertPopupMinimapMercoxitColour { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap incursion Indicator colour")]
		[Description("Colour to use for incursion indicators on the alert popup")]
		[DefaultValue(typeof(Color), "DarkGreen")]
		public Color AlertPopupMinimapIncursionColour { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap incursion Indicator outline colour")]
		[Description("Colour to use for outline to incursion indicators on the alert popup")]
		[DefaultValue(typeof(Color), "Red")]
		public Color AlertPopupMinimapIncursionOutlineColour { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap incursion Indicator alpha")]
		[Description("Alpha to use for incursion indicators on the alert popup")]
		[DefaultValue(typeof(int), "60")]
		[Range(0, 100)]
        public int AlertPopupMinimapIncursionAlpha { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap incursion Indicator size")]
		[Description("Size of each incursion indicator on the alert popup")]
		[DefaultValue(typeof(int), "65")]
		[Range(0, 100)]
		public int AlertPopupMinimapIncursionCloudSize { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap sec limit Indicator colour")]
		[Description("Colour to use for security limit indicators on the minimap")]
		[DefaultValue(typeof(Color), "DarkBlue")]
		public Color AlertPopupMinimapSecLimitColour { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap sec limit Indicator outline colour")]
		[Description("Colour to use for security limit indicators on the minimap")]
		[DefaultValue(typeof(Color), "Blue")]
		public Color AlertPopupMinimapSecLimitOutlineColour { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap sec limit Indicator enabled")]
		[Description("use security limit indicators on the minimap")]
		[DefaultValue(typeof(bool), "true")]
		public bool AlertPopupMinimapSecLimit { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap seclimit Indicator alpha")]
		[Description("Alpha to use for seclimit indicators on the minimap")]
		[DefaultValue(typeof(int), "70")]
		[Range(0, 100)]
		public int AlertPopupMinimapSecLimitAlpha { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap seclimit cloud size")]
		[Description("Size to use for seclimit indicators on the minimap")]
		[DefaultValue(typeof(int), "55")]
		[Range(0, 100)]
		public int AlertPopupMinimapSecLimitCloudSize { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap seclimit")]
		[Description("seclimit on the minimap")]
		[DefaultValue(typeof(float), "-0.7")]
		[Range(0, 100)]
		public float AlertPopupMinimapSecLimitValue { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap Connection line gap size")]
		[Description("Size of gap and each end of connections on minimap")]
		[DefaultValue(typeof(int), "10")]
		[Range(0, 100)]
		public int AlertPopupMinimapConnectionGap { get; set; }

		[Category("Alert")]
        [DisplayName("Which maptype to use for alert windows")]
        [Description("Which minimap to use for Alert windows")]
        [DefaultValue(typeof(MapType), "Dotlan")]
        public MapType AlertPopupMinimapRawXY { get; set; }


		[Category("Alert")]
		[DisplayName("Show Zkill Victim Logo")]
		[Description("When zkill alert - show victim alliance/corp logo")]
		[DefaultValue(typeof(Boolean), "True")]
		public Boolean AlertPopupShowVictimLogo { get; set; }

		[Category("Alert")]
		[DisplayName("Show Zkill Killer Logo")]
		[Description("When zkill alert - show killers alliance/corp logo")]
		[DefaultValue(typeof(Boolean), "True")]
		public Boolean AlertPopupShowKillerLogo { get; set; }

		[Category("Alert")]
		[DisplayName("Zkill Logo size")]
		[Description("Size to use for zkill corp/alliance logo")]
		[DefaultValue(typeof(int), "150")]
		public int AlertPopZkillLogoSize { get; set; }

		[Category("Alert")]
		[DisplayName("Zkill ISK height")]
		[Description("Height to use for zkill isk bar")]
		[DefaultValue(typeof(int), "130")]
		public int  AlertPopZkillHeight { get; set; }

		[Category("Alert")]
        [DisplayName("ZKillAlertText")]
        [Description("")]
        [DefaultValue(typeof(string), "{victim} died to {killer} in {system}")]
        public string ZKillAlertText { get; set; }

        [Category("Alert")]
        [DisplayName("ZKillContextText")]
        [Description("")]
        [DefaultValue(typeof(string), "{victim} {victimship}\nKilled by {killer}")]
        public string ZKillContextText { get; set; }

        [Category("Alert")]
        [DisplayName("ZKillMenuText")]
        [Description("")]
        [DefaultValue(typeof(string), "{killer} killed {victim} in {system} {value}")]
        public string ZKillMenuText { get; set; }


		[Category("Alert")]
        [DisplayName("ZKillSound")]
        [Description("")]
        [DefaultValue(typeof(string), "quiet_click")]
        [ReadOnly(true)]
        public string ZKillSound { get; set; }

        [Category("Alert")]
        [DisplayName("ZKill Circle Colour")]
        [Description("Colour of circle around ship picture for zkill alerts")]
        [DefaultValue(typeof(Color), "Red")]
        public Color ZKillCircleColour { get; set; }

        [Category("Alert")]
        [DisplayName("ZKill Isk Colour")]
        [Description("Colour of ISK amount for zkill alerts")]
        [DefaultValue(typeof(Color), "White")]
        public Color ZKillIskColour { get; set; }

        [Category("Alert")]
        [DisplayName("ZKillBarColour")]
        [Description("Colour of bar under ISK amount for zkill alerts")]
        [DefaultValue(typeof(Color), "LimeGreen")]
        public Color ZKillBarColour { get; set; }

        [Category("Alert")]
        [DisplayName("ZKillShowBar")]
        [Description("Show coloured bar under zkill ISK amounts")]
        [DefaultValue(typeof(bool), "true")]
        public bool ZKillShowBar { get; set; }

        [Category("General")]
        [DisplayName("Alert expiry seconds")]
        [Description("How many seconds to consider an alert as old and no longer need to track")]
        [DefaultValue(typeof(int), "300")]
        [ReadOnly(true)]
        public int ExpireAlertSeconds { get; set; }
        
        [Category("General")]
        [DisplayName("Combat expiry seconds")]
        [Description("How many seconds to consider combat entry as old and no longer need to track")]
        [DefaultValue(typeof(int), "1200")]
        [ReadOnly(true)]
        public int ExpireCombatSeconds { get; set; }

        [Category("General")]
        [DisplayName("ISK earned expiry seconds")]
        [Description("How many seconds to consider ISK bounty entry as old and no longer need to track")]
        [DefaultValue(typeof(int), "5400")]
        [ReadOnly(true)]
        public int ExpireISKSeconds { get; set; }

        [Category("General")]
        [DisplayName("Amount mined expiry seconds")]
        [Description("How many seconds to consider Mining events as old and no longer need to track")]
        [DefaultValue(typeof(int), "5400")]
        [ReadOnly(true)]
        public int ExpireMiningSeconds { get; set; }

        [Category("General")]
        [DisplayName("ZKill expiry seconds")]
        [Description("How many seconds to consider zkill entry as old and no longer need to track")]
        [DefaultValue(typeof(int), "600")]
        [ReadOnly(true)]
        public int ExpireZKillSeconds { get; set; }

        [Category("General")]
        [DisplayName("OBS Integration port")]
        [Description("TCP/IP Port number to bind to for OBS integration")]
        [DefaultValue(typeof(int), "65432")]
        public int IntegrationPort { get; set; }

        [Category("General")]
        [DisplayName("Dotlan map expiry - minutes")]
        [Description("How many minutes to consider dotlan map data as old")]
        [DefaultValue(typeof(int), "90")]
        public int DotlanMapExpiresMinutes { get; set; }

        [Category("General")]
        [DisplayName("Dotlan delta data expiry - minutes")]
        [Description("How many minutes to consider dotlan npc delta data as old")]
        [DefaultValue(typeof(int), "61")]
        public int DotlanDeltaExpiresMinutes { get; set; }

        [Category("General")]
        [DisplayName("Pull dotlan map layout svg")]
        [Description("Should dotlan svg be pulled and parsed regularly")]
        [DefaultValue(typeof(bool), "true")]
        [ReadOnly(true)]
        public bool DotlanMapPull { get; set; }

        [Category("General")]
        [DisplayName("Pull dotlan delta information")]
        [Description("Should dotlan npc delta feed be pulled regularly")]
        [DefaultValue(typeof(bool), "true")]
        public bool DotlanDeltaPull { get; set; }

		[Category("General")]
		[DisplayName("Keep current toon as last selected")]
		[Description("If current process is not an eve process - treat last toon selected as current")]
		[DefaultValue(typeof(bool), "true")]
		public bool KeepLastToonWhenNotEve { get; set; }
		
        public List<AlertConfig> AlertConfiguration { get; set; }
        public List<LogfileConfig> Logfiles { get; set; }
        public List<CSSData> CSSData { get; set; }

        public List<Regex> ignoreStrings { get; set; }
        public List<int> ignoreSystems { get; set; }

        [Category("Planet Info")]
        [DisplayName("Size of Planet on main window")]
        [Description("When showing planet information on main window - how BIG should they be")]
        [DefaultValue(typeof(int), "6")]
        public int PlanetInfoViewSize { get; set; }

        [Category("Planet Info")]
        [DisplayName("Show planet information")]
        [Description("Should quesadilla show planet infomation")]
        [DefaultValue(typeof(bool), "true")]
        public bool ShowPlanetInformation { get; set; }

        [Category("Planet Info")]
        [DisplayName("Temperate colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "LightSkyBlue")]
        public Color PlanetInfoTemperateColour { get; set; }

        [Category("Planet Info")]
        [DisplayName("Ice colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "RoyalBlue")]
        public Color PlanetInfoIceColour { get; set; }

        [Category("Planet Info")]
        [DisplayName("Gas colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Yellow")]
        public Color PlanetInfoGasColour { get; set; }

        [Category("Planet Info")]
        [DisplayName("Oceanic colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "DodgerBlue")]
        public Color PlanetInfoOceanicColour { get; set; }

        [Category("Planet Info")]
        [DisplayName("Lava colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Red")]
        public Color PlanetInfoLavaColour { get; set; }

        [Category("Planet Info")]
        [DisplayName("Barren colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Orange")]
        public Color PlanetInfoBarrenColour { get; set; }

        [Category("Planet Info")]
        [DisplayName("Storm colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "MediumBlue")]
        public Color PlanetInfoStormColour { get; set; }

        [Category("Planet Info")]
        [DisplayName("Plasma colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Cyan")]
        public Color PlanetInfoPlasmaColour { get; set; }

        [Category("Planet Info")]
        [DisplayName("Planet info font size")]
        [Description("")]
        [DefaultValue(typeof(int), "8")]
        public int PlanetInfoFontSize { get; set; }

        [Category("HeartBeat")]
        [DisplayName("Heartbeat indicator Colour")]
        [Description("")]
        [DefaultValue(typeof(Color), "Red")]
        public Color HeartBeatIndicatorColour { get; set; }

        [Category("HeartBeat")]
        [DisplayName("Heartbeat indicator text")]
        [Description("Check homepage for some examples")]
        //        [DefaultValue(typeof(string), " ▁▂▃▄▅▆▇█▇▆▅▄▃▁")]
        //        [DefaultValue(typeof(string)," ▏▎▍▌▋▊▉▊▋▌▍")] 
        //        [DefaultValue(typeof(string), "▖▘▝▗")]
        //        [DefaultValue(typeof(string), "⠁⠁⠉⠙⠚⠒⠂⠂⠒⠲⠴⠤⠄⠄⠤⠠⠠⠤⠦⠖⠒⠐⠐⠒⠓⠋⠉⠈⠈")]
        [DefaultValue(typeof(string), "⠈⠉⠋⠓⠒⠐⠐⠒⠖⠦⠤⠠⠠⠤⠦⠖⠒⠐⠐⠒⠓⠋⠉⠈")]
        public string HeartBeatIndicatorWorkingText { get; set; }
        [Category("HeartBeat")]
        [DisplayName("Show Heartbeat indicator")]
        [Description("")]
        [DefaultValue(typeof(bool), "true")]
        public bool HeartBeatIndicatorVisible { get; set; }

        [Category("Alert")]
        [DisplayName("Show alerts as windows notifications")]
        [Description("")]
        [DefaultValue(typeof(bool), "false")]
        public bool ShowAlertWindowsNotification { get; set; }

        [Category("Alert")]
        [DisplayName("Show alerts as Quesadilla notifications")]
        [Description("")]
        [DefaultValue(typeof(bool), "true")]
        public bool ShowAlertFancyNotification { get; set; }



		[Category("OverlayMap")]
		[DisplayName("Overlay map Window size")]
		[Description("Size of the Overlay map")]
		public Size OverlayMapWindowSize { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Overlay Map position inside client")]
		[Description("Overlay Map position inside client")]
		public Point OverlayMapWindowPosition { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Show Overlay Map")]
		[Description("")]
		[DefaultValue(typeof(bool), "true")]
		public bool ShowWindowOverlayMap { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Overlay Minimap zoom map scale")]
		[Description("How to scale overlay window minimap - default 70")]
		[DefaultValue(typeof(int), "90")]
		public int OverlayMapMapScale { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Show Minimap Border")]
		[Description("Show border on the minimap")]
		[DefaultValue(typeof(bool), "false")]
		public bool OverlayShowMinimapBorder { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap Jumps")]
		[Description("How many jumps to show in the minimap away from starting system")]
		[DefaultValue(typeof(int), "3")]
		public int OverlayMinimapJumps { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Show system text on minimap")]
		[Description("")]
		[DefaultValue(typeof(bool), "true")]
		public bool OverlayMinimapShowSystemText { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Show all system text on minimap")]
		[Description("")]
		[DefaultValue(typeof(bool), "false")]
		public bool OverlayMinimapShowAllSystemText { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Show connecting system text on minimap")]
		[Description("")]
		[DefaultValue(typeof(bool), "true")]
		public bool OverlayMinimapShowConnectingSystemText { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap Normal connection colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "Gainsboro")]
		public Color OverlayMinimapLineColourNormal { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap Out of region connection colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "DimGray")]
		public Color OverlayMinimapLineColourOutOfRegion { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap Border Colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "SteelBlue")]
		public Color OverlayMinimapBorderColour { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap Route connection colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "Red")]
		public Color OverlayMinimapLineColourRoute { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap normal system colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "Gainsboro")]
		public Color OverlayMinimapSystemColourNormal { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap route system colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "Red")]
		public Color OverlayMinimapSystemColourRoute { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap from system colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "Red")]
		public Color OverlayMinimapSystemColourFrom { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap to system colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "Green")]
		public Color OverlayMinimapSystemColourTo { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap system name colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "LemonChiffon")]
		public Color OverlayMinimapSystemColourText { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap all system name colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "PaleGoldernRod")]
		public Color OverlayMinimapAllSystemColourText { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap connecting system name colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "DarkKhaki")]
		public Color OverlayMinimapConnectingSystemColourText { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap zkill indicator colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "Red")]
		public Color OverlayMinimapZKillColour { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap intel indicator colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "Orange")]
		public Color OverlayMinimapIntelColour { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap system name colour for mouseover")]
		[Description("")]
		[DefaultValue(typeof(Color), "Orchid")]
		public Color OverlayMinimapMouseoverSystemColour { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap zoom delta")]
		[Description("How much to adjust zoom on mouse scroll")]
		[DefaultValue(typeof(int), "5")]
		public int OverlayMainWindowMouseScrollAmount { get; set; }

		[Category("OverlayMap")]
				[DisplayName("Minimap Alt Indicator colour")]
				[Description("Colour to use for the logged in alt indicators on the minimap")]
				[DefaultValue(typeof(Color), "Aqua")]
				public Color OverlayMinimapAltPositionColour { get; set; }

		[Category("OverlayMap")]
				[DisplayName("Minimap Unihole Indicator colour")]
				[Description("Colour to use for Unihole indicators on the minimap")]
				[DefaultValue(typeof(Color), "Fuchsia")]
				public Color OverlayMinimapUniHoleColour { get; set; }

		[Category("OverlayMap")]
				[DisplayName("Minimap incursion Indicator colour")]
				[Description("Colour to use for incursion indicators on the minimap")]
				[DefaultValue(typeof(Color), "DarkGreen")]
				public Color OverlayMinimapIncursionColour { get; set; }

		[Category("OverlayMap")]
				[DisplayName("Minimap sec limit Indicator colour")]
				[Description("Colour to use for security limit indicators on the minimap")]
				[DefaultValue(typeof(Color), "DarkBlue")]
				public Color OverlayMinimapSecLimitColour { get; set; }

		[Category("OverlayMap")]
				[DisplayName("Minimap sec limit Indicator outline colour")]
				[Description("Colour to use for security limit indicators on the minimap")]
				[DefaultValue(typeof(Color), "Blue")]
				public Color OverlayMinimapSecLimitOutlineColour { get; set; }

		[Category("OverlayMap")]
				[DisplayName("Minimap sec limit Indicator enabled")]
				[Description("use security limit indicators on the minimap")]
				[DefaultValue(typeof(bool), "true")]
				public bool OverlayMinimapSecLimit { get; set; }

		[Category("OverlayMap")]
				[DisplayName("Minimap seclimit Indicator alpha")]
				[Description("Alpha to use for seclimit indicators on the minimap")]
				[DefaultValue(typeof(int), "70")]
				[Range(0, 100)]
				public int OverlayMinimapSecLimitAlpha { get; set; }

		[Category("OverlayMap")]
				[DisplayName("Minimap seclimit cloud size")]
				[Description("Size to use for seclimit indicators on the minimap")]
				[DefaultValue(typeof(int), "55")]
				[Range(0, 100)]
				public int OverlayMinimapSecLimitCloudSize { get; set; }

		[Category("OverlayMap")]
				[DisplayName("Minimap seclimit")]
				[Description("seclimit on the minimap")]
				[DefaultValue(typeof(float), "-0.7")]
				[Range(0, 100)]
				public float OverlayMinimapSecLimitValue { get; set; }

		[Category("OverlayMap")]
				[DisplayName("Minimap incursion Indicator outline colour")]
				[Description("Colour to use for outline for incursion indicators on the minimap")]
				[DefaultValue(typeof(Color), "Red")]
				public Color OverlayMinimapIncursionOutlineColour { get; set; }

		[Category("OverlayMap")]
				[DisplayName("Minimap incursion Indicator alpha")]
				[Description("Alpha to use for incursion indicators on the minimap")]
				[DefaultValue(typeof(int), "60")]
				[Range(0, 100)]
				public int OverlayMinimapIncursionAlpha { get; set; }

		[Category("OverlayMap")]
				[DisplayName("Minimap incursion cloud size")]
				[Description("Size to use for incursion indicators on the minimap")]
				[DefaultValue(typeof(int), "65")]
				[Range(0, 100)]
				public int OverlayMinimapIncursionCloudSize { get; set; }

		[Category("OverlayMap")]
				[DisplayName("Minimap connection line gap size")]
				[Description("Size of gap at each side of connection on minimap")]
				[DefaultValue(typeof(int), "10")]
				[Range(0, 100)]
				public int OverlayMinimapConnectionGap { get; set; }

		[Category("OverlayMap")]
				[DisplayName("Minimap Ice Indicator colour")]
				[Description("Colour to use for ice indicatos on the minimap")]
				[DefaultValue(typeof(Color), "Cyan")]
				public Color OverlayMinimapIceColour { get; set; }

		[Category("OverlayMap")]
				[DisplayName("Minimap Mercoxit Indicator colour")]
				[Description("Colour to use for Mercoxit indicatos on the minimap")]
				[DefaultValue(typeof(Color), "IndianRed")]
				public Color OverlayMinimapMercoxitColour { get; set; }

		[Category("OverlayMap")]
				[DisplayName("Maptype to use")]
				[Description("Which map layout to use for the Minimap")]
				[DefaultValue(typeof(MapType), "Dotlan")]
				public MapType OverlayMinimapRawXY { get; set; }

		[Category("OverlayMap")]
				[DisplayName("Minimap show out of region systems")]
				[Description("Show systems outside current region if in configured jumps range {MinimapJumps}")]
				[DefaultValue(typeof(bool), "false")]
				public bool OverlayMinimapShowOutOfRegion { get; set; }

		[Category("OverlayMap")]
		[DisplayName("ISK Graph Label colour")]
		[Description("Colour to use for ISK Graph Label")]
		[DefaultValue(typeof(Color), "LightGoldenrodYellow")]
		public Color OverlayISKGraphLabelColour { get; set; }

		[Category("OverlayMap")]
		[DisplayName("ISK Graph Y Line colour")]
		[Description("Colour to use for ISK Graph Y Line")]
		[DefaultValue(typeof(Color), "Purple")]
		public Color OverlayISKGraphYColour { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Mined Graph Label colour")]
		[Description("Colour to use for Mined Graph Label")]
		[DefaultValue(typeof(Color), "LightGoldenrodYellow")]
		public Color OverlayMinedGraphLabelColour { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Mined Graph Y Line colour")]
		[Description("Colour to use for Mined Graph Y Line")]
		[DefaultValue(typeof(Color), "Purple")]
		public Color OverlayMinedGraphYColour { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Mined Graph Mined Amount colour")]
		[Description("Colour to use for Mined Graph Mined Amount")]
		[DefaultValue(typeof(Color), "Green")]
		public Color OverlayMinedGraphMinedColour { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Mined Graph Wasted Amount colour")]
		[Description("Colour to use for Mined Graph Wasted Amount")]
		[DefaultValue(typeof(Color), "Red")]
		public Color OverlayMinedGraphWastedColour { get; set; }

		[Category("OverlayMap")]
		[DisplayName("ISK Graph Low Boundry")]
		[Description("ISK Amount for Low boundry")]
		[DefaultValue(typeof(double), "1000000")]
		public double OverlayISKGraphLowBoundry { get; set; }

		[Category("OverlayMap")]
		[DisplayName("ISK Graph Low Boundry Colour")]
		[Description("Colour to use for ISK Graph Low Boundry")]
		[DefaultValue(typeof(Color), "Green")]
		public Color OverlayISKGraphLowBoundryColour { get; set; }

		[Category("OverlayMap")]
		[DisplayName("ISK Graph Low Boundry")]
		[Description("ISK Amount for Low boundry")]
		[DefaultValue(typeof(double), "5000000")]
		public double OverlayISKGraphMediumBoundry { get; set; }
		[Category("OverlayMap")]
		[DisplayName("ISK Graph Low Boundry Colour")]
		[Description("Colour to use for ISK Graph Low Boundry")]
		[DefaultValue(typeof(Color), "Orange")]
		public Color OverlayISKGraphMediumBoundryColour { get; set; }

		[Category("OverlayMap")]
		[DisplayName("ISK Graph Low Boundry")]
		[Description("ISK Amount for Low boundry")]
		[DefaultValue(typeof(double), "10000000")]
		public double OverlayISKGraphHighBoundry { get; set; }
		[Category("OverlayMap")]
		[DisplayName("ISK Graph Low Boundry Colour")]
		[Description("Colour to use for ISK Graph Low Boundry")]
		[DefaultValue(typeof(Color), "Red")]
		public Color OverlayISKGraphHighBoundryColour { get; set; }




		[Category("OverlayMap")]
		[DisplayName("Minimap alpha")]
		[Description("Alpha to use for minimap on overlay window")]
		[DefaultValue(typeof(int), "70")]
		[Range(0, 100)]
		public int OverlayMinimapAlpha { get; set; }

		[Category("OverlayMap")]
		[DisplayName("ISK per min graph alpha")]
		[Description("Alpha to use for ISK per min graph")]
		[DefaultValue(typeof(int), "70")]
		[Range(0, 100)]
		public int OverlayISKGraphAlpha { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Mined per min graph alpha")]
		[Description("Alpha to use for Mined per min graph")]
		[DefaultValue(typeof(int), "60")]
		[Range(0, 100)]
		public int OverlayMinedGraphAlpha { get; set; }

		[Category("OverlayMap")]
		[DisplayName("ISK per min graph Y reversed")]
		[Description("Flip Y axis from bottom to top")]
		[DefaultValue(typeof(bool), "true")]
		public bool OverlayISKGraphYReversed { get; set; }

		[Category("OverlayMap")]
		[DisplayName("ISK per min graph Y legends on right")]
		[Description("Y legends on right side of graph")]
		[DefaultValue(typeof(bool), "true")]
		public bool OverlayISKGraphYLegendRight { get; set; }

		[Category("OverlayMap")]
		[DisplayName("ISK Graph All toons together")]
		[Description("Combine data from all toons")]
		[DefaultValue(typeof(bool), "true")]
		public bool OverlayISKGraphAllToons { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Mined per min graph Y reversed")]
		[Description("Flip Y axis from bottom to top")]
		[DefaultValue(typeof(bool), "false")]
		public bool OverlayMinedGraphYReversed { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Mined per min graph Y legends on right")]
		[Description("Y legends on right side of graph")]
		[DefaultValue(typeof(bool), "true")]
		public bool OverlayMinedGraphYLegendRight { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Mined Graph All toons together")]
		[Description("Combine data from all toons")]
		[DefaultValue(typeof(bool), "true")]
		public bool OverlayMinedGraphAllToons { get; set; }


		[Category("OverlayMap")]
		[DisplayName("Item Locations")]
		public List<ConfigurationItemLocationInformation> OverlayWindowItemLocations { get; set; }

	}
}