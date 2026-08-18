using EVEStandard.Models;
using Microsoft.Extensions.Configuration;
using Quesadilla.Classes;
using Quesadilla.Classes.Configuration;
using Quesadilla.Classes.SolarSystems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Quesadilla
{
	public partial class QuesadillaOverlayMap : TransparentOverlayForm
	{
		private Quesadilla _main;
		private AnimationState AnimationStateKills;
		private AnimationState AnimationStateIntel;
		private AnimationState AnimationStateAlts;
		private AnimationState AnimationStateUniHoles;

		private Image _sourceImage = null;

		private List<MinimapSystem> MinimapSystems = new List<MinimapSystem>();
		private List<MinimapSystem> mouseOverMinimapSystems = new List<MinimapSystem>();
		private Point mouseOverPoint;
		private int mapScale = 100;

		public Label RattingData = new Label() { Name = "RattingData" };
		public Label MiningData = new Label() { Name = "MiningData" };
		public Label EveScoutStatus = new Label() { Name = "EveScoutStatus" };

		public QuesadillaOverlayMap(Quesadilla mainForm) : base()
		{
			_main = mainForm;
			InitializeComponent();

			Width = Program.MainConfiguration.OverlayMapWindowSize.Width <= 100 ? 100 : Program.MainConfiguration.OverlayMapWindowSize.Width;
			Height = Program.MainConfiguration.OverlayMapWindowSize.Height <= 50 ? 50 : Program.MainConfiguration.OverlayMapWindowSize.Height;

			AnimationStateKills = new AnimationState(state: 0, direction: -1, maximum: 36, pulse: false);
			AnimationStateIntel = new AnimationState(state: 1, direction: 1, maximum: 18, pulse: false);
			AnimationStateAlts = new AnimationState(state: 2, direction: -1, maximum: 12, pulse: false);
			AnimationStateUniHoles = new AnimationState(state: 3, direction: -1, maximum: 36, pulse: true);

			EnableMouseTracking = true;
			SetScreenPosition();

			this.Controls.Add(RattingData);
			this.Controls.Add(MiningData);
			this.Controls.Add(EveScoutStatus);

			Program.MainConfiguration.ApplyConfiguration(this, Program.MainConfiguration.OverlayWindowItemLocations, 0, false, false, true);

			mapScale = Program.MainConfiguration.OverlayMapMapScale;
			if (mapScale == 0) mapScale = 100;
		}

		public void SetScreenPosition()
		{
			// position on screen
			Location = new Point()
			{
				X = _main.currentEveWindowRect.Left + Program.MainConfiguration.OverlayMapWindowPosition.X,
				Y = _main.currentEveWindowRect.Top + SystemInformation.CaptionHeight + Program.MainConfiguration.OverlayMapWindowPosition.Y,
			};
		}

		protected override void RenderOverlay(Graphics g)
		{

			if (_sourceImage != null)
			{
				g.DrawImage(_sourceImage, 0, 0, Width, Height); // Image works directly with DrawImage

				ImageGenerator.PaintDrawText(g, EveScoutStatus);
			}

			if (mouseOverMinimapSystems.Count > 0)
			{
				var brush = new SolidBrush(Program.MainConfiguration.OverlayMinimapMouseoverSystemColour);
				var font = new Font("Segoe UI", 12f, FontStyle.Bold);
				g.DrawString($"{mouseOverMinimapSystems[0].solarSystem.Name}", font, brush, mouseOverPoint);
			}


			ImageGenerator.PaintDrawText(g, RattingData);

		}

		public void UpdateMapData()
		{
			string Toon = "";
			if (_main.CurrentActiveToon != null && _main.CurrentActiveToon != "")
			{
				Toon = _main.CurrentActiveToon;
			}

			if (Toon == null || Toon == "")
			{
				//				OverlayMinimap.Visible = false;
				return;
			}
			else
			{
				//				OverlayMinimap.Visible = true;
				// Set position based on eve window ?
				SetScreenPosition();
			}

			var c = Program.CharacterList.Where(o => o.CharacterName == Toon).FirstOrDefault();

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

			string extraInformation = "";

			AnimationStateKills.Advance();
			AnimationStateIntel.Advance();
			AnimationStateAlts.Advance();
			AnimationStateUniHoles.Advance();

			Image minimapImage = ImageGenerator.ClipMiniMap(new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb), _dummyPath,
				Program.MainConfiguration.OverlayMinimapJumps, Program.MainConfiguration.OverlayMapMapScale, Program.MainConfiguration.MinimapRawXY, ref extraInformation, ref MinimapSystems,
				Program.MainConfiguration.OverlayMinimapShowSystemText, Program.MainConfiguration.OverlayMinimapShowAllSystemText, Program.MainConfiguration.OverlayMinimapShowConnectingSystemText,
				Program.MainConfiguration.OverlayShowMinimapBorder, Program.MainConfiguration.MinimapBorderColour,
				Program.MainConfiguration.OverlayMinimapLineColourNormal,
				Program.MainConfiguration.OverlayMinimapLineColourOutOfRegion,
				Program.MainConfiguration.OverlayMinimapLineColourRoute,
				Program.MainConfiguration.OverlayMinimapSystemColourNormal,
				Program.MainConfiguration.OverlayMinimapSystemColourRoute,
				Program.MainConfiguration.OverlayMinimapSystemColourFrom,
				Program.MainConfiguration.OverlayMinimapSystemColourTo,
				Program.MainConfiguration.OverlayMinimapSystemColourText, Program.MainConfiguration.OverlayMinimapAllSystemColourText, Program.MainConfiguration.OverlayMinimapConnectingSystemColourText,
				Program.MainConfiguration.OverlayMinimapZKillColour,
				Program.MainConfiguration.OverlayMinimapIntelColour,
				Program.MainConfiguration.OverlayMinimapAltPositionColour,
				Program.MainConfiguration.OverlayMinimapUniHoleColour,
				Program.MainConfiguration.OverlayMinimapIncursionColour,
				Program.MainConfiguration.OverlayMinimapIncursionOutlineColour,
				Program.MainConfiguration.OverlayMinimapSecLimitColour,
				Program.MainConfiguration.OverlayMinimapSecLimitOutlineColour,
				Program.MainConfiguration.OverlayMinimapIceColour,
				Program.MainConfiguration.OverlayMinimapMercoxitColour,
					AnimationStateIntel.Value(),
					AnimationStateKills.Value(),
					AnimationStateAlts.Value(),
					AnimationStateUniHoles.Value(),
				false,
				Program.MainConfiguration.OverlayMinimapShowOutOfRegion,
				Program.MainConfiguration.OverlayMinimapSecLimit,
				Program.MainConfiguration.OverlayMinimapIncursionAlpha,
				Program.MainConfiguration.OverlayMinimapIncursionCloudSize,
				Program.MainConfiguration.OverlayMinimapSecLimitAlpha,
				Program.MainConfiguration.OverlayMinimapSecLimitCloudSize,
				Program.MainConfiguration.OverlayMinimapConnectionGap,
				Program.MainConfiguration.OverlayMinimapSecLimitValue
				);
			//                Bitmap minimapBitmap = new Bitmap(minimapImage);
			//                minimapBitmap.MakeTransparent(minimapBitmap.GetPixel(0, 0));
			_sourceImage = minimapImage;
			RenderAndUpdate();
		}



		protected override void OnOverlayMouseMove(Point clientLocation)
		{
			var hovered = Program.Universe.MouseOverSystem(clientLocation.X, clientLocation.Y, MinimapSystems);

			if (hovered.Count() > 0)
			{
				foreach (var s in hovered)
					Program.Universe.GetSystemInfo((int)s.solarSystem.Id);
			}

			// Only redraw if the hover set actually changed, to avoid re-rendering
			// on every single poll tick while the mouse is merely sitting still inside.
			if (!mouseOverMinimapSystems.SequenceEqual(hovered))
			{
				mouseOverMinimapSystems = hovered;
				mouseOverPoint = new Point(clientLocation.X, clientLocation.Y);
				RenderAndUpdate(); // replaces the old Invalidate() — painting is manual now, Invalidate does nothing
			}
		}

		protected override void OnOverlayMouseLeave()
		{
			if (mouseOverMinimapSystems.Count > 0)
			{
				mouseOverMinimapSystems.Clear();
				RenderAndUpdate();
			}
		}
	}
}
