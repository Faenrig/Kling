using Adb_gui_Apkbox_plugin;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Components
{
    /// <summary>
    /// Interaction logic for KeyUI.xaml
    /// </summary>
    public partial class SettingsUI : Window
    {
        private int _RWidth, _RHeight;

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        public SettingsUI(int height, int width)
        {
            InitializeComponent();
            _RWidth = width;
            _RHeight = height;

            // Setting Options for Location combo box
            _locationComBo.ItemsSource = new string[] { "Top Left", "Top Right", "Bottom Left", "Bottom Right" };
            _cancel.Click += (o, e) => { Close(); };

            _save.Click += (o, e) =>
            {
                File.WriteAllText(@"config.ini",
                "[Settings]" + Environment.NewLine +
                $"displayindex={_locationComBo.SelectedIndex}" + Environment.NewLine +
                $"xaxis={_xaxis.Text}" + Environment.NewLine +
                $"yaxis={_yaxis.Text}" + Environment.NewLine +
                $"displaytime={(int)_timeSlider.Value}" + Environment.NewLine +
                $"notify={_messageCheckBox.IsChecked}" + Environment.NewLine +
                $"logkeys={_logkeys.IsChecked}" + Environment.NewLine +
                $"stdkeys={_standardCheckBox.IsChecked}" + Environment.NewLine);
                
                System.Windows.MessageBox.Show("Restart the application in order to apply the settings", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            };

            LoadConfigs();
            _locationComBo.SelectionChanged += (o, e) => {
                var ht = SystemInformation.VirtualScreen.Height;
                var wt = SystemInformation.VirtualScreen.Width;

                switch (_locationComBo.SelectedIndex)
                {
                    case 0:
                        UpdateValues("20", "20");
                        break;
                    case 1:
                        UpdateValues((wt - _RWidth).ToString(), "20");
                        break; 
                    case 2:
                        UpdateValues("20", (ht - Convert.ToInt16(_RHeight) - 60).ToString());
                        break;
                    case 3:
                        UpdateValues((wt - _RWidth).ToString(), (ht - Convert.ToInt16(_RHeight) - 60).ToString());
                        break;
                }
            };
        }
        private void UpdateValues(string x, string y)
        {
            _xaxis.Text = x;
            _yaxis.Text = y;
        }

        private void LoadConfigs()
        {
            if (File.Exists(@"config.ini"))
            {
                var myIni = new IniFile(@"config.ini");
                _locationComBo.SelectedIndex = Convert.ToInt16(myIni.Read("displayindex", "Settings"));
                _xaxis.Text = myIni.Read("xaxis", "Settings");
                _yaxis.Text = myIni.Read("yaxis", "Settings");
                _timeSlider.Value = Convert.ToDouble(myIni.Read("displaytime", "Settings"));
                _messageCheckBox.IsChecked = Convert.ToBoolean(myIni.Read("notify", "Settings"));
                _standardCheckBox.IsChecked = Convert.ToBoolean(myIni.Read("stdkeys", "Settings"));
                _logkeys.IsChecked = Convert.ToBoolean(myIni.Read("logkeys", "Settings"));
            }
        }

        private void TimeSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
           try
           {
                _timeText.Text = $"{(int)e.NewValue} seconds, till the text will fade out.";
           }
           catch { }
        }
    }
}
