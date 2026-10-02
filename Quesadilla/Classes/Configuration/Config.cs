using Quesadilla.Classes.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Quesadilla.Classes
{
    public class Config
    {
		public void ApplyConfiguration(Form form,List<ConfigurationItemLocationInformation> locations,int topAdjust,bool miningPerHourTotal,bool iskPerHourTotal,bool ignoreVisible)
		{
			foreach (ConfigurationItemLocationInformation item in locations)
			{
				var foundItem = form.Controls.Find(item.Name, true).FirstOrDefault();

				if (foundItem != null)
				{
					if (foundItem.InvokeRequired)
					{
						var itemToApply = item;
						foundItem.BeginInvoke((MethodInvoker)(() => ApplyItemConfiguration(foundItem, itemToApply, topAdjust, miningPerHourTotal, iskPerHourTotal, ignoreVisible)));
						continue;
					}

					ApplyItemConfiguration(foundItem, item, topAdjust, miningPerHourTotal, iskPerHourTotal, ignoreVisible);
				}
			}
		}

		private static void ApplyItemConfiguration(Control foundItem, ConfigurationItemLocationInformation item, int topAdjust, bool miningPerHourTotal, bool iskPerHourTotal, bool ignoreVisible)
		{
			if (!foundItem.Name.Equals("ChatServerDown") && !ignoreVisible) foundItem.Visible = item.IsVisible;

			foundItem.Location = new Point(item.Location.X, item.Location.Y + topAdjust);

			foundItem.ForeColor = item.Color;
			foundItem.BackColor = item.BackColor;
			foundItem.Size = item.Size;

			foundItem.Font = item.Font;

			if (foundItem.Name.Equals("MiningData") && miningPerHourTotal) foundItem.ForeColor = item.AlternateColor;
			if (foundItem.Name.Equals("RattingData") && iskPerHourTotal) foundItem.ForeColor = item.AlternateColor;
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

		private List<ConfigurationItemLocationInformation> MergeLocations(List<ConfigurationItemLocationInformation> mergeData, List<ConfigurationItemLocationInformation> original)
		{
			var merged = new List<ConfigurationItemLocationInformation>();
			var mergeLookup = mergeData.ToDictionary(x => x.Name);

			foreach (var originalItem in original)
			{
				var source = mergeLookup.TryGetValue(originalItem.Name, out var loadedItem)
					? loadedItem
					: originalItem;

				merged.Add(new ConfigurationItemLocationInformation
				{
					Name = source.Name,
					Location = new Point(source.Location.X, source.Location.Y),
					Color = source.Color,
					IsVisible = source.IsVisible,
					Size = new Size(source.Size.Width, source.Size.Height),
					Font = source.Font,
					BackColor = source.BackColor,
					AlternateColor = source.AlternateColor
				});
			}

			return merged;
		}
		private void InitialiseCollections()
		{
			IgnoreSystems = new List<int>();

			IgnoreStrings = new List<string>
				{
					"\\bclr\\b",
					"\\bclear\\b",
					"\\bstatus\\b"
				};

			AlertConfiguration = new List<AlertConfig>();
			Logfiles = new List<LogfileConfig>();
			CSSData = new List<CSSData>();
		}

		private void ApplyDefaultAttributes()
		{
			foreach (PropertyDescriptor p in TypeDescriptor.GetProperties(this))
			{
				var property = GetType().GetProperty(p.Name);
				var defaultValue = (DefaultValueAttribute)p.Attributes[typeof(DefaultValueAttribute)];
				if (defaultValue != null) property?.SetValue(this, defaultValue.Value);
			}
		}

		private void InitialiseGeneralSettings()
		{
			InitialPosition = new Point(0, 0);

			MainWindowSize = new Size(760, 404);
			AlertWindowSize = new Size(400, 180);

			OverlayMapWindowSize = new Size(600, 350);
			OverlayMapWindowPosition = new Point(275, 1);

			OverlayDPSWindowPosition = new Point(1000, 150);
			OverlayDPSWindowSize = new Size(250, 130);

			AlertPopupLocation = PopupLocationType.BottomLeft;
			DPSLocation = PopupLocationType.TopRight;

			AlertPopZkillLogoSize = 120;
			AlertPopZkillHeight = 130;

			AlertPopupShowKillerLogo = true;
			AlertPopupShowVictimLogo = true;

			MinimapSecLimitValue = -0.7f;

			MinimapSecLimitShow = true;
			OverlayMinimapSecLimitShow = true;
			AlertPopupMinimapSecLimitShow = true;
		}

		private void InitialiseAlertDefaults()
		{
			AlertConfiguration.Add(new AlertConfig()
			{
				Name = "All online toons <= 5 jumps",
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
			/*
							AlertConfiguration.Add(new AlertConfig()
							{
								Name = "Izakbar (if online) <= 5 jumps",
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
			*/
			AlertConfiguration.Add(new AlertConfig()
			{
				Name = "Home system < 5 jumps",
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
			AlertConfiguration.Add(new AlertConfig()
			{
				Name = "1DQ1-A < 5 jumps",
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
			AlertConfiguration.Add(new AlertConfig()
			{
				Name = "ESS robbery",
				Enabled = false,
				AlertType = AlertConfigType.Text,
				TextDisplay = "ESS robbery {text} reported by {reporter}",
				MenuDisplay = "ESS robbery {text} reported by {reporter}",
				SoundFile = "Boo2",
				TriggerText = "ess",
			});
			AlertConfiguration.Add(new AlertConfig()
			{
				Name = "Skyhook robbery",
				Enabled = false,
				AlertType = AlertConfigType.Text,
				TextDisplay = "Skyhook robbery {text} reported by {reporter}",
				MenuDisplay = "Skyhook robbery {text} reported by {reporter}",
				SoundFile = "Boo2",
				TriggerText = "skyhook",
			});
			AlertConfiguration.Add(new AlertConfig()
			{
				Name = "Faction Spawn",
				Enabled = true,
				AlertType = AlertConfigType.FactionSpawn,
				TextDisplay = "Spawn {context}",
				MenuDisplay = "Spawn {context}",
				SoundFile = "trumpet",
				CustomTriggerEverySeconds = 30,
			});
			AlertConfiguration.Add(new AlertConfig()
			{
				Name = "Decloak",
				Enabled = true,
				AlertType = AlertConfigType.Decloak,
				TextDisplay = "Decloak by {context}",
				MenuDisplay = "Decloak by {context}",
				SoundFile = "tuba",
			});
			AlertConfiguration.Add(new AlertConfig()
			{
				Name = "Out of rock",
				Enabled = true,
				AlertType = AlertConfigType.MiningEndedRoidPopped,
				TextDisplay = "{character} rock depleated",
				MenuDisplay = "rock depleated",
				SoundFile = "pop1",
			});
			AlertConfiguration.Add(new AlertConfig()
			{
				Name = "Miner full",
				Enabled = true,
				AlertType = AlertConfigType.MiningEndedFull,
				TextDisplay = "{character} cargo full",
				MenuDisplay = "cargo full",
				SoundFile = "full",
			});

		}

		private void InitialiseLogfileDefaults()
		{
			Logfiles.Add(new LogfileConfig()
			{
				Enabled = true,
				Name = "wc.Dek+Fa+PB",
				Logfile = "wc.Dek+Fa+PB",
				LogType = Logfile.LogFileType.ChatIntel,
			});
			Logfiles.Add(new LogfileConfig()
			{
				Enabled = true,
				Name = "wc.Poch",
				Logfile = "wc.Poch",
				LogType = Logfile.LogFileType.ChatIntel,
			});
			Logfiles.Add(new LogfileConfig()
			{
				Enabled = true,
				Name = "wc.Vale",
				Logfile = "wc.Vale",
				LogType = Logfile.LogFileType.ChatIntel,
			});
			Logfiles.Add(new LogfileConfig()
			{
				Enabled = true,
				Name = "wc.Venal+Br+Te",
				Logfile = "wc.Venal+Br+Te",
				LogType = Logfile.LogFileType.ChatIntel,
			});
		}

		private void InitialiseCssDefaults()
		{
			CSSData.Add(new CSSData()
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

			CSSData.Add(new CSSData()
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

			CSSData.Add(new CSSData()
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

			CSSData.Add(new CSSData()
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

			CSSData.Add(new CSSData()
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


		private ConfigurationItemLocationInformation CreateItem(
			string name,
			Color color,
			Point location,
			Size size,
			Font font,
			Color? alternateColor = null,
			Color? backColor = null,
			bool isVisible = true)
		{
			return new ConfigurationItemLocationInformation
			{
				Name = name,
				Color = color,
				AlternateColor = alternateColor ?? Color.Empty,
				BackColor = backColor ?? Color.Transparent,
				IsVisible = isVisible,
				Location = location,
				Size = size,
				Font = font
			};
		}

		private void InitialiseMainWindowLayout()
		{
			using (var font1 = new Font("Microsoft Sans Serif", 12f, FontStyle.Bold))
			using (var font2 = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular))
			using (var font3 = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold))
			using (var font4 = new Font("Segoe UI Variable", 8.25f, FontStyle.Regular))
			using (var font5 = new Font("Segoe UI", 8.25f, FontStyle.Bold))
			using (var font6 = new Font("Segoe UI", 12f, FontStyle.Bold))
			using (var font7 = new Font("Segoe UI", 8.5f, FontStyle.Bold))
			using (var font8 = new Font("Impact", 10f, FontStyle.Regular))
			using (var font9 = new Font("Segoe UI Variable", 9f, FontStyle.Regular))
			using (var font10 = new Font("Microsoft Sans Serif", 9.3f, FontStyle.Bold))
			using (var font11 = new Font("Microsoft Sans Serif", 9.2f, FontStyle.Regular))
			using (var font12 = new Font("Segoe UI", 8.0f, FontStyle.Regular))
			using (var font13 = new Font("Segoe UI Variable", 8.25f, FontStyle.Bold))
			using (var font14 = new Font("Tahoma", 8.25f, FontStyle.Regular))
			using (var font15 = new Font("Segoe UI Variable", 20f, FontStyle.Bold))
			using (var font16 = new Font("Segoe UI Variable", 8f, FontStyle.Bold))
			using (var font17 = new Font("Verdana", 10f, FontStyle.Bold))
			using (var font18 = new Font("Tahoma", 9f, FontStyle.Regular))
			MainWindowItemLocations = new List<ConfigurationItemLocationInformation>
			{
				CreateItem("WorkingIndicator",Color.Red,new Point(625, -1),new Size(13, 15),font1),
				CreateItem("CurrentClient",Color.MediumTurquoise,new Point(2, 2),new Size(265, 21),font6),
				CreateItem("EveScoutStatus",Color.LimeGreen,new Point(0, 218),new Size(163, 13),font5),
				CreateItem("AlertsOutstanding",Color.LightCoral,new Point(240, 3),new Size(114, 11),font5),
				CreateItem("MiningData",Color.Aqua,new Point(200, 3),new Size(118, 11),font5,Color.Orange),
				CreateItem("RattingData",Color.Aqua,new Point(350, 3),new Size(118, 11),font3,Color.Orange),
				CreateItem("MainMinimap",Color.White,new Point(3, 3),new Size(260, 200),font2,Color.Red),
				CreateItem("MouseoverSystemName",Color.Orchid,new Point(445, 203),new Size(193, 11),font2,Color.Red),
				CreateItem("ExtraInformation",Color.LimeGreen,new Point(270, 18),new Size(360, 170),font4,Color.Brown),
				CreateItem("ChatServerDown",Color.Red,new Point(471, 3),new Size(152, 20),font1,Color.Brown),
				CreateItem("IntelLog",Color.LightGoldenrodYellow,new Point(471, 3),new Size(152, 20),font4,Color.Brown,ColorTranslator.FromHtml("#0f0f11"))
			};
		}
		private void InitialiseAlertWindowLayout()
		{
			using (var font1 = new Font("Microsoft Sans Serif", 12f, FontStyle.Bold))
			using (var font2 = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular))
			using (var font3 = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold))
			using (var font4 = new Font("Segoe UI Variable", 8.25f, FontStyle.Regular))
			using (var font5 = new Font("Segoe UI", 8.25f, FontStyle.Bold))
			using (var font6 = new Font("Segoe UI", 12f, FontStyle.Bold))
			using (var font7 = new Font("Segoe UI", 8.5f, FontStyle.Bold))
			using (var font8 = new Font("Impact", 10f, FontStyle.Regular))
			using (var font9 = new Font("Segoe UI Variable", 9f, FontStyle.Regular))
			using (var font10 = new Font("Microsoft Sans Serif", 9.3f, FontStyle.Bold))
			using (var font11 = new Font("Microsoft Sans Serif", 9.2f, FontStyle.Regular))
			using (var font12 = new Font("Segoe UI", 8.0f, FontStyle.Regular))
			using (var font13 = new Font("Segoe UI Variable", 8.25f, FontStyle.Bold))
			using (var font14 = new Font("Tahoma", 8.25f, FontStyle.Regular))
			using (var font15 = new Font("Segoe UI Variable", 20f, FontStyle.Bold))
			using (var font16 = new Font("Segoe UI Variable", 8f, FontStyle.Bold))
			using (var font17 = new Font("Verdana", 10f, FontStyle.Bold))
			using (var font18 = new Font("Tahoma", 9f, FontStyle.Regular)) 
			AlertWindowItemLocations = new List<ConfigurationItemLocationInformation>
			{
				CreateItem("AlertText",Color.White,new Point(0, 165),new Size(400, 12),font14),
				CreateItem("SystemName",Color.Lime,new Point(115, 4),new Size(280, 31),font15),
				CreateItem("RegionName",Color.Cyan,new Point(275, 36),new Size(120, 13),font16),
				CreateItem("ContextInfo",Color.Yellow,new Point(2, 47),new Size(396, 19),font17),
				CreateItem("IntelLine",Color.White,new Point(155, 64),new Size(240, 57),font18),
				CreateItem("ShipName",Color.Lime,new Point(180, 125),new Size(150, 13),font13),
				CreateItem("AlertImage",Color.White,new Point(335, 95),new Size(53, 57),font3),
				CreateItem("MinimapImage",Color.White,new Point(34, 3),new Size(193, 128),font3),
				CreateItem("AlertLogoKiller",Color.White,new Point(437, 138),new Size(34, 34),font3),
				CreateItem("AlertLogoVictim",Color.White,new Point(516, 122),new Size(34, 34),font3)
			};
		}
		private void InitialiseOverlayLayout()
		{
			using (var font1 = new Font("Microsoft Sans Serif", 12f, FontStyle.Bold))
			using (var font2 = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular))
			using (var font3 = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold))
			using (var font4 = new Font("Segoe UI Variable", 8.25f, FontStyle.Regular))
			using (var font5 = new Font("Segoe UI", 8.25f, FontStyle.Bold))
			using (var font6 = new Font("Segoe UI", 12f, FontStyle.Bold))
			using (var font7 = new Font("Segoe UI", 8.5f, FontStyle.Bold))
			using (var font8 = new Font("Impact", 10f, FontStyle.Regular))
			using (var font9 = new Font("Segoe UI Variable", 9f, FontStyle.Regular))
			using (var font10 = new Font("Microsoft Sans Serif", 9.3f, FontStyle.Bold))
			using (var font11 = new Font("Microsoft Sans Serif", 9.2f, FontStyle.Regular))
			using (var font12 = new Font("Segoe UI", 8.0f, FontStyle.Regular))
			using (var font13 = new Font("Segoe UI Variable", 8.25f, FontStyle.Bold))
			using (var font14 = new Font("Tahoma", 8.25f, FontStyle.Regular))
			using (var font15 = new Font("Segoe UI Variable", 20f, FontStyle.Bold))
			using (var font16 = new Font("Segoe UI Variable", 8f, FontStyle.Bold))
			using (var font17 = new Font("Verdana", 10f, FontStyle.Bold))
			using (var font18 = new Font("Tahoma", 9f, FontStyle.Regular))
			OverlayWindowItemLocations = new List<ConfigurationItemLocationInformation>
			{
				CreateItem("RattingData",Color.Aqua,new Point(440, 12),new Size(118, 15),font8,Color.Orange),
				CreateItem("MiningData",Color.Aqua,new Point(300, 323),new Size(118, 15),font8,Color.Orange),
				CreateItem("EveScoutStatus",Color.LimeGreen,new Point(1, 1),new Size(640, 13),font9),
				CreateItem("Minimap",Color.White,new Point(0, 0),new Size(500, 350),font2,Color.Red),
				CreateItem("IskPerMinuteChart",Color.Orchid,new Point(320, 32),new Size(280, 80),font2,Color.Red),
				CreateItem("MinedPerMinuteChart",Color.Orchid,new Point(320, 270),new Size(280, 80),font2,Color.Red),
				CreateItem("MouseOver",Color.Red,new Point(-1, -1),new Size(120, 80),font2,Color.Red,Color.Black),
				CreateItem("MouseOverSystemName",Color.White,new Point(2, 2),new Size(120, 11),font10,Color.Red),
				CreateItem("MouseOverSecurity",Color.Yellow,new Point(2, 13),new Size(25, 11),font11,Color.Red),
				CreateItem("MouseOverSkyhookInfo",Color.GreenYellow,new Point(2, 50),new Size(116, 22),font12,Color.Red),
				CreateItem("MouseOverNPCDelta",Color.LightSkyBlue,new Point(30, 13),new Size(30, 11),font7,Color.Red),
				CreateItem("MouseOverKills",Color.Crimson,new Point(65, 13),new Size(20, 11),font7,Color.Red),
				CreateItem("MouseOverAlerts",Color.DarkOrange,new Point(90, 13),new Size(20, 11),font7,Color.Red),
				CreateItem("MouseOverJumpsPerHour",Color.FloralWhite,new Point(2, 28),new Size(30, 11),font7,Color.Red),
				CreateItem("MouseOverKillsPerHour",Color.Chartreuse,new Point(45, 28),new Size(30, 11),font7,Color.Red),
			};
		}

		private void InitialiseDpsLayout()
		{
			DPSWindowItemLocations = new List<ConfigurationItemLocationInformation>
			{
				CreateItem("DPSChart",Color.Aqua,new Point(5, 5),new Size(300, 200),new Font("Impact", 10f, FontStyle.Regular),Color.Orange)
			};
		}

		public Config()
		{
			ApplyDefaultAttributes();
			InitialiseCollections();
			InitialiseGeneralSettings();

			InitialiseAlertDefaults();
			InitialiseLogfileDefaults();
			InitialiseCssDefaults();

			InitialiseMainWindowLayout();
			InitialiseAlertWindowLayout();
			InitialiseOverlayLayout();
			InitialiseDpsLayout();
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
				MinimapMapType = MinimapMapType,
				OverlayMinimapMapType = OverlayMinimapMapType,
				OverlayDPSWindowPosition = OverlayDPSWindowPosition,
				OverlayDPSWindowSize = OverlayDPSWindowSize,
				OverlayMapWindowPosition = OverlayMapWindowPosition,
				OverlayMapWindowSize = OverlayMapWindowSize,
			};
            // copy as many config items as possible
            foreach(var p in typeof(Config).GetProperties().Where(o => o.PropertyType == typeof(Color) || o.PropertyType == typeof(int) || o.PropertyType == typeof(string) || o.PropertyType == typeof(bool) || o.PropertyType == typeof(float)))
            {
                var propertyType = typeof(Config).GetProperty(p.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.ExactBinding);
                var value = propertyType.GetValue(this);
                propertyType.SetValue(c, value);
            }

            c.IgnoreStringRegex = new List<Regex>();
            c.IgnoreSystems = new List<int>();
            c.AlertConfiguration = new List<AlertConfig>();
            c.Logfiles = new List<LogfileConfig>();
            c.CSSData = new List<CSSData>();

            foreach (var s in IgnoreStringRegex)
            {
                c.IgnoreStringRegex.Add(s);
            }
            foreach (var s in IgnoreSystems)
            {
                c.IgnoreSystems.Add(s);
            }
            foreach (var a in AlertConfiguration)
            {
                c.AlertConfiguration.Add(new AlertConfig()
                {
                    AlertRangeType = a.AlertRangeType,
                    AlertType = a.AlertType,
                    CustomTriggerEverySeconds = a.CustomTriggerEverySeconds,
                    Enabled = a.Enabled,
                    Name = a.Name,
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
			c.DPSWindowItemLocations = DuplicateLocations(DPSWindowItemLocations);

			c.InitialPosition = new Point(InitialPosition.X, InitialPosition.Y);
			c.MainWindowSize = new Size(MainWindowSize.Width, MainWindowSize.Height);
			c.AlertWindowSize = new Size(AlertWindowSize.Width, AlertWindowSize.Height);

            return (c);
        }

		private static JsonSerializerOptions CreateJsonOptions()
		{
			var options = new JsonSerializerOptions
			{
				WriteIndented = true
			};

			options.Converters.Add(new ColorJsonConverter());

			return options;
		}

		public static Config Load(string filename)
		{
			try
			{
				if (!File.Exists(filename))
					return new Config();

				var c = new Config();

				var _MainWindowItemLocations = c.DuplicateLocations(c.MainWindowItemLocations);
				var _AlertWindowItemLocations = c.DuplicateLocations(c.AlertWindowItemLocations);
				var _OverlayWindowItemLocations = c.DuplicateLocations(c.OverlayWindowItemLocations);
				var _DPSWindowItemLocations = c.DuplicateLocations(c.DPSWindowItemLocations);

				c = JsonSerializer.Deserialize<Config>(
						   File.ReadAllText(filename),
						   CreateJsonOptions())
					   ?? new Config();

				c.MainWindowItemLocations = c.MergeLocations(c.MainWindowItemLocations, _MainWindowItemLocations);
				c.AlertWindowItemLocations = c.MergeLocations(c.AlertWindowItemLocations, _AlertWindowItemLocations);
				c.OverlayWindowItemLocations = c.MergeLocations(c.OverlayWindowItemLocations, _OverlayWindowItemLocations);
				c.DPSWindowItemLocations = c.MergeLocations(c.DPSWindowItemLocations, _DPSWindowItemLocations);

				return c;
			}
			catch
			{
				return new Config();
			}
		}

		public void Save(string filename)
        {
			var json = JsonSerializer.Serialize(this, CreateJsonOptions());
			File.WriteAllText(filename, json);
        }

		[Category("General")]
		[DisplayName("Staging system")]
		[Description("When alerts are generated relative to your home/staging system - this is the value used")]
		[DefaultValue(typeof(string), "BKG-Q2")]
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
        [DefaultValue(typeof(int), "125")]
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
        [DefaultValue(typeof(int), "4")]
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
        [DefaultValue(typeof(int), "6")]
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
        
        [Category("Main Window")]
        public Point InitialPosition { get; set; }

        [Category("Main Window")]
        public Size MainWindowSize { get; set; }

		[Category("Alert")]
		public Size AlertWindowSize { get; set; }

		[Category("Main Window")]
        [DisplayName("Stay On Top")]
        [Description("Should the main window stay on top of other windows")]
        [DefaultValue(typeof(bool), "true")]
        public bool StayOnTop { get; set; }

        [Category("Main Window")]
        [DisplayName("Text Colour")]
        [Description("The Text Foreground colour for the main window")]
        [DefaultValue(typeof(Color), "LightGoldenrodYellow")]
        public Color MainWindowTextColour { get; set; }

        [Category("Main Window")]
        [DisplayName("Background Colour")]
        [Description("Background Colour for the main window")]
        [DefaultValue(typeof(Color), "#0f0f11")]
        public Color MainWindowBackgroundColour { get; set; }

        [Category("Main Window")]
        [DisplayName("Show Minimap")]
        [Description("Show minimap")]
        [DefaultValue(typeof(bool), "true")]
        public bool ShowMinimap { get; set; }

        [Category("Main Window")]
        [DisplayName("Minimap system name colour for mouseover")]
        [Description("")]
        [DefaultValue(typeof(Color), "OrangeRed")]
        public Color MinimapMouseoverSystemColour { get; set; }

        [Category("Main Window")]
        [DisplayName("Minimap zoom delta")]
        [Description("How much to adjust zoom on mouse scroll")]
        [DefaultValue(typeof(int), "5")]
        public int MainWindowMouseScrollAmount { get; set; }

        [Category("Main Window")]
        [DisplayName("Minimap zoom initial map scale")]
        [Description("How to scale main window minimap - default 100")]
        [DefaultValue(typeof(int), "90")]
        public int MainWindowMapScale { get; set; }
        

        [Category("Alert")]
        [DisplayName("Minimap system name colour for mouseover")]
        [Description("")]
        [DefaultValue(typeof(Color), "OrangeRed")]
        public Color AlertPopupMinimapMouseoverSystemColour { get; set; }

        [Category("Main Window")]
        [DisplayName("Maptype to use")]
        [Description("Which map layout to use for the Minimap")]
        [DefaultValue(typeof(MapType), "Dotlan")]
        public MapType MinimapMapType { get; set; }


        [Category("Main Window")]
        [DisplayName("Show total ISK per Hour")]
        [Description("Show the total ISK per hour earned as well as per toon")]
        [DefaultValue(typeof(bool), "true")]
        public bool ShowISKTotal { get; set; }

        [Category("Main Window")]
        [DisplayName("Show total Mining per Hour")]
        [Description("Show the total Mining per hour as well as per toon")]
        [DefaultValue(typeof(bool), "true")]
        public bool ShowMiningTotal { get; set; }

		[Category("Main Window")]
		[DisplayName("Item Locations")]
		[Editor(typeof(LocationCollectionEditor), typeof(UITypeEditor))]
		public List<ConfigurationItemLocationInformation> MainWindowItemLocations { get; set; }

		[Category("Alert")]
		[DisplayName("Item Locations")]
		[Editor(typeof(LocationCollectionEditor), typeof(UITypeEditor))]
		public List<ConfigurationItemLocationInformation> AlertWindowItemLocations { get; set; }

		[Category("DPS")]
        [DisplayName("DPS Graph location")]
        [Description("Location to display DPS graph when required")]
        public PopupLocationType DPSLocation { get; set; }

        [Category("DPS")]
        [DisplayName("Show DPS")]
        [Description("")]
        [DefaultValue(typeof(bool), "true")]
        public bool ShowWindowOverlayDPS { get; set; }

        [Category("DPS")]
        [DisplayName("Combine DPS")]
        [Description("Should all DPS values be combined for the graph")]
        [DefaultValue(typeof(bool), "true")]
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
        [DefaultValue(typeof(int), "3")]
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

		
		private List<string> _ignoreStrings = new List<string>();
		[JsonIgnore]
		public List<Regex> IgnoreStringRegex { get; private set; } = new List<Regex>();

		public List<string> IgnoreStrings
		{
			get => _ignoreStrings;
			set
			{
				_ignoreStrings = value ?? new List<string>();

				IgnoreStringRegex = _ignoreStrings
					.Select(s => new Regex(
						s,
						RegexOptions.IgnoreCase | RegexOptions.Compiled))
					.ToList();
			}
		}

		public List<int> IgnoreSystems { get; set; }

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
        [DisplayName("Heartbeat indicator text")]
        [Description("Check homepage for some examples")]
        //        [DefaultValue(typeof(string), " ▁▂▃▄▅▆▇█▇▆▅▄▃▁")]
        //        [DefaultValue(typeof(string)," ▏▎▍▌▋▊▉▊▋▌▍")] 
        //        [DefaultValue(typeof(string), "▖▘▝▗")]
        //        [DefaultValue(typeof(string), "⠁⠁⠉⠙⠚⠒⠂⠂⠒⠲⠴⠤⠄⠄⠤⠠⠠⠤⠦⠖⠒⠐⠐⠒⠓⠋⠉⠈⠈")]
        [DefaultValue(typeof(string), "⠈⠉⠋⠓⠒⠐⠐⠒⠖⠦⠤⠠⠠⠤⠦⠖⠒⠐⠐⠒⠓⠋⠉⠈")]
        public string HeartBeatIndicatorWorkingText { get; set; }

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
		[DisplayName("Minimap system name colour for mouseover")]
		[Description("")]
		[DefaultValue(typeof(Color), "Orchid")]
		public Color OverlayMinimapMouseoverSystemColour { get; set; }

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
		[DisplayName("Mined Graph Critical Amount colour")]
		[Description("Colour to use for Mined Graph Critical Amount")]
		[DefaultValue(typeof(Color), "Blue")]
		public Color OverlayMinedGraphCriticalColour { get; set; }

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
		[Editor(typeof(LocationCollectionEditor), typeof(UITypeEditor))]
		public List<ConfigurationItemLocationInformation> OverlayWindowItemLocations { get; set; }

		[Category("DPS")]
		[DisplayName("DPS Graph alpha")]
		[Description("Alpha to use for DPS graph")]
		[DefaultValue(typeof(int), "60")]
		[Range(0, 100)]
		public int OverlayDPSGraphAlpha { get; set; }

		[Category("DPS")]
		[DisplayName("Overlay DPS position inside client")]
		[Description("Overlay DPS position inside client")]
		public Point OverlayDPSWindowPosition { get; set; }

		[Category("DPS")]
		[DisplayName("Overlay DPS Window size")]
		[Description("Size of the Overlay DPS")]
		public Size OverlayDPSWindowSize { get; set; }

		[Category("DPS")]
		[DisplayName("Item Locations")]
		[Editor(typeof(LocationCollectionEditor), typeof(UITypeEditor))]
		public List<ConfigurationItemLocationInformation> DPSWindowItemLocations { get; set; }


		// Storm Alpha - main minimap
		[Category("Main Window")]
		[DisplayName("Storm Strong Outline Alpha")]
		[Description("Alpha to use for storm indicators on the main minimap")]
		[DefaultValue(typeof(int), "155")]
		[Range(0, 255)]
		public int MinimapStormStrongOutlineAlpha { get; set; }
		[Category("Main Window")]
		[DisplayName("Storm Weak Outline Alpha")]
		[Description("Alpha to use for storm indicators on the main minimap")]
		[DefaultValue(typeof(int), "120")]
		[Range(0, 255)]
		public int MinimapStormWeakOutlineAlpha { get; set; }
		[Category("Main Window")]
		[DisplayName("Storm Centre Outline Alpha")]
		[Description("Alpha to use for storm indicators on the main minimap")]
		[DefaultValue(typeof(int), "90")]
		[Range(0, 255)]
		public int MinimapStormCentreOutlineAlpha { get; set; }
		[Category("Main Window")]
		[DisplayName("Storm Main Outline Alpha")]
		[Description("Alpha to use for storm indicators on the main minimap")]
		[DefaultValue(typeof(int), "40")]
		[Range(0, 255)]
		public int MinimapStormMainAlpha { get; set; }
		[Category("Main Window")]
		[DisplayName("Show Storm")]
		[Description("Whether to show storm indicators on the main minimap")]
		[DefaultValue(typeof(bool), "true")]
		public bool MinimapShowStorm { get; set; }
		[Category("Main Window")]
		[DisplayName("Storm Size")]
		[Description("Size of storm indicators on the main minimap")]
		[DefaultValue(typeof(int), "55")]
		public int MinimapStormSize { get; set; }

		// Storm Alpha - overlay minimap
		[Category("OverlayMap")]
		[DisplayName("Storm Strong Outline Alpha")]
		[Description("Alpha to use for storm indicators on the overlay minimap")]
		[DefaultValue(typeof(int), "155")]
		[Range(0, 255)]
		public int OverlayMinimapStormStrongOutlineAlpha { get; set; }
		[Category("OverlayMap")]
		[DisplayName("Storm Weak Outline Alpha")]
		[Description("Alpha to use for storm indicators on the overlay minimap")]
		[DefaultValue(typeof(int), "120")]
		[Range(0, 255)]
		public int OverlayMinimapStormWeakOutlineAlpha { get; set; }
		[Category("OverlayMap")]
		[DisplayName("Storm Centre Outline Alpha")]
		[Description("Alpha to use for storm indicators on the overlay minimap")]
		[DefaultValue(typeof(int), "90")]
		[Range(0, 255)]
		public int OverlayMinimapStormCentreOutlineAlpha { get; set; }
		[Category("OverlayMap")]
		[DisplayName("Storm Main Alpha")]
		[Description("Alpha to use for storm indicators on the overlay minimap")]
		[DefaultValue(typeof(int), "40")]
		[Range(0, 255)]
		public int OverlayMinimapStormMainAlpha { get; set; }
		[Category("OverlayMap")]
		[DisplayName("Show Storm")]
		[Description("Whether to show storm indicators on the overlay minimap")]
		[DefaultValue(typeof(bool), "true")]
		public bool OverlayMinimapShowStorm { get; set; }
		[Category("OverlayMap")]
		[DisplayName("Storm Size")]
		[Description("Size of storm indicators on the overlay minimap")]
		[DefaultValue(typeof(int), "55")]
		public int OverlayMinimapStormSize { get; set; }

		// Storm alpha - alert minimap
		[Category("Alert")]
		[DisplayName("Storm Strong Outline Alpha")]
		[Description("Alpha to use for storm indicators on the alert popup minimap")]
		[DefaultValue(typeof(int), "155")]
		[Range(0, 255)]
		public int AlertPopupMinimapStormStrongOutlineAlpha { get; set; }
		[Category("Alert")]
		[DisplayName("Storm Weak Outline Alpha")]
		[Description("Alpha to use for storm indicators on the alert popup minimap")]
		[DefaultValue(typeof(int), "120")]
		[Range(0, 255)]
		public int AlertPopupMinimapStormWeakOutlineAlpha { get; set; }
		[Category("Alert")]
		[DisplayName("Storm Centre Outline Alpha")]
		[Description("Alpha to use for storm indicators on the alert popup minimap")]
		[DefaultValue(typeof(int), "90")]
		[Range(0, 255)]
		public int AlertPopupMinimapStormCentreOutlineAlpha { get; set; }
		[Category("Alert")]
		[DisplayName("Storm Main Outline Alpha")]
		[Description("Alpha to use for storm indicators on the alert popup minimap")]
		[DefaultValue(typeof(int), "40")]
		[Range(0, 255)]
		public int AlertPopupMinimapStormMainAlpha { get; set; }
		[Category("Alert")]
		[DisplayName("Show Storm")]
		[Description("Whether to show storm indicators on the alert popup minimap")]
		[DefaultValue(typeof(bool), "true")]
		public bool AlertPopupMinimapShowStorm { get; set; }
		[Category("Alert")]
		[DisplayName("Storm Size")]
		[Description("Size of storm indicators on the alert popup minimap")]
		[DefaultValue(typeof(int), "300")]
		public int AlertPopupMinimapStormSize { get; set; }


		// Seclimit - main minimap
		[Category("Main Window")]
		[DisplayName("Minimap sec limit Indicator colour")]
		[Description("Colour to use for security limit indicators on the minimap")]
		[DefaultValue(typeof(Color), "DarkBlue")]
		public Color MinimapSecLimitColour { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap sec limit Indicator outline colour")]
		[Description("Colour to use for security limit indicators on the minimap")]
		[DefaultValue(typeof(Color), "Blue")]
		public Color MinimapSecLimitOutlineColour { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap sec limit Indicator enabled")]
		[Description("use security limit indicators on the minimap")]
		[DefaultValue(typeof(bool), "true")]
		public bool MinimapSecLimitShow { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap seclimit Indicator alpha")]
		[Description("Alpha to use for seclimit indicators on the minimap")]
		[DefaultValue(typeof(int), "70")]
		[Range(0, 100)]
		public int MinimapSecLimitAlpha { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap seclimit cloud size")]
		[Description("Size to use for seclimit indicators on the minimap")]
		[DefaultValue(typeof(int), "60")]
		[Range(0, 100)]
		public int MinimapSecLimitCloudSize { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap seclimit")]
		[Description("seclimit on the minimap")]
		[DefaultValue(typeof(float), "-0.7")]
		[Range(0, 100)]
		public float MinimapSecLimitValue { get; set; }

		// seclimit Overlay minimap
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
		public bool OverlayMinimapSecLimitShow { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap seclimit Indicator alpha")]
		[Description("Alpha to use for seclimit indicators on the minimap")]
		[DefaultValue(typeof(int), "70")]
		[Range(0, 100)]
		public int OverlayMinimapSecLimitAlpha { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap seclimit cloud size")]
		[Description("Size to use for seclimit indicators on the minimap")]
		[DefaultValue(typeof(int), "60")]
		[Range(0, 100)]
		public int OverlayMinimapSecLimitCloudSize { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap seclimit")]
		[Description("seclimit on the minimap")]
		[DefaultValue(typeof(float), "-0.7")]
		[Range(0, 100)]
		public float OverlayMinimapSecLimitValue { get; set; }

		// Seclimit Alert minimap
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
		public bool AlertPopupMinimapSecLimitShow { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap seclimit Indicator alpha")]
		[Description("Alpha to use for seclimit indicators on the minimap")]
		[DefaultValue(typeof(int), "70")]
		[Range(0, 100)]
		public int AlertPopupMinimapSecLimitAlpha { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap seclimit cloud size")]
		[Description("Size to use for seclimit indicators on the minimap")]
		[DefaultValue(typeof(int), "60")]
		[Range(0, 100)]
		public int AlertPopupMinimapSecLimitCloudSize { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap seclimit")]
		[Description("seclimit on the minimap")]
		[DefaultValue(typeof(float), "-0.7")]
		[Range(0, 100)]
		public float AlertPopupMinimapSecLimitValue { get; set; }

		// Overlap map Incursion
		[Category("OverlayMap")]
		[DisplayName("Minimap incursion Indicator colour")]
		[Description("Colour to use for incursion indicators on the overlay minimap")]
		[DefaultValue(typeof(Color), "DarkGreen")]
		public Color OverlayMinimapIncursionColour { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap incursion Indicator outline colour")]
		[Description("Colour to use for outline for incursion indicators on the overlay minimap")]
		[DefaultValue(typeof(Color), "Red")]
		public Color OverlayMinimapIncursionOutlineColour { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap incursion Indicator alpha")]
		[Description("Alpha to use for incursion indicators on the overlay minimap")]
		[DefaultValue(typeof(int), "60")]
		[Range(0, 100)]
		public int OverlayMinimapIncursionAlpha { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap incursion cloud size")]
		[Description("Size to use for incursion indicators on the overlay minimap")]
		[DefaultValue(typeof(int), "65")]
		[Range(0, 100)]
		public int OverlayMinimapIncursionCloudSize { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Show Incursion")]
		[Description("Whether to show incursion indicators on the overlay minimap")]
		[DefaultValue(typeof(bool), "true")]
		public bool OverlayMinimapIncursionShow { get; set; }

		// Main window Incursion
		[Category("Main Window")]
		[DisplayName("Minimap incursion Indicator colour")]
		[Description("Colour to use for incursion indicators on the minimap")]
		[DefaultValue(typeof(Color), "DarkGreen")]
		public Color MinimapIncursionColour { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap incursion Indicator outline colour")]
		[Description("Colour to use for outline for incursion indicators on the minimap")]
		[DefaultValue(typeof(Color), "Red")]
		public Color MinimapIncursionOutlineColour { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap incursion Indicator alpha")]
		[Description("Alpha to use for incursion indicators on the minimap")]
		[DefaultValue(typeof(int), "60")]
		[Range(0, 100)]
		public int MinimapIncursionAlpha { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap incursion cloud size")]
		[Description("Size to use for incursion indicators on the minimap")]
		[DefaultValue(typeof(int), "65")]
		[Range(0, 100)]
		public int MinimapIncursionCloudSize { get; set; }

		[Category("Main Window")]
		[DisplayName("Show Incursion")]
		[Description("Whether to show incursion indicators on the main minimap")]
		[DefaultValue(typeof(bool), "true")]
		public bool MinimapIncursionShow { get; set; }


		// Alert popup Incursion
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
		[DisplayName("Show Incursion")]
		[Description("Whether to show incursion indicators on the Alert minimap")]
		[DefaultValue(typeof(bool), "true")]
		public bool AlertPopupMinimapIncursionShow { get; set; }


		[Category("OverlayMap")]
		[DisplayName("Minimap Border Colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "SteelBlue")]
		public Color OverlayMinimapBorderColour { get; set; }

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
		public Color OverlayMinimapColourAllText { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap connecting system name colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "DarkKhaki")]
		public Color OverlayMinimapColourConnectingText { get; set; }

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
		[DisplayName("Minimap Alt Indicator colour")]
		[Description("Colour to use for the logged in alt indicators on the minimap")]
		[DefaultValue(typeof(Color), "Aqua")]
		public Color OverlayMinimapAltColour { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap Unihole Indicator colour")]
		[Description("Colour to use for Unihole indicators on the minimap")]
		[DefaultValue(typeof(Color), "Fuchsia")]
		public Color OverlayMinimapUniHoleColour { get; set; }

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
		[DisplayName("Minimap Wormhole colour")]
		[Description("Colour to use for Wormhole system on the minimap")]
		[DefaultValue(typeof(Color), "DeepPink")]
		public Color OverlayMinimapWormholeColour { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap Wormhole Outline colour")]
		[Description("Colour to use for Wormhole Outline on the minimap")]
		[DefaultValue(typeof(Color), "DarkKhaki")]
		public Color OverlayMinimapWormholeOutlineColour { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap Wormhole alpha")]
		[Description("Alpha to use for Wormhole on the overlay minimap")]
		[DefaultValue(typeof(int), "60")]
		[Range(0, 100)]
		public int OverlayMinimapWormholeAlpha { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap Wormhole size")]
		[Description("Size to use for Wormhole on the minimap")]
		[DefaultValue(typeof(int), "30")]
		[Range(0, 100)]
		public int OverlayMinimapWormholeSize { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap Border Colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "SteelBlue")]
		public Color MinimapBorderColour { get; set; }
		[Category("Main Window")]
		[DisplayName("Minimap Normal connection colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "Gainsboro")]
		public Color MinimapLineColourNormal { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap Out of region connection colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "DimGray")]
		public Color MinimapLineColourOutOfRegion { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap Route connection colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "Tomato")]
		public Color MinimapLineColourRoute { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap normal system colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "Gainsboro")]
		public Color MinimapSystemColourNormal { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap route system colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "Pink")]
		public Color MinimapSystemColourRoute { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap from system colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "Red")]
		public Color MinimapSystemColourFrom { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap to system colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "Green")]
		public Color MinimapSystemColourTo { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap system name colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "LemonChiffon")]
		public Color MinimapSystemColourText { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap all system name colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "PaleGoldenrod")]
		public Color MinimapColourAllText { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap connecting system name colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "DarkKhaki")]
		public Color MinimapColourConnectingText { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap zkill indicator colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "Red")]
		public Color MinimapZKillColour { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap intel indicator colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "Orange")]
		public Color MinimapIntelColour { get; set; }
		[Category("Main Window")]
		[DisplayName("Minimap Alt Indicator colour")]
		[Description("Colour to use for the logged in alt indicators on the minimap")]
		[DefaultValue(typeof(Color), "Aqua")]
		public Color MinimapAltColour { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap Unihole Indicator colour")]
		[Description("Colour to use for Unihole indicators on the minimap")]
		[DefaultValue(typeof(Color), "Fuchsia")]
		public Color MinimapUniHoleColour { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap Ice Indicator colour")]
		[Description("Colour to use for ice indicatos on the minimap")]
		[DefaultValue(typeof(Color), "Cyan")]
		public Color MinimapIceColour { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap Mercoxit Indicator colour")]
		[Description("Colour to use for Mercoxit indicatos on the minimap")]
		[DefaultValue(typeof(Color), "IndianRed")]
		public Color MinimapMercoxitColour { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap Wormhole colour")]
		[Description("Colour to use for Wormhole system on the minimap")]
		[DefaultValue(typeof(Color), "DeepPink")]
		public Color MinimapWormholeColour { get; set; }
		[Category("Main Window")]
		[DisplayName("Minimap Wormhole outline colour")]
		[Description("Colour to use for wormhole system outline on the minimap")]
		[DefaultValue(typeof(Color), "DarkKhaki")]
		public Color MinimapWormholeOutlineColour { get; set; }


		[Category("Main Window")]
		[DisplayName("Minimap wormhole alpha")]
		[Description("Alpha to use for wormhole indicators on the minimap")]
		[DefaultValue(typeof(int), "60")]
		[Range(0, 100)]
		public int MinimapWormholeAlpha { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap wormhole size")]
		[Description("Size to use for wormhole indicators on the minimap")]
		[DefaultValue(typeof(int), "32")]
		[Range(0, 100)]
		public int MinimapWormholeSize { get; set; }


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

		[Category("Main Window")]
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
		public Color AlertPopupMinimapColourAllText { get; set; }
		[Category("Alert")]
		[DisplayName("Minimap connecting system name colour")]
		[Description("")]
		[DefaultValue(typeof(Color), "DarkKhaki")]
		public Color AlertPopupMinimapColourConnectingText { get; set; }

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
		public Color AlertPopupMinimapAltColour { get; set; }

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
		[DisplayName("Minimap Mercoxit Indicator colour")]
		[Description("Colour to use for Wormhole on the alert popup")]
		[DefaultValue(typeof(Color), "DeepPink")]
		public Color AlertPopupMinimapWormholeColour { get; set; }
		[Category("Alert")]
		[DisplayName("Minimap Mercoxit Indicator colour")]
		[Description("Colour to use for Wormhole outline on the alert popup")]
		[DefaultValue(typeof(Color), "DarkKhaki")]
		public Color AlertPopupMinimapWormholeOutlineColour { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap wormhole alpha")]
		[Description("Alpha to use for wormhole on the minimap")]
		[DefaultValue(typeof(int), "70")]
		[Range(0, 100)]
		public int AlertPopupMinimapWormholeAlpha { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap wormhole size")]
		[Description("Size to use for wormhole on the minimap")]
		[DefaultValue(typeof(int), "32")]
		[Range(0, 100)]
		public int AlertPopupMinimapWormholeSize { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap Jumps")]
		[Description("How many jumps to show in the minimap away from starting system")]
		[DefaultValue(typeof(int), "3")]
		public int MinimapCloseSystems { get; set; }

		[Category("Main Window")]
		[DisplayName("Show Minimap Border")]
		[Description("Show border on the minimap")]
		[DefaultValue(typeof(bool), "false")]
		public bool MinimapShowBorder { get; set; }

		[Category("Main Window")]
		[DisplayName("Show system text on minimap")]
		[Description("")]
		[DefaultValue(typeof(bool), "true")]
		public bool MinimapShowSystemText { get; set; }

		[Category("Main Window")]
		[DisplayName("Show all system text on minimap")]
		[Description("")]
		[DefaultValue(typeof(bool), "false")]
		public bool MinimapShowAllSystemText { get; set; }

		[Category("Main Window")]
		[DisplayName("Show connecting system text on minimap")]
		[Description("")]
		[DefaultValue(typeof(bool), "true")]
		public bool MinimapShowConnectingSystemsText { get; set; }

		[Category("Main Window")]
		[DisplayName("Use default text size on minimap")]
		[Description("")]
		[DefaultValue(typeof(bool), "false")]
		public bool MinimapDefaultTextSize { get; set; }

		[Category("Main Window")]
		[DisplayName("Minimap show out of region systems")]
		[Description("Show systems outside current region if in configured jumps range {MinimapJumps}")]
		[DefaultValue(typeof(bool), "false")]
		public bool MinimapShowOutOfRegion { get; set; }




		[Category("OverlayMap")]
		[DisplayName("Minimap Jumps")]
		[Description("How many jumps to show in the minimap away from starting system")]
		[DefaultValue(typeof(int), "3")]
		public int OverlayMinimapCloseSystems { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Show Minimap Border")]
		[Description("Show border on the minimap")]
		[DefaultValue(typeof(bool), "false")]
		public bool OverlayMinimapShowBorder { get; set; }

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

		public bool OverlayMinimapShowConnectingSystemsText { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Maptype to use")]
		[Description("Which map layout to use for the Minimap")]
		[DefaultValue(typeof(MapType), "Dotlan")]
		public MapType OverlayMinimapMapType { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap show out of region systems")]
		[Description("Show systems outside current region if in configured jumps range {MinimapJumps}")]
		[DefaultValue(typeof(bool), "false")]
		public bool OverlayMinimapShowOutOfRegion { get; set; }



		[Category("Main Window")]
		[DisplayName("Show Extra Information")]
		[Description("The extra information display shows zkill and alert activity recently in systems on the current minimap view")]
		[DefaultValue(typeof(bool), "false")]
		public bool MainWindowShowExtraInformation { get; set; }


		[Category("Main Window")]
		[DisplayName("Minimap Storm glow layers")]
		[Description("Number of glow layers for storms on the minimap")]
		[DefaultValue(typeof(int), "3")]
		[Range(0, 15)]
		public int MinimapStormGlowLayers { get; set; }

		[Category("Alert")]
		[DisplayName("Minimap Storm glow layers")]
		[Description("Number of glow layers for storms on the alert popup minimap")]
		[DefaultValue(typeof(int), "3")]
		[Range(0, 15)]
		public int AlertPopupMinimapStormGlowLayers { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Minimap Storm glow layers")]
		[Description("Number of glow layers for storms on the overlay minimap")]
		[DefaultValue(typeof(int), "6")]
		[Range(0, 15)]
		public int OverlayMinimapStormGlowLayers { get; set; }

		[Category("OverlayMap")]
		[DisplayName("Mouse Over Graph Alpha")]
		[Description("Alpha to use for the mouse over graph on the overlay minimap")]
		[DefaultValue(typeof(int), "95")]
		[Range(0, 100)]
		public int OverlayMouseOverGraphAlpha { get; set; }

	}
}