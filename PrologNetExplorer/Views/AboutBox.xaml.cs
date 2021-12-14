using System.Diagnostics;
using System.Reflection;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Navigation;

namespace DatalogExplorer.Views
{
    /// <summary>
    /// Interaction logic for AboutBox.xaml
    /// </summary>
    public partial class AboutBox : Window
    {
        public AboutBox()
        {
            this.DataContext = this;
            InitializeComponent();
        }

        private static Assembly? assembly = Assembly.GetEntryAssembly();

        public static string? AssemblyVersion
        {
            get
            {
                return assembly?.GetName()?.Version?.ToString();
            }
        }

        public string? AssemblyName
        {
            get
            {
                return assembly?.GetName().ToString();
            }
        }

        public string? FrameworkName
        {
            get
            {
                return assembly?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
            }
        }

        public string? AssemblyTitle => assembly?.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
        public string? Copyright => assembly?.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
        public string? Description => assembly?.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
        public string? Company => assembly?.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;

        public void Navigate(object sender, RequestNavigateEventArgs e)
        {
            var psi = new ProcessStartInfo
            {
                FileName = e.Uri.ToString(),
                UseShellExecute = true
            };
            Process.Start(psi);

            e.Handled = true;
        }
    }
}
