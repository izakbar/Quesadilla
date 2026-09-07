using Microsoft.Toolkit.Uwp.Notifications;
using Quesadilla.Classes;
using Quesadilla.Classes.Alerts;
using Quesadilla.Classes.Configuration;
using Quesadilla.Classes.Dotlan;
using Quesadilla.Classes.ESIData;
using Quesadilla.Classes.Events;
using Quesadilla.Classes.EVEOPreview;
using Quesadilla.Classes.EveScout;
using Quesadilla.Classes.ImageGeneration;
using Quesadilla.Classes.InterOp;
using Quesadilla.Classes.Logfile;
using Quesadilla.Classes.Processes;
using Quesadilla.Classes.Queues;
using Quesadilla.Classes.SolarSystems;
using Quesadilla.Classes.ZKill;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static Quesadilla.Extensions.StringExtensions;

namespace Quesadilla
{
    public partial class Quesadilla : Form
    {
        private List<String> _FactionRats = new List<string>()
        {
            "Dark Blood",
            "Dread Gurista",
            "True Sansha",
            "Sentient",
            "Shadow Serpentis",
            "Domination",
            " Titan",
            " Dreadnought",

            " Hauler",
            " Ferrier",
            " Gatherer",
            " Harvester",
            " Loader",
            " Bulker",
            " Carrier",
            " Convoy",
            " Courier",
            " Trailer",
            " Transporter",
            " Trucker",
        };

        public Bitmap latestAlertBitmap = null;
        public DateTime latestAlertTimestamp = DateTime.Now;

        private readonly Regex _CombatRegexMissesYouCompletely = new Regex("(?<combatEntity>.*)\\smisses\\syou\\scompletely", RegexOptions.Compiled);
        private readonly Regex _CombatRegexCombatData = new Regex("^<color=0x(?<combatColour>........)><b>(?<combatAmount>.*)</b>\\s*<color=0x........><font size=10>\\s*(?<combatContext>.*)\\s*</font>\\s*<b><color=0xffffffff>(?<combatEntity>.*)</b>(<font size=10>|<color=0x77ffffff>)*\\s-\\s(?<combatText>.*)", RegexOptions.Compiled);

        private readonly Regex _MiningRegexRoidExploded = new Regex("(?<miningDevice>.*)\\sdeactivates as it finds the resource it was harvesting\\s(?<miningHowItEnded>.*)", RegexOptions.Compiled);
        private readonly Regex _MiningRegexCargoFull = new Regex("Your\\s*(?<miningDevice>.*)\\s*has completed operations. Ship.s cargo hold is full.*", RegexOptions.Compiled);

        private readonly Regex _MiningRegexAmounts = new Regex("^<color=.*>\\s*You mined\\s*<font size=..><color=.*>(?<miningAmount>.*)<color=.*><font size=..>\\s*units* of\\s*<color=.*><font size=..>\\s*(?<miningContext>.*)", RegexOptions.Compiled);
        private readonly Regex _MiningRegexAmountsCritical = new Regex("^^<color=.*>\\s*Critical mining success!\\s*<color=.*><font size=..>\\sYou mined an additional\\s<color=.*><font size=..>(?<miningAmount>.*)<color=.*><font size=..>\\s*units* of\\s*<color=.*><font size=..>\\s*(?<miningContext>.*)", RegexOptions.Compiled);
        private readonly Regex _MiningRegexAmountsResidue = new Regex("^^<color=.*>\\s*Additional\\s*<font size=..><color=.*>(?<miningResidue>.*)<color=.*><font size=..>\\s*units depleted from asteroid as residue", RegexOptions.Compiled);
        // normal: <color=0x77ffffff>You mined <font size=12><color=#ff8dc169>40<color=0x77ffffff><font size=10> units of <color=0xffffffff><font size=12>Arkonor
        // bonus:  <color=#fff0ff45>Critical mining success!<color=0x77ffffff><font size=10> You mined an additional <color=#fff0ff45><font size=12>107<color=0x77ffffff><font size=10> units of <color=0xffffffff><font size=12>Otavite
        // residue: <color=0x77ffffff>Additional <font size=12><color=#ffff454b>23<color=0x77ffffff><font size=10> units depleted from asteroid as residue

        private readonly Regex _DecloakRegexMobileObservatory = new Regex("cloak deactivates due to a pulse from a Mobile Observatory", RegexOptions.Compiled);
        private readonly Regex _DecloakRegexProximity = new Regex("Your cloak deactivates due to proximity to (?<decloakProximity>.*)", RegexOptions.Compiled);

        private readonly Regex _ISKRegexISKData = new Regex("^<font size=12><b><color=0x........>\\s*(?<iskAmount>.*) ISK\\s*</b><color=0x........> added to next bounty payout", RegexOptions.Compiled);

        private Queue<LogEntry> _newIntelQueue = new Queue<LogEntry>();

        private int _intelSizeMax = 200;
        private int _intelSizeMin = 48;
        private int _windowExtraHeight = 60;
		private const int _maxTrayTooltipLength = 63;
        bool _refreshProcessList = false;
		private bool shouldUpdateMinimap = false;
        private bool shouldUpdatePlanetInfo = false;

        private MenuItem menuConfiguredAlerts = new MenuItem("Configured Alerts");
        private MenuItem menuBackgrounds = new MenuItem("Popup Background");
        private MenuItem menuLatestAlerts = new MenuItem("Latest Alerts");
        private MenuItem menuLogWatchers = new MenuItem("Logfiles");
        private MenuItem menuConfiguration = new MenuItem("Configuration");
        private MenuItem menuConfigurationWindow = new MenuItem("Edit Configuration");

        private MenuItem menuConfigurationAlwaysOnTop = new MenuItem("Always on top");

        private MenuItem menuConfigurationDPSShowDPS = new MenuItem("Show DPS");
        private MenuItem menuConfigurationDPSShowZkill = new MenuItem("Show ZKill Alerts");
        private MenuItem menuConfigurationDPSSaveCSV = new MenuItem("DPS Save CSV");
        private MenuItem menuConfigurationDPSShowAll = new MenuItem("DPS Combine");
        private DotlanManager DotlanProcessor;

        public bool AlertShowing = false;

        private bool ChatServerStatus = true;
        private bool InCombat = false;
        private bool CombatAdded = false;

        private string combatNeutOut = "ff7fffff";

        private bool ConfigurationWindowOpen = false;

        private List<PlanetInfoDisplay> CurrentPlanetInfos = new List<PlanetInfoDisplay>();

		public TickManager TickManagerUpdates;
		public TickManager TickManagerUI;

        public QuesadillaSplashScreen SplashScreen;

		public QuesadillaOverlayMap OverlayMapWindow = null;
		public QuesadillaOverlayDPS OverlayDPSWindow = null;

		private Color OriginalBackgroundColour;
        private int mapScale = 100;

        private List<ProcessInfo> knowProcesses = new List<ProcessInfo>();
        public string CurrentActiveToon;

        // TODO - config things - really need to go into config structure later

        private NotifyIcon trayIcon;

        private DateTime LastFactionAlert = DateTime.Now;

        public User32NativeMethods.Rect currentEveWindowRect;

        private List<MinimapSystem> MinimapSystems = new List<MinimapSystem>();
        private List<MinimapSystem> mouseOverMinimapSystems = new List<MinimapSystem>();

        private int activityLine = 0;
        private int ActivityLineInc = 1;
        private int ActivityLineMax = 1;

        AnimationManager AnimationManager = new AnimationManager();
		private bool ISKPerHourTotal = false;
        private bool MiningPerHourTotal = false;

        private bool NoActiveEveProcess = false;
		private Font _planetInfoFont = new Font("Verdana", 10, FontStyle.Regular);
		private int _cachedPlanetSystemId = -1;
        private List<PlanetSummary> _cachedPlanetInfo;
        private MinimapParameters _minimapParameters = new MinimapParameters();
		public Quesadilla()
        {
            OriginalBackgroundColour = BackColor;

            InitializeComponent();
            Program.InitializeResources();

            this.TitleIcon.Image = Properties.Resources.q.ToBitmap();

            this.MainMinimap.MouseWheel += pMinimapImage_MouseWheel;

            // Initialize tray
            trayIcon = new NotifyIcon()
            {
                Icon = Properties.Resources.q,
                ContextMenu = new ContextMenu(),
                Visible = true
            };

            trayIcon.ContextMenu = new ContextMenu(new MenuItem[]
            {
                    menuConfiguration,
                    menuLatestAlerts,
                    new MenuItem("-"),
                    new MenuItem("Exit", new EventHandler(trayIconMenuClose)),
            });

            trayIcon.MouseDoubleClick += trayIconDoubleClick;

			TickManagerUpdates = new TickManager(Program.MainConfiguration.TickMiliseconds);
			TickManagerUpdates.RegisterTick("UpdateEveOLocations", 500, Program.MainConfiguration.HowManyTicksBeforeEveOLocationUpdate, RefreshEveLocations);
			TickManagerUpdates.RegisterTick("IncursionData", 10000, Program.MainConfiguration.HowManyTicksBeforeIncursionData, RefreshIncursionData);
			TickManagerUpdates.RegisterTick("SinceLastCleardown",0,Program.MainConfiguration.HowManyTicksBeforeCleardown,PerformCleardown);
			TickManagerUpdates.RegisterTick("SinceLastDPSGraphUpdate",0,Program.MainConfiguration.HowManyTicksBeforeDPSGraphUpdate,UpdateDPSGraphWindow);
			TickManagerUpdates.RegisterTick("GeneralTick", 0, 2, GeneralTick);

			TickManagerUI = new TickManager(Program.MainConfiguration.TickMiliseconds);
			TickManagerUI.RegisterTick("AnimationRefresh", 0, Program.MainConfiguration.HowManyTicksMapAnimation, RefreshAnimations);
			TickManagerUI.RegisterTick("MiningDetail", 0, Program.MainConfiguration.HowManyTicksMiningCSVOutput, RefreshMiningCSV);
			TickManagerUI.RegisterTick("MiningPerHour", 0, Program.MainConfiguration.HowManyTicksBeforeSwitchMiningPerHour, RefreshMiningPerHour);
			TickManagerUI.RegisterTick("ISKPerHour", 0, Program.MainConfiguration.HowManyTicksBeforeSwitchISKPerHour, RefreshISKPerHour);
			TickManagerUI.RegisterTick("SinceLastAlert", 0, Program.MainConfiguration.HowManyTicksBeforeAlert, ProcessAlertQueue);

			TickManagerUI.RegisterTick("RefreshProcesses", 0, Program.MainConfiguration.HowManyTicksRefreshProcesses, RefreshProcesses);
			TickManagerUI.RegisterTick("UpdateProcesses", 0, 2, UpdateProcessesTick);

			menuConfiguration.MenuItems.Add(menuConfigurationWindow = new MenuItem("Edit Configuration", trayIconMenuConfigurationClick));
            menuConfiguration.MenuItems.Add(new MenuItem("-"));

            menuConfiguration.MenuItems.Add(menuConfigurationAlwaysOnTop = new MenuItem("Always on top", trayIconMenuAlwaysOnTop) { Checked = Program.MainConfiguration.StayOnTop, });
            menuConfiguration.MenuItems.Add(menuConfigurationDPSShowDPS = new MenuItem("Show DPS", trayIconMenuDPSShowWindow) { Checked = Program.MainConfiguration.ShowWindowOverlayDPS, });
            menuConfiguration.MenuItems.Add(menuConfigurationDPSShowZkill = new MenuItem("Show ZKill Alerts", trayIconMenuShowZKillAlerts) { Checked = Program.MainConfiguration.ShowZKillAlerts, });
            menuConfiguration.MenuItems.Add(menuConfigurationDPSSaveCSV = new MenuItem("DPS Save CSV", trayIconMenuDPSSaveCSV) { Checked = Program.MainConfiguration.DPSSaveCSV, });
            menuConfiguration.MenuItems.Add(menuConfigurationDPSShowAll = new MenuItem("DPS Combine", trayIconMenuDPSAllToons) { Checked = Program.MainConfiguration.DPSAllToons, });

            menuConfiguration.MenuItems.Add(menuBackgrounds);
            menuConfiguration.MenuItems.Add(menuConfiguredAlerts);
            menuConfiguration.MenuItems.Add(menuLogWatchers);

            ReloadConfiguration();

            Program.EveScoutProcessor.EveScoutEventHandler += new EventHandler<EveScoutEventArgs>(this.EveScout_ProcessNewData);

            DotlanProcessor = new DotlanManager();
            DotlanProcessor.DotlanEventHandler += new EventHandler<DotlanEventArgs>(this.Dotlan_ProcessNewData);

            SetupLogfileEvents();

            RebuildLogfilesMenu();
            Program.LogWatcherManager.Start();

            // Program.ZKillHandler.ZKillFeedRedisQ.ZKillEventHandler += new EventHandler<ZKillEventArgs>(this.ZKill_ProcessNewData);
            Program.ZKillHandler.ZKillFeedR2Z2.ZKillEventHandler += new EventHandler<ZKillEventArgs>(this.ZKill_ProcessNewData);
            Program.ESIHandler.ESIEventHandler += new EventHandler<ESIEventArgs>(this.ESI_ProcessNewData);

            ActivityLineInc = 1;
            ActivityLineMax = Program.MainConfiguration.HeartBeatIndicatorWorkingText.Length - 1;

            foreach (Control c in this.TopbarPanel.Controls)
            {
                if (!(c is System.Windows.Forms.Button)) //Change here depending on the Library you use for your contols
                {
                    c.MouseDown += TopbarPanel_MouseDown;
                }
            }

            // get processes
            UpdateEveProcesses(true);

            GetSkyhookTimers();
            Program.ESIHandler.EnqueueRequestIncursions();

			if (Program.MainConfiguration.ShowWindowOverlayMap)
			{
				OverlayMapWindow = new QuesadillaOverlayMap(this);
				OverlayMapWindow.Show();
			}

			if (Program.MainConfiguration.ShowWindowOverlayDPS)
			{
				OverlayDPSWindow = new QuesadillaOverlayDPS(this);
				OverlayDPSWindow.Show();
			}

            if (Program.MainConfiguration.ShowAlertWindowsNotification) SetupToastAlert();

			TickManagerUpdates.Start();
			TickManagerUI.Start();
		}

        private void UpdateMinimapParameters()
		{
			_minimapParameters.StormStrongOutlineAlpha = Program.MainConfiguration.MinimapStormStrongOutlineAlpha;
			_minimapParameters.StormWeakOutlineAlpha = Program.MainConfiguration.MinimapStormWeakOutlineAlpha;
			_minimapParameters.StormCentreOutlineAlpha = Program.MainConfiguration.MinimapStormCentreOutlineAlpha;
			_minimapParameters.StormMainAlpha = Program.MainConfiguration.MinimapStormMainAlpha;
			_minimapParameters.ShowStorm = Program.MainConfiguration.MinimapShowStorm;
			_minimapParameters.StormGlowLayers = Program.MainConfiguration.MinimapStormGlowLayers;

			_minimapParameters.SecLimitValue = Program.MainConfiguration.MinimapSecLimitValue;
            _minimapParameters.SecLimitAlphaAmount = Program.MainConfiguration.MinimapSecLimitAlpha;
            _minimapParameters.SecLimitCloudSize = Program.MainConfiguration.MinimapSecLimitCloudSize;
            _minimapParameters.SecLimitShow = Program.MainConfiguration.MinimapSecLimitShow;
			_minimapParameters.SecLimitColour = Program.MainConfiguration.MinimapSecLimitColour;
            _minimapParameters.SecLimitOutlineColour = Program.MainConfiguration.MinimapSecLimitOutlineColour;
            _minimapParameters.StormSize = Program.MainConfiguration.MinimapStormSize;

            _minimapParameters.IncursionAlphaAmount = Program.MainConfiguration.MinimapIncursionAlpha;
			_minimapParameters.IncursionCloudSize = Program.MainConfiguration.MinimapIncursionCloudSize;
			_minimapParameters.IncursionColour = Program.MainConfiguration.MinimapIncursionColour;
			_minimapParameters.IncursionOutlineColour = Program.MainConfiguration.MinimapIncursionOutlineColour;
			_minimapParameters.IncursionShow = Program.MainConfiguration.MinimapIncursionShow;

			_minimapParameters.BorderColor = Program.MainConfiguration.MinimapBorderColour;
			_minimapParameters.LineColourNormal = Program.MainConfiguration.MinimapLineColourNormal;
			_minimapParameters.LineColourOutOfRegion = Program.MainConfiguration.MinimapLineColourOutOfRegion;
			_minimapParameters.LineColourRoute = Program.MainConfiguration.MinimapLineColourRoute;
			_minimapParameters.SystemColourNormal = Program.MainConfiguration.MinimapSystemColourNormal;
			_minimapParameters.SystemColourRoute = Program.MainConfiguration.MinimapSystemColourRoute;
			_minimapParameters.SystemColourFrom = Program.MainConfiguration.MinimapSystemColourFrom;
			_minimapParameters.SystemColourTo = Program.MainConfiguration.MinimapSystemColourTo;
			_minimapParameters.SystemColourText = Program.MainConfiguration.MinimapSystemColourText;
			_minimapParameters.SystemColourAllText = Program.MainConfiguration.MinimapColourAllText;
			_minimapParameters.SystemColourConnectingText = Program.MainConfiguration.MinimapColourConnectingText;
			_minimapParameters.ZKillColour = Program.MainConfiguration.MinimapZKillColour;
			_minimapParameters.IntelColour = Program.MainConfiguration.MinimapIntelColour;
			_minimapParameters.AltColour = Program.MainConfiguration.MinimapAltColour;
			_minimapParameters.UniHoleColour = Program.MainConfiguration.MinimapUniHoleColour;
			_minimapParameters.IceColour = Program.MainConfiguration.MinimapIceColour;
			_minimapParameters.MercoxitColour = Program.MainConfiguration.MinimapMercoxitColour;

            _minimapParameters.CloseSystems = Program.MainConfiguration.MinimapCloseSystems;
            _minimapParameters.MapType = Program.MainConfiguration.MinimapMapType;
            _minimapParameters.ShowSystemText = Program.MainConfiguration.MinimapShowSystemText;
			_minimapParameters.ShowAllSystemsText = Program.MainConfiguration.MinimapShowAllSystemText;
			_minimapParameters.ShowConnectingSystemsText = Program.MainConfiguration.MinimapShowConnectingSystemsText;
            _minimapParameters.ShowBorder = Program.MainConfiguration.MinimapShowBorder;
            _minimapParameters.DefaultTextSize = Program.MainConfiguration.MinimapDefaultTextSize;
            _minimapParameters.ShowOutOfRegion = Program.MainConfiguration.MinimapShowOutOfRegion;

			if (OverlayMapWindow != null)
			{
				OverlayMapWindow.UpdateMinimapParameters();
			}
		}


		private void EveScoutProcessor_EveScoutEventHandler(object sender, EveScoutEventArgs e)
        {
            throw new NotImplementedException();
        }

		private void RebuildLogfilesMenu()
		{
			menuLogWatchers.MenuItems.Clear();

			foreach (var watcherFriendlyName in Program.LogWatcherManager.GetFriendlyNames())
			{
				var watcher = Program.LogWatcherManager.GetWatcherByFriendlyName(watcherFriendlyName);
				var item = new MenuItem(
					watcherFriendlyName,
					trayIconMenuLogfileClick)
				{
					Checked = watcher.Enabled,
					Tag = watcher
				};

				menuLogWatchers.MenuItems.Add(item);
			}
		}
		private void RebuildLatestAlertsMenu()
        {
            menuLatestAlerts.MenuItems.Clear();
            bool first = true;
            DateTime now = DateTime.Now;

            foreach (var alert in Program.AlertQueueManager.GetLastActiveAlerts(Program.MainConfiguration.ShowLastUnexpiredAlerts)) 
            {
                DateTime timeAdded = alert.Entry != null ? alert.Entry.TimeAdded : alert.Kill.TimeAdded;

				string systemName = alert.Path?.ToSystemName ?? string.Empty;
				string menuText = $"{timeAdded:HH:mm} {systemName} - {alert.MenuText}";

				string TrayString = alert.Kill == null ? (timeAdded.ToString("HH:mm") + " " + alert.Entry.LineContent) : menuText;
                if (TrayString.Length > _maxTrayTooltipLength)
                {
                    TrayString = TrayString.Substring(0, _maxTrayTooltipLength);
                }
                if (first)
                {
                    trayIcon.Text = TrayString;
					first = false;
				}
				MenuItem menuAlert = new MenuItem(menuText, trayIconMenuShowHistoricAlert) { Tag = alert };
                menuLatestAlerts.MenuItems.Add(menuAlert);
            }
        }
        private void RebuildAlertsMenu()
        {
            menuConfiguredAlerts.MenuItems.Clear();
            foreach (var alertConfigItem in Program.MainConfiguration.AlertConfiguration)
            {
                MenuItem menuAlert = new MenuItem(alertConfigItem.FriendlyName, trayIconMenuAlertClick);
                menuAlert.Checked = alertConfigItem.Enabled;
                menuConfiguredAlerts.MenuItems.Add(menuAlert);
            }
        }
        private void RebuildBackgroundsMenu()
        {
            menuBackgrounds.MenuItems.Clear();
            foreach (var background in Program.Backgrounds.OrderBy(o => o.BackgroundName))
            {
                MenuItem menuBackground = new MenuItem(background.BackgroundName, trayIconMenuBackgroundClick)
                {
                    Checked = Program.MainConfiguration.AlertPopupBackground == background.BackgroundName
                };
                menuBackgrounds.MenuItems.Add(menuBackground);
            }
        }
		private bool IsWindowVisibleOnCurrentScreen(Point location)
		{
			var bounds = Screen.FromControl(this).Bounds;

			return location.X >= bounds.Left &&
				   location.X <= bounds.Right &&
				   location.Y >= bounds.Top &&
				   location.Y <= bounds.Bottom;
		}
		public void ReloadConfiguration()
        {
            BackColor = Program.MainConfiguration.MainWindowBackgroundColour;
            ForeColor = Program.MainConfiguration.MainWindowTextColour;

            Program.MainConfiguration.ApplyConfiguration(this, Program.MainConfiguration.MainWindowItemLocations, TopbarPanel.Height, MiningPerHourTotal, ISKPerHourTotal, false);
			if (OverlayMapWindow != null)
			{
				Program.MainConfiguration.ApplyConfiguration(OverlayMapWindow, Program.MainConfiguration.OverlayWindowItemLocations, 0, MiningPerHourTotal, ISKPerHourTotal, false);
			}

			if (OverlayDPSWindow != null)
			{
				Program.MainConfiguration.ApplyConfiguration(OverlayDPSWindow, Program.MainConfiguration.DPSWindowItemLocations, 0, MiningPerHourTotal, ISKPerHourTotal, false);
			}

			RebuildAlertsMenu();
            RebuildBackgroundsMenu();
            RebuildLatestAlertsMenu();

            if (Program.MainConfiguration.InitialPosition != null)
            {
                this.Location = Program.MainConfiguration.InitialPosition;
				if (!IsWindowVisibleOnCurrentScreen(Location))
				{
					Location = Point.Empty;
				}
			}

            TopMost = Program.MainConfiguration.StayOnTop;
            menuConfigurationAlwaysOnTop.Checked = Program.MainConfiguration.StayOnTop;
            menuConfigurationDPSShowDPS.Checked = Program.MainConfiguration.ShowWindowOverlayDPS;
            menuConfigurationDPSSaveCSV.Checked = Program.MainConfiguration.DPSSaveCSV;
            menuConfigurationDPSShowAll.Checked = Program.MainConfiguration.DPSAllToons;

            Program.Universe.HomeSolarSystemID = Program.Universe.Names[Program.MainConfiguration.HomeSystem];

            RecalculateWindowHeight();

			mapScale =Program.MainConfiguration.MainWindowMapScale == 0 ? 100 : Program.MainConfiguration.MainWindowMapScale;

            Program.CachedBackground = Program.LoadImageFromFile(Program.MainConfiguration.AlertPopupBackground);

			_planetInfoFont = new Font("Verdana", Program.MainConfiguration.PlanetInfoFontSize, FontStyle.Regular);

            UpdateMinimapParameters();
		}

		private void Quesadilla_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

            CurrentClient.Text = "";
            RattingData.Text = "";
            MiningData.Text = "";
            AlertsOutstanding.Text = "";
            ExtraInformation.Text = "";
            MouseoverSystemName.Text = "";

            Invalidate();

            ReloadConfiguration();
            ToggleIntelSize();

			Title.Text = $"Quesadilla v{Program.version}";

			Program.ESIHandler.Resume();
        }
        private bool CheckJumpsRange(AlertConfig alert, PathInfo path)
        {
			bool CheckJumpsRangeComparison(
				int jumps,
				AlertConfigJumpsComparison comparison,
				int? comparisonNumber)
			{
				if (comparison == AlertConfigJumpsComparison.None)
					return true;

				if (comparisonNumber is null)
					return false;

				switch (comparison)
				{
					case AlertConfigJumpsComparison.Equal:
						if (jumps == comparisonNumber) return true;
						break;
					case AlertConfigJumpsComparison.LessThan:
						if (jumps < comparisonNumber) return true;
						break;
					case AlertConfigJumpsComparison.LessThanEqual:
						if (jumps <= comparisonNumber) return true;
						break;
					case AlertConfigJumpsComparison.GreaterThan:
						if (jumps > comparisonNumber) return true;
						break;
					case AlertConfigJumpsComparison.GreaterThanEqual:
						if (jumps >= comparisonNumber) return true;
						break;
					default:
						return false;
						break;
				}
				return false;
			}

			if (!alert.Enabled) return false;
            if (path == null) return false;

			return
				CheckJumpsRangeComparison(path.TotalJumps,alert.Jumps1Comparison,alert.Jumps1Number) &&
				CheckJumpsRangeComparison(path.TotalJumps,alert.Jumps2Comparison,alert.Jumps2Number) &&
				path.TotalJumps < Program.MainConfiguration.IgnoreAsTooManyJumps;
		}
		private List<PlanetSummary> GetPlanetInfo(int systemId)
		{
			if (_cachedPlanetSystemId == systemId)
			{
				return _cachedPlanetInfo;
			}

			_cachedPlanetSystemId = systemId;

			_cachedPlanetInfo = Program.Universe.SolarSystems[systemId]
				.Planets
				.GroupBy(p => p.PlanetTypeName.Replace("Planet ", ""))
				.Select(group => new PlanetSummary
				{
					Type = group.Key,
					Count = group.Count(),
					PlanetNames = string.Join(",", group.Select(p => p.PlanetName.Substring(p.PlanetName.LastIndexOf(' ') + 1)))
				})
				.OrderBy(x => x.Type)
				.ToList();

			return _cachedPlanetInfo;
		}
		private string ReplacePathTokens(string text, PathInfo path)
		{
			if (path == null)
			{
				return text;
			}

			text = text.Replace("{jumps}", path.TotalJumps.ToString());
			text = text.Replace("{route}", Program.Universe.PathToString(path));
			text = text.Replace("{tosystem}", Program.Universe.SolarSystems[path.ToSystemId].Name);
			text = text.Replace("{region}", Program.Universe.SolarSystems[path.ToSystemId].RegionName);
			text = text.Replace("{fromsystem}", Program.Universe.SolarSystems[path.FromSystemId].Name);

			return text;
		}

		private string GenerateAlertText(LogEntry entry, PathInfo path, AlertConfig alert, string characterName, AlertTextType type, string context)
        {
			string returnString = (type == AlertTextType.Alert ? alert.TextDisplay : ( type == AlertTextType.Menu ? alert.MenuDisplay : String.Empty));

			if (path != null)
				returnString = ReplacePathTokens(returnString, path);

            returnString = returnString.Replace("{character}", characterName);

            returnString = returnString.Replace("{text}", entry.LineContent);
            returnString = returnString.Replace("{context}", context);
            returnString = returnString.Replace("{reporter}", entry.PlayerName);
            return returnString;
        }
        private string generateZKillAlertText(ZKillKill kill, PathInfo path, string zkilltext, string characterName)
        {
            // type == 0 - Alert Text, 1 == Menu text
            string returnString = zkilltext;

			if (path != null)
				returnString = ReplacePathTokens(returnString, path);
			
            returnString = returnString.Replace("{character}", characterName);

            returnString = returnString.Replace("{text}", kill.href);
            returnString = returnString.Replace("{value}", kill.TotalValue);
            returnString = returnString.Replace("{victim}", kill.VictimCharacterName);
            returnString = returnString.Replace("{system}", Program.Universe.SolarSystems.ContainsKey((int)kill.SolarSystemId) ? Program.Universe.SolarSystems[(int)kill.SolarSystemId].Name : kill.SolarSystemId.ToString());
            returnString = returnString.Replace("{killer}", (kill.KillerCharacterId == null || kill.KillerCharacterId == 0) ? "NPC" : Program.Universe.GetCharacterName((int)kill.KillerCharacterId));
            returnString = returnString.Replace("{victimship}", Program.Universe.ItemTypes.ContainsKey((int)kill.VictimShipTypeId) ? Program.Universe.ItemTypes[(int)kill.VictimShipTypeId].Name : "Unknown:" + kill.VictimShipTypeId.ToString());
            if (kill.KillerShipTypeId != null)
            {
                returnString = returnString.Replace("{killership}", Program.Universe.ItemTypes.ContainsKey((int)kill.KillerShipTypeId) ? Program.Universe.ItemTypes[(int)kill.KillerShipTypeId].Name : kill.KillerShipTypeId.ToString());
            }
            returnString = returnString.Replace("{killerweapon}", kill.KillerWeaponTypeId.ToString());
            return returnString;
        }

        private void logFile_ProcessNewData(object sender, ProcessNewDataEventArgs e)
        {
			AlertQueueItem QueueIntelAlert(LogEntry le,AlertConfig alert,PathInfo path,string context,string character,string triggerText)
			{
				var alertText = GenerateAlertText(le,path,alert,character,AlertTextType.Alert,triggerText);
				var alertItem = new AlertQueueItem
				{
					AlertText = alertText,
					MenuText = GenerateAlertText(le,path,alert,character,AlertTextType.Menu,triggerText),
					Entry = le,
					Path = path,
					Context = context,
					SoundFile = alert.SoundFile,
					FriendlyName = alert.FriendlyName,
					ShowAlert = true
				};

				Program.AlertQueueManager.EnqueueNewAlert(alertItem);
				Log.Debug($"ALERT - {alertText}");
				return alertItem;
			}

			LogEntry entry = e.LogEntry;
            int alertCount = 0;

            if (entry == null || entry.PlayerName == "EVE System")
                return;
            if (entry.EntryType != LogEntryType.ChatEvent && entry.EntryType != LogEntryType.UnknownChatLogEvent && entry.EntryType != LogEntryType.UnknownGameLogEvent)
            {
                var c = Program.CharacterManager.GetOrCreate(entry.CharacterName);

                switch (entry.EntryType)
                {
                    case LogEntryType.OpenChatLogEvent:
                        Log.Debug(e.LogPrefix + " > Chat log opened: (" + entry.CharacterName + ") " + entry.FileName);
                        break;
                    case LogEntryType.NewChatLogEvent:
                        Log.Debug(e.LogPrefix + " > New chat log detected: (" + entry.CharacterName + ") " + entry.FileName);
                        break;
                    case LogEntryType.OpenGameLogEvent:
                        Log.Debug(e.LogPrefix + " > Game log opened: (" + entry.CharacterName + ") " + entry.FileName);
                        break;
                    case LogEntryType.NewGameLogEvent:
                        Log.Debug(e.LogPrefix + " > New game log detected: (" + entry.CharacterName + ") " + entry.FileName);
                        break;
                }
            }
            else
            {
                if (!entry.ParseSuccess) return;

                HashSet<int> intSet = new HashSet<int>();
				bool ignored = Program.MainConfiguration.ignoreStrings.Any(r => r.IsMatch(entry.LineContent));
				if (!ignored && entry.LogType == LogFileType.ChatIntel)
				{
					foreach (var solarSystem in Program.Universe.SolarSystems.Values)
					{
						if (!solarSystem.MatchNameRegex(entry.LineContent)) continue;
						if (!Program.Universe.Names.TryGetValue(solarSystem.Name,out var systemId)) continue;
						if (Program.MainConfiguration.ignoreSystems.Contains(systemId)) continue;

						intSet.Add(systemId);
					}
				}


				if (intSet.Count > 0)
                {
                    AlertQueueItem insertedAlert = null;

                    Log.Information(entry.LineContent);
                    entry.MatchedIds = intSet;
                    foreach (int toSystemId in intSet)
                    {
                        int homeSystemId = Program.Universe.HomeSolarSystemID;

                        // Cache paths - if not already stored
                        var homeSystemPathInfo = Program.Universe.FindAndCachePath(homeSystemId, toSystemId);
                        if (homeSystemPathInfo != null)
                        {
                            Log.Debug("Jumps from HOME:" + Program.Universe.SolarSystems[homeSystemId].Name + " to " + Program.Universe.SolarSystems[toSystemId].Name + " is " + homeSystemPathInfo.TotalJumps);
                        }

                        foreach (var c in Program.CharacterManager.CharactersWithLocation())
                        {
                            var CurrectLocationPathInfo = Program.Universe.FindAndCachePath(c.LocationId, toSystemId);
                            if (CurrectLocationPathInfo != null)
                            {
                                Log.Debug("Jumps from " + c.CharacterName + ":" + Program.Universe.SolarSystems[c.LocationId].Name + " to " + Program.Universe.SolarSystems[toSystemId].Name + " is " + CurrectLocationPathInfo.TotalJumps);
                            }
                        }

						// Range alerts
						foreach (var alert in GetEnabledAlerts(AlertConfigType.Range))
                        {
                            bool sendAlert = false;
                            PathInfo path = new PathInfo();

                            switch (alert.AlertRangeType)
                            {
                                case AlertConfigRangeType.Home:
                                    path = homeSystemPathInfo;
                                    sendAlert = CheckJumpsRange(alert, path);
                                    if (sendAlert)
                                    {
										insertedAlert = QueueIntelAlert(entry,alert,path,homeSystemPathInfo.FromSystemName,"","");
										alertCount++;
                                    }
                                    break;

                                case AlertConfigRangeType.System:
                                    // check against specific system
                                    path = Program.Universe.FindAndCachePath(Program.Universe.Names[alert.TriggerText], toSystemId);
                                    sendAlert = CheckJumpsRange(alert, path);
                                    if (sendAlert)
                                    {
										insertedAlert = QueueIntelAlert(entry,alert,path,path.FromSystemName,"","");
										alertCount++;
									}
                                    break;
                                case AlertConfigRangeType.OneCharacter:
                                case AlertConfigRangeType.OneOnlineCharacter:
                                    // check against one specific character
                                    foreach (var x in Program.CharacterManager.CharactersNamed(alert.TriggerText))
                                    {
                                        if (x.LocationId != 0)
                                        {
                                            path = Program.Universe.FindAndCachePath(x.LocationId, toSystemId);
                                            sendAlert = CheckJumpsRange(alert, path);
                                            if (sendAlert)
                                            {
                                                if (
                                                    alert.AlertRangeType == AlertConfigRangeType.OneCharacter ||
                                                    alert.AlertRangeType == AlertConfigRangeType.OneOnlineCharacter && x.IsClientActive
                                                    )
                                                {
													insertedAlert = QueueIntelAlert(entry, alert, path, x.CharacterName, x.CharacterName, "");
													alertCount++;
												}
											}
                                        }
                                    }
                                    break;
                                case AlertConfigRangeType.AllCharacters:
								case AlertConfigRangeType.AllOnlineCharacters:
									// check against all characters
									foreach (var x in Program.CharacterManager.CharactersWithLocation())
									{
										path = Program.Universe.FindAndCachePath(x.LocationId, toSystemId);
										sendAlert = CheckJumpsRange(alert, path);
										if (sendAlert)
										{
											if (
												alert.AlertRangeType == AlertConfigRangeType.AllCharacters ||
												alert.AlertRangeType == AlertConfigRangeType.AllOnlineCharacters && x.IsClientActive
												)
											{
												insertedAlert = QueueIntelAlert(entry,alert,path,x.CharacterName,x.CharacterName,"");
												alertCount++;
											}
										}
									}
									break;
								case AlertConfigRangeType.None:
                                    // must be custom text
                                    if (entry.LineContent.Contains(alert.TriggerText, StringComparison.OrdinalIgnoreCase))
                                    {
                                        foreach (var x in Program.CharacterManager.Characters)
                                        {
                                            path = Program.Universe.FindAndCachePath(x.LocationId, toSystemId);
											insertedAlert = QueueIntelAlert(entry,alert,path,alert.TriggerText,alert.TriggerText,alert.TriggerText);
											alertCount++;
										}
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    int systemId1 = -1;
                    foreach (int systemId2 in intSet)
                    {
                        // if no alert - still STORE alert for later checks and display on map
                        if (insertedAlert == null)
                        {
                            insertedAlert = new AlertQueueItem()
                            {
                                AlertText = "",
                                MenuText = "",
                                Entry = entry,
                                Path = Program.Universe.FindAndCachePath(systemId2, systemId2),
                                Context = "",
                                SoundFile = "",
                                FriendlyName = "",
                                ShowAlert = false,
                            };
                            Program.AlertQueueManager.EnqueueNewAlert(insertedAlert);
                        }

                        //Program.Universe.AddAlert(systemId2, insertedAlert);
                        systemId1 = systemId2;
                    }
                    //                    if (this.CheckZoomTo(entry.LogPrefix)) this.ZoomTo(systemId1);
                }

                // check against custom alerts (no system name needed) - only if no alerts generated
                if (alertCount == 0)
                {
					foreach (var alert in GetEnabledAlerts(AlertConfigType.Text))
                    {
                        if (entry.LineContent.Contains(alert.TriggerText, StringComparison.OrdinalIgnoreCase))
                        {
							QueueIntelAlert(entry,alert,null,alert.TriggerText,alert.TriggerText,alert.TriggerText);
							alertCount++;
						}
                    }
                }
            }
        }

		private SolarSystem FindSystem(string systemName)
		{
			if (int.TryParse(systemName, out var nativeId))
			{
				return Program.Universe.SolarSystems.Values.FirstOrDefault(s => s.Id == nativeId);
			}

			return Program.Universe.SolarSystems.Values.FirstOrDefault(s => s.MatchNameRegex(systemName));
		}

		private void _localWatcher_SystemChange(object sender, ProcessSystemChangeEventArgs e)
        {
            var foundSystem = FindSystem(e.SystemName);

			string characterName = e.CharName.Trim();
			if (!string.IsNullOrEmpty(characterName) && foundSystem != null)
			{
                Program.CharacterManager.UpdateLocation(characterName, foundSystem.Id, foundSystem.Name);
                EveOClient.UpdateSystem(characterName, foundSystem?.Name);
			
                DotlanProcessor.AddRegionToCheck(foundSystem?.RegionName);

                CheckEveScoutConnection(characterName);

                Program.Universe.GetSystemInfo(foundSystem.Id);

                GetSkyhookTimers();
            }

            // System not found and not WH space... so Abyss
            if (!string.IsNullOrEmpty(characterName) && foundSystem == null && !e.SystemName.StartsWith("J"))
            {
                Program.CharacterManager.UpdateLocation(characterName, 0, e.SystemName);
				EveOClient.UpdateSystem(characterName, e.SystemName);
                Program.CharacterManager.SetInAbyss(characterName);
			}

			SetupStatusBar();

            if (foundSystem == null && e.SystemName.StartsWith("J"))
            {
                // wormhole ? - get info from http://anoik.is/
            }
            Log.Information("System Change: " + characterName + " (" + foundSystem?.Name ?? " Ukknown " + ")");

            // Should we update system information in system log file ?
            if (Program.MainConfiguration.OutputCharacterLocationsToFile)
            {
                try
                {
                    StringBuilder s = new StringBuilder();
					foreach (var c in Program.CharacterManager.Characters)
					{
						s.AppendLine($"{c.CharacterName}:{c.Location}{(c.InAbyss ? " *IN-ABYSS*" : "")}");
                        File.WriteAllText($"Quesadilla.Locations.{c.CharacterName}", $"{c.Location} {(c.InAbyss ? " *IN-ABYSS*" : "")}");
                    }
                    File.WriteAllText($"Quesadilla.Locations", s.ToString());
                }
                catch (Exception ex)
                {
                    //
                }
            }

            shouldUpdatePlanetInfo = true;
            shouldUpdateMinimap = true;
        }


		private void _localWatcher_ChatServerChange(object sender,ProcessChatServerChangeEventArgs e)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action(() => _localWatcher_ChatServerChange(sender, e)));
				return;
			}

			Log.Information($"Chat Server: {(e.ChatServerUp ? "Up" : "Down")}");

			ChatServerStatus = e.ChatServerUp;
			ChatServerDown.Visible = !e.ChatServerUp;
		}

		private void Quesadilla_FormClosing(object sender, FormClosingEventArgs e)
        {
			if (WindowState == FormWindowState.Normal)
			{
				Program.MainConfiguration.InitialPosition = Location;
				Program.MainConfiguration.MainWindowSize = Size;
			}
			else
			{
				Program.MainConfiguration.InitialPosition = RestoreBounds.Location;
				Program.MainConfiguration.MainWindowSize = RestoreBounds.Size;
			}
			
            Program.MainConfiguration.StayOnTop = TopMost;
            Program.Universe.WriteESICharacters(Program.CHARACTER_CACHE_FILE);
            ClearEveLocations();
        }


        private void trayIconMenuClose(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void trayIconMenuAlwaysOnTop(object sender, EventArgs e)
        {
            Program.MainConfiguration.StayOnTop = !Program.MainConfiguration.StayOnTop;
            TopMost = Program.MainConfiguration.StayOnTop;
            ((MenuItem)sender).Checked = !((MenuItem)sender).Checked;
        }
        private void trayIconMenuDPSShowWindow(object sender, EventArgs e)
        {
            Program.MainConfiguration.ShowWindowOverlayDPS = !Program.MainConfiguration.ShowWindowOverlayDPS;
            ((MenuItem)sender).Checked = !((MenuItem)sender).Checked;
        }
        private void trayIconMenuShowZKillAlerts(object sender, EventArgs e)
        {
            Program.MainConfiguration.ShowZKillAlerts = !Program.MainConfiguration.ShowZKillAlerts;
            ((MenuItem)sender).Checked = !((MenuItem)sender).Checked;
        }
        private void trayIconMenuDPSSaveCSV(object sender, EventArgs e)
        {
            Program.MainConfiguration.DPSSaveCSV = !Program.MainConfiguration.DPSSaveCSV;
            ((MenuItem)sender).Checked = !((MenuItem)sender).Checked;
        }
        private void trayIconMenuDPSAllToons(object sender, EventArgs e)
        {
            Program.MainConfiguration.DPSAllToons = !Program.MainConfiguration.DPSAllToons;
            ((MenuItem)sender).Checked = !((MenuItem)sender).Checked;
		}
        private void trayIconMenuConfiguration(object sender, EventArgs e)
        {
            //
        }

        private void trayMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }
        private void trayIconMenuAlertClick(object sender, EventArgs e)
        {
            // enable / disable config item
            MenuItem m = (MenuItem)sender;
            var configAlert = Program.MainConfiguration.AlertConfiguration.Find(o => o.FriendlyName == m.Text);
            if (configAlert != null)
            {
				configAlert.Enabled = !configAlert.Enabled;
				m.Checked = configAlert.Enabled;
            }
        }

        private void trayIconDoubleClick(object sender, EventArgs e)
        {
			var alert = Program.AlertQueueManager
				.GetLastActiveAlerts(1)
				.FirstOrDefault();

			if (alert != null)
			{
				ShowAlert(alert, false);
			}
		}

        private void trayIconMenuConfigurationClick(object sender, EventArgs e)
        {
            Program.MainConfiguration.InitialPosition = Location;
            Program.MainConfiguration.MainWindowSize = new Size(Width, Height);
            Program.MainConfiguration.StayOnTop = TopMost;

            var ConfigurationWindow = new QuesadillaConfiguration(Program.MainConfiguration, menuConfigurationWindow, this);
            ((MenuItem)sender).Enabled = false;
            ConfigurationWindow.Show(this);
        }

        private void trayIconMenuLogfileClick(object sender, EventArgs e)
        {
            // enable / disable config item
            MenuItem m = (MenuItem)sender;
            var watcher = Program.LogWatcherManager.GetWatcherByFriendlyName(m.Text);
            if (watcher != null)
            {
                watcher.Enabled = !watcher.Enabled;
				m.Checked = watcher.Enabled;
			}
		}
        private void trayIconMenuBackgroundClick(object sender, EventArgs e)
        {
            // enable clicked background
            MenuItem m = (MenuItem)sender;
            foreach (MenuItem z in menuBackgrounds.MenuItems)
            {
                z.Checked = false;
            }
            m.Checked = true;
            Program.MainConfiguration.AlertPopupBackground = m.Text;
        }
		private void logFile_ProcessCombat(object sender, ProcessCombatEventArgs e)
		{
			void AddCombatEntry(string entity, string text, int damage, int neut, int cap, int shield, int armor, int hull, bool received)
			{
				Program.GraphDataManager.AddCombat(
					e.Entry.CharacterName,
					new CombatLine
					{
						TimeAdded = e.Entry.TimeAdded,
						Entity = entity,
						Text = text,
						DamageAmount = damage,
						NeutAmount = neut,
						CapAmount = cap,
						ShieldAmount = shield,
						ArmorAmount = armor,
						HullAmount = hull,
						Received = received
					});
			}

			if (e.Entry == null)
			{
				InCombat = false;
				Invalidate();
				return;
			}

			if (!InCombat)
			{
				InCombat = true;
				Invalidate();
			}

			EveOClient.Agression(e.Entry.CharacterName, true);

			// Misses you completly
			Match match = _CombatRegexMissesYouCompletely.Match(e.Entry.LineContent);
			if (match.Success) AddCombatEntry(match.Groups["combatEntity"].Value, "", 0, 0, 0, 0, 0, 0, true);

			match = _CombatRegexCombatData.Match(e.Entry.LineContent);
			if (match.Success)
			{
				int dmgAmount = 0;
				int neutAmount = 0;
				int capAmount = 0;
				int shieldAmount = 0;
				int armorAmount = 0;
				int hullAmount = 0;

				var amount = match.Groups["combatAmount"].Value.Trim();
				bool received = true;

				var combatEntity = match.Groups["combatEntity"].Value;
				var combatContext = match.Groups["combatContext"].Value;
				var combatText = match.Groups["combatText"].Value; 
                
                if (amount.Contains("GJ"))
				{
					int.TryParse(amount.Replace(" GJ", "").Trim(), out neutAmount);
					if (match.Groups["combatColour"].Value.Equals(combatNeutOut)) received = false;
				}
				else
				{
					int.TryParse(amount.Trim(), out var parsedAmount);

					if (combatContext == "to") received = false;
					if (combatContext == "to" || combatContext == "from")
					{
                        dmgAmount = parsedAmount;
					}
					else
					{
                        if (combatContext.StartsWith("remote shield boosted ")) shieldAmount = parsedAmount;
						if (combatContext.StartsWith("remote armor boosted ")) armorAmount = parsedAmount;
						if (combatContext.StartsWith("remote hull boosted ")) hullAmount = parsedAmount;
						if (combatContext.StartsWith("remote capacitor transmitted ")) capAmount = parsedAmount;
						if (combatContext.Trim().EndsWith("to")) received = false;
					}
				}

				AddCombatEntry(combatEntity, combatText, dmgAmount, neutAmount, capAmount, shieldAmount, armorAmount, hullAmount, received);

				// check if faction 
				foreach (var alert in GetEnabledAlerts(AlertConfigType.FactionSpawn))
				{
					// active alert config for faction spawn
					if (_FactionRats.Any(s => combatEntity.Contains(s)))
					{
						string alertText = GenerateAlertText(e.Entry, null, alert, "Faction Spawn", AlertTextType.Alert, combatEntity);

						PathInfo path = null;
						var c = Program.CharacterManager.Get(e.CharacterName);
						bool sendAlert = true;

						if (c != null)
						{
							path = new PathInfo()
							{
								FromSystemId = c.LocationId,
								ToSystemId = c.LocationId,
								FromSystemName = c.Location,
								ToSystemName = c.Location,
								ToSystemRegionName = Program.Universe.SolarSystems[c.LocationId].RegionName,
								PathSystems = new List<int>() { c.LocationId },
							};

							if (c.LastFactionAlert.AddSeconds(alert.CustomTriggerEverySeconds == null ? 0 : (double)alert.CustomTriggerEverySeconds) > DateTime.Now) { sendAlert = false; }
						}
						else
						{
							if (LastFactionAlert.AddSeconds(alert.CustomTriggerEverySeconds == null ? 0 : (double)alert.CustomTriggerEverySeconds) > DateTime.Now) { sendAlert = false; }
						}

						if (sendAlert)
						{
							LastFactionAlert = DateTime.Now;
							if (c != null) { Program.CharacterManager.SetLastFactionAlert(c.CharacterName, LastFactionAlert); }
							Log.Debug("ALERT - " + alertText);
							Program.AlertQueueManager.EnqueueNewAlert(
								new AlertQueueItem()
								{
									AlertText = alertText,
									MenuText = GenerateAlertText(e.Entry, null, alert, "Faction spawn", AlertTextType.Menu, combatEntity),
									Entry = e.Entry,
									Path = path,
									Context = combatEntity,
									SoundFile = alert.SoundFile,
									FriendlyName = alert.FriendlyName,
									isFaction = true,
									ShowAlert = true,
								});
						}
					}
				}
			}
			CombatAdded = true;
		}
		private void logFile_ProcessISK(object sender, ProcessISKEventArgs e)
        {
            //Log.Information("COMBAT:" + e.CombatEvent);
            if (e.Entry != null)
            {
                Match match = _ISKRegexISKData.Match(e.Entry.LineContent);
                if (match.Success)
                {
                    var amount = match.Groups["iskAmount"].Value;
					if (double.TryParse(amount.Trim(), out var iskAmount))
					{
						Program.GraphDataManager.AddISK(e.Entry.CharacterName,
							new IskLine
							{
								TimeAdded = e.Entry.TimeAdded,
								IskAmount = iskAmount
							});
					}
				}
			}
		}
		private void logFile_ProcessMining(object sender, ProcessMiningEventArgs e)
		{
			void AddMiningEntry(LogEntry entry, string item, int amount, int wastage, bool critical)
			{
				Program.GraphDataManager.AddMining(
					entry.CharacterName,
					new MiningLine
					{
						TimeAdded = entry.TimeAdded,
						MiningItem = item,
						MiningAmount = amount,
						WastageAmount = wastage,
						Critical = critical
					});
			}

			if (e.Entry == null) return;

			// is this actual mining data
			Match match = _MiningRegexAmounts.Match(e.Entry.LineContent);
			if (match.Success)
			{
				var minedAmount = match.Groups["miningAmount"].Value;
				if (int.TryParse(minedAmount.Trim(), out var mined)) AddMiningEntry(e.Entry, match.Groups["miningContext"].Value, mined, 0, false);
			}

			// is this critical mining data
			match = _MiningRegexAmountsCritical.Match(e.Entry.LineContent);
			if (match.Success)
			{
				var minedAmount = match.Groups["miningAmount"].Value;
				if (int.TryParse(minedAmount.Trim(), out var mined)) AddMiningEntry(e.Entry, match.Groups["miningContext"].Value, mined, 0, true);
			}

			// is this residue mining data
			match = _MiningRegexAmountsResidue.Match(e.Entry.LineContent);
			if (match.Success)
			{
				var wastedAmount = match.Groups["miningResidue"].Value;
				if (int.TryParse(wastedAmount.Trim(), out var wasted)) AddMiningEntry(e.Entry, "WASTE", 0, wasted, false);
			}

			// is event that roid popped
			match = _MiningRegexRoidExploded.Match(e.Entry.LineContent);
			if (match.Success)
			{

				foreach (var alert in GetEnabledAlerts(AlertConfigType.MiningEndedRoidPopped))
				{

					Log.Information("Mining: Rock expired");
					var alertText = GenerateAlertText(e.Entry, null, alert, e.Entry.CharacterName, AlertTextType.Alert, match.Groups["miningHowItEnded"].Value);
					PathInfo path = null;

					Program.AlertQueueManager.EnqueueNewAlert(
						new AlertQueueItem()
						{
							AlertText = alertText,
							MenuText = alertText,
							Entry = e.Entry,
							Path = path,
							Context = alertText,
							SoundFile = alert.SoundFile,
							FriendlyName = alert.FriendlyName + " - " + e.Entry.CharacterName,
							isMiningOver = true,
							ShowAlert = true,
						});
				}
			}

			// is event cargo full ?
			match = _MiningRegexCargoFull.Match(e.Entry.LineContent);
			if (match.Success)
			{
                foreach (var alert in GetEnabledAlerts(AlertConfigType.MiningEndedFull))
				{
					Log.Information("Mining: cargo full");
					var alertText = GenerateAlertText(e.Entry, null, alert, e.Entry.CharacterName, AlertTextType.Alert, match.Groups["miningDevice"].Value + " stopped - Cargo full");
					PathInfo path = null;

					Program.AlertQueueManager.EnqueueNewAlert(
						new AlertQueueItem()
						{
							AlertText = alertText,
							MenuText = e.Entry.CharacterName + alertText,
							Entry = e.Entry,
							Path = path,
							Context = alertText,
							SoundFile = alert.SoundFile,
							FriendlyName = alert.FriendlyName + " - " + e.Entry.CharacterName,
							isMiningOver = true,
							ShowAlert = true,
						});
				}
			}

		}

		private void logFile_ProcessDecloak(object sender, ProcessDecloakEventArgs e)
        {
            //Log.Information("COMBAT:" + e.CombatEvent);
            if (e.Entry != null)
            {

                PathInfo path = null;
                string alertText = "";

                foreach (var alert in GetEnabledAlerts(AlertConfigType.Decloak))
                {

                    Match match = _DecloakRegexMobileObservatory.Match(e.Entry.LineContent);
                    if (match.Success)
                    {
                        Log.Information("DECLOAK: mobile observatory");
                        alertText = GenerateAlertText(e.Entry, null, alert, "Decloaked pilot", AlertTextType.Alert, "mobile observatory");
                    }

                    match = _DecloakRegexProximity.Match(e.Entry.LineContent);
                    if (match.Success)
                    {
                        alertText = GenerateAlertText(e.Entry, null, alert, "Decloaked pilot", AlertTextType.Alert, match.Groups["decloakProximity"].ToString());
                        Log.Information("DECLOAK: proximity to " + alertText);
                    }

                    Program.AlertQueueManager.EnqueueNewAlert(
                    new AlertQueueItem()
                    {
                        AlertText = alertText,
                        MenuText = e.CharacterName + alertText,
                        Entry = e.Entry,
                        Path = path,
                        Context = alertText,
                        SoundFile = alert.SoundFile,
                        FriendlyName = alert.FriendlyName,
                        isDecloak = true,
                        ShowAlert = true,
                    });
                }
            }
        }

        private void RecalculateWindowHeight()
        {
            Height = IntelLog.Height + _windowExtraHeight + (Program.MainConfiguration.ShowMinimap ? MainMinimap.Height : 20);

            if (Program.MainConfiguration.ShowMinimap)
            {
            }

            buttonIntelSize.Top = IntelLog.Top + 2;
            buttonIntelSize.Left = IntelLog.Width - buttonIntelSize.Width - 2;
        }
        private void ToggleIntelSize()
        {
			if (IntelLog.Height == _intelSizeMin)
			{
				// max it big
				IntelLog.Height = _intelSizeMax;
				buttonIntelSize.Text = "▼";
			}
			else
			{
				// make it small
				IntelLog.Height = _intelSizeMin;
				buttonIntelSize.Text = "▲";
			}
			RecalculateWindowHeight();
			IntelLog.ScrollToCaret();
		}

		private void buttonIntelSize_Click(object sender, EventArgs e)
        {
            ToggleIntelSize();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // Set the form click-through

                //                cp.ExStyle |= 
                //                    0x80000 /* WS_EX_LAYERED */ | 
                //                    0x20 /* WS_EX_TRANSPARENT */
                //                    ;
                return cp;
            }
        }

        private void intelLog_TextChanged(object sender, EventArgs e)
        {

        }

		private void ActivateOverlay(Form overlay)
		{
			User32NativeMethods.SendWindowTop(overlay.Handle);
			overlay.Invalidate();
			overlay.TopMost = false;
			overlay.TopMost = true;
		}
		private void DeactivateOverlay(Form overlay)
		{
			overlay.TopMost = true;
			overlay.TopMost = false;
		}

		private void UpdateEveProcesses(bool refreshProcesses)
        {
            if (refreshProcesses)
            {
                Program.ProcessMonitorHandler.GetUpdatedProcesses();
            }

            string previousToon = CurrentActiveToon;

            string foregroundWindowTitle = null;
            var ptr = User32NativeMethods.GetForegroundWindowHandle();
			foregroundWindowTitle = Program.ProcessMonitorHandler.GetTitleOfKnownProcess(ptr);

            var PrevRect = currentEveWindowRect;

			var result = User32NativeMethods.GetWindowDimensions(ptr, ref currentEveWindowRect);
            if (ptr != IntPtr.Zero)
            {
                if (foregroundWindowTitle != null && foregroundWindowTitle != String.Empty)
                {
                    //lblCurrentClient.Visible = true;
                    CurrentActiveToon = foregroundWindowTitle.Replace("EVE - ", "");
                    CurrentClient.Text = CurrentActiveToon;

                    SetupStatusBar();

                    if (string.IsNullOrEmpty(previousToon) || previousToon != CurrentActiveToon || NoActiveEveProcess == true)
                    {
                       
						if (Program.MainConfiguration.ShowWindowOverlayMap && OverlayMapWindow != null)
						{
							if (string.IsNullOrEmpty(CurrentActiveToon))
							{
								OverlayMapWindow.Hide();
							}
							else
							{
								OverlayMapWindow.UpdateMapData();
								if (NoActiveEveProcess)
									ActivateOverlay(OverlayMapWindow);
								else
									DeactivateOverlay(OverlayMapWindow);
							}
                            OverlayMapWindow.SetScreenPosition();
						}

						if (Program.MainConfiguration.ShowWindowOverlayDPS && OverlayDPSWindow != null)
						{
							if (string.IsNullOrEmpty(CurrentActiveToon))
							{
								OverlayDPSWindow.Hide();
							}
							else
							{
                                if (NoActiveEveProcess)
                                    ActivateOverlay(OverlayDPSWindow);
                                else
                                    DeactivateOverlay(OverlayDPSWindow);


								OverlayDPSWindow.Show();
							}
							OverlayDPSWindow.SetScreenPosition();
						}

						UpdateISK();
                        shouldUpdateMinimap = true;
                        shouldUpdatePlanetInfo = true;
                    }

                    if (
                        PrevRect.Top != currentEveWindowRect.Top || PrevRect.Bottom != currentEveWindowRect.Bottom ||
                        PrevRect.Left != currentEveWindowRect.Left || PrevRect.Right != currentEveWindowRect.Right
                        )
                    {
						if (OverlayDPSWindow != null) OverlayDPSWindow.SetScreenPosition();
						if (OverlayMapWindow != null) OverlayMapWindow.SetScreenPosition();
					}

					if (NoActiveEveProcess) NoActiveEveProcess = false;

                }
                else
                {
                    // not eve window 
                    if (Program.MainConfiguration.ShowWindowOverlayDPS && OverlayDPSWindow != null)
                    {
						if (!Program.MainConfiguration.KeepLastToonWhenNotEve)
						{
							OverlayDPSWindow.Hide();
							DeactivateOverlay(OverlayDPSWindow);
						}
						else
						{
							if (OverlayDPSWindow.TopMost != false)
							{
								currentEveWindowRect = PrevRect; // smaybe move this out
								OverlayDPSWindow.TopMost = false;
								User32NativeMethods.SendWindowBack(OverlayDPSWindow.Handle);
							}
						}
					}
					if (Program.MainConfiguration.ShowWindowOverlayMap && OverlayMapWindow != null)
                    {
                        if (!Program.MainConfiguration.KeepLastToonWhenNotEve)
                        {
                            DeactivateOverlay(OverlayMapWindow);
							OverlayMapWindow.Hide();
                        }
                        else
                        {
                            if (OverlayMapWindow.TopMost != false)
                            {
                                currentEveWindowRect = PrevRect; // smaybe move this out
                                DeactivateOverlay(OverlayMapWindow);
								User32NativeMethods.SendWindowBack(OverlayMapWindow.Handle);
                            }
                        }
                    }
                }
            }

            if (foregroundWindowTitle == null)
            {
                if (!Program.MainConfiguration.KeepLastToonWhenNotEve)
                {
                    //lblCurrentClient.Visible = false;
                    CurrentActiveToon = "";
                    CurrentClient.Text = CurrentActiveToon;
                    SetupStatusBar();
                    shouldUpdateMinimap = true;
                }
                else
                {
                    NoActiveEveProcess = true;
                }
            }

			// just double check thru character list active clients to make sure there is still an active client.
			Program.CharacterManager.SetCurrentlyActiveClient(CurrentActiveToon);
			foreach (var c in Program.CharacterManager.Characters)
            {
                Program.CharacterManager.SetIsActive(c.CharacterName, Program.ProcessMonitorHandler.IsCharStillActive(c.CharacterName));
            }
        }

		private void ESI_ProcessNewData(object sender, ESIEventArgs e)
		{
			if (e.RequestType != ESIRequestType.System)
				return;

			if (InvokeRequired)
			{
				BeginInvoke(new Action(() =>
					ESI_ProcessNewData(sender, e)));
				return;
			}

			shouldUpdatePlanetInfo = true;
			Invalidate();
		}

		private void ZKill_ProcessNewData(object sender, ZKillEventArgs e)
        {
			void QueueZKillAlert(ZKillKill kill, PathInfo path, string context, string menuTextCharacter, string friendlyName)
			{
				kill.Alert = true;
				var alertText = generateZKillAlertText(kill, path, Program.MainConfiguration.ZKillAlertText, menuTextCharacter);

				Program.AlertQueueManager.EnqueueNewAlert(
					new AlertQueueItem
					{
						AlertText = alertText,
						MenuText = generateZKillAlertText(kill, path, Program.MainConfiguration.ZKillMenuText, menuTextCharacter),
						Kill = kill,
						Path = path,
						Context = context,
						SoundFile = Program.MainConfiguration.ZKillSound,
						FriendlyName = friendlyName,
						ShowAlert = true
					});

				Log.Information($"ALERT - {alertText}");
			}

			string systemName = "---";
            string regionName = "---";

            var victimShipName = Program.Universe.ItemTypes.ContainsKey((int)e.Kill.VictimShipTypeId) ? Program.Universe.ItemTypes[(int)e.Kill.VictimShipTypeId].Name : "Unknown:" + e.Kill.VictimShipTypeId.ToString();
            e.Kill.VictimShipName = victimShipName;


			if (Program.Universe.SolarSystems.TryGetValue((int)e.Kill.SolarSystemId,out var system))
			{
				systemName = system.Name;
				regionName = system.RegionName;

				e.Kill.SolarSystemName = systemName;
				e.Kill.RegionName = regionName;

				Program.ZKillHandler.EnqueueZKillAlert(e.Kill,system);
			}

            Log.Information($"ZKILL Event handler {e.Kill.ID} {regionName}:{systemName} {victimShipName} {Program.Universe.GetCharacterName((e.Kill.VictimCharacterId != null ? (int)e.Kill.VictimCharacterId : 0))}");
            e.Kill.Context = generateZKillAlertText(e.Kill, null, Program.MainConfiguration.ZKillContextText, "");

            // check if zkill system matches any range alerts
            // and that report zkill flag is set
            if (!Program.MainConfiguration.ShowZKillAlerts) return;

            var toSystemId = (int)e.Kill.SolarSystemId;

            int homeSystemId = Program.Universe.HomeSolarSystemID;
            var homeSystemPathInfo = Program.Universe.FindAndCachePath(homeSystemId, toSystemId);
            if (homeSystemPathInfo != null)
            {
                Log.Debug("Jumps from HOME:" + Program.Universe.SolarSystems[homeSystemId].Name + " to " + system?.Name + " is " + homeSystemPathInfo.TotalJumps);
            }
            foreach (var c in Program.CharacterManager.CharactersWithLocation())
            {
                var currentLocationPathInfo = Program.Universe.FindAndCachePath(c.LocationId, toSystemId);
                if (currentLocationPathInfo != null)
                {
                    Log.Debug("Jumps from " + c.CharacterName + ":" + Program.Universe.SolarSystems[c.LocationId].Name + " to " + system?.Name + " is " + currentLocationPathInfo.TotalJumps);
                }
            }

            // Range alerts
            foreach (var alert in GetEnabledAlerts(AlertConfigType.Range))
            {
                bool sendAlert = false;
                PathInfo path = new PathInfo();
                string alertText = "";

                switch (alert.AlertRangeType)
                {
                    case AlertConfigRangeType.Home:
                        path = homeSystemPathInfo;
                        sendAlert = CheckJumpsRange(alert, path);
                        if (sendAlert) QueueZKillAlert(e.Kill,path,homeSystemPathInfo.FromSystemName,"","zkill");
                        break;

                    case AlertConfigRangeType.System:
                        // check against specific system
						if (!Program.Universe.Names.TryGetValue(alert.TriggerText,out var sourceSystemId)) continue;
						path = Program.Universe.FindAndCachePath(sourceSystemId,toSystemId);
						sendAlert = CheckJumpsRange(alert, path);
                        if (sendAlert) QueueZKillAlert(e.Kill,path,path.FromSystemName,"","zkill");
                        break;
                    case AlertConfigRangeType.OneCharacter:
                    case AlertConfigRangeType.OneOnlineCharacter:
						// check against one specific character
						foreach (var x in Program.CharacterManager.CharactersNamed(alert.TriggerText))
						{
							if (x.LocationId == 0) continue;
							path = Program.Universe.FindAndCachePath(x.LocationId, toSystemId);
							sendAlert = CheckJumpsRange(alert, path);
							if (!sendAlert) continue;
							bool alertAllowed = alert.AlertRangeType == AlertConfigRangeType.OneCharacter || (alert.AlertRangeType == AlertConfigRangeType.OneOnlineCharacter && x.IsClientActive);
							if (!alertAllowed) continue;
							QueueZKillAlert(e.Kill, path, x.CharacterName, x.CharacterName, alert.FriendlyName);
						}
						break;
                    case AlertConfigRangeType.AllCharacters:
                    case AlertConfigRangeType.AllOnlineCharacters:
                        // check against all characters
                        foreach (var x in Program.CharacterManager.Characters)
                        {
							if (x.LocationId == 0) continue;
                            path = Program.Universe.FindAndCachePath(x.LocationId,toSystemId);
                            sendAlert = CheckJumpsRange(alert, path);
                            if (!sendAlert) continue;
                            bool alertAllowed = alert.AlertRangeType == AlertConfigRangeType.AllCharacters || (alert.AlertRangeType == AlertConfigRangeType.AllOnlineCharacters && x.IsClientActive);
                            if (!alertAllowed) continue;
                            QueueZKillAlert(e.Kill,path,x.CharacterName,x.CharacterName,"zkill");
						}
                        break;
                    default:
                        break;
                }

				if (sendAlert) shouldUpdateMinimap = true;
			}
        }
        public void SetupStatusBar()
        {
            StringBuilder status = new StringBuilder();

			if (!string.IsNullOrEmpty(CurrentActiveToon))
			{
                var c = Program.CharacterManager.Get(CurrentActiveToon);
                if (c != null)
                {
                    if (c.InAbyss)
                    {
                        status.Append("*IN-ABYSS* ");
                    }

					if (c.ClosestThera != null && c.ClosestTheraPath != null)
					{
                        status.Append("  Thera <- ");
                        status.Append(c.ClosestTheraPath.ToSystemName);
						status.Append($" ({c.ClosestTheraPath.TotalJumps}) SIG:");
						status.Append((c.ClosestThera.FromSystemId == c.ClosestTheraPath.ToSystemId) ? c.ClosestThera.FromSignature : c.ClosestThera.ToSignature);
                    }

					if (c.ClosestTurnur != null && c.ClosestTurnurPath != null)
                    {
                        status.Append("  Turnur <- ");
                        status.Append(c.ClosestTurnurPath.ToSystemName);
						status.Append($" ({c.ClosestTurnurPath.TotalJumps}) SIG:");
                        status.Append((c.ClosestTurnur.FromSystemId == c.ClosestTurnurPath.ToSystemId) ? c.ClosestTurnur.FromSignature : c.ClosestTurnur.ToSignature);
                    }
				}
			}
            else
            {
                status.Append("No active Character");
            }

			UI(() =>
			{
				EveScoutStatus.Text = status.ToString().Trim();
				if (OverlayMapWindow!= null) OverlayMapWindow.EveScoutStatus.Text = EveScoutStatus.Text;
			});
		}

		private void GetSkyhookTimers()
        {
            Program.ESIHandler.EnqueueRequestSkyhookData();
        }

        private void CheckEveScoutConnection(string characterName)
        {
            var c = Program.CharacterManager.GetOrCreate(characterName);

            if (c.LocationId == 0) return;

            var ClosestThera = Program.EveScoutProcessor.FindClosest(Program.Universe, c.LocationId, "Thera");
            var ClosestTurnur = Program.EveScoutProcessor.FindClosest(Program.Universe, c.LocationId, "Turnur");

            if ( ClosestThera != null )
			Program.CharacterManager.SetClosestThera(characterName,ClosestThera,
				Program.Universe.FindAndCachePath(c.LocationId, (ClosestThera.FromSystemId == EveScoutManager.SYSTEMID_THERA) ? (int)ClosestThera.ToSystemId : (int)ClosestThera.FromSystemId)
				);

            if ( ClosestTurnur != null)
			Program.CharacterManager.SetClosestTurnur(characterName,ClosestTurnur,
				Program.Universe.FindAndCachePath(c.LocationId, (ClosestTurnur.FromSystemId == EveScoutManager.SYSTEMID_TURNUR) ? (int)ClosestTurnur.ToSystemId : (int)ClosestTurnur.FromSystemId)
				);

			Log.Information($"EveScout: Closest Thera {c.ClosestThera?.ToSystemId}:{c.ClosestThera?.FromSystemId} {c.ClosestTheraPath?.TotalJumps} {c.ClosestTheraPath?.ToSystemName}");
			Log.Information($"EveScout: Closest Turnur {c.ClosestTurnur?.ToSystemId}:{c.ClosestTurnur?.FromSystemId} {c.ClosestTurnurPath?.TotalJumps} {c.ClosestTurnurPath?.ToSystemName}");
		}

		private void EveScout_ProcessNewData(object sender, EveScoutEventArgs e)
        {
            foreach (var c in Program.CharacterManager.IsActive())
            {
                CheckEveScoutConnection(c.CharacterName);
            }
            EveScoutProcessStormData();
        }

        private void EveScoutProcessStormData()
        {
            Program.Universe.Storms = Program.EveScoutProcessor.Storms.ToList(); // copy storms
            foreach (var storm in Program.EveScoutProcessor.Storms)
            {
                storm.StrongSystemIds = Program.Universe.FindConnections(storm.SystemId, 1);
                storm.WeakSystemIds = Program.Universe.FindConnections(storm.SystemId, 3);
                storm.WeakSystemIds.Remove(storm.SystemId);
                storm.WeakSystemIds = storm.WeakSystemIds.Except(storm.StrongSystemIds).ToList();

                // we could just leave it there - but ingame some 3 jump systems are not in the storm
                // why is this ?
                // I think because they are over 2.5 ly away (either by gate or direct)
                // I do not KNOW this for sure
                // :(

                /*
                                var jumps2 = Program.Universe.FindConnections(storm.SystemId, 2);
                                if (jumps2.Contains(storm.SystemId))
                                    jumps2.Remove(storm.SystemId);
                                jumps2 = jumps2.Except(storm.StrongSystemIds).ToList();

                                var jumps3 = Program.Universe.FindConnections(storm.SystemId, 3);
                                if (jumps3.Contains(storm.SystemId))
                                    jumps3.Remove(storm.SystemId);
                                jumps3 = jumps3.Except(storm.StrongSystemIds).ToList();
                                jumps3 = jumps3.Except(jumps2).ToList();

                                List<int> RemoveSystems = new List<int>();
                                var s1 = Program.Universe.SolarSystems[storm.SystemId];
                                foreach (var system in jumps3)
                                {
                                    var s2 = Program.Universe.SolarSystems[system];

                                    double dx = s1.X - s2.X;
                                    double dy = s1.Y - s2.Y;
                                    double dz = s1.Z - s2.Z;
                                    double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz) / Program.Universe.MetersPerLightYear;
                                    Log.Information($"Storm {s1.Name} to {s2.Name} distance LY direct {distance}");

                                    var p = Program.Universe.FindAndCachePath(s1.Id, s2.Id);
                                    var lastSystem = Program.Universe.SolarSystems[s1.Id];
                                    double distance2 = 0.0;
                                    foreach(var pathSystem in p.PathSystems)
                                    {
                                        var thisSystem = Program.Universe.SolarSystems[pathSystem];
                                        double dx2 = lastSystem.X - thisSystem.X;
                                        double dy2 = lastSystem.Y - thisSystem.Y;
                                        double dz2 = lastSystem.Z - thisSystem.Z;
                                        distance2 += Math.Sqrt(dx2 * dx2 + dy2 * dy2 + dz2 * dz2) / Program.Universe.MetersPerLightYear;
                                        lastSystem = thisSystem;
                                    }
                                    Log.Information($"Storm {s1.Name} to {s2.Name} distance LY via gates {distance2}");

                //					if (distance >= 2.5 || distance2 >= 2.5)
                                    if (distance >= 2.5 )
                                        {
                                            RemoveSystems.Add(s2.Id);
                                            Log.Information($"Removing {s2.Name} from storm");
                                        }
                                }
                                storm.WeakSystemIds = storm.WeakSystemIds.Except(RemoveSystems).ToList();
                */
            }
        }

        private void Dotlan_ProcessNewData(object sender, DotlanEventArgs e)
        {
            Log.Information($"Processing dotlan data for {e.RegionName}");

            // if update is map data
            if (!string.IsNullOrEmpty(e.MapContent))
            {
                Log.Information($"Processing dotlan map data for {e.RegionName}");
                XmlDocument xmldoc = new XmlDocument
                {
                    XmlResolver = null
                };
                XmlNodeList dotlanXYNodes = null;
                //XmlNodeList dotlanIncursionNodes = null;
                xmldoc.LoadXml(e.MapContent);
                dotlanXYNodes = xmldoc.SelectNodes(@"//*[@id='sysuse']/*");
                //                dotlanIncursionNodes = xmldoc.SelectNodes(@"//*[@id='glow']/*");

                double dotlanX = 0;
                double dotlanY = 0;

                foreach (XmlNode xn in dotlanXYNodes)
                {
                    long systemID = long.Parse(xn.Attributes["id"].Value.Replace("sys", ""));
                    float x = float.Parse(xn.Attributes["x"].Value);
                    float y = float.Parse(xn.Attributes["y"].Value);

                    string link = xn.Attributes["xlink:href"].Value.Replace("#", "");
                    // XmlNode dotlanDEFNodes = xmldoc.SelectSingleNode($@"//*[@id='{link}']/*");

                    var s = Program.Universe.SolarSystems[(int)systemID];

                    if (s.RegionName == e.RegionName)
                    {
                        s.DotlanX = x;
                        s.DotlanY = y;

                        s.IceField = false;
                        XmlNodeList iceFieldNode = xmldoc.SelectNodes($@"//*[@id='{link}']/*/*[@id='{link.Replace("def", "ice")}']");
                        if (iceFieldNode != null && iceFieldNode.Count > 0)
                        {
                            s.IceField = true;
                        }

                        /*
                                                // default to no incursion
                                                s.Incursion = false;
                                                XmlNode incursionNode = xmldoc.SelectSingleNode($@"//*[@id='glow']/*[@id='{link.Replace("def", "inc")}']");
                                                if (incursionNode != null)
                                                {
                                                    s.Incursion = true;
                                                }
                        */
                    }
                }
            }

            // if update is delta data 
            if (!string.IsNullOrEmpty(e.DeltaContent))
            {
                Log.Information($"Processing dotlan delta data for {e.RegionName}");

				Dictionary<string, string> killDeltaData;

				try
				{
					killDeltaData = JsonSerializer.Deserialize<Dictionary<string, string>>(e.DeltaContent);
				}
				catch (Exception ex)
				{
					Log.Error(ex, $"Failed to parse dotlan delta data {e.RegionName}");
					return;
				}


                foreach (var kvp in killDeltaData)
                {
                    if (kvp.Key.Equals("error"))
                    {
                        Log.Error($"{kvp.Value} Processing dotlan delta data for {e.RegionName}");
                    }
                    else
                    {
						if (!int.TryParse(kvp.Key, out var systemId))continue;
						if (!int.TryParse(kvp.Value, out var killDelta)) continue;

						if (Program.Universe.SolarSystems.TryGetValue(systemId,out var system)) system.SetNPCKillsDeltaLastHour(killDelta);
					}
                }
            }

            // pull more information from dotlan data
            // save Solarsystem data ?
        }

        private void trayIconMenuShowHistoricAlert(object sender, EventArgs e)
        {
            AlertQueueItem alert = (AlertQueueItem)((MenuItem)sender).Tag;
            //var NewAlertWindow = new QuesadillaAlertPopup(alert, Program.MainConfiguration, Program.LoadImageFromFile(Program.MainConfiguration.AlertPopupBackground), this);
            //NewAlertWindow.Show(this);
            ShowAlert(alert);
        }

        private void SetupLogfileEvents()
        {
            Program.LogWatcherManager.ProcessCombat += new EventHandler<ProcessCombatEventArgs>(this.logFile_ProcessCombat);
            Program.LogWatcherManager.ProcessMining += new EventHandler<ProcessMiningEventArgs>(this.logFile_ProcessMining);
			Program.LogWatcherManager.ProcessISK += new EventHandler<ProcessISKEventArgs>(this.logFile_ProcessISK);
			Program.LogWatcherManager.ProcessDecloak += new EventHandler<ProcessDecloakEventArgs>(this.logFile_ProcessDecloak);
			Program.LogWatcherManager.ProcessSystemChange += new EventHandler<ProcessSystemChangeEventArgs>(this._localWatcher_SystemChange);
			Program.LogWatcherManager.ProcessChatServerChange += new EventHandler<ProcessChatServerChangeEventArgs>(this._localWatcher_ChatServerChange);
			Program.LogWatcherManager.ProcessNewData += new EventHandler<ProcessNewDataEventArgs>(this.logFile_ProcessNewData);
			Program.LogWatcherManager.SetupEvents();
        }
		private void UpdatePlanetInfo()
		{
			shouldUpdatePlanetInfo = false;

			var c = Program.CharacterManager.Get(CurrentActiveToon);
			if (c != null)
			{
				if (!Program.Universe.SolarSystems.TryGetValue(c.LocationId, out var solarSystem)) return;
				if (solarSystem.Planets == null) return;
				if (Program.Universe.SolarSystems[c.LocationId].Planets != null)
				{
					CurrentPlanetInfos = GetPlanetInfo(c.LocationId)
						.Select(p => new PlanetInfoDisplay
						{
							PlanetType = p.Type,
							CurrentMouseOver = false,
							Planets = p.PlanetNames
						})
						.ToList();
				}
			}
		}

		private void UpdateMinimap()
        {
            shouldUpdateMinimap = false;

            if (string.IsNullOrEmpty(CurrentActiveToon))
            {
                MainMinimap.Visible = false;
                //this.Invalidate();
                return;
            }

            //UpdateISK();
            if (OverlayMapWindow != null)
            {
                OverlayMapWindow.UpdateMapData();
/*
                if (NoActiveEveProcess)
				{
					OverlayMapWindow.TopMost = false;
					OverlayMapWindow.TopMost = true;
					SendWindowTop(OverlayMapWindow.Handle);
				}
				//OverlayMapWindow.Show();
*/
			}

			var c = Program.CharacterManager.Get(CurrentActiveToon);

			PathInfo _dummyPath = null;
            if (c != null)
            {
                _dummyPath = new PathInfo()
                {
                    FromSystemName = c.Location,
                    FromSystemId = c.LocationId,
                    ToSystemName = c.Location,
                    ToSystemId = c.LocationId,
                    ToSystemRegionName = "",
                    PathSystems = new List<int> { c.LocationId },
                };
            }

            if (Program.MainConfiguration.ShowMinimap)
            {
                string extraInformation = "";
                Image minimapImage = ImageGenerator.ClipMiniMap(new Bitmap(MainMinimap.Width, MainMinimap.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb), _dummyPath,
                    mapScale, ref extraInformation, ref MinimapSystems,
                    AnimationManager[AnimationType.Intel].GetNextValue(),
                    AnimationManager[AnimationType.Kills].GetNextValue(),
					AnimationManager[AnimationType.Alts].GetNextValue(),
					AnimationManager[AnimationType.UniHoles].GetNextValue(),
					AnimationManager[AnimationType.Incursion].GetNextValue(),
					AnimationManager[AnimationType.SecLimit].GetNextValue(),
                    _minimapParameters
                    );
                //                Bitmap minimapBitmap = new Bitmap(minimapImage);
                //                minimapBitmap.MakeTransparent(minimapBitmap.GetPixel(0, 0));
                var _oldImage = MainMinimap.Image;
				MainMinimap.Image = minimapImage;
                _oldImage?.Dispose();
                ExtraInformation.Text = extraInformation;
                MainMinimap.Visible = true;
                //this.Invalidate();
            }
            else
            {
                ExtraInformation.Text = "";
                MainMinimap.Visible = false;
                //this.Invalidate();
            }
        }
        private void pMinimapImage_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0) mapScale += Program.MainConfiguration.MainWindowMouseScrollAmount;
            if (e.Delta < 0) mapScale -= Program.MainConfiguration.MainWindowMouseScrollAmount;
            //UpdateMinimap();
			shouldUpdateMinimap = true;
		}

        private void Quesadilla_Paint(object sender, PaintEventArgs e)
        {
            ImageGenerator.PaintDrawText(e.Graphics, CurrentClient);
            ImageGenerator.PaintDrawText(e.Graphics, RattingData);
            ImageGenerator.PaintDrawText(e.Graphics, MiningData);
            ImageGenerator.PaintDrawText(e.Graphics, AlertsOutstanding);

            if (MainMinimap.Visible)
            {
                if (Program.MainConfiguration.MainWindowShowExtraInformation) ImageGenerator.PaintDrawText(e.Graphics, ExtraInformation);
                
                if (mouseOverMinimapSystems.Count() > 0) ImageGenerator.PaintDrawText(e.Graphics, MouseoverSystemName);
            }

            // draw red dot
            if (InCombat)
            {
                int combatTop = 0;
                int combatLeft = 0;

                int CIRCLE_RADIUS = 5;

                if (Program.MainConfiguration.ShowMinimap)
                {
                    combatLeft = MainMinimap.Left + 10;
                    combatTop = MainMinimap.Top + MainMinimap.Height - 3 - 10;
                }
                else
                {
                    combatLeft = AlertsOutstanding.Left + AlertsOutstanding.Width - 10;
                    combatTop = AlertsOutstanding.Top + AlertsOutstanding.Height + 8;
                }

                //Brush systemBrush = GDICache.GetBrush(Color.Red);
                //e.Graphics.FillEllipse(systemBrush, combatLeft - CIRCLE_RADIUS, combatTop - CIRCLE_RADIUS, CIRCLE_RADIUS * 2, CIRCLE_RADIUS * 2);

                ImageGenerator.DrawPulseOrb(e.Graphics, new Point(combatLeft, combatTop), Color.Red, CIRCLE_RADIUS, AnimationManager[AnimationType.Combat].Value(), glowLayers: 4, pulseSize: 4, maxAlpha: 120);
            }


            if (Program.MainConfiguration.ShowPlanetInformation && CurrentActiveToon != null && CurrentActiveToon != "")
            {
				var c = Program.CharacterManager.Get(CurrentActiveToon);
                if (c != null)
                {
                    try
                    {
                        if (Program.Universe.SolarSystems[c.LocationId].Planets != null)
                        {
                            if (CurrentPlanetInfos.Count() == 0)
                            {
                                UpdatePlanetInfo();
                            }

							int currentX = ExtraInformation.Left;
                            int currentY = EveScoutStatus.Top - (Program.MainConfiguration.PlanetInfoViewSize);
							var planetInfo = GetPlanetInfo(c.LocationId);
                            foreach (var p in planetInfo)
                            {
                                Brush systemBrush;
                                Color dotColour = Color.Red;
                                switch (p.Type)
                                {
                                    case "(Storm)": dotColour = Program.MainConfiguration.PlanetInfoStormColour; break;
                                    case "(Barren)": dotColour = Program.MainConfiguration.PlanetInfoBarrenColour; break;
                                    case "(Temperate)": dotColour = Program.MainConfiguration.PlanetInfoTemperateColour; break;
                                    case "(Lava)": dotColour = Program.MainConfiguration.PlanetInfoLavaColour; break;
                                    case "(Plasma)": dotColour = Program.MainConfiguration.PlanetInfoPlasmaColour; break;
                                    case "(Gas)": dotColour = Program.MainConfiguration.PlanetInfoGasColour; break;
                                    case "(Ice)": dotColour = Program.MainConfiguration.PlanetInfoIceColour; break;
                                    case "(Oceanic)": dotColour = Program.MainConfiguration.PlanetInfoOceanicColour; break;
                                    default: dotColour = Color.Red; break;
                                }
                                systemBrush = GDICache.GetBrush(dotColour);
                                e.Graphics.FillEllipse(systemBrush, currentX - Program.MainConfiguration.PlanetInfoViewSize, currentY - Program.MainConfiguration.PlanetInfoViewSize, Program.MainConfiguration.PlanetInfoViewSize * 2, Program.MainConfiguration.PlanetInfoViewSize * 2);

                                // set x/y
                                var cpi = CurrentPlanetInfos.Where(o => o.PlanetType == p.Type).First();
                                cpi.X = currentX; cpi.Y = currentY;

                                currentX += (Program.MainConfiguration.PlanetInfoViewSize);
                                Size size = ImageGenerator.PaintDrawText(e, p.Count.ToString(), currentX, currentY - (Program.MainConfiguration.PlanetInfoViewSize), dotColour, _planetInfoFont);
                                currentX += size.Width + (Program.MainConfiguration.PlanetInfoViewSize);

                            }

                            foreach (var mouseOverPlanet in CurrentPlanetInfos.Where(o => o.CurrentMouseOver))
                            {
                                Color fontColour = Color.Red;
                                switch (mouseOverPlanet.PlanetType)
                                {
                                    case "(Storm)": fontColour = Program.MainConfiguration.PlanetInfoStormColour; break;
                                    case "(Barren)": fontColour = Program.MainConfiguration.PlanetInfoBarrenColour; break;
                                    case "(Temperate)": fontColour = Program.MainConfiguration.PlanetInfoTemperateColour; break;
                                    case "(Lava)": fontColour = Program.MainConfiguration.PlanetInfoLavaColour; break;
                                    case "(Plasma)": fontColour = Program.MainConfiguration.PlanetInfoPlasmaColour; break;
                                    case "(Gas)": fontColour = Program.MainConfiguration.PlanetInfoGasColour; break;
                                    case "(Ice)": fontColour = Program.MainConfiguration.PlanetInfoIceColour; break;
                                    case "(Oceanic)": fontColour = Program.MainConfiguration.PlanetInfoOceanicColour; break;
                                    default: fontColour = Color.Red; break;
                                }
                                Size size = ImageGenerator.PaintDrawText(e, mouseOverPlanet.PlanetType + " " + mouseOverPlanet.Planets,
                                    mouseOverPlanet.X - (Program.MainConfiguration.PlanetInfoViewSize),
                                    mouseOverPlanet.Y - ((Program.MainConfiguration.PlanetInfoViewSize * 2) + Program.MainConfiguration.PlanetInfoFontSize),
                                    fontColour, _planetInfoFont);
                            }
                            ;
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }

        private void UpdateISK()
        {
			void UpdateOverlayRatting(string text, Color colour)
			{
				if (OverlayMapWindow == null)
					return;

				OverlayMapWindow.RattingData.Text = text;
				OverlayMapWindow.RattingData.ForeColor = colour;
				OverlayMapWindow.UpdateMapData();
			}
			void UpdateRattingData(string text, Color colour)
			{
				RattingData.Text = text;
				RattingData.ForeColor = colour;
			}

			var mainConfig = Program.MainConfiguration.MainWindowItemLocations.Find(o => o.Name.Equals("RattingData"));
			var overlayConfig = Program.MainConfiguration.OverlayWindowItemLocations.Find(o => o.Name.Equals("RattingData"));

			if (ISKPerHourTotal == false)
            {
                if (CurrentActiveToon != null && Program.GraphDataManager.GetIskDataKeys().Contains(CurrentActiveToon))
                {
                    UpdateRattingData(Program.GraphDataManager.GetIskLastHour(CurrentActiveToon), mainConfig.Color);
                    UpdateOverlayRatting(RattingData.Text, overlayConfig.Color);
                }
                else
                {
                    UpdateRattingData("", mainConfig.Color);
                    UpdateOverlayRatting("", overlayConfig.Color);
                }
            }
            else
            {
                    UpdateRattingData(Program.GraphDataManager.GetIskLastHour(""), mainConfig.AlternateColor);
                    UpdateOverlayRatting(RattingData.Text, overlayConfig.AlternateColor);
            }
        }
        private void UpdateMining()
        {
            void UpdateOverlayMining(string text, Color colour)
		    {
			    if (OverlayMapWindow == null)
				    return;

			    OverlayMapWindow.MiningData.Text = text;
			    OverlayMapWindow.MiningData.ForeColor = colour;
			    OverlayMapWindow.UpdateMapData();
		    }
			void UpdateMiningData(string text, Color colour)
			{
				MiningData.Text = text;
				MiningData.ForeColor = colour;
			}

			var mainConfig = Program.MainConfiguration.MainWindowItemLocations.Find(o => o.Name.Equals("MiningData"));
			var overlayConfig =Program.MainConfiguration.OverlayWindowItemLocations.Find(o => o.Name.Equals("MiningData"));

			if (MiningPerHourTotal == false)
            {
                if (CurrentActiveToon != null && Program.GraphDataManager.GetMiningDataKeys().Contains(CurrentActiveToon))
                {
                    UpdateMiningData(Program.GraphDataManager.GetMiningLastHour(CurrentActiveToon), mainConfig.Color);
                    UpdateOverlayMining(MiningData.Text, overlayConfig.Color);
                }
                else
                {
					UpdateMiningData("", mainConfig.Color);
					UpdateOverlayMining("", overlayConfig.Color);
				}
            }
            else
            {
				UpdateMiningData(Program.GraphDataManager.GetMiningLastHour(""), mainConfig.AlternateColor);
				UpdateOverlayMining(MiningData.Text, overlayConfig.AlternateColor);
            }            
        }

        private void lblEtxraInformation_Click(object sender, EventArgs e)
        {

        }

		private long? _mouseMoveLastSystemId;

		private void pMinimapImage_MouseMove(object sender, MouseEventArgs e)
		{
			var systems = Program.Universe.MouseOverSystem(e.X,e.Y,MinimapSystems);
			mouseOverMinimapSystems = systems;

			var system = systems.FirstOrDefault();
			if (system == null)
			{
				_mouseMoveLastSystemId = null;
				MouseoverSystemName.Text = "";
				return;
			}

			var systemId = system.solarSystem.Id;
			if (_mouseMoveLastSystemId == systemId) return;

			MouseoverSystemName.Text = "";
			foreach (var m in mouseOverMinimapSystems)
			{
				if (!string.IsNullOrEmpty(MouseoverSystemName.Text)) MouseoverSystemName.Text += ",";
				MouseoverSystemName.Text += m.solarSystem.Name + (m.solarSystem.GetNPCKillsDeltaLastHour() == null ? "" : $"  {m.solarSystem.GetNPCKillsDeltaLastHour()}");
			}

			_mouseMoveLastSystemId = systemId;
			Program.Universe.GetSystemInfo((int)systemId);
			Invalidate();
		}
		private void pMinimapImage_Click(object sender, EventArgs e)
        {

        }
        private void ShowAlert(AlertQueueItem alert, bool sendToEveO = true)
        {

			if (InvokeRequired)
			{
				BeginInvoke((MethodInvoker)(() => ShowAlert(alert, sendToEveO)));
				return;
			}

			if (Program.MainConfiguration.ShowAlertFancyNotification) ShowAlertWindow(alert);
            if (Program.MainConfiguration.ShowAlertWindowsNotification) ShowAlertToast(alert);
            if (sendToEveO) EveOClient.AlertClient(alert);
        }
        private void ShowAlertWindow(AlertQueueItem alert)
        {
            // try to prevent new alert stealing focus
            var NewAlertWindow = new QuesadillaAlertPopup(alert, Program.MainConfiguration, Program.CachedBackground, this);
            //var ptr =   User32NativeMethods.GetForegroundWindowHandle();
            NewAlertWindow.Show(this);
            //var result = User32NativeMethods.SetForegroundWindowHandle(ptr);
        }

        private void SetupToastAlert()
        {
			ToastNotificationManagerCompat.OnActivated += toastArgs =>
			{
				// Obtain the arguments from the notification
				ToastArguments args = ToastArguments.Parse(toastArgs.Argument);
				// https://stackoverflow.com/questions/4580263/how-to-open-in-default-browser-in-c-sharp
				System.Diagnostics.Process.Start(args["url"]);
			};
		}
		private void ShowAlertToast(AlertQueueItem alert)
        {
            var t = new ToastContentBuilder();

            if (alert.Path != null)
            {

				var dotlanUrl = $"https://evemaps.dotlan.net/map/{alert.Path.ToSystemRegionName.Replace(" ", "_")}/{alert.Path.ToSystemName}";

				t.AddButton(new ToastButton()
                    .SetContent($"View {alert.Path.ToSystemName}")
                    .AddArgument("action", "viewURL")
                    .AddArgument("url", dotlanUrl)
                    );

                t.AddText($"System: {alert.Path.ToSystemName} ({alert.Path.ToSystemRegionName})");
            }

            if (alert.Kill != null)
            {
				var zkillUrl = $"https://zkillboard.com/kill/{alert.Kill.ID}/";

                t.AddButton(new ToastButton()
                    .SetContent($"View Zkill")
                    .AddArgument("action", "viewURL")
                    .AddArgument("url", zkillUrl)
                    );

				var shipUrl = $"https://images.evetech.net/types/{alert.Kill.VictimShipTypeId}/render?size=128";

				if (Uri.TryCreate(shipUrl, UriKind.Absolute, out var img))
				{
					t.AddAppLogoOverride(img, ToastGenericAppLogoCrop.Circle);
					t.AddInlineImage(img, alert.Kill.VictimShipName);
				}

			}

            string contextInfo = "";
            if (alert.Path != null && !(alert.isFaction || alert.isDecloak))
            {
                contextInfo = alert.Path.TotalJumps.ToString() + " jump" + (alert.Path.TotalJumps == 1 ? "" : "s") + "-> " + alert.Context;
            }
            else
            {
                if (alert.isFaction)
                {
                    contextInfo = alert.Context;
                }
                else if (alert.isDecloak)
                {
                    contextInfo = alert.Context;
                }
                else

                {
                    contextInfo = alert.FriendlyName;
                }
            }

            t.AddText(contextInfo);

            string intelLine = "";
            if (alert.isFaction)
            {
                intelLine = "Faction Spawn";
                t.AddHeader("faction", "Faction spawn", "");
            }
            else if (alert.Kill != null)
            {
                t.AddHeader("zkill", "Zkill alert", "");
				intelLine = alert.Kill.Context?.Replace("\n", " - ") ?? "";
			}
            else
            {
                t.AddHeader("intel", "Intel alert", "");
				intelLine = alert.Entry?.LineContent ?? "";
			}
            t.AddText(intelLine);

            t.Show(toast =>
            {
                toast.ExpirationTime = DateTime.Now.AddSeconds(Program.MainConfiguration.AlertPopupSeconds);
            });

        }

        private void Quesadilla_MouseMove(object sender, MouseEventArgs e)
        {
            var WasMouseOver = CurrentPlanetInfos.Where(o => o.CurrentMouseOver == true).Count() > 0 ? true : false;
            foreach (var p in CurrentPlanetInfos)
            {
                if (
                    (p.X >= e.X - Program.MainConfiguration.PlanetInfoViewSize && p.X <= e.X + Program.MainConfiguration.PlanetInfoViewSize) &&
                    (p.Y >= e.Y - Program.MainConfiguration.PlanetInfoViewSize && p.Y <= e.Y + Program.MainConfiguration.PlanetInfoViewSize))
                {
                    p.CurrentMouseOver = true;
                }
                else
                {
                    p.CurrentMouseOver = false;
                }
            }
            var IsMouseOver = CurrentPlanetInfos.Where(o => o.CurrentMouseOver == true).Count() > 0 ? true : false;
            if (WasMouseOver || IsMouseOver)
            {
                this.Invalidate();
            }
        }

        private void pMinimapImage_Paint(object sender, PaintEventArgs e)
        {
            if (MainMinimap.Visible)
            {
                if (Program.MainConfiguration.MainWindowShowExtraInformation) ImageGenerator.PaintDrawText(e.Graphics, ExtraInformation);
                if (mouseOverMinimapSystems.Count() > 0)
                {
                    foreach (var m in mouseOverMinimapSystems)
                    {
                        ImageGenerator.DrawSystemName(e.Graphics, m.X + MainMinimap.Left, m.Y + MainMinimap.Top, m.solarSystem.Name, Program.MainConfiguration.MinimapMouseoverSystemColour, 10);
                    }
                }
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ButtonMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void TopbarPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
				User32NativeMethods.ReleaseCapture();
				User32NativeMethods.SendMessage(Handle, User32NativeMethods.WM_NCLBUTTONDOWN, User32NativeMethods.HT_CAPTION, 0);
            }
        }

        public void UpdateAlertsOutstanding()
		{
			if (Program.AlertQueueManager.HasActiveAlerts())
			{
				AlertsOutstanding.Text = Program.AlertQueueManager.ActiveAlertCount().ToString();
			}
			else
			{
				AlertsOutstanding.Text = "";
			}
			if (Program.ZKillHandler.ZKillKills.Any())
			{
				AlertsOutstanding.Text += "/" + Program.ZKillHandler.ZKillKills.Count;
			}
		}

		public void UI(Action action)
		{
			if (InvokeRequired)
			{
				BeginInvoke(action);
			}
			else
			{
				action();
			}
		}


        #region Timer Ticks
        private void RefreshAnimations()
        {
			UI(() =>
			{
				UpdateMinimap();
			});
		}

		private void RefreshEveLocations()
		{
			foreach (var c in Program.CharacterManager.Characters)
			{
				EveOClient.UpdateSystem(c.CharacterName, c.Location);
			}
		}
		private void ClearEveLocations()
		{
			foreach (var c in Program.CharacterManager.Characters)
			{
				EveOClient.UpdateSystem(c.CharacterName, "");
			}
		}

		private void RefreshIncursionData()
        {
            Program.ESIHandler.EnqueueRequestIncursions();
		}
        private void RefreshMiningCSV()
        {
            var miningDetail = Program.GraphDataManager.GetMiningLastHourDetail("", false);
            var miningSummary = Program.GraphDataManager.GetMiningItemSummary(miningDetail);

            try
            {
                using (var writer = new StreamWriter("miningdata.csv"))
                {
                    writer.WriteLine("Character,MinedItem,Amount,Wastage");
                    foreach (var m in miningDetail)
                    {
                        foreach (var md in m.Value)
                        {
                            writer.WriteLine($"{m.Key},{md.MiningItem},{md.MiningAmount},{md.WastageAmount}");
                        }
                    }

                    writer.WriteLine();
                    writer.WriteLine("-----------------------------------------------------------------------------------");
                    writer.WriteLine();
                    foreach (var m in miningSummary)
                    {
                        foreach (var md in m.Value)
                        {
                            writer.WriteLine($"{m.Key},{md.MiningItem},{md.MiningAmount},{md.WastageAmount}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
		private void RefreshMiningPerHour()
		{
			MiningPerHourTotal = !MiningPerHourTotal;

			if (!Program.MainConfiguration.ShowMiningTotal)
			{
				MiningPerHourTotal = false;
			}
			else
			{
				UI(() =>
				{
					UpdateMining();
				});
			}
		}
		private void RefreshISKPerHour()
		{
			ISKPerHourTotal = !ISKPerHourTotal;

			if (!Program.MainConfiguration.ShowISKTotal)
			{
				ISKPerHourTotal = false;
			}
			else
			{
				UI(() =>
				{
					UpdateISK();
				});
			}
		}

		private void ProcessAlertQueue()
		{
			if (!Program.AlertQueueManager.HasNewAlerts())
			{
				return;
			}

			if (!Program.AlertQueueManager.TryGetNextNewAlert(out var newAlert))
			{
				return;
			}

			if (newAlert.ShowAlert)
			{
				Log.Information(newAlert.AlertText);

				shouldUpdateMinimap = true;
				shouldUpdatePlanetInfo = true;

				ShowAlert(newAlert);

				SoundPlayer sound = Program.LoadSoundFromFile(newAlert.SoundFile);

				if (sound != null)
				{
					sound.Play();
				}

				UI(() =>
				{
					RebuildLatestAlertsMenu();
				});
			}
		}

		private void PerformCleardown()
		{
			int alertsExpired = 0;
			int zkillsExpired = 0;

			UI(() =>
			{
				if (Program.AlertQueueManager.HasActiveAlerts())
				{
					alertsExpired = Program.AlertQueueManager.ExpireAlerts();

					if (alertsExpired > 0)
					{
						RebuildLatestAlertsMenu();
					}
				}
                UpdateAlertsOutstanding();
			});

			Program.GraphDataManager.ExpireCombat();
			Program.GraphDataManager.ExpireISK();
			Program.GraphDataManager.ExpireMining();

			zkillsExpired = Program.ZKillHandler.ExpireZKillQueue();

			UI(() =>
			{
				if ((zkillsExpired + alertsExpired) > 0)
				{
					UpdateMinimap();
				}

				UpdateISK();
				UpdateMining();
			});
		}

		private void UpdateDPSGraphWindow()
		{
			if (!CombatAdded)
			{
				return;
			}


		}

		private void GeneralTick()
        {
			activityLine += ActivityLineInc;
			if (activityLine > ActivityLineMax)
			{
				activityLine = 0;
			}

			UI(() =>
			{
				WorkingIndicator.Text = Program.MainConfiguration.HeartBeatIndicatorWorkingText[activityLine].ToString();

				if (shouldUpdateMinimap)
				{
					UpdateMinimap();
				}
				if (shouldUpdatePlanetInfo)
				{
					UpdatePlanetInfo();
				}
				UpdateAlertsOutstanding();
			});
		}
        private void UpdateProcessesTick()
        {
            UI(() =>
            {
                UpdateEveProcesses(_refreshProcessList);
				_refreshProcessList = false;
            });
		}
		private void RefreshProcesses()
        {
            _refreshProcessList = true;
		}

		#endregion

		private async void Quesadilla_Shown(object sender, EventArgs e)
		{
			await Task.Delay(500);
            SplashScreen?.FadeOut();
		}


		private IEnumerable<AlertConfig> GetEnabledAlerts(AlertConfigType alertType)
		{
			return Program.MainConfiguration.AlertConfiguration
				.Where(a => a.Enabled && a.AlertType == alertType).ToList();
		}

	}
}