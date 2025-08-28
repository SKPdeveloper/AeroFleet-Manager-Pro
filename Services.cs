using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AeroFleetManagerPro.Models;
using AeroFleetManagerPro.ViewModels;

namespace AeroFleetManagerPro.Services
{
    // Простий сервіс навігації
    public class NavigationService : INotifyPropertyChanged
    {
        private BaseViewModel? _currentViewModel;

        public BaseViewModel? CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                _currentViewModel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentViewModel)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void NavigateTo(BaseViewModel viewModel)
        {
            CurrentViewModel = viewModel;
        }
    }

    // Простий контейнер для DI без зависимостей
    public class SimpleServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new();
        private readonly Dictionary<Type, Func<object>> _factories = new();

        public void RegisterSingleton<T>(T instance) where T : class
        {
            _services[typeof(T)] = instance;
        }

        public void RegisterTransient<T>(Func<T> factory) where T : class
        {
            _factories[typeof(T)] = () => factory();
        }

        public T GetService<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }

            if (_factories.TryGetValue(typeof(T), out var factory))
            {
                return (T)factory();
            }

            throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered");
        }
    }

    // Простий сервіс для роботи з даними з файловою персистентністю
    public class DataService
    {
        private readonly List<Aircraft> _aircraft = new();
        private readonly List<MaintenanceRecord> _maintenanceRecords = new();
        private readonly List<FlightSchedule> _flightSchedules = new();
        
        private readonly string _aircraftFilePath = "aircraft_data.json";
        private readonly string _flightSchedulesFilePath = "flight_schedules_data.json";

        public DataService()
        {
            LoadDataFromFiles();
        }

        private void LoadDataFromFiles()
        {
            try
            {
                // Завантажуємо літаки з файлу
                if (File.Exists(_aircraftFilePath))
                {
                    var aircraftJson = File.ReadAllText(_aircraftFilePath);
                    var aircraftData = JsonSerializer.Deserialize<List<Aircraft>>(aircraftJson);
                    if (aircraftData != null)
                    {
                        _aircraft.AddRange(aircraftData);
                        Console.WriteLine($"[DataService] Завантажено {_aircraft.Count} літаків з файлу");
                    }
                }
                else
                {
                    InitializeSampleAircraftData();
                    SaveAircraftToFile();
                }

                // Завантажуємо розклад рейсів з файлу
                if (File.Exists(_flightSchedulesFilePath))
                {
                    var flightSchedulesJson = File.ReadAllText(_flightSchedulesFilePath);
                    var flightScheduleData = JsonSerializer.Deserialize<List<FlightSchedule>>(flightSchedulesJson);
                    if (flightScheduleData != null)
                    {
                        _flightSchedules.AddRange(flightScheduleData);
                        Console.WriteLine($"[DataService] Завантажено {_flightSchedules.Count} рейсів з файлу");
                    }
                }
                else
                {
                    InitializeSampleFlightData();
                    SaveFlightSchedulesToFile();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DataService] Помилка завантаження даних: {ex.Message}");
                Console.WriteLine("[DataService] Ініціалізація стандартними даними");
                InitializeSampleAircraftData();
                InitializeSampleFlightData();
            }
        }

        private void SaveAircraftToFile()
        {
            try
            {
                var json = JsonSerializer.Serialize(_aircraft, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_aircraftFilePath, json);
                Console.WriteLine($"[DataService] Збережено {_aircraft.Count} літаків у файл");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DataService] Помилка збереження літаків: {ex.Message}");
            }
        }

        private void SaveFlightSchedulesToFile()
        {
            try
            {
                var json = JsonSerializer.Serialize(_flightSchedules, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_flightSchedulesFilePath, json);
                Console.WriteLine($"[DataService] Збережено {_flightSchedules.Count} рейсів у файл");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DataService] Помилка збереження рейсів: {ex.Message}");
            }
        }

        private void InitializeSampleAircraftData()
        {
            // Розширені тестові дані - 22 літаки (пасажирські, вантажні та бізнес-авіація)
            _aircraft.AddRange(new[]
            {
                // Пасажирські літаки
                new Aircraft
                {
                    Id = 1, RegistrationNumber = "UR-PSA", Manufacturer = "Boeing", Model = "737-800",
                    Type = "Пасажирський", YearOfManufacture = 2015, PassengerCapacity = 189, FlightHours = 12500, FlightCycles = 8750, Range = 5765,
                    Status = AircraftStatus.Active, AcquisitionDate = new DateTime(2015, 3, 15),
                    LastMaintenanceDate = DateTime.Now.AddDays(-30), NextMaintenanceDate = DateTime.Now.AddDays(60),
                    BaseLocation = "Київ (Бориспіль)", CurrentLocation = "Київ (Бориспіль)"
                },
                new Aircraft
                {
                    Id = 2, RegistrationNumber = "UR-PSB", Manufacturer = "Airbus", Model = "A320",
                    Type = "Пасажирський", YearOfManufacture = 2018, PassengerCapacity = 180, FlightHours = 8750, FlightCycles = 5600, Range = 6150,
                    Status = AircraftStatus.Maintenance, AcquisitionDate = new DateTime(2018, 7, 20),
                    LastMaintenanceDate = DateTime.Now.AddDays(-10), NextMaintenanceDate = DateTime.Now.AddDays(80),
                    BaseLocation = "Львів", CurrentLocation = "Львів"
                },
                new Aircraft
                {
                    Id = 3, RegistrationNumber = "UR-PSC", Manufacturer = "Embraer", Model = "E190",
                    Type = "Пасажирський", YearOfManufacture = 2020, PassengerCapacity = 100, FlightHours = 3200, FlightCycles = 2800, Range = 4450,
                    Status = AircraftStatus.Active, AcquisitionDate = new DateTime(2020, 12, 5),
                    LastMaintenanceDate = DateTime.Now.AddDays(-20), NextMaintenanceDate = DateTime.Now.AddDays(70),
                    BaseLocation = "Одеса", CurrentLocation = "Дніпро"
                },
                new Aircraft
                {
                    Id = 4, RegistrationNumber = "UR-AUA", Manufacturer = "Airbus", Model = "A330-300",
                    Type = "Пасажирський", YearOfManufacture = 2016, PassengerCapacity = 277, FlightHours = 15600, FlightCycles = 4800, Range = 11750,
                    Status = AircraftStatus.Active, AcquisitionDate = new DateTime(2016, 9, 12),
                    LastMaintenanceDate = DateTime.Now.AddDays(-45), NextMaintenanceDate = DateTime.Now.AddDays(45),
                    BaseLocation = "Київ (Бориспіль)", CurrentLocation = "Лондон"
                },
                new Aircraft
                {
                    Id = 5, RegistrationNumber = "UR-EMA", Manufacturer = "Embraer", Model = "E175",
                    Type = "Пасажирський", YearOfManufacture = 2019, PassengerCapacity = 88, FlightHours = 4500, FlightCycles = 3200, Range = 3900,
                    Status = AircraftStatus.Active, AcquisitionDate = new DateTime(2019, 4, 8),
                    LastMaintenanceDate = DateTime.Now.AddDays(-15), NextMaintenanceDate = DateTime.Now.AddDays(75),
                    BaseLocation = "Харків", CurrentLocation = "Варшава"
                },
                new Aircraft
                {
                    Id = 6, RegistrationNumber = "UR-BOA", Manufacturer = "Boeing", Model = "777-200ER",
                    Type = "Пасажирський", YearOfManufacture = 2014, PassengerCapacity = 314, FlightHours = 18200, FlightCycles = 6100, Range = 14260,
                    Status = AircraftStatus.Active, AcquisitionDate = new DateTime(2014, 11, 22),
                    LastMaintenanceDate = DateTime.Now.AddDays(-60), NextMaintenanceDate = DateTime.Now.AddDays(30),
                    BaseLocation = "Київ (Бориспіль)", CurrentLocation = "Нью-Йорк"
                },
                new Aircraft
                {
                    Id = 7, RegistrationNumber = "UR-AIB", Manufacturer = "Airbus", Model = "A321",
                    Type = "Пасажирський", YearOfManufacture = 2017, PassengerCapacity = 220, FlightHours = 11400, FlightCycles = 7600, Range = 7400,
                    Status = AircraftStatus.Active, AcquisitionDate = new DateTime(2017, 6, 14),
                    LastMaintenanceDate = DateTime.Now.AddDays(-25), NextMaintenanceDate = DateTime.Now.AddDays(65),
                    BaseLocation = "Одеса", CurrentLocation = "Стамбул"
                },
                new Aircraft
                {
                    Id = 8, RegistrationNumber = "UR-BOM", Manufacturer = "Bombardier", Model = "CRJ900",
                    Type = "Пасажирський", YearOfManufacture = 2021, PassengerCapacity = 90, FlightHours = 2100, FlightCycles = 1800, Range = 2956,
                    Status = AircraftStatus.Active, AcquisitionDate = new DateTime(2021, 2, 18),
                    LastMaintenanceDate = DateTime.Now.AddDays(-10), NextMaintenanceDate = DateTime.Now.AddDays(80),
                    BaseLocation = "Дніпро", CurrentLocation = "Будапешт"
                },
                new Aircraft
                {
                    Id = 9, RegistrationNumber = "UR-ATR", Manufacturer = "ATR", Model = "72-600",
                    Type = "Пасажирський", YearOfManufacture = 2022, PassengerCapacity = 78, FlightHours = 1200, FlightCycles = 950, Range = 1528,
                    Status = AircraftStatus.Active, AcquisitionDate = new DateTime(2022, 8, 5),
                    LastMaintenanceDate = DateTime.Now.AddDays(-5), NextMaintenanceDate = DateTime.Now.AddDays(85),
                    BaseLocation = "Львів", CurrentLocation = "Краків"
                },
                new Aircraft
                {
                    Id = 10, RegistrationNumber = "UR-BOB", Manufacturer = "Boeing", Model = "737-900",
                    Type = "Пасажирський", YearOfManufacture = 2016, PassengerCapacity = 215, FlightHours = 9800, FlightCycles = 6200, Range = 5665,
                    Status = AircraftStatus.Reserved, AcquisitionDate = new DateTime(2016, 1, 20),
                    LastMaintenanceDate = DateTime.Now.AddDays(-40), NextMaintenanceDate = DateTime.Now.AddDays(50),
                    BaseLocation = "Київ (Жуляни)", CurrentLocation = "Київ (Жуляни)"
                },
                new Aircraft
                {
                    Id = 11, RegistrationNumber = "UR-AIC", Manufacturer = "Airbus", Model = "A319",
                    Type = "Пасажирський", YearOfManufacture = 2013, PassengerCapacity = 156, FlightHours = 16800, FlightCycles = 11200, Range = 6850,
                    Status = AircraftStatus.Grounded, AcquisitionDate = new DateTime(2013, 5, 10),
                    LastMaintenanceDate = DateTime.Now.AddDays(-90), NextMaintenanceDate = DateTime.Now.AddDays(-10),
                    BaseLocation = "Харків", CurrentLocation = "Харків"
                },
                new Aircraft
                {
                    Id = 12, RegistrationNumber = "UR-EMB", Manufacturer = "Embraer", Model = "E195",
                    Type = "Пасажирський", YearOfManufacture = 2018, PassengerCapacity = 124, FlightHours = 6700, FlightCycles = 4500, Range = 4815,
                    Status = AircraftStatus.Active, AcquisitionDate = new DateTime(2018, 10, 3),
                    LastMaintenanceDate = DateTime.Now.AddDays(-18), NextMaintenanceDate = DateTime.Now.AddDays(72),
                    BaseLocation = "Одеса", CurrentLocation = "Афіни"
                },
                
                // Вантажні літаки
                new Aircraft
                {
                    Id = 13, RegistrationNumber = "UR-CGA", Manufacturer = "Boeing", Model = "747-8F",
                    Type = "Вантажний", YearOfManufacture = 2019, PassengerCapacity = 0, FlightHours = 5600, FlightCycles = 1800, Range = 8130,
                    Status = AircraftStatus.Active, AcquisitionDate = new DateTime(2019, 11, 15),
                    LastMaintenanceDate = DateTime.Now.AddDays(-35), NextMaintenanceDate = DateTime.Now.AddDays(55),
                    BaseLocation = "Київ (Бориспіль)", CurrentLocation = "Франкфурт"
                },
                new Aircraft
                {
                    Id = 14, RegistrationNumber = "UR-CGB", Manufacturer = "Airbus", Model = "A330-200F",
                    Type = "Вантажний", YearOfManufacture = 2017, PassengerCapacity = 0, FlightHours = 8900, FlightCycles = 3200, Range = 7400,
                    Status = AircraftStatus.Active, AcquisitionDate = new DateTime(2017, 3, 28),
                    LastMaintenanceDate = DateTime.Now.AddDays(-50), NextMaintenanceDate = DateTime.Now.AddDays(40),
                    BaseLocation = "Дніпro", CurrentLocation = "Доха"
                },
                new Aircraft
                {
                    Id = 15, RegistrationNumber = "UR-CGC", Manufacturer = "Boeing", Model = "777F",
                    Type = "Вантажний", YearOfManufacture = 2020, PassengerCapacity = 0, FlightHours = 3800, FlightCycles = 1200, Range = 9070,
                    Status = AircraftStatus.Maintenance, AcquisitionDate = new DateTime(2020, 7, 12),
                    LastMaintenanceDate = DateTime.Now.AddDays(-5), NextMaintenanceDate = DateTime.Now.AddDays(85),
                    BaseLocation = "Київ (Бориспіль)", CurrentLocation = "Київ (Бориспіль)"
                },
                new Aircraft
                {
                    Id = 16, RegistrationNumber = "UR-CGD", Manufacturer = "Boeing", Model = "767-300F",
                    Type = "Вантажний", YearOfManufacture = 2015, PassengerCapacity = 0, FlightHours = 12100, FlightCycles = 4800, Range = 6025,
                    Status = AircraftStatus.Active, AcquisitionDate = new DateTime(2015, 9, 8),
                    LastMaintenanceDate = DateTime.Now.AddDays(-28), NextMaintenanceDate = DateTime.Now.AddDays(62),
                    BaseLocation = "Львів", CurrentLocation = "Мілан"
                },
                new Aircraft
                {
                    Id = 17, RegistrationNumber = "UR-ANT", Manufacturer = "Antonov", Model = "An-124-100",
                    Type = "Вантажний", YearOfManufacture = 2014, PassengerCapacity = 88, FlightHours = 8500, FlightCycles = 2200, Range = 4800,
                    Status = AircraftStatus.Active, AcquisitionDate = new DateTime(2014, 4, 25),
                    LastMaintenanceDate = DateTime.Now.AddDays(-42), NextMaintenanceDate = DateTime.Now.AddDays(48),
                    BaseLocation = "Київ (Антонов)", CurrentLocation = "Лейпциг"
                },
                new Aircraft
                {
                    Id = 18, RegistrationNumber = "UR-CGE", Manufacturer = "Boeing", Model = "737-800BCF",
                    Type = "Вантажний", YearOfManufacture = 2012, PassengerCapacity = 0, FlightHours = 18600, FlightCycles = 12400, Range = 5765,
                    Status = AircraftStatus.Active, AcquisitionDate = new DateTime(2012, 6, 18),
                    LastMaintenanceDate = DateTime.Now.AddDays(-55), NextMaintenanceDate = DateTime.Now.AddDays(35),
                    BaseLocation = "Одеса", CurrentLocation = "Астана"
                },
                new Aircraft
                {
                    Id = 19, RegistrationNumber = "UR-CGF", Manufacturer = "Airbus", Model = "A310F",
                    Type = "Вантажний", YearOfManufacture = 2008, PassengerCapacity = 0, FlightHours = 22400, FlightCycles = 8900, Range = 8050,
                    Status = AircraftStatus.Retired, AcquisitionDate = new DateTime(2008, 2, 14),
                    LastMaintenanceDate = DateTime.Now.AddDays(-120), NextMaintenanceDate = DateTime.Now.AddDays(-30),
                    BaseLocation = "Харків", CurrentLocation = "Харків"
                },
                new Aircraft
                {
                    Id = 20, RegistrationNumber = "UR-CGG", Manufacturer = "Boeing", Model = "757-200F",
                    Type = "Вантажний", YearOfManufacture = 2010, PassengerCapacity = 0, FlightHours = 19800, FlightCycles = 7200, Range = 7222,
                    Status = AircraftStatus.Active, AcquisitionDate = new DateTime(2010, 12, 1),
                    LastMaintenanceDate = DateTime.Now.AddDays(-38), NextMaintenanceDate = DateTime.Now.AddDays(52),
                    BaseLocation = "Дніпро", CurrentLocation = "Баку"
                },
                
                // Бізнес авіація 
                new Aircraft
                {
                    Id = 21, RegistrationNumber = "UR-BIZ", Manufacturer = "Bombardier", Model = "Global 6000",
                    Type = "Бізнес-авіація", YearOfManufacture = 2019, PassengerCapacity = 17, FlightHours = 2800, FlightCycles = 1400, Range = 11100,
                    Status = AircraftStatus.Active, AcquisitionDate = new DateTime(2019, 8, 22),
                    LastMaintenanceDate = DateTime.Now.AddDays(-12), NextMaintenanceDate = DateTime.Now.AddDays(78),
                    BaseLocation = "Київ (Жуляни)", CurrentLocation = "Женева"
                },
                new Aircraft
                {
                    Id = 22, RegistrationNumber = "UR-JET", Manufacturer = "Embraer", Model = "Legacy 650",
                    Type = "Бізнес-авіація", YearOfManufacture = 2021, PassengerCapacity = 14, FlightHours = 1100, FlightCycles = 890, Range = 7223,
                    Status = AircraftStatus.Active, AcquisitionDate = new DateTime(2021, 5, 30),
                    LastMaintenanceDate = DateTime.Now.AddDays(-8), NextMaintenanceDate = DateTime.Now.AddDays(82),
                    BaseLocation = "Київ (Бориспіль)", CurrentLocation = "Дубай"
                }
            });
        }

        private void InitializeSampleFlightData()
        {
            // Розширені дані обслуговування
            _maintenanceRecords.AddRange(new[]
            {
                new MaintenanceRecord
                {
                    Id = 1, AircraftId = 1, Aircraft = _aircraft[0], Type = MaintenanceType.Routine,
                    Description = "Планове технічне обслуговування A-check", ScheduledDate = DateTime.Now.AddDays(45),
                    Status = MaintenanceStatus.Scheduled, Location = "Київ (Бориспіль)", DurationHours = 8, Cost = 15000,
                    CreatedDate = DateTime.Now.AddDays(-30)
                },
                new MaintenanceRecord
                {
                    Id = 2, AircraftId = 2, Aircraft = _aircraft[1], Type = MaintenanceType.Emergency,
                    Description = "Позапланова заміна двигуна після виявлення несправності", ScheduledDate = DateTime.Now.AddDays(-5),
                    ActualDate = DateTime.Now.AddDays(-2), Status = MaintenanceStatus.InProgress, Location = "Львів",
                    DurationHours = 24, Cost = 125000, PerformedBy = "Львівське АТБ", CreatedDate = DateTime.Now.AddDays(-15)
                },
                new MaintenanceRecord
                {
                    Id = 3, AircraftId = 11, Aircraft = _aircraft[10], Type = MaintenanceType.Heavy,
                    Description = "Капітальний ремонт C-check з заміною компонентів", ScheduledDate = DateTime.Now.AddDays(-10),
                    Status = MaintenanceStatus.Overdue, Location = "Харків", DurationHours = 120, Cost = 350000,
                    CreatedDate = DateTime.Now.AddDays(-45)
                },
                new MaintenanceRecord
                {
                    Id = 4, AircraftId = 6, Aircraft = _aircraft[5], Type = MaintenanceType.Routine,
                    Description = "Планове обслуговування B-check", ScheduledDate = DateTime.Now.AddDays(30),
                    Status = MaintenanceStatus.Scheduled, Location = "Київ (Бориспіль)", DurationHours = 16, Cost = 28000,
                    CreatedDate = DateTime.Now.AddDays(-20)
                },
                new MaintenanceRecord
                {
                    Id = 5, AircraftId = 15, Aircraft = _aircraft[14], Type = MaintenanceType.Routine,
                    Description = "Технічне обслуговування вантажного відсіку", ScheduledDate = DateTime.Now.AddDays(-3),
                    ActualDate = DateTime.Now, Status = MaintenanceStatus.InProgress, Location = "Київ (Бориспіль)",
                    DurationHours = 12, Cost = 22000, PerformedBy = "Київське АТБ", CreatedDate = DateTime.Now.AddDays(-10)
                }
            });

            // Розширені розклади рейсів (17 рейсів включаючи чартери)
            _flightSchedules.AddRange(new[]
            {
                // Регулярні пасажирські рейси
                new FlightSchedule
                {
                    Id = 1, FlightNumber = "PS101", AircraftId = 1, Aircraft = _aircraft[0],
                    Origin = "Київ (Бориспіль)", Destination = "Париж", FlightDate = DateTime.Today,
                    DepartureTime = DateTime.Today.AddHours(8), ArrivalTime = DateTime.Today.AddHours(11),
                    Status = FlightStatus.Scheduled, PassengerCount = 165, CargoWeight = 2500, FuelConsumption = 8500,
                    CreatedDate = DateTime.Now.AddDays(-7)
                },
                new FlightSchedule
                {
                    Id = 2, FlightNumber = "PS205", AircraftId = 3, Aircraft = _aircraft[2],
                    Origin = "Одеса", Destination = "Варшава", FlightDate = DateTime.Today,
                    DepartureTime = DateTime.Today.AddHours(14), ArrivalTime = DateTime.Today.AddHours(16),
                    Status = FlightStatus.Boarding, PassengerCount = 85, CargoWeight = 1200, FuelConsumption = 4200,
                    CreatedDate = DateTime.Now.AddDays(-3)
                },
                new FlightSchedule
                {
                    Id = 3, FlightNumber = "PS330", AircraftId = 4, Aircraft = _aircraft[3],
                    Origin = "Київ (Бориспіль)", Destination = "Лондон", FlightDate = DateTime.Today.AddDays(1),
                    DepartureTime = DateTime.Today.AddDays(1).AddHours(10), ArrivalTime = DateTime.Today.AddDays(1).AddHours(13),
                    Status = FlightStatus.Scheduled, PassengerCount = 245, CargoWeight = 4200, FuelConsumption = 12500,
                    CreatedDate = DateTime.Now.AddDays(-5)
                },
                new FlightSchedule
                {
                    Id = 4, FlightNumber = "PS450", AircraftId = 5, Aircraft = _aircraft[4],
                    Origin = "Харків", Destination = "Варшава", FlightDate = DateTime.Today,
                    DepartureTime = DateTime.Today.AddHours(15), ArrivalTime = DateTime.Today.AddHours(17),
                    Status = FlightStatus.Departed, PassengerCount = 72, CargoWeight = 800, FuelConsumption = 3200,
                    CreatedDate = DateTime.Now.AddDays(-2)
                },
                new FlightSchedule
                {
                    Id = 5, FlightNumber = "PS777", AircraftId = 6, Aircraft = _aircraft[5],
                    Origin = "Київ (Бориспіль)", Destination = "Нью-Йорк", FlightDate = DateTime.Today.AddDays(2),
                    DepartureTime = DateTime.Today.AddDays(2).AddHours(22), ArrivalTime = DateTime.Today.AddDays(3).AddHours(4),
                    Status = FlightStatus.Scheduled, PassengerCount = 298, CargoWeight = 8500, FuelConsumption = 28000,
                    CreatedDate = DateTime.Now.AddDays(-10)
                },
                new FlightSchedule
                {
                    Id = 6, FlightNumber = "PS188", AircraftId = 7, Aircraft = _aircraft[6],
                    Origin = "Одеса", Destination = "Стамбул", FlightDate = DateTime.Today,
                    DepartureTime = DateTime.Today.AddHours(12), ArrivalTime = DateTime.Today.AddHours(14),
                    Status = FlightStatus.Arrived, PassengerCount = 195, CargoWeight = 2800, FuelConsumption = 6200,
                    CreatedDate = DateTime.Now.AddDays(-4)
                },
                new FlightSchedule
                {
                    Id = 7, FlightNumber = "PS290", AircraftId = 8, Aircraft = _aircraft[7],
                    Origin = "Дніпро", Destination = "Будапешт", FlightDate = DateTime.Today.AddDays(1),
                    DepartureTime = DateTime.Today.AddDays(1).AddHours(7), ArrivalTime = DateTime.Today.AddDays(1).AddHours(9),
                    Status = FlightStatus.Scheduled, PassengerCount = 78, CargoWeight = 950, FuelConsumption = 2800,
                    CreatedDate = DateTime.Now.AddDays(-6)
                },
                new FlightSchedule
                {
                    Id = 8, FlightNumber = "PS372", AircraftId = 9, Aircraft = _aircraft[8],
                    Origin = "Львів", Destination = "Краків", FlightDate = DateTime.Today,
                    DepartureTime = DateTime.Today.AddHours(16), ArrivalTime = DateTime.Today.AddHours(17),
                    Status = FlightStatus.Delayed, PassengerCount = 65, CargoWeight = 450, FuelConsumption = 1200,
                    Notes = "Затримка через погодні умови", CreatedDate = DateTime.Now.AddDays(-1)
                },
                new FlightSchedule
                {
                    Id = 9, FlightNumber = "PS195", AircraftId = 12, Aircraft = _aircraft[11],
                    Origin = "Одеса", Destination = "Афіни", FlightDate = DateTime.Today.AddDays(1),
                    DepartureTime = DateTime.Today.AddDays(1).AddHours(11), ArrivalTime = DateTime.Today.AddDays(1).AddHours(13),
                    Status = FlightStatus.Scheduled, PassengerCount = 108, CargoWeight = 1800, FuelConsumption = 4800,
                    CreatedDate = DateTime.Now.AddDays(-8)
                },
                
                // Вантажні рейси
                new FlightSchedule
                {
                    Id = 10, FlightNumber = "CG747", AircraftId = 13, Aircraft = _aircraft[12],
                    Origin = "Київ (Бориспіль)", Destination = "Франкфурт", FlightDate = DateTime.Today,
                    DepartureTime = DateTime.Today.AddHours(2), ArrivalTime = DateTime.Today.AddHours(5),
                    Status = FlightStatus.Departed, PassengerCount = 0, CargoWeight = 124000, FuelConsumption = 45000,
                    Notes = "Спеціальний вантаж - медичне обладнання", CreatedDate = DateTime.Now.AddDays(-3)
                },
                new FlightSchedule
                {
                    Id = 11, FlightNumber = "CG330", AircraftId = 14, Aircraft = _aircraft[13],
                    Origin = "Дніпро", Destination = "Доха", FlightDate = DateTime.Today.AddDays(1),
                    DepartureTime = DateTime.Today.AddDays(1).AddHours(4), ArrivalTime = DateTime.Today.AddDays(1).AddHours(10),
                    Status = FlightStatus.Scheduled, PassengerCount = 0, CargoWeight = 68000, FuelConsumption = 32000,
                    CreatedDate = DateTime.Now.AddDays(-5)
                },
                new FlightSchedule
                {
                    Id = 12, FlightNumber = "CG767", AircraftId = 16, Aircraft = _aircraft[15],
                    Origin = "Львів", Destination = "Мілан", FlightDate = DateTime.Today,
                    DepartureTime = DateTime.Today.AddHours(18), ArrivalTime = DateTime.Today.AddHours(21),
                    Status = FlightStatus.Scheduled, PassengerCount = 0, CargoWeight = 45000, FuelConsumption = 18500,
                    CreatedDate = DateTime.Now.AddDays(-4)
                },
                new FlightSchedule
                {
                    Id = 13, FlightNumber = "AN124", AircraftId = 17, Aircraft = _aircraft[16],
                    Origin = "Київ (Антонов)", Destination = "Лейпциг", FlightDate = DateTime.Today.AddDays(2),
                    DepartureTime = DateTime.Today.AddDays(2).AddHours(6), ArrivalTime = DateTime.Today.AddDays(2).AddHours(10),
                    Status = FlightStatus.Scheduled, PassengerCount = 12, CargoWeight = 150000, FuelConsumption = 65000,
                    Notes = "Перевезення спеціального промислового обладнання", CreatedDate = DateTime.Now.AddDays(-12)
                },
                
                // Чартерні рейси
                new FlightSchedule
                {
                    Id = 14, FlightNumber = "CH001", AircraftId = 21, Aircraft = _aircraft[20],
                    Origin = "Київ (Жуляни)", Destination = "Женева", FlightDate = DateTime.Today,
                    DepartureTime = DateTime.Today.AddHours(13), ArrivalTime = DateTime.Today.AddHours(16),
                    Status = FlightStatus.Scheduled, PassengerCount = 12, CargoWeight = 500, FuelConsumption = 8200,
                    Notes = "Приватний чартер - бізнес делегація", CreatedDate = DateTime.Now.AddDays(-2)
                },
                new FlightSchedule
                {
                    Id = 15, FlightNumber = "CH002", AircraftId = 22, Aircraft = _aircraft[21],
                    Origin = "Київ (Бориспіль)", Destination = "Дубай", FlightDate = DateTime.Today.AddDays(3),
                    DepartureTime = DateTime.Today.AddDays(3).AddHours(9), ArrivalTime = DateTime.Today.AddDays(3).AddHours(15),
                    Status = FlightStatus.Scheduled, PassengerCount = 8, CargoWeight = 300, FuelConsumption = 12500,
                    Notes = "VIP чартер", CreatedDate = DateTime.Now.AddDays(-7)
                },
                new FlightSchedule
                {
                    Id = 16, FlightNumber = "CH003", AircraftId = 10, Aircraft = _aircraft[9],
                    Origin = "Київ (Жуляни)", Destination = "Анталія", FlightDate = DateTime.Today.AddDays(4),
                    DepartureTime = DateTime.Today.AddDays(4).AddHours(6), ArrivalTime = DateTime.Today.AddDays(4).AddHours(9),
                    Status = FlightStatus.Scheduled, PassengerCount = 189, CargoWeight = 3200, FuelConsumption = 9500,
                    Notes = "Туристичний чартер", CreatedDate = DateTime.Now.AddDays(-9)
                },
                new FlightSchedule
                {
                    Id = 17, FlightNumber = "CH004", AircraftId = 7, Aircraft = _aircraft[6],
                    Origin = "Одеса", Destination = "Тель-Авів", FlightDate = DateTime.Today.AddDays(1),
                    DepartureTime = DateTime.Today.AddDays(1).AddHours(20), ArrivalTime = DateTime.Today.AddDays(1).AddHours(23),
                    Status = FlightStatus.Scheduled, PassengerCount = 198, CargoWeight = 2600, FuelConsumption = 7800,
                    Notes = "Чартер для групової подорожі", CreatedDate = DateTime.Now.AddDays(-6)
                }
            });
        }

        public async Task<List<Aircraft>> GetAllAircraftAsync()
        {
            // Імітуємо асинхронну операцію
            await Task.Delay(100);
            return _aircraft.ToList();
        }

        public async Task<Aircraft?> GetAircraftByIdAsync(int id)
        {
            await Task.Delay(50);
            return _aircraft.FirstOrDefault(a => a.Id == id);
        }

        public async Task SaveAircraftAsync(Aircraft aircraft)
        {
            await Task.Delay(200);
            
            if (aircraft.Id == 0)
            {
                aircraft.Id = _aircraft.Count > 0 ? _aircraft.Max(a => a.Id) + 1 : 1;
                _aircraft.Add(aircraft);
                System.Diagnostics.Debug.WriteLine($"Додано новий літак: {aircraft.RegistrationNumber}, ID: {aircraft.Id}, Всього літаків: {_aircraft.Count}");
                System.Console.WriteLine($"[DataService] Додано новий літак: {aircraft.RegistrationNumber}, ID: {aircraft.Id}, Всього літаків: {_aircraft.Count}");
            }
            else
            {
                var existing = _aircraft.FirstOrDefault(a => a.Id == aircraft.Id);
                if (existing != null)
                {
                    var index = _aircraft.IndexOf(existing);
                    _aircraft[index] = aircraft;
                    System.Diagnostics.Debug.WriteLine($"Оновлено літак: {aircraft.RegistrationNumber}, ID: {aircraft.Id}");
                }
            }
            
            // Зберігаємо дані у файл після кожної зміни
            SaveAircraftToFile();
        }

        public async Task DeleteAircraftAsync(int id)
        {
            await Task.Delay(100);
            var aircraft = _aircraft.FirstOrDefault(a => a.Id == id);
            if (aircraft != null)
            {
                _aircraft.Remove(aircraft);
                SaveAircraftToFile();
            }
        }

        public async Task<List<MaintenanceRecord>> GetAllMaintenanceRecordsAsync()
        {
            await Task.Delay(100);
            return _maintenanceRecords.ToList();
        }

        public async Task SaveMaintenanceRecordAsync(MaintenanceRecord record)
        {
            await Task.Delay(200);
            
            if (record.Id == 0)
            {
                record.Id = _maintenanceRecords.Count > 0 ? _maintenanceRecords.Max(r => r.Id) + 1 : 1;
                _maintenanceRecords.Add(record);
            }
            else
            {
                var existing = _maintenanceRecords.FirstOrDefault(r => r.Id == record.Id);
                if (existing != null)
                {
                    var index = _maintenanceRecords.IndexOf(existing);
                    _maintenanceRecords[index] = record;
                }
            }
        }

        public async Task<List<FlightSchedule>> GetAllFlightSchedulesAsync()
        {
            await Task.Delay(100);
            return _flightSchedules.ToList();
        }

        public async Task<List<FlightSchedule>> GetFlightSchedulesAsync(DateTime date)
        {
            await Task.Delay(100);
            return _flightSchedules
                .Where(fs => fs.DepartureTime.Date == date.Date)
                .ToList();
        }

        public async Task SaveFlightScheduleAsync(FlightSchedule schedule)
        {
            await Task.Delay(200);
            
            if (schedule.Id == 0)
            {
                schedule.Id = _flightSchedules.Count > 0 ? _flightSchedules.Max(s => s.Id) + 1 : 1;
                _flightSchedules.Add(schedule);
                System.Diagnostics.Debug.WriteLine($"Додано новий рейс: {schedule.FlightNumber}, ID: {schedule.Id}, Всього рейсів: {_flightSchedules.Count}");
                System.Console.WriteLine($"[DataService] Додано новий рейс: {schedule.FlightNumber}, ID: {schedule.Id}, Всього рейсів: {_flightSchedules.Count}");
            }
            else
            {
                var existing = _flightSchedules.FirstOrDefault(s => s.Id == schedule.Id);
                if (existing != null)
                {
                    var index = _flightSchedules.IndexOf(existing);
                    _flightSchedules[index] = schedule;
                    System.Diagnostics.Debug.WriteLine($"Оновлено рейс: {schedule.FlightNumber}, ID: {schedule.Id}");
                }
            }
            
            // Зберігаємо дані у файл після кожної зміни
            SaveFlightSchedulesToFile();
        }

        public async Task DeleteFlightScheduleAsync(FlightSchedule schedule)
        {
            await Task.Delay(100);
            _flightSchedules.RemoveAll(s => s.Id == schedule.Id);
            SaveFlightSchedulesToFile();
        }

        public async Task<FleetStatistics> GetFleetStatisticsAsync()
        {
            await Task.Delay(100);
            
            return new FleetStatistics
            {
                TotalAircraft = _aircraft.Count,
                ActiveAircraft = _aircraft.Count(a => a.Status == AircraftStatus.Active),
                MaintenanceAircraft = _aircraft.Count(a => a.Status == AircraftStatus.Maintenance),
                TotalFlightHours = _aircraft.Sum(a => a.FlightHours),
                TotalMaintenanceCost = _maintenanceRecords.Sum(r => r.Cost)
            };
        }

        public async Task<Dictionary<string, int>> GetAircraftByStatusAsync()
        {
            await Task.Delay(50);
            
            return _aircraft
                .GroupBy(a => a.Status.ToString())
                .ToDictionary(g => TranslateStatus(g.Key), g => g.Count());
        }

        public async Task<Dictionary<string, int>> GetAircraftByManufacturerAsync()
        {
            await Task.Delay(50);
            
            return _aircraft
                .GroupBy(a => a.Manufacturer)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        private static string TranslateStatus(string status)
        {
            return status switch
            {
                "Active" => "Активні",
                "Maintenance" => "На обслуговуванні",
                "Grounded" => "На землі",
                "Reserved" => "Зарезервовані",
                "Retired" => "Списані",
                _ => status
            };
        }
    }

    // Статистика флоту
    public class FleetStatistics
    {
        public int TotalAircraft { get; set; }
        public int ActiveAircraft { get; set; }
        public int MaintenanceAircraft { get; set; }
        public int TotalFlightHours { get; set; }
        public decimal TotalMaintenanceCost { get; set; }
    }
}