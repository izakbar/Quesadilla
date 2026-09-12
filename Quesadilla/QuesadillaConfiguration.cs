using Cyotek.Windows.Forms;
using Quesadilla.Classes;
using Quesadilla.Classes.Configuration;
using Quesadilla.Classes.Extensions;
using Quesadilla.Classes.Queues;
using Quesadilla.Classes.SolarSystems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Quesadilla
{
    public partial class QuesadillaConfiguration : Form
    {
        private MenuItem _configMenu;
        private Config _configuration;
        private Quesadilla _mainForm;

        private QuesadillaAlertPopup _alertPopup;
        private AlertQueueItem _dummyAlert;

        private bool _formLoaded = false;

		private CustomPropertyExtractorWrapper _propertySource;
		private int _fixedWidth;

		public QuesadillaConfiguration(Config configuration, MenuItem configWindowMenu, Quesadilla mainForm)
        {
            InitializeComponent();
            _configuration = configuration.Duplicate();
            _configMenu = configWindowMenu;
            _mainForm = mainForm;

			LocationCollectionEditor.CollectionChanged += OnCollectionChanged;
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
			_fixedWidth = Width;

			MinimumSize = new Size(Width, 640);
			MaximumSize = new Size(Width, 5000);

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

            SetBackground(cmbBackground, _configuration.AlertPopupBackground);

            _formLoaded = true;
            ShowAlertBitmap();

			_propertySource = new CustomPropertyExtractorWrapper(_configuration);
			ConfigurationProperties.SelectedObject = _propertySource;
			PopulateConfigurationFilter();
        }

		private void PopulateConfigurationFilter()
		{
			var categories = TypeDescriptor.GetProperties(_propertySource)
				.Cast<PropertyDescriptor>()
				.Select(p => p.Category)
				.Where(c => !string.IsNullOrWhiteSpace(c))
				.Distinct()
				.OrderBy(c => c)
				.ToList();

			cmbFilterCategory.Items.Clear();
			cmbFilterCategory.Items.Add("All Categories");

			foreach (var category in categories)
			{
				cmbFilterCategory.Items.Add(category);
			}

			cmbFilterCategory.SelectedIndex = 0;
		}

		private void ApplyPropertyFilter()
		{
			string filterText = txtFilterProperties.Text?.Trim();

			string category = null;

			if (cmbFilterCategory.SelectedIndex > 0)
			{
				category = cmbFilterCategory.SelectedItem.ToString();
			}

			ConfigurationProperties.SelectedObject = new PropertyGridFilterWrapper(
				_configuration,
				filterText,
				category);

			ConfigurationProperties.Refresh();
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
            if (!_formLoaded) return;

			_alertPopup?.Close();
			_alertPopup?.Dispose();

			_alertPopup = new QuesadillaAlertPopup(_dummyAlert, _configuration, Program.LoadImageFromFile(_configuration.AlertPopupBackground), _mainForm);
            _alertPopup.Show();
            _alertPopup.Hide();
            var _oldBitmap = alertDemo.Image;
			alertDemo.Image = GetFormImageWithoutBorders(_alertPopup);
            _oldBitmap?.Dispose();
		}

		private void OnCollectionChanged(object sender, EventArgs e)
		{
			ShowAlertBitmap();
		}

		private void QuesadillaConfiguration_FormClosing(object sender, FormClosingEventArgs e)
        {
            _configMenu.Enabled = true;

			_alertPopup?.Dispose();
			alertDemo.Image?.Dispose();

			LocationCollectionEditor.CollectionChanged -= OnCollectionChanged;
		}

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Program.SaveAndReloadConfiguration(_configuration);
			ShowAlertBitmap();
		}

        private void cmbBackground_SelectedIndexChanged(object sender, EventArgs e)
        {
            _configuration.AlertPopupBackground = cmbBackground.Text;
            ShowAlertBitmap();
        }
        private void cmbHomeSystem_SelectedIndexChanged(object sender, EventArgs e)
        {
            _configuration.HomeSystem = cmbHomeSystem.Text;
        }

		private void txtFilterProperties_TextChanged(object sender, EventArgs e)
		{
			ApplyPropertyFilter();
		}

		private void cmbFilterCategory_SelectedIndexChanged(object sender, EventArgs e)
		{
			ApplyPropertyFilter();
		}

		private void ConfigurationProperties_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
		{
			ShowAlertBitmap();
		}

		private void ConfigurationProperties_SelectedObjectsChanged(object sender, EventArgs e)
		{
			ShowAlertBitmap();
		}

		private void QuesadillaConfiguration_ResizeEnd(object sender, EventArgs e)
		{
			if (Width != _fixedWidth) Width = _fixedWidth;
		}
	}

	public class PropertyGridFilterWrapper : CustomTypeDescriptor
	{
		private readonly object _instance;
		private readonly string _filterText;
		private readonly string _category;

		public PropertyGridFilterWrapper(
			object instance,
			string filterText,
			string category)
			: base(TypeDescriptor.GetProvider(instance).GetTypeDescriptor(instance))
		{
			_instance = instance;
			_filterText = filterText ?? string.Empty;
			_category = category;
		}

		public override object GetPropertyOwner(PropertyDescriptor pd)
		{
			return _instance;
		}

		public override PropertyDescriptorCollection GetProperties()
		{
			return Filter(base.GetProperties());
		}

		public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			return Filter(base.GetProperties(attributes));
		}

		private PropertyDescriptorCollection Filter(PropertyDescriptorCollection properties)
		{
			List<PropertyDescriptor> filtered = new List<PropertyDescriptor>();

			foreach (PropertyDescriptor property in properties)
			{
				if (!string.IsNullOrEmpty(_category) &&
					!string.Equals(property.Category, _category, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				if (!string.IsNullOrWhiteSpace(_filterText))
				{
					bool match =
						property.Name.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) >= 0
						|| property.DisplayName.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) >= 0
						|| (property.Description?.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0;

					if (!match)
					{
						continue;
					}
				}

				filtered.Add(property);
			}

			return new PropertyDescriptorCollection(filtered.ToArray());
		}
	}


}
