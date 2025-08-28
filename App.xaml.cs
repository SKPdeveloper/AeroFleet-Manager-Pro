using System.Windows;
using AeroFleetManagerPro.Services;
using AeroFleetManagerPro.ViewModels;

namespace AeroFleetManagerPro
{
    public partial class App : Application
    {
        private SimpleServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Ініціалізація простого DI контейнера
            _serviceProvider = new SimpleServiceProvider();
            
            ConfigureServices();
            
            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        private void ConfigureServices()
        {
            if (_serviceProvider == null) return;

            // Реєструємо сервіси
            var navigationService = new NavigationService();
            var dataService = new DataService();
            
            _serviceProvider.RegisterSingleton(navigationService);
            _serviceProvider.RegisterSingleton(dataService);

            // Реєструємо ViewModels
            _serviceProvider.RegisterTransient(() => new MainViewModel(_serviceProvider, navigationService));
            _serviceProvider.RegisterTransient(() => new DashboardViewModel(dataService));
            _serviceProvider.RegisterTransient(() => new FleetViewModel(dataService));
            _serviceProvider.RegisterTransient(() => new AnalyticsViewModel(dataService));
            _serviceProvider.RegisterTransient(() => new PlanningViewModel(dataService));
            _serviceProvider.RegisterTransient(() => new MaintenanceViewModel(dataService));

            // Реєструємо головне вікно
            _serviceProvider.RegisterTransient(() => new MainWindow(_serviceProvider.GetService<MainViewModel>()));
        }
    }
}