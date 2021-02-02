using System.Diagnostics;
using System.Windows;

namespace Components
{
    /// <summary>
    /// Interaction logic for AboutUI.xaml
    /// </summary>
    public partial class AboutUI : Window
    {
        public AboutUI()
        {
            InitializeComponent();

            _github.Click += (o, e) =>
            {
                Process.Start("https://github.com/KaustubhPatange/Kling");
            };
        }
    }
}
