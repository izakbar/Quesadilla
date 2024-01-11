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

        public QuesadillaConfiguration(Config configuration, MenuItem configWindowMenu, Quesadilla mainForm)
        {
            InitializeComponent();
            _configuration = configuration;
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
            _configuration.StayOnTop = cbStayOnTop.Checked;
            _configuration.HomeSystem = cmbHomeSystem.Text;
            _mainForm.ReloadConfiguration();

            Close();
        }
    }
}
