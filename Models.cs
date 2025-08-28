using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace AeroFleetManagerPro.Models
{
    // Статуси літаків
    public enum AircraftStatus
    {
        Active,      // Активний
        Maintenance, // На обслуговуванні
        Grounded,    // На землі
        Reserved,    // Зарезервований
        Retired      // Списаний
    }

    // Типи технічного обслуговування
    public enum MaintenanceType
    {
        Routine,     // Рутинне
        Scheduled,   // Планове
        Unscheduled, // Позапланове
        ACheck,      // A-перевірка
        BCheck,      // B-перевірка
        CCheck,      // C-перевірка
        DCheck,      // D-перевірка
        Heavy,       // Капітальний ремонт
        Emergency,   // Екстрене
        Modification // Модифікація
    }

    // Статуси обслуговування
    public enum MaintenanceStatus
    {
        Scheduled,   // Заплановано
        InProgress,  // В процесі
        Completed,   // Завершено
        Cancelled,   // Скасовано
        Overdue      // Прострочено
    }

    // Статуси рейсів
    public enum FlightStatus
    {
        Scheduled,   // Заплановано
        Boarding,    // Посадка
        Departed,    // Відправлено
        InFlight,    // В польоті
        Arrived,     // Прибуло
        Cancelled,   // Скасовано
        Delayed      // Затримано
    }

    // Модель літака
    public class Aircraft
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string RegistrationNumber { get; set; } = string.Empty;
        
        [Required]
        public string Manufacturer { get; set; } = string.Empty;
        
        [Required]
        public string Model { get; set; } = string.Empty;
        
        public string Type { get; set; } = string.Empty;
        public int YearOfManufacture { get; set; }
        public int PassengerCapacity { get; set; }
        public double CargoCapacity { get; set; }
        public double FuelCapacity { get; set; }
        public double Range { get; set; }
        public double CruisingSpeed { get; set; }
        public int FlightHours { get; set; }
        public int FlightCycles { get; set; }
        public DateTime LastMaintenanceDate { get; set; }
        public DateTime NextMaintenanceDate { get; set; }
        public AircraftStatus Status { get; set; }
        public string BaseLocation { get; set; } = string.Empty;
        public string CurrentLocation { get; set; } = string.Empty;
        public DateTime AcquisitionDate { get; set; }
        public decimal AcquisitionCost { get; set; }
        public string Notes { get; set; } = string.Empty;

        public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
        public ICollection<FlightSchedule> FlightSchedules { get; set; } = new List<FlightSchedule>();
    }

    // Модель запису про обслуговування
    public class MaintenanceRecord
    {
        [Key]
        public int Id { get; set; }
        
        public int AircraftId { get; set; }
        public Aircraft Aircraft { get; set; } = null!;
        
        public MaintenanceType Type { get; set; }
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        public DateTime ScheduledDate { get; set; }
        public DateTime? ActualDate { get; set; }
        public MaintenanceStatus Status { get; set; }
        public decimal Cost { get; set; }
        public string PerformedBy { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int DurationHours { get; set; }
        public string PartsReplaced { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    // Модель розкладу рейсів
    public class FlightSchedule
    {
        [Key]
        public int Id { get; set; }
        
        public int AircraftId { get; set; }
        public Aircraft Aircraft { get; set; } = null!;
        
        [Required]
        public string FlightNumber { get; set; } = string.Empty;
        
        [Required]
        public string Origin { get; set; } = string.Empty;
        
        [Required]
        public string Destination { get; set; } = string.Empty;
        
        public DateTime FlightDate { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public FlightStatus Status { get; set; }
        public int PassengerCount { get; set; }
        public double CargoWeight { get; set; }
        public double FuelConsumption { get; set; }
        public string CrewMembers { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}