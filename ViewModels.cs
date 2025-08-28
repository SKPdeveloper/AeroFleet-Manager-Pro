using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using AeroFleetManagerPro.Models;
using AeroFleetManagerPro.Services;

namespace AeroFleetManagerPro.ViewModels
{
    // Базовый класс для всех ViewModels
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    // Простая реализация RelayCommand
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object? parameter) => _execute();
        
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => !_isExecuting && (_canExecute?.Invoke() ?? true);
        
        public async void Execute(object? parameter)
        {
            _isExecuting = true;
            CommandManager.InvalidateRequerySuggested();
            try
            {
                await _execute();
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
        
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    // Главная модель відображення
    public class MainViewModel : BaseViewModel
    {
        private readonly SimpleServiceProvider _serviceProvider;
        private readonly NavigationService _navigationService;
        private string _title = "AeroFleet Manager Pro - Система управління авіаційним флотом";
        private BaseViewModel? _currentViewModel;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public BaseViewModel? CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToFleetCommand { get; }
        public ICommand NavigateToAnalyticsCommand { get; }
        public ICommand NavigateToPlanningCommand { get; }
        public ICommand NavigateToMaintenanceCommand { get; }
        public ICommand AddAircraftCommand { get; }
        public ICommand AddFlightCommand { get; }

        public MainViewModel(SimpleServiceProvider serviceProvider, NavigationService navigationService)
        {
            _serviceProvider = serviceProvider;
            _navigationService = navigationService;
            
            NavigateToDashboardCommand = new RelayCommand(NavigateToDashboard);
            NavigateToFleetCommand = new RelayCommand(NavigateToFleet);
            NavigateToAnalyticsCommand = new RelayCommand(NavigateToAnalytics);
            NavigateToPlanningCommand = new RelayCommand(NavigateToPlanning);
            NavigateToMaintenanceCommand = new RelayCommand(NavigateToMaintenance);
            AddAircraftCommand = new RelayCommand(AddAircraft);
            AddFlightCommand = new RelayCommand(AddFlight);
            
            _navigationService.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(NavigationService.CurrentViewModel))
                {
                    CurrentViewModel = _navigationService.CurrentViewModel;
                }
            };

            NavigateToDashboard();
        }

        private void NavigateToDashboard()
        {
            var viewModel = _serviceProvider.GetService<DashboardViewModel>();
            _navigationService.NavigateTo(viewModel);
            Title = "AeroFleet Manager Pro - Панель управління";
        }

        private void NavigateToFleet()
        {
            var viewModel = _serviceProvider.GetService<FleetViewModel>();
            _navigationService.NavigateTo(viewModel);
            Title = "AeroFleet Manager Pro - Управління флотом";
        }

        private void NavigateToAnalytics()
        {
            var viewModel = _serviceProvider.GetService<AnalyticsViewModel>();
            _navigationService.NavigateTo(viewModel);
            Title = "AeroFleet Manager Pro - Аналітика";
        }

        private void NavigateToPlanning()
        {
            var viewModel = _serviceProvider.GetService<PlanningViewModel>();
            _navigationService.NavigateTo(viewModel);
            Title = "AeroFleet Manager Pro - Планування рейсів";
        }

        private void NavigateToMaintenance()
        {
            var viewModel = _serviceProvider.GetService<MaintenanceViewModel>();
            _navigationService.NavigateTo(viewModel);
            Title = "AeroFleet Manager Pro - Технічне обслуговування";
        }

        private async void AddAircraft()
        {
            var dataService = _serviceProvider.GetService<DataService>();
            var viewModel = new AircraftEditViewModel(dataService);
            var window = new Views.AircraftEditWindow(viewModel);
            window.Owner = System.Windows.Application.Current.MainWindow;
            
            if (window.ShowDialog() == true && viewModel.DialogResult == true)
            {
                System.Diagnostics.Debug.WriteLine("Літак успішно збережено, оновлюємо UI");
                System.Console.WriteLine("[MainViewModel] Літак успішно збережено, оновлюємо UI");
                
                // Якщо поточний ViewModel - FleetViewModel, оновлюємо його
                if (CurrentViewModel is FleetViewModel fleetViewModel)
                {
                    System.Diagnostics.Debug.WriteLine("Оновлюємо FleetViewModel");
                    System.Console.WriteLine("[MainViewModel] Оновлюємо FleetViewModel");
                    await fleetViewModel.LoadAircraftAsync();
                }
                else
                {
                    System.Console.WriteLine($"[MainViewModel] CurrentViewModel: {CurrentViewModel?.GetType().Name ?? "null"}");
                }
                
            }
        }

        private async void AddFlight()
        {
            var dataService = _serviceProvider.GetService<DataService>();
            var viewModel = new FlightEditViewModel(dataService);
            var window = new Views.FlightEditWindow(viewModel);
            window.Owner = System.Windows.Application.Current.MainWindow;
            
            if (window.ShowDialog() == true && viewModel.DialogResult == true)
            {
                System.Diagnostics.Debug.WriteLine("Рейс успішно збережено, оновлюємо UI");
                System.Console.WriteLine("[MainViewModel] Рейс успішно збережено, оновлюємо UI");
                
                // Якщо поточний ViewModel - PlanningViewModel, оновлюємо його
                if (CurrentViewModel is PlanningViewModel planningViewModel)
                {
                    System.Diagnostics.Debug.WriteLine("Оновлюємо PlanningViewModel");
                    System.Console.WriteLine("[MainViewModel] Оновлюємо PlanningViewModel");
                    await planningViewModel.LoadSchedulesAsync();
                }
                else
                {
                    System.Console.WriteLine($"[MainViewModel] CurrentViewModel: {CurrentViewModel?.GetType().Name ?? "null"}");
                }
                
            }
        }
    }

    // Панель управління
    public class DashboardViewModel : BaseViewModel
    {
        private readonly DataService _dataService;
        private int _totalAircraft;
        private int _activeAircraft;
        private int _maintenanceAircraft;
        private int _totalFlightHours;
        private ObservableCollection<Aircraft> _upcomingMaintenance = new();

        public int TotalAircraft
        {
            get => _totalAircraft;
            set => SetProperty(ref _totalAircraft, value);
        }

        public int ActiveAircraft
        {
            get => _activeAircraft;
            set => SetProperty(ref _activeAircraft, value);
        }

        public int MaintenanceAircraft
        {
            get => _maintenanceAircraft;
            set => SetProperty(ref _maintenanceAircraft, value);
        }

        public int TotalFlightHours
        {
            get => _totalFlightHours;
            set => SetProperty(ref _totalFlightHours, value);
        }

        public ObservableCollection<Aircraft> UpcomingMaintenance
        {
            get => _upcomingMaintenance;
            set => SetProperty(ref _upcomingMaintenance, value);
        }

        public ICommand RefreshDashboardCommand { get; }

        public DashboardViewModel(DataService dataService)
        {
            _dataService = dataService;
            RefreshDashboardCommand = new AsyncRelayCommand(LoadDashboardDataAsync);
            _ = LoadDashboardDataAsync();
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                var statistics = await _dataService.GetFleetStatisticsAsync();
                
                TotalAircraft = statistics.TotalAircraft;
                ActiveAircraft = statistics.ActiveAircraft;
                MaintenanceAircraft = statistics.MaintenanceAircraft;
                TotalFlightHours = statistics.TotalFlightHours;

                var aircraft = await _dataService.GetAllAircraftAsync();
                var upcoming = aircraft
                    .Where(a => a.NextMaintenanceDate <= DateTime.Now.AddDays(30))
                    .OrderBy(a => a.NextMaintenanceDate)
                    .Take(10);

                UpcomingMaintenance.Clear();
                foreach (var ac in upcoming)
                {
                    UpcomingMaintenance.Add(ac);
                }
            }
            catch (Exception ex)
            {
                // Логування помилки
                System.Diagnostics.Debug.WriteLine($"Помилка завантаження даних панелі: {ex.Message}");
            }
        }
    }

    // Управління флотом
    public class FleetViewModel : BaseViewModel
    {
        private readonly DataService _dataService;
        private ObservableCollection<Aircraft> _aircraft = new();
        private Aircraft? _selectedAircraft;
        private string _searchText = string.Empty;
        private string _selectedStatusFilter = "Всі";
        private bool _isLoading;

        public ObservableCollection<Aircraft> Aircraft
        {
            get => _aircraft;
            set => SetProperty(ref _aircraft, value);
        }

        public Aircraft? SelectedAircraft
        {
            get => _selectedAircraft;
            set => SetProperty(ref _selectedAircraft, value);
        }

        public string SearchText
        {
            get => _searchText;
            set 
            { 
                if (SetProperty(ref _searchText, value))
                    AircraftView.Refresh();
            }
        }

        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set 
            { 
                if (SetProperty(ref _selectedStatusFilter, value))
                    AircraftView.Refresh();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICollectionView AircraftView { get; }
        public ObservableCollection<string> StatusFilters { get; } = new()
        {
            "Всі", "Активні", "На обслуговуванні", "На землі", "Зарезервовані", "Списані"
        };
        
        public ObservableCollection<string> ManufacturerFilters { get; } = new()
        {
            "Всі", "Boeing", "Airbus", "Embraer", "Bombardier", "ATR", "Antonov"
        };
        
        private string _selectedManufacturerFilter = "Всі";
        
        public string SelectedManufacturerFilter
        {
            get => _selectedManufacturerFilter;
            set 
            { 
                if (SetProperty(ref _selectedManufacturerFilter, value))
                    AircraftView.Refresh();
            }
        }

        public ICommand LoadAircraftCommand { get; }
        public ICommand AddAircraftCommand { get; }
        public ICommand EditAircraftCommand { get; }
        public ICommand SaveAircraftCommand { get; }
        public ICommand DeleteAircraftCommand { get; }

        public FleetViewModel(DataService dataService)
        {
            _dataService = dataService;
            
            AircraftView = CollectionViewSource.GetDefaultView(Aircraft);
            AircraftView.Filter = FilterAircraft;

            LoadAircraftCommand = new AsyncRelayCommand(LoadAircraftAsync);
            AddAircraftCommand = new RelayCommand(AddAircraft);
            EditAircraftCommand = new RelayCommand(EditAircraft, () => SelectedAircraft != null);
            SaveAircraftCommand = new AsyncRelayCommand(SaveAircraftAsync);
            DeleteAircraftCommand = new AsyncRelayCommand(DeleteAircraftAsync);

            _ = LoadAircraftAsync();
        }

        private void AddAircraft()
        {
            var viewModel = new AircraftEditViewModel(_dataService);
            var window = new Views.AircraftEditWindow(viewModel);
            window.Owner = System.Windows.Application.Current.MainWindow;
            
            if (window.ShowDialog() == true && viewModel.DialogResult == true)
            {
                _ = LoadAircraftAsync();
            }
        }

        private void EditAircraft()
        {
            if (SelectedAircraft == null) return;

            var viewModel = new AircraftEditViewModel(_dataService, SelectedAircraft);
            var window = new Views.AircraftEditWindow(viewModel);
            window.Owner = System.Windows.Application.Current.MainWindow;
            
            if (window.ShowDialog() == true && viewModel.DialogResult == true)
            {
                _ = LoadAircraftAsync();
            }
        }

        private async Task SaveAircraftAsync()
        {
            if (SelectedAircraft == null) return;

            try
            {
                IsLoading = true;
                await _dataService.SaveAircraftAsync(SelectedAircraft);
                await LoadAircraftAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteAircraftAsync()
        {
            if (SelectedAircraft == null || SelectedAircraft.Id == 0) return;

            try
            {
                IsLoading = true;
                await _dataService.DeleteAircraftAsync(SelectedAircraft.Id);
                await LoadAircraftAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoadAircraftAsync()
        {
            try
            {
                IsLoading = true;
                var aircraftList = await _dataService.GetAllAircraftAsync();
                
                System.Diagnostics.Debug.WriteLine($"Завантажено літаків з DataService: {aircraftList.Count}");
                
                Aircraft.Clear();
                foreach (var aircraft in aircraftList)
                {
                    Aircraft.Add(aircraft);
                    System.Diagnostics.Debug.WriteLine($"Додано в UI літак: {aircraft.RegistrationNumber}");
                }
                
                System.Diagnostics.Debug.WriteLine($"Всього літаків в UI: {Aircraft.Count}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool FilterAircraft(object obj)
        {
            if (obj is not Aircraft aircraft) return false;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                if (!aircraft.RegistrationNumber.ToLower().Contains(searchLower) &&
                    !aircraft.Model.ToLower().Contains(searchLower) &&
                    !aircraft.Manufacturer.ToLower().Contains(searchLower))
                {
                    return false;
                }
            }

            if (SelectedStatusFilter != "Всі")
            {
                var statusMapping = new Dictionary<string, AircraftStatus>
                {
                    { "Активні", AircraftStatus.Active },
                    { "На обслуговуванні", AircraftStatus.Maintenance },
                    { "На землі", AircraftStatus.Grounded },
                    { "Зарезервовані", AircraftStatus.Reserved },
                    { "Списані", AircraftStatus.Retired }
                };

                if (statusMapping.ContainsKey(SelectedStatusFilter) && 
                    aircraft.Status != statusMapping[SelectedStatusFilter])
                {
                    return false;
                }
            }

            if (SelectedManufacturerFilter != "Всі")
            {
                if (!aircraft.Manufacturer.Equals(SelectedManufacturerFilter, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }
    }

    // Аналітика
    public class AnalyticsViewModel : BaseViewModel
    {
        private readonly DataService _dataService;
        private double _utilizationRate = 75.5;
        private int _totalFlights = 1240;
        private double _averageFuelConsumption = 2450;
        private bool _isLoading;
        private ObservableCollection<ChartData> _statusData = new();
        private ObservableCollection<ChartData> _manufacturerData = new();

        public double UtilizationRate
        {
            get => _utilizationRate;
            set => SetProperty(ref _utilizationRate, value);
        }

        public int TotalFlights
        {
            get => _totalFlights;
            set => SetProperty(ref _totalFlights, value);
        }

        public double AverageFuelConsumption
        {
            get => _averageFuelConsumption;
            set => SetProperty(ref _averageFuelConsumption, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ObservableCollection<ChartData> StatusData
        {
            get => _statusData;
            set => SetProperty(ref _statusData, value);
        }

        public ObservableCollection<ChartData> ManufacturerData
        {
            get => _manufacturerData;
            set => SetProperty(ref _manufacturerData, value);
        }

        public ICommand RefreshAnalyticsCommand { get; }

        public AnalyticsViewModel(DataService dataService)
        {
            _dataService = dataService;
            RefreshAnalyticsCommand = new AsyncRelayCommand(LoadAnalyticsDataAsync);
            _ = LoadAnalyticsDataAsync();
        }

        private async Task LoadAnalyticsDataAsync()
        {
            try
            {
                IsLoading = true;

                var statusStats = await _dataService.GetAircraftByStatusAsync();
                StatusData.Clear();
                foreach (var kvp in statusStats)
                {
                    StatusData.Add(new ChartData { Label = kvp.Key, Value = kvp.Value });
                }

                var manufacturerStats = await _dataService.GetAircraftByManufacturerAsync();
                ManufacturerData.Clear();
                foreach (var kvp in manufacturerStats)
                {
                    ManufacturerData.Add(new ChartData { Label = kvp.Key, Value = kvp.Value });
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    // Планування рейсів
    public class PlanningViewModel : BaseViewModel
    {
        private readonly DataService _dataService;
        private ObservableCollection<FlightSchedule> _flightSchedules = new();
        private FlightSchedule? _selectedFlight;
        private DateTime _selectedDate = DateTime.Today;
        private bool _isLoading;

        public ObservableCollection<FlightSchedule> FlightSchedules
        {
            get => _flightSchedules;
            set => SetProperty(ref _flightSchedules, value);
        }

        public FlightSchedule? SelectedFlight
        {
            get => _selectedFlight;
            set => SetProperty(ref _selectedFlight, value);
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set 
            { 
                if (SetProperty(ref _selectedDate, value))
                    _ = LoadSchedulesAsync();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand LoadSchedulesCommand { get; }
        public ICommand AddFlightCommand { get; }
        public ICommand EditFlightCommand { get; }
        public ICommand DeleteFlightCommand { get; }
        public ICommand SaveFlightCommand { get; }

        public PlanningViewModel(DataService dataService)
        {
            _dataService = dataService;
            LoadSchedulesCommand = new AsyncRelayCommand(LoadSchedulesAsync);
            AddFlightCommand = new RelayCommand(AddFlight);
            EditFlightCommand = new RelayCommand(EditFlight, () => SelectedFlight != null);
            DeleteFlightCommand = new AsyncRelayCommand(DeleteFlightAsync, () => SelectedFlight != null);
            SaveFlightCommand = new AsyncRelayCommand(SaveFlightAsync);
            _ = LoadSchedulesAsync();
        }

        private void AddFlight()
        {
            var viewModel = new FlightEditViewModel(_dataService);
            var window = new Views.FlightEditWindow(viewModel);
            window.Owner = System.Windows.Application.Current.MainWindow;
            
            if (window.ShowDialog() == true && viewModel.DialogResult == true)
            {
                _ = LoadSchedulesAsync();
            }
        }

        private void EditFlight()
        {
            if (SelectedFlight == null) return;

            var viewModel = new FlightEditViewModel(_dataService, SelectedFlight);
            var window = new Views.FlightEditWindow(viewModel);
            window.Owner = System.Windows.Application.Current.MainWindow;
            
            if (window.ShowDialog() == true && viewModel.DialogResult == true)
            {
                _ = LoadSchedulesAsync();
            }
        }

        private async Task DeleteFlightAsync()
        {
            if (SelectedFlight == null) return;

            var result = System.Windows.MessageBox.Show(
                $"Ви впевнені, що хочете видалити рейс {SelectedFlight.FlightNumber}?",
                "Підтвердження видалення",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    await _dataService.DeleteFlightScheduleAsync(SelectedFlight);
                    _ = LoadSchedulesAsync();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Помилка видалення: {ex.Message}", "Помилка", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void OptimizeSchedule()
        {
            System.Windows.MessageBox.Show("Функція оптимізації розкладу буде реалізована в наступних версіях.", 
                "Інформація", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private async Task SaveFlightAsync()
        {
            if (SelectedFlight == null) return;

            try
            {
                IsLoading = true;
                await _dataService.SaveFlightScheduleAsync(SelectedFlight);
                await LoadSchedulesAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoadSchedulesAsync()
        {
            try
            {
                IsLoading = true;
                // Завантажуємо всі рейси, щоб новостворені рейси були видимі
                var allSchedules = await _dataService.GetAllFlightSchedulesAsync();
                var schedules = allSchedules.Where(fs => fs.FlightDate.Date == SelectedDate.Date).ToList();

                FlightSchedules.Clear();
                foreach (var schedule in schedules)
                {
                    FlightSchedules.Add(schedule);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    // Технічне обслуговування
    public class MaintenanceViewModel : BaseViewModel
    {
        private readonly DataService _dataService;
        private ObservableCollection<MaintenanceRecord> _maintenanceRecords = new();
        private MaintenanceRecord? _selectedRecord;
        private bool _isLoading;
        private decimal _totalMaintenanceCost;

        public ObservableCollection<MaintenanceRecord> MaintenanceRecords
        {
            get => _maintenanceRecords;
            set => SetProperty(ref _maintenanceRecords, value);
        }

        public MaintenanceRecord? SelectedRecord
        {
            get => _selectedRecord;
            set => SetProperty(ref _selectedRecord, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public decimal TotalMaintenanceCost
        {
            get => _totalMaintenanceCost;
            set => SetProperty(ref _totalMaintenanceCost, value);
        }

        public ObservableCollection<string> MaintenanceStatusOptions { get; } = new()
        {
            "Всі", "Заплановано", "В процесі", "Завершено", "Скасовано", "Прострочено"
        };

        public ICommand RefreshDataCommand { get; }
        public ICommand AddMaintenanceRecordCommand { get; }
        public ICommand SaveMaintenanceRecordCommand { get; }

        public MaintenanceViewModel(DataService dataService)
        {
            _dataService = dataService;
            RefreshDataCommand = new AsyncRelayCommand(LoadMaintenanceDataAsync);
            AddMaintenanceRecordCommand = new RelayCommand(AddMaintenanceRecord);
            SaveMaintenanceRecordCommand = new AsyncRelayCommand(SaveMaintenanceRecordAsync);
            _ = LoadMaintenanceDataAsync();
        }

        private void AddMaintenanceRecord()
        {
            var newRecord = new MaintenanceRecord
            {
                Type = MaintenanceType.Routine,
                Description = "Планове технічне обслуговування",
                ScheduledDate = DateTime.Now.AddDays(7),
                Status = MaintenanceStatus.Scheduled,
                Location = "Головна база",
                CreatedDate = DateTime.Now
            };

            SelectedRecord = newRecord;
        }

        private async Task SaveMaintenanceRecordAsync()
        {
            if (SelectedRecord == null) return;

            try
            {
                IsLoading = true;
                await _dataService.SaveMaintenanceRecordAsync(SelectedRecord);
                await LoadMaintenanceDataAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadMaintenanceDataAsync()
        {
            try
            {
                IsLoading = true;
                var records = await _dataService.GetAllMaintenanceRecordsAsync();
                
                MaintenanceRecords.Clear();
                foreach (var record in records)
                {
                    MaintenanceRecords.Add(record);
                }

                var statistics = await _dataService.GetFleetStatisticsAsync();
                TotalMaintenanceCost = statistics.TotalMaintenanceCost;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    // ViewModel для редагування літака
    public class AircraftEditViewModel : BaseViewModel
    {
        private readonly DataService _dataService;
        private Aircraft _aircraft;
        private string _windowTitle;

        public Aircraft Aircraft
        {
            get => _aircraft;
            set => SetProperty(ref _aircraft, value);
        }

        public string WindowTitle
        {
            get => _windowTitle;
            set => SetProperty(ref _windowTitle, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private bool? _dialogResult;
        public bool? DialogResult 
        { 
            get => _dialogResult;
            private set => SetProperty(ref _dialogResult, value);
        }

        public AircraftEditViewModel(DataService dataService, Aircraft? aircraft = null)
        {
            _dataService = dataService;
            _aircraft = aircraft ?? new Aircraft
            {
                RegistrationNumber = "",
                Type = "Пасажирський",
                Manufacturer = "",
                Model = "",
                YearOfManufacture = DateTime.Now.Year,
                Status = AircraftStatus.Active,
                PassengerCapacity = 0,
                Range = 0,
                FlightHours = 0,
                FlightCycles = 0,
                BaseLocation = "Київ (Бориспіль)",
                CurrentLocation = "Київ (Бориспіль)",
                AcquisitionDate = DateTime.Today,
                NextMaintenanceDate = DateTime.Today.AddDays(90)
            };

            _windowTitle = aircraft == null ? "Додати новий літак" : "Редагувати літак";

            SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
            CancelCommand = new RelayCommand(Cancel);
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Aircraft?.RegistrationNumber) &&
                   !string.IsNullOrWhiteSpace(Aircraft?.Manufacturer) &&
                   !string.IsNullOrWhiteSpace(Aircraft?.Model) &&
                   Aircraft?.YearOfManufacture > 1900;
        }

        private async Task SaveAsync()
        {
            try
            {
                // Валідація та встановлення значень по замовчуванню
                if (Aircraft != null)
                {
                    if (string.IsNullOrWhiteSpace(Aircraft.RegistrationNumber))
                    {
                        System.Windows.MessageBox.Show("Реєстраційний номер не може бути пустим", "Помилка валідації");
                        return;
                    }
                    
                    // Встановлення значень по замовчуванню якщо вони порожні
                    if (string.IsNullOrWhiteSpace(Aircraft.CurrentLocation))
                        Aircraft.CurrentLocation = Aircraft.BaseLocation;
                    
                    if (Aircraft.LastMaintenanceDate == default)
                        Aircraft.LastMaintenanceDate = DateTime.Now.AddDays(-30);
                    
                    if (Aircraft.NextMaintenanceDate == default)
                        Aircraft.NextMaintenanceDate = DateTime.Now.AddDays(90);
                }

                await _dataService.SaveAircraftAsync(Aircraft!);
                System.Windows.MessageBox.Show("Літак успішно збережено!", "Успіх", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Помилка збереження: {ex.Message}", "Помилка", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            DialogResult = false;
        }
    }

    // ViewModel для редагування рейсу
    public class FlightEditViewModel : BaseViewModel
    {
        private readonly DataService _dataService;
        private FlightSchedule _flight;
        private string _windowTitle;
        private string _departureTime;
        private string _arrivalTime;
        private ObservableCollection<Aircraft> _availableAircraft = new();

        public FlightSchedule Flight
        {
            get => _flight;
            set 
            { 
                if (SetProperty(ref _flight, value))
                {
                    // Повідомляємо про зміни для оновлення команди
                    ((AsyncRelayCommand)SaveCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string WindowTitle
        {
            get => _windowTitle;
            set => SetProperty(ref _windowTitle, value);
        }

        public string DepartureTime
        {
            get => _departureTime;
            set 
            { 
                if (SetProperty(ref _departureTime, value))
                {
                    var dateStr = $"{Flight.FlightDate:yyyy-MM-dd} {value}";
                    System.Diagnostics.Debug.WriteLine($"Парсинг часу відправлення: '{dateStr}'");
                    
                    if (DateTime.TryParse(dateStr, out var dt))
                    {
                        Flight.DepartureTime = dt;
                        System.Diagnostics.Debug.WriteLine($"Час відправлення встановлено: {dt}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Не вдалося розпарсити час відправлення: '{dateStr}'");
                    }
                }
            }
        }

        public string ArrivalTime
        {
            get => _arrivalTime;
            set 
            { 
                if (SetProperty(ref _arrivalTime, value))
                {
                    var dateStr = $"{Flight.FlightDate:yyyy-MM-dd} {value}";
                    System.Diagnostics.Debug.WriteLine($"Парсинг часу прибуття: '{dateStr}'");
                    
                    if (DateTime.TryParse(dateStr, out var dt))
                    {
                        Flight.ArrivalTime = dt;
                        System.Diagnostics.Debug.WriteLine($"Час прибуття встановлено: {dt}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Не вдалося розпарсити час прибуття: '{dateStr}'");
                    }
                }
            }
        }

        public ObservableCollection<Aircraft> AvailableAircraft
        {
            get => _availableAircraft;
            set => SetProperty(ref _availableAircraft, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private bool? _dialogResult;
        public bool? DialogResult 
        { 
            get => _dialogResult;
            private set => SetProperty(ref _dialogResult, value);
        }

        public FlightEditViewModel(DataService dataService, FlightSchedule? flight = null)
        {
            _dataService = dataService;
            _flight = flight ?? new FlightSchedule
            {
                FlightNumber = "",
                Origin = "Київ (Бориспіль)",
                Destination = "",
                FlightDate = DateTime.Today,
                DepartureTime = DateTime.Today.AddHours(9),
                ArrivalTime = DateTime.Today.AddHours(12),
                Status = FlightStatus.Scheduled,
                PassengerCount = 0,
                CargoWeight = 0,
                FuelConsumption = 0,
                Notes = "",
                CreatedDate = DateTime.Now
            };

            _windowTitle = flight == null ? "Додати новий рейс" : "Редагувати рейс";
            _departureTime = _flight.DepartureTime.ToString("HH:mm");
            _arrivalTime = _flight.ArrivalTime.ToString("HH:mm");

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelCommand = new RelayCommand(Cancel);

            _ = LoadAircraftAsync();
        }

        private async Task LoadAircraftAsync()
        {
            try
            {
                var aircraft = await _dataService.GetAllAircraftAsync();
                AvailableAircraft.Clear();
                foreach (var plane in aircraft.Where(a => a.Status == AircraftStatus.Active))
                {
                    AvailableAircraft.Add(plane);
                }
            }
            catch
            {
                // Ignore errors during loading
            }
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Flight?.FlightNumber) &&
                   !string.IsNullOrWhiteSpace(Flight?.Origin) &&
                   !string.IsNullOrWhiteSpace(Flight?.Destination);
        }

        private async Task SaveAsync()
        {
            try
            {
                // Валідація перед збереженням
                if (Flight != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Збереження рейсу: {Flight.FlightNumber}");
                    System.Diagnostics.Debug.WriteLine($"Aircraft: {Flight.Aircraft?.RegistrationNumber ?? "null"}");
                    System.Diagnostics.Debug.WriteLine($"Origin: {Flight.Origin}");
                    System.Diagnostics.Debug.WriteLine($"Destination: {Flight.Destination}");
                    System.Diagnostics.Debug.WriteLine($"FlightDate: {Flight.FlightDate}");
                    System.Diagnostics.Debug.WriteLine($"DepartureTime: {Flight.DepartureTime}");
                    System.Diagnostics.Debug.WriteLine($"ArrivalTime: {Flight.ArrivalTime}");
                    
                    if (string.IsNullOrWhiteSpace(Flight.FlightNumber))
                    {
                        System.Windows.MessageBox.Show("Номер рейсу не може бути пустим", "Помилка валідації");
                        return;
                    }
                    
                    if (string.IsNullOrWhiteSpace(Flight.Origin))
                    {
                        System.Windows.MessageBox.Show("Пункт відправлення не може бути пустим", "Помилка валідації");
                        return;
                    }
                    
                    if (string.IsNullOrWhiteSpace(Flight.Destination))
                    {
                        System.Windows.MessageBox.Show("Пункт призначення не може бути пустим", "Помилка валідації");
                        return;
                    }
                    
                    if (Flight.Aircraft == null)
                    {
                        System.Windows.MessageBox.Show("Будь ласка, оберіть літак для рейсу", "Помилка валідації");
                        return;
                    }
                    
                    // Встановлення значень по замовчуванню якщо вони порожні
                    if (Flight.CreatedDate == default)
                        Flight.CreatedDate = DateTime.Now;
                }

                await _dataService.SaveFlightScheduleAsync(Flight!);
                System.Windows.MessageBox.Show("Рейс успішно збережено!", "Успіх", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Помилка збереження: {ex.Message}", "Помилка", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            DialogResult = false;
        }
    }

    // Допоміжний клас для діаграм
    public class ChartData
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
    }
}