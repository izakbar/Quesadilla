using EVEStandard.Models;
using Microsoft.Extensions.Configuration;
using Quesadilla.Classes;
using Quesadilla.Classes.Configuration;
using Quesadilla.Classes.ImageGeneration;
using Quesadilla.Classes.Queues;
using Quesadilla.Classes.SolarSystems;
using Serilog.Debugging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Media;
using System.Numerics;
using System.Reflection.Emit;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace Quesadilla
{
	public partial class QuesadillaAlertPopup : Form
	{
		private AlertQueueItem _alert;
		private int _seconds;

		private int _opacity;
		private int _opacitystep;
		private PopupLocationType _location;

		private bool ShowShipname = false;

		private Quesadilla _main;

		public bool IsZkill = false;
		public bool IsAlert = true;

		private const string ALERT_TEXT = "Alert";
		private const string FACTION_TEXT = "Faction Spawn";
		private const string DECLOAK_TEXT = "Pilot decloaked";

		private const int ZOOM_SIZE_INCREASE = 1;

		private bool _rightMouseDown = false;
		private Point diffPoint;

		private List<MinimapSystem> MinimapSystems = new List<MinimapSystem>();
		private List<MinimapSystem> mouseOverMinimapSystems = new List<MinimapSystem>();

		private Config _config;
		private MinimapParameters _minimapParameters = new MinimapParameters();

		public QuesadillaAlertPopup(AlertQueueItem alert, Config configuration, Image background, Quesadilla mainForm)
		{
			_main = mainForm;
			InitializeComponent();

			SetStyle(ControlStyles.SupportsTransparentBackColor, true);

			_alert = alert;
			_seconds = configuration.AlertPopupSeconds;
			_opacity = configuration.AlertPopupOpacity;
			_opacitystep = configuration.AlertPopupOpacityStep;

			_config = configuration;

			UpdateMinimapParameters();

			Opacity = 0;

			configuration.ApplyConfiguration(this, configuration.AlertWindowItemLocations, 0, false, false, true);

			Width = configuration.AlertWindowSize.Width;
			Height = configuration.AlertWindowSize.Height;

			AlertText.Text = _alert.AlertText;

			if (alert.Kill != null)
			{
				IsZkill = true;
				IsAlert = false;
			}

			if (alert.Path != null)
			{
				SystemName.Text = _alert.Path.ToSystemName;
			}
			else
			{
				if (alert.isFaction)
				{
					SystemName.Text = FACTION_TEXT;
				}
				else if (alert.isDecloak)
				{
					SystemName.Text = DECLOAK_TEXT;
				}
				else
				{
					SystemName.Text = ALERT_TEXT;
				}
			}

			if (alert.Path != null)
			{
				RegionName.Text = _alert.Path.ToSystemRegionName;
			}
			else
			{
				RegionName.Text = "";
				if (alert.isDecloak)
				{
					RegionName.Text = alert.Entry.CharacterName;
				}
			}

			if (alert.Path != null && !(alert.isFaction || alert.isDecloak))
			{
				ContextInfo.Text = _alert.Path.TotalJumps.ToString() + " jump" + (_alert.Path.TotalJumps == 1 ? "" : "s") + "-> " + _alert.Context;
			}
			else
			{
				if (alert.isFaction)
				{
					ContextInfo.Text = alert.Context;
				}
				else if (alert.isDecloak)
				{
					ContextInfo.Text = alert.Context;
				}
				else

				{
					ContextInfo.Text = alert.FriendlyName;
				}
			}

			if (alert.isFaction)
			{
				IntelLine.Text = FACTION_TEXT;
				AlertImage.Height = AlertImage.Width;
			}
			else if (IsZkill)
			{
				IntelLine.Text = alert.Kill.Context;
				try
				{
					AlertImage.Load($"https://images.evetech.net/types/{alert.Kill.VictimShipTypeId}/render");

					// Should we show alliance/corp logo ?
					PictureBox zKillAgressorLogo = null;
					PictureBox zKillVictimLogo = null;
					var KillerLogoUrl = "";
					if (_config.AlertPopupShowKillerLogo)
					{
						if ((alert.Kill.KillerAllianceId ?? 0) != 0)
						{
							KillerLogoUrl = $"https://images.evetech.net/alliances/{alert.Kill.KillerAllianceId}/logo";
						}
						if (KillerLogoUrl.Length == 0 && (alert.Kill.KillerCorporationId ?? 0) != 0)
						{
							KillerLogoUrl = $"https://images.evetech.net/corporations/{alert.Kill.KillerCorporationId}/logo";
						}
						if (KillerLogoUrl.Length > 0)
						{
							zKillAgressorLogo = new PictureBox();
							zKillAgressorLogo.Load($"{KillerLogoUrl}");
						}
					}

					var VictimLogoUrl = "";
					if (_config.AlertPopupShowVictimLogo)
					{
						if ((alert.Kill.VictimAllianceId ?? 0) != 0)
						{
							VictimLogoUrl = $"https://images.evetech.net/alliances/{alert.Kill.VictimAllianceId}/logo";
						}
						if (VictimLogoUrl.Length == 0 && (alert.Kill.VictimCorporationId ?? 0) != 0)
						{
							VictimLogoUrl = $"https://images.evetech.net/corporations/{alert.Kill.VictimCorporationId}/logo";
						}
						if (VictimLogoUrl.Length > 0)
						{
							zKillVictimLogo = new PictureBox();
							zKillVictimLogo.Load($"{VictimLogoUrl}");
						}
					}

					Image RoundedImage = ImageGenerator.ClipToCircle(AlertImage.Image, new PointF(AlertImage.Image.Width / 2, AlertImage.Image.Height / 2), AlertImage.Image.Width / 2,
						Color.Transparent, configuration.ZKillCircleColour, 10, alert.Kill.TotalValue, configuration.ZKillIskColour, configuration.ZKillShowBar, configuration.ZKillBarColour,
						zKillAgressorLogo?.Image, zKillVictimLogo?.Image,
						_config.AlertPopZkillHeight, _config.AlertPopZkillLogoSize);

					zKillAgressorLogo?.Dispose();
					zKillVictimLogo?.Dispose();

					Bitmap killBitmap = new Bitmap(RoundedImage);
					killBitmap.MakeTransparent(killBitmap.GetPixel(0, 0));
					var _oldImage = AlertImage.Image;
					AlertImage.Image = killBitmap;
					_oldImage?.Dispose();
					RoundedImage.Dispose();
				}
				catch (Exception e)
				{

				}
				ShipName.Text = alert.Kill.VictimShipName;
			}
			else
			{
				// Not zKill 
				AlertImage.Height = AlertImage.Width;

				IntelLine.Text = _alert.Entry.LineContent;
				ShipName.Text = "";
			}

			if (configuration.AlertPopupShowMinimap && (alert.Path != null && alert.Path.PathSystems != null && alert.Path.PathSystems.Count > 0 && alert.Path.FromSystemId != 0))
			{
				string extraInformation = "";
				Image minimapImage = ImageGenerator.ClipMiniMap(MinimapImage.Width, MinimapImage.Height, alert.Path, 100,
					ref extraInformation, ref MinimapSystems,
					null, null, null, null,null, null,null,
					_minimapParameters
					);
				Bitmap minimapBitmap = new Bitmap(minimapImage);
				minimapBitmap.MakeTransparent(minimapBitmap.GetPixel(0, 0));
				var _oldImage = MinimapImage.Image;
				MinimapImage.Image = minimapBitmap;
				_oldImage?.Dispose();
				MinimapImage.Visible = true;
			}
			else
			{
				MinimapImage.Visible = false;
			}

			BackgroundImage = background;

			// position on screen
			// position on screen
			Screen screen = Screen.FromControl(this);
			switch (configuration.AlertPopupLocation)
			{
				case PopupLocationType.TopLeft:
					Location = new Point()
					{
						X = screen.WorkingArea.Left,
						Y = screen.WorkingArea.Top,
					};
					break;
				case PopupLocationType.TopCenter:
					Location = new Point()
					{
						X = (screen.WorkingArea.Right / 2) - (Width / 2),
						Y = screen.WorkingArea.Top,
					};
					break;
				case PopupLocationType.TopRight:
					Location = new Point()
					{
						X = screen.WorkingArea.Right - Width,
						Y = screen.WorkingArea.Top,
					};
					break;
				case PopupLocationType.BottomLeft:
					Location = new Point()
					{
						X = screen.WorkingArea.Left,
						Y = screen.WorkingArea.Bottom - Height - 1,
					};
					break;
				case PopupLocationType.BottomCenter:
					Location = new Point()
					{
						X = (screen.WorkingArea.Right / 2) - (Width / 2),
						Y = screen.WorkingArea.Bottom - Height - 1,
					};
					break;
				case PopupLocationType.BottomRight:
					Location = new Point()
					{
						X = screen.WorkingArea.Right - Width,
						Y = screen.WorkingArea.Bottom - Height - 1,
					};
					break;
				case PopupLocationType.Center:
				default:
					Location = new Point()
					{
						X = (screen.WorkingArea.Right / 2) - (Width / 2),
						Y = (screen.WorkingArea.Bottom / 2) - (Height / 2),
					};
					break;
			}
		}
		public void UpdateMinimapParameters()
		{
			_minimapParameters.StormCentreOutlineAlpha = Program.MainConfiguration.AlertPopupMinimapStormCentreOutlineAlpha;
			_minimapParameters.StormStrongOutlineAlpha = Program.MainConfiguration.AlertPopupMinimapStormStrongOutlineAlpha;
			_minimapParameters.StormWeakOutlineAlpha = Program.MainConfiguration.AlertPopupMinimapStormWeakOutlineAlpha;
			_minimapParameters.StormMainAlpha = Program.MainConfiguration.AlertPopupMinimapStormMainAlpha;
			_minimapParameters.ShowStorm = Program.MainConfiguration.AlertPopupMinimapShowStorm;
			_minimapParameters.StormGlowLayers = Program.MainConfiguration.AlertPopupMinimapStormGlowLayers;

			_minimapParameters.SecLimitValue = Program.MainConfiguration.AlertPopupMinimapSecLimitValue;
			_minimapParameters.SecLimitAlphaAmount = Program.MainConfiguration.AlertPopupMinimapSecLimitAlpha;
			_minimapParameters.SecLimitCloudSize = Program.MainConfiguration.AlertPopupMinimapSecLimitCloudSize;
			_minimapParameters.SecLimitShow = Program.MainConfiguration.AlertPopupMinimapSecLimitShow;
			_minimapParameters.SecLimitColour = Program.MainConfiguration.AlertPopupMinimapSecLimitColour;
			_minimapParameters.SecLimitOutlineColour = Program.MainConfiguration.AlertPopupMinimapSecLimitOutlineColour;
			_minimapParameters.StormSize = Program.MainConfiguration.AlertPopupMinimapStormSize;

			_minimapParameters.IncursionAlphaAmount = Program.MainConfiguration.AlertPopupMinimapIncursionAlpha;
			_minimapParameters.IncursionCloudSize = Program.MainConfiguration.AlertPopupMinimapIncursionCloudSize;
			_minimapParameters.IncursionColour = Program.MainConfiguration.AlertPopupMinimapIncursionColour;
			_minimapParameters.IncursionOutlineColour = Program.MainConfiguration.AlertPopupMinimapIncursionOutlineColour;
			_minimapParameters.IncursionShow = Program.MainConfiguration.AlertPopupMinimapIncursionShow;

			_minimapParameters.BorderColor = Program.MainConfiguration.AlertPopupMinimapBorderColour;
			_minimapParameters.LineColourNormal = Program.MainConfiguration.AlertPopupMinimapLineColourNormal;
			_minimapParameters.LineColourOutOfRegion = Program.MainConfiguration.AlertPopupMinimapLineColourOutOfRegion;
			_minimapParameters.LineColourRoute = Program.MainConfiguration.AlertPopupMinimapLineColourRoute;
			_minimapParameters.SystemColourNormal = Program.MainConfiguration.AlertPopupMinimapSystemColourNormal;
			_minimapParameters.SystemColourRoute = Program.MainConfiguration.AlertPopupMinimapSystemColourRoute;
			_minimapParameters.SystemColourFrom = Program.MainConfiguration.AlertPopupMinimapSystemColourFrom;
			_minimapParameters.SystemColourTo = Program.MainConfiguration.AlertPopupMinimapSystemColourTo;
			_minimapParameters.SystemColourText = Program.MainConfiguration.AlertPopupMinimapSystemColourText;
			_minimapParameters.SystemColourAllText = Program.MainConfiguration.AlertPopupMinimapColourAllText;
			_minimapParameters.SystemColourConnectingText = Program.MainConfiguration.AlertPopupMinimapColourConnectingText;
			_minimapParameters.ZKillColour = Program.MainConfiguration.AlertPopupMinimapZKillColour;
			_minimapParameters.IntelColour = Program.MainConfiguration.AlertPopupMinimapIntelColour;
			_minimapParameters.AltColour = Program.MainConfiguration.AlertPopupMinimapAltColour;
			_minimapParameters.UniHoleColour = Program.MainConfiguration.AlertPopupMinimapUniHoleColour;
			_minimapParameters.IceColour = Program.MainConfiguration.AlertPopupMinimapIceColour;
			_minimapParameters.MercoxitColour = Program.MainConfiguration.AlertPopupMinimapMercoxitColour;
		}

		private void QuesadillaAlertPopup_Load(object sender, EventArgs e)
		{
			Visible = true;
			Opacity = 0;
			timerAlertWindow.Interval = _seconds * 1000;
			timerAlertWindow.Enabled = true;
			timerOpacityOpen.Enabled = true;
			ShowInTaskbar = false;
		}

		private void CloseWindow()
		{
			timerOpacityClose.Enabled = true;
		}

		private void timerAlertWindow_Tick(object sender, EventArgs e)
		{
			CloseWindow();
		}

		private void timerOpacityOpen_Tick(object sender, EventArgs e)
		{
			Opacity = Math.Min(
				(double)_opacity / 100,
				Opacity + ((double)_opacitystep / 100));

			if (Opacity >= (double)_opacity / 100)
			{
				timerOpacityOpen.Enabled = false;
			}
		}

		private void label1_Click(object sender, EventArgs e)
		{

		}

		private void QuesadillaAlertPopup_Click(object sender, EventArgs e)
		{
			MouseEventArgs e2 = (MouseEventArgs)e;
			Point clickLocation = new Point(e2.X, e2.Y);
			Rectangle r = new Rectangle(SystemName.Location.X, SystemName.Location.Y, SystemName.Width, SystemName.Height);
			if (r.Contains(clickLocation))
			{
				SystemName_Click(sender, e);
			}
			else
			{
				Opacity = 0;
				CloseWindow();
			}
		}

		private void timerOpacityClose_Tick(object sender, EventArgs e)
		{
			Opacity -= (double)_opacitystep / 100;

			if (Opacity <= 0)
			{
				Close();
			}
		}

		private void SystemName_Click(object sender, EventArgs e)
		{
			if (SystemName.Text != ALERT_TEXT)
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = "https://evemaps.dotlan.net/map/" + RegionName.Text.Replace(" ", "_") + "/" + SystemName.Text,
					UseShellExecute = true
				});
			}
		}

		protected override bool ShowWithoutActivation
		{
			get { return true; }
		}

		private void pAlertImage_Click(object sender, EventArgs e)
		{
			if (IsZkill)
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = $"https://zkillboard.com/kill/{_alert.Kill.ID}/",
					UseShellExecute = true
				});
			}
		}

		private void Zoom_MouseEnter(object sender, EventArgs e)
		{
			System.Windows.Forms.Label x = sender as System.Windows.Forms.Label;
			var _oldFont = x.Font;
			x.Font = new Font(x.Font.Name, x.Font.Size + ZOOM_SIZE_INCREASE, FontStyle.Bold);
			_oldFont?.Dispose();
			x.TextAlign = ContentAlignment.TopLeft;
		}

		private void Zoom_MouseLeave(object sender, EventArgs e)
		{
			System.Windows.Forms.Label x = sender as System.Windows.Forms.Label;
			var _oldFont = x.Font;
			x.Font = new Font(x.Font.Name, x.Font.Size - ZOOM_SIZE_INCREASE, FontStyle.Regular);
			_oldFont?.Dispose();
			x.TextAlign = ContentAlignment.TopRight;
		}

		private void IntelLine_Click(object sender, EventArgs e)
		{

		}

		private void QuesadillaAlertPopup_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				_rightMouseDown = true;
				diffPoint.X = System.Windows.Forms.Cursor.Position.X - this.Left;
				diffPoint.Y = System.Windows.Forms.Cursor.Position.Y - this.Top;
			}
		}

		private void QuesadillaAlertPopup_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				_rightMouseDown = false;
			}
		}

		private void QuesadillaAlertPopup_MouseMove(object sender, MouseEventArgs e)
		{
			if (_rightMouseDown)
			{
				this.Left = System.Windows.Forms.Cursor.Position.X - diffPoint.X;
				this.Top = System.Windows.Forms.Cursor.Position.Y - diffPoint.Y;
			}
		}

		private void pAlertImage_MouseEnter(object sender, EventArgs e)
		{
			if (IsZkill)
			{
				//ShipName.Visible = true;
				ShowShipname = true;
				this.Invalidate();
			}
		}

		private void pAlertImage_MouseLeave(object sender, EventArgs e)
		{
			//ShipName.Visible = false;
			ShowShipname = false;
			this.Invalidate();
		}

		private void pMinimap_Click(object sender, EventArgs e)
		{

		}

		private void QuesadillaAlertPopup_Paint(object sender, PaintEventArgs e)
		{
			if (_config.GetConfiguration("AlertText", _config.AlertWindowItemLocations).IsVisible) ImageGenerator.PaintDrawText(e.Graphics, AlertText);
			if (_config.GetConfiguration("SystemName", _config.AlertWindowItemLocations).IsVisible) ImageGenerator.PaintDrawText(e.Graphics, SystemName);
			if (_config.GetConfiguration("RegionName", _config.AlertWindowItemLocations).IsVisible) ImageGenerator.PaintDrawText(e.Graphics, RegionName);
			if (_config.GetConfiguration("ShipName", _config.AlertWindowItemLocations).IsVisible && ShowShipname)
			{
				ImageGenerator.PaintDrawText(e.Graphics, ShipName);
			}
			if (_config.GetConfiguration("ContextInfo", _config.AlertWindowItemLocations).IsVisible) ImageGenerator.PaintDrawText(e.Graphics, ContextInfo);
			if (_config.GetConfiguration("IntelLine", _config.AlertWindowItemLocations).IsVisible) ImageGenerator.PaintDrawText(e.Graphics, IntelLine);

			if (MinimapImage.Visible)
			{
				if (mouseOverMinimapSystems.Count() > 0)
				{
					foreach (var m in mouseOverMinimapSystems)
					{
						ImageGenerator.DrawSystemName(e.Graphics, m.X + MinimapImage.Left, m.Y + MinimapImage.Top, m.solarSystem.Name, Program.MainConfiguration.AlertPopupMinimapMouseoverSystemColour);
					}
				}
			}
		}

		private void ShipName_Click(object sender, EventArgs e)
		{

		}

		private void QuesadillaAlertPopup_Shown(object sender, EventArgs e)
		{
			var _oldBitmap = _main.latestAlertBitmap;
			_main.latestAlertBitmap = new Bitmap(this.Width, this.Height);
			this.DrawToBitmap(_main.latestAlertBitmap, new Rectangle(Point.Empty, _main.latestAlertBitmap.Size));
			_main.latestAlertTimestamp = DateTime.Now;
			_oldBitmap?.Dispose();
			// _main.latestAlertBitmap.Save("QuesadillaLatestAlert.png");
		}

		private void ContextInfo_Click(object sender, EventArgs e)
		{

		}

		private void pMinimapImage_MouseMove(object sender, MouseEventArgs e)
		{
			mouseOverMinimapSystems = Program.Universe.MouseOverSystem(e.X, e.Y, MinimapSystems);
			if (mouseOverMinimapSystems.Count() > 0)
			{
				this.Invalidate();
			}
		}

		private void RegionName_Click(object sender, EventArgs e)
		{

		}

		protected override CreateParams CreateParams
		{
			get
			{
				const int WS_EX_NOACTIVATE = 0x08000000;

				var cp = base.CreateParams;
				cp.ExStyle |= WS_EX_NOACTIVATE;

				return cp;
			}
		}
	}
}
