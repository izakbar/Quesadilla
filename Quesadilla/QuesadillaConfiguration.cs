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
using Cyotek.Windows.Forms;
using Quesadilla.Classes.Extensions;

namespace Quesadilla
{
    public partial class QuesadillaConfiguration : Form
    {
        private MenuItem _configMenu;
        private Config _configuration;
        private Quesadilla _mainForm;

        private QuesadillaAlertPopup _alertPopup;
        private AlertQueueItem _dummyAlert;

        private bool formLoaded = false;


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

            ConfigurationProperties.SetLabelColumnWidth(270);

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
                    FileName = "fooe.txt",
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

            trackAlertPopupMinimapSizeWidth.Minimum = 1;
            trackAlertPopupMinimapSizeHeight.Minimum = 1;
            trackAlertPopupMinimapPositionX.Minimum = 0;
            trackAlertPopupMinimapPositionY.Minimum = 0;

            trackAlertPopupMinimapSizeWidth.Value = _configuration.AlertPopupMinimapSize.X > trackAlertPopupMinimapSizeWidth.Maximum ? trackAlertPopupMinimapSizeWidth.Maximum : _configuration.AlertPopupMinimapSize.X;
            trackAlertPopupMinimapSizeHeight.Value = _configuration.AlertPopupMinimapSize.Y > trackAlertPopupMinimapSizeHeight.Maximum ? trackAlertPopupMinimapSizeHeight.Maximum : _configuration.AlertPopupMinimapSize.Y; 
            trackAlertPopupMinimapPositionX.Value = _configuration.AlertPopupMinimapPosition.X;
            trackAlertPopupMinimapPositionY.Value = _configuration.AlertPopupMinimapPosition.Y;

            formLoaded = true;
            ShowAlertBitmap();

            ConfigurationProperties.SelectedObject = new CustomPropertyExtractorWrapper(_configuration);

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
            if (!formLoaded) return;

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
            /*
                        colourPicker.Color = c;
                        colourPicker.SolidColorOnly = false;
                        colourPicker.AnyColor = true;
                        colourPicker.AllowFullOpen = true;
                        if (colourPicker.ShowDialog() == DialogResult.OK)
                        {
                            return(colourPicker.Color);
                        }
            */

            using (ColorPickerDialog dialog = new ColorPickerDialog
            {
                Color = c,
            })
            {
                //dialog.CustomColorsLoading += this.DialogCustomColorsLoadingHandler;
                //dialog.PreviewColorChanged += this.DialogColorChangedHandler;

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    c = dialog.Color;
                }

                //dialog.PreviewColorChanged -= this.DialogColorChangedHandler;
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
            _configuration.AlertPopupOpacity = ((TrackBar)sender).Value;
            lblOpacity.Text = ((TrackBar)sender).Value.ToString();
            ShowAlertBitmap();
        }

        private void trackPopupTime_Scroll(object sender, EventArgs e)
        {
            _configuration.AlertPopupSeconds = ((TrackBar)sender).Value;
            lblPopupSeconds.Text = ((TrackBar)sender).Value.ToString();
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

        private void Properties_Click(object sender, EventArgs e)
        {

        }

        private void ColourProperties_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            ShowAlertBitmap();
        }
    }
}
