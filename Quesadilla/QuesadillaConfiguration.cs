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

            /*
            foreach(var s in _solarSystemManager.SolarSystems)
            {
                cmb.Items.Add(s.Value.Name);
            }
            */

            cmb.DataSource = _mainForm._solarSystems.Names.Keys.OrderBy(x => x).ToList();
            cmb.Text = systemName;
        }
        private void QuesadillaConfiguration_Load(object sender, EventArgs e)
        {
            SetSystem(cmbHomeSystem, _configuration.HomeSystem);
            cbStayOnTop.Checked = _configuration.StayOnTop;
            _dummyAlert = new AlertQueueItem()
            {
                AlertText = "X-x-X-x-X-x-X-x-X-x-X-x-X-x-X-x-X-x-X-x-X",
                Context = "systemXYZ",
                FriendlyName = "XxXxXxXxXxXxXxXxXxXxXx",
                MenuText = "XXxxXXxxXXxxXXxx",
                RouteText = "XxX -> xXx -> yYy -> YyY",
                Path = new PathInfo()
                {
                    FromSystemName = "fromsystem",
                    ToSystemName = "tosystem",
                    ToSystemRegionName = "regionname",
                    PathSystems = new List<int> { 1, 2, 3 },
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
            _alertPopup = new QuesadillaAlertPopup(_dummyAlert, _configuration, _mainForm.LoadImageFromFile(_configuration.AlertPopupBackground));
            _alertPopup.Show();
            _alertPopup.Hide();
            alertDemo.Image = GetFormImageWithoutBorders(_alertPopup);
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
            // Store items into config
            _mainForm.Configuration = _configuration;
            _configuration.Save(_mainForm.configuration_filename);
            _mainForm.ReloadConfiguration();
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
    }
}
