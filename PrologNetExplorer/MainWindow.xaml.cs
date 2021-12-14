using System.Windows;
namespace DatalogExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ViewModels.ViewModel viewModel;
        public MainWindow()
        {
            this.InitializeComponent();
            this.viewModel = new ViewModels.ViewModel(this);
            this.DataContext = viewModel;
        }
    }
}
