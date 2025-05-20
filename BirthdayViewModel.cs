using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System;
using EMS.Services;
using EMS.Models;

namespace EMS.ViewModels
{
    public class BirthdayViewModel : INotifyPropertyChanged
    {
        private string _employeeName = string.Empty;
        private bool _hasBirthdayToday;
        private readonly IEmployeeService _employeeService;
        private readonly UserRole _currentUserRole;

        public BirthdayViewModel(IEmployeeService employeeService, UserRole currentUserRole)
        {
            _employeeService = employeeService;
            _currentUserRole = currentUserRole;
            InitializeAsync();
        }

        public string EmployeeName
        {
            get => _employeeName;
            set
            {
                _employeeName = value;
                OnPropertyChanged();
            }
        }

        public bool HasBirthdayToday
        {
            get => _hasBirthdayToday;
            set
            {
                _hasBirthdayToday = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void InitializeAsync()
        {
            var employees = await _employeeService.GetAllEmployeesAsync(_currentUserRole);
            foreach (var employee in employees)
            {
                if (employee.DateOfBirth.Date == DateTime.Today.Date)
                {
                    EmployeeName = employee.Name;
                    HasBirthdayToday = true;
                    break;
                }
            }
        }
    }
}