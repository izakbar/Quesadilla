using Quesadilla.Classes.Queues;
using Quesadilla.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Quesadilla.Classes.SolarSystems;
using System.Xml.Xsl;

namespace Quesadilla
{
    public partial class QuesadillaConfiguration : Form
    {
        private MenuItem _configMenu;
        private Config _configuration;
        private Quesadilla _mainForm;

        private QuesadillaAlertPopup _alertPopup;
        private AlertQueueItem _dummyAlert;

        public QuesadillaConfiguration(Config configuration, MenuItem configWindowMenu, Quesadilla mainForm)
        {
            InitializeComponent();
            _configuration = configuration.Duplicate();
            _configMenu = configWindowMenu;
            _mainForm = mainForm;
        }

        private void SetSystem(ComboBox cmb, string systemName)
        {
            cmb.Items.Clear();
            cmb.DataSource = Program.Universe.Names.Keys.OrderBy(x => x).ToList();
            cmb.Text = systemName;
        }
        private void SetBackground(ComboBox cmb, string background)
        {
            cmb.Items.Clear();
            cmb.DataSource = Program.Backgrounds.Select(o => o.BackgroundName).ToList();
            cmb.Text = background;
        }
        private void QuesadillaConfiguration_Load(object sender, EventArgs e)
        {
            SetSystem(cmbHomeSystem, _configuration.HomeSystem);
            _dummyAlert = new AlertQueueItem()
            {
                AlertText = "X-x-X-x-X-x-X-x-X-x-X-x-X-x-X-x-X-x-X-x-X",
                Context = "systemXYZ",
                FriendlyName = "XxXxXxXxXxXxXxXxXxXxXx",
                MenuText = "XXxxXXxxXXxxXXxx",
                RouteText = "XxX -> xXx -> yYy -> YyY",
                Path = new PathInfo()
                {
                    FromSystemName = "Jita",
                    FromSystemId = 30000142,
                    ToSystemName = "Urlen",
                    ToSystemId = 30000139,
                    ToSystemRegionName = "The Forge",
                    PathSystems = new List<int> { 30000142, 30000144, 30000139 },
                }, 
                Entry = new Classes.Logfile.LogEntry()
                {
                    CharacterName = "xxxYYYzzz",
                    EntryType = Classes.Logfile.LogEntryType.ChatEvent,
                    FileName = "foo.txt",
                    LineContent = "XXpppPPPXXX",
                    LogPrefix = "foo",
                    LogTime = DateTime.Now.ToString(),
                    LogType = Classes.Logfile.LogFileType.ChatIntel,
                    PlayerName = "fofofo",
                    TimeAdded = DateTime.Now,
                    RawLine = "rawXXXraw",
                },
            };
            trackOpacity.Value = _configuration.AlertPopupOpacity;
            trackPopupTime.Value = _configuration.AlertPopupSeconds;
            trackDPSGraphLineSize.Value = _configuration.DPSGraphLineSize;
            trackShowLastUnexpiredAlerts.Value = _configuration.ShowLastUnexpiredAlerts;

            SetBackground(cmbBackground, _configuration.AlertPopupBackground);
            lblOpacity.Text = _configuration.AlertPopupOpacity.ToString();
            lblPopupSeconds.Text = _configuration.AlertPopupSeconds.ToString();
            lblDPSGraphLineSize.Text = _configuration.DPSGraphLineSize.ToString();
            lblShowLastUnexpiredAlerts.Text = _configuration.ShowLastUnexpiredAlerts.ToString();

            switch (_configuration.AlertPopupLocation)
            {
                case Classes.Configuration.PopupLocationType.Center:
                    radioAlertCenter.Checked = true; break;
                case Classes.Configuration.PopupLocationType.TopLeft:
                    radioAlertTopLeft.Checked = true; break;
                case Classes.Configuration.PopupLocationType.TopCenter:
                    radioAlertTopCenter.Checked = true; break;
                case Classes.Configuration.PopupLocationType.TopRight:
                    radioAlertTopRight.Checked = true; break;
                case Classes.Configuration.PopupLocationType.BottomLeft:
                    radioAlertBottomLeft.Checked = true;break;
                case Classes.Configuration.PopupLocationType.BottomCenter:
                    radioAlertBottomCenter.Checked = true; break;
                case Classes.Configuration.PopupLocationType.BottomRight:
                    radioAlertBottomRight.Checked = true;break;
            }

            switch (_configuration.DPSLocation)
            {
                case Classes.Configuration.PopupLocationType.Center:
                    radioDPSCenter.Checked = true; break;
                case Classes.Configuration.PopupLocationType.TopLeft:
                    radioDPSTopLeft.Checked = true; break;
                case Classes.Configuration.PopupLocationType.TopCenter:
                    radioDPSTopCenter.Checked = true; break;
                case Classes.Configuration.PopupLocationType.TopRight:
                    radioDPSTopRight.Checked = true; break;
                case Classes.Configuration.PopupLocationType.BottomLeft:
                    radioDPSBottomLeft.Checked = true; break;
                case Classes.Configuration.PopupLocationType.BottomCenter:
                    radioDPSBottomCenter.Checked = true; break;
                case Classes.Configuration.PopupLocationType.BottomRight:
                    radioDPSBottomRight.Checked = true; break;
            }

            cbDPSStayOnTop.Checked = _configuration.DPSGraphStayOnTop;
            cbDPSTransparent.Checked = _configuration.DPSGraphTransparent;
            cbDPSShowWindow.Checked = _configuration.DPSShowWindow;
            cbDPSAllToons.Checked = _configuration.DPSAllToons;
            cbStayOnTop.Checked = _configuration.StayOnTop;
            cbAlertTransparent.Checked = _configuration.AlertTransparent;
            cbShowZKillAlerts.Checked = _configuration.ShowZKillAlerts;
            cbZkillShowBar.Checked = _configuration.ZKillShowBar;
            cbAlertPopupMinimapShowSystemText.Checked = _configuration.AlertPopupMinimapShowSystemText;

            cbAlertPopupShowMinimapBorder.Checked = _configuration.AlertPopupShowMinimapBorder ;
            cbAlertPopupMinimap.Checked = _configuration.AlertPopupMinimap;

            trackAlertPopupMinimapSizeWidth.Minimum = 1;
            trackAlertPopupMinimapSizeHeight.Minimum = 1;
            trackAlertPopupMinimapPositionX.Minimum = 0;
            trackAlertPopupMinimapPositionY.Minimum = 0;

            trackAlertPopupMinimapSizeWidth.Value = _configuration.AlertPopupMinimapSize.X > trackAlertPopupMinimapSizeWidth.Maximum ? trackAlertPopupMinimapSizeWidth.Maximum : _configuration.AlertPopupMinimapSize.X;
            trackAlertPopupMinimapSizeHeight.Value = _configuration.AlertPopupMinimapSize.Y > trackAlertPopupMinimapSizeHeight.Maximum ? trackAlertPopupMinimapSizeHeight.Maximum : _configuration.AlertPopupMinimapSize.Y; 
            trackAlertPopupMinimapPositionX.Value = _configuration.AlertPopupMinimapPosition.X;
            trackAlertPopupMinimapPositionY.Value = _configuration.AlertPopupMinimapPosition.Y;

            ShowAlertBitmap();

        }
        private Bitmap GetControlImage(Control ctl)
        {
            Bitmap bm = new Bitmap(ctl.Width, ctl.Height);
            ctl.DrawToBitmap(bm,
                new Rectangle(0, 0, ctl.Width, ctl.Height));
            return bm;
        }
        private Bitmap GetFormImageWithoutBorders(Form frm)
        {
            // Get the form's whole image.
            using (Bitmap whole_form = GetControlImage(frm))
            {
                // See how far the form's upper left corner is
                // from the upper left corner of its client area.
                Point origin = frm.PointToScreen(new Point(0, 0));
                int dx = origin.X - frm.Left;
                int dy = origin.Y - frm.Top;

                // Copy the client area into a new Bitmap.
                int wid = frm.ClientSize.Width;
                int hgt = frm.ClientSize.Height;
                Bitmap bm = new Bitmap(wid, hgt);
                using (Graphics gr = Graphics.FromImage(bm))
                {
                    gr.DrawImage(whole_form, 0, 0,
                        new Rectangle(dx, dy, wid, hgt),
                        GraphicsUnit.Pixel);
                }
                return bm;
            }
        }

        private void ShowAlertBitmap()
        {
            if ( _alertPopup != null )
            {
                _alertPopup.Close();
            }
            _alertPopup = new QuesadillaAlertPopup(_dummyAlert, _configuration, Program.LoadImageFromFile(_configuration.AlertPopupBackground), _mainForm);
            _alertPopup.Show();
            _alertPopup.Hide();
            alertDemo.Image = GetFormImageWithoutBorders(_alertPopup);

            trackAlertPopupMinimapSizeWidth.Maximum = _alertPopup.Width - _configuration.AlertPopupMinimapPosition.X - 1;
            trackAlertPopupMinimapSizeHeight.Maximum = _alertPopup.Height - -_configuration.AlertPopupMinimapPosition.Y - 1;

            trackAlertPopupMinimapPositionX.Maximum = _alertPopup.Width - _configuration.AlertPopupMinimapSize.X - 1;
            trackAlertPopupMinimapPositionY.Maximum = _alertPopup.Height - -_configuration.AlertPopupMinimapSize.Y - 1;
        }

        private void QuesadillaConfiguration_FormClosing(object sender, FormClosingEventArgs e)
        {
            _configMenu.Enabled = true;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Program.SaveAndReloadConfiguration(_configuration);
            Close();
        }
        private Color ChooseColour(Color c)
        {
            colourPicker.Color = c;
            if (colourPicker.ShowDialog() == DialogResult.OK)
            {
                return(colourPicker.Color);
            }
            return (c);
        }

        private void buttonPickSystemColour_Click(object sender, EventArgs e)
        {
            _configuration.AlertPopupSystemNameColour = ChooseColour(_configuration.AlertPopupSystemNameColour);
            ShowAlertBitmap();
        }

        private void buttonPickContextColour_Click(object sender, EventArgs e)
        {
            _configuration.AlertPopupContextInfoColour = ChooseColour(_configuration.AlertPopupContextInfoColour);
            ShowAlertBitmap();
        }

        private void buttonPickRegionColour_Click(object sender, EventArgs e)
        {
            _configuration.AlertPopupRegionNameColour = ChooseColour(_configuration.AlertPopupRegionNameColour);
            ShowAlertBitmap();
        }

        private void buttonPickIntelLineColour_Click(object sender, EventArgs e)
        {
            _configuration.AlertPopupIntelLineColour = ChooseColour(_configuration.AlertPopupIntelLineColour);
            ShowAlertBitmap();
        }

        private void buttonPickAlertTextColour_Click(object sender, EventArgs e)
        {
            _configuration.AlertPopupAlertTextColour = ChooseColour(_configuration.AlertPopupAlertTextColour);
            ShowAlertBitmap();
        }

        private void cmbBackground_SelectedIndexChanged(object sender, EventArgs e)
        {
            _configuration.AlertPopupBackground = cmbBackground.Text;
            ShowAlertBitmap();
        }

        private void trackOpacity_Scroll(object sender, EventArgs e)
        {
            _alertPopup.Opacity = ((TrackBar)sender).Value;
            _configuration.AlertPopupOpacity = ((TrackBar)sender).Value;
            lblOpacity.Text = ((TrackBar)sender).Value.ToString();
        }

        private void trackPopupTime_Scroll(object sender, EventArgs e)
        {
            _configuration.AlertPopupSeconds = ((TrackBar)sender).Value;
            lblPopupSeconds.Text = ((TrackBar)sender).Value.ToString();
        }

        private void radioAlertTopLeft_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.AlertPopupLocation = Classes.Configuration.PopupLocationType.TopLeft;
        }

        private void radioAlertTopRight_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.AlertPopupLocation = Classes.Configuration.PopupLocationType.TopRight;
        }

        private void radioAlertBottomLeft_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.AlertPopupLocation = Classes.Configuration.PopupLocationType.BottomLeft;
        }

        private void radioAlertBottomRight_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.AlertPopupLocation = Classes.Configuration.PopupLocationType.BottomRight;
        }

        private void radioAlertCentre_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.AlertPopupLocation = Classes.Configuration.PopupLocationType.Center;
        }

        private void cbDPSStayOnTop_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.DPSGraphStayOnTop = cbDPSStayOnTop.Checked;
        }

        private void cbStayOnTop_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.StayOnTop = cbStayOnTop.Checked;
        }

        private void cbDPSTransparent_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.DPSGraphTransparent= cbDPSTransparent.Checked;
        }

        private void radioDPSTopLeft_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.DPSLocation = Classes.Configuration.PopupLocationType.TopLeft;
        }

        private void radioDPSTopRight_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.DPSLocation = Classes.Configuration.PopupLocationType.TopRight;
        }

        private void radioDPSCenter_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.DPSLocation = Classes.Configuration.PopupLocationType.Center;
        }

        private void radioDPSBottomLeft_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.DPSLocation = Classes.Configuration.PopupLocationType.BottomLeft;
        }

        private void radioDPSBottomRight_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.DPSLocation = Classes.Configuration.PopupLocationType.BottomRight;
        }

        private void cbDPSShowWindow_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.DPSShowWindow= cbDPSShowWindow.Checked;
        }

        private void cbDPSAllToons_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.DPSAllToons= cbDPSAllToons.Checked;
        }

        private void radioAlertTopCenter_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.AlertPopupLocation = Classes.Configuration.PopupLocationType.TopCenter;
        }

        private void radioAlertBottomCenter_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.AlertPopupLocation = Classes.Configuration.PopupLocationType.BottomCenter;
        }

        private void cbAlertTransparent_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.AlertTransparent= cbAlertTransparent.Checked;
        }

        private void cbShowZKillAlerts_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.ShowZKillAlerts= cbShowZKillAlerts.Checked;
        }

        private void buttonPickZKillCircleColour_Click(object sender, EventArgs e)
        {
            _configuration.ZKillCircleColour = ChooseColour(_configuration.ZKillCircleColour);
        }

        private void buttonPickZKillIskColour_Click(object sender, EventArgs e)
        {
            _configuration.ZKillIskColour = ChooseColour(_configuration.ZKillIskColour);
        }

        private void buttonPickZKillBarColour_Click(object sender, EventArgs e)
        {
            _configuration.ZKillBarColour = ChooseColour(_configuration.ZKillBarColour);
        }

        private void cbZkillShowBar_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.ZKillShowBar = cbZkillShowBar.Checked;
        }

        private void trackDPSGraphLineSize_Scroll(object sender, EventArgs e)
        {
            _configuration.DPSGraphLineSize = ((TrackBar)sender).Value;
            lblDPSGraphLineSize.Text = ((TrackBar)sender).Value.ToString();
        }

        private void trackShowLastUnexpiredAlerts_Scroll(object sender, EventArgs e)
        {
            _configuration.ShowLastUnexpiredAlerts = ((TrackBar)sender).Value;
            lblShowLastUnexpiredAlerts.Text = ((TrackBar)sender).Value.ToString();
        }

        private void cmbHomeSystem_SelectedIndexChanged(object sender, EventArgs e)
        {
            _configuration.HomeSystem = cmbHomeSystem.Text;
        }

        private void cbShowMinimap_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.AlertPopupMinimap = cbAlertPopupMinimap.Checked;
            ShowAlertBitmap();
        }

        private void buttonPickMinimapBorderColour_Click(object sender, EventArgs e)
        {
            _configuration.AlertPopupMinimapBorderColour = ChooseColour(_configuration.AlertPopupMinimapBorderColour);
            ShowAlertBitmap();
        }

        private void cbAlertPopupShowMinimapBorder_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.AlertPopupShowMinimapBorder = cbAlertPopupShowMinimapBorder.Checked;
            ShowAlertBitmap();
        }

        private void cbAlertPopupMinimapShowSystemText_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.AlertPopupMinimapShowSystemText = cbAlertPopupMinimapShowSystemText.Checked;
            ShowAlertBitmap();
        }

        private void buttonPickAlertPopupMinimapLineColorNormal_Click(object sender, EventArgs e)
        {
            _configuration.AlertPopupMinimapLineColourNormal = ChooseColour(_configuration.AlertPopupMinimapLineColourNormal);
            ShowAlertBitmap();
        }

        private void buttonPickAlertPopupMinimapLineColorRoute_Click(object sender, EventArgs e)
        {
            _configuration.AlertPopupMinimapLineColourRoute = ChooseColour(_configuration.AlertPopupMinimapLineColourRoute);
            ShowAlertBitmap();
        }

        private void buttonPickAlertPopupMinimapSystemColorNormal_Click(object sender, EventArgs e)
        {
            _configuration.AlertPopupMinimapSystemColourNormal = ChooseColour(_configuration.AlertPopupMinimapSystemColourNormal);
            ShowAlertBitmap();
        }

        private void buttonPickAlertPopupMinimapSystemColorRoute_Click(object sender, EventArgs e)
        {
            _configuration.AlertPopupMinimapSystemColourRoute = ChooseColour(_configuration.AlertPopupMinimapSystemColourRoute);
            ShowAlertBitmap();
        }

        private void buttonPickAlertPopupMinimapSystemColorFrom_Click(object sender, EventArgs e)
        {
            _configuration.AlertPopupMinimapSystemColourFrom = ChooseColour(_configuration.AlertPopupMinimapSystemColourFrom);
            ShowAlertBitmap();
        }

        private void buttonPickAlertPopupMinimapSystemColorTo_Click(object sender, EventArgs e)
        {
            _configuration.AlertPopupMinimapSystemColourTo = ChooseColour(_configuration.AlertPopupMinimapSystemColourTo);
            ShowAlertBitmap();
        }

        private void buttonPickAlertPopupMinimapSystemColorText_Click(object sender, EventArgs e)
        {
            _configuration.AlertPopupMinimapSystemColourText = ChooseColour(_configuration.AlertPopupMinimapSystemColourText);
            ShowAlertBitmap();
        }

        private void trackAlertPopupMinimapSizeWidth_Scroll(object sender, EventArgs e)
        {
            _configuration.AlertPopupMinimapSize = (new Point(((TrackBar)sender).Value, _configuration.AlertPopupMinimapSize.Y));
            ShowAlertBitmap();
        }

        private void trackAlertPopupMinimapSizeHeight_Scroll(object sender, EventArgs e)
        {
            _configuration.AlertPopupMinimapSize = (new Point(_configuration.AlertPopupMinimapSize.X ,((TrackBar)sender).Value));
            ShowAlertBitmap();

        }

        private void trackAlertPopupMinimapPositionX_Scroll(object sender, EventArgs e)
        {
            _configuration.AlertPopupMinimapPosition = (new Point(((TrackBar)sender).Value, _configuration.AlertPopupMinimapPosition.Y));
            ShowAlertBitmap();
        }

        private void trackAlertPopupMinimapPositionY_Scroll(object sender, EventArgs e)
        {
            _configuration.AlertPopupMinimapPosition = (new Point(_configuration.AlertPopupMinimapPosition.X, ((TrackBar)sender).Value));
            ShowAlertBitmap();
        }

        private void buttonPickMainWindowsBackgroundColour_Click(object sender, EventArgs e)
        {
            _configuration.MainWindowBackgroundColour = ChooseColour(_configuration.MainWindowBackgroundColour);
        }

        private void buttonPickMainWindowColourText_Click(object sender, EventArgs e)
        {
            _configuration.MainWindowColourText = ChooseColour(_configuration.MainWindowColourText);
        }

        private void buttonPickMainWindowLogBackgroundColour_Click(object sender, EventArgs e)
        {
            _configuration.MainWindowLogBackgroundColour = ChooseColour(_configuration.MainWindowLogBackgroundColour);
        }

        private void buttonPickMainWindowLogColourText_Click(object sender, EventArgs e)
        {
            _configuration.MainWindowLogColourText = ChooseColour(_configuration.MainWindowLogColourText);
        }
    }
}
