using System.Windows;
using AeroFleetManagerPro.ViewModels;

namespace AeroFleetManagerPro
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}