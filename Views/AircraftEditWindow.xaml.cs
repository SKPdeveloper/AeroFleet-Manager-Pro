using System.Windows;

namespace AeroFleetManagerPro.Views
{
    public partial class AircraftEditWindow : Window
    {
        private ViewModels.AircraftEditViewModel? _viewModel;

        public AircraftEditWindow()
        {
            InitializeComponent();
        }

        public AircraftEditWindow(ViewModels.AircraftEditViewModel viewModel) : this()
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            
            // Підписуємося на зміни DialogResult для закриття вікна
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModels.AircraftEditViewModel.DialogResult) && _viewModel != null)
            {
                if (_viewModel.DialogResult.HasValue)
                {
                    DialogResult = _viewModel.DialogResult.Value;
                    Close();
                }
            }
        }

        protected override void OnClosed(System.EventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            base.OnClosed(e);
        }
    }
}