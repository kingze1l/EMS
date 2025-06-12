using EMS.Models;
using EMS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace EMS.Views
{
    public partial class AuditLogView : UserControl
    {
        private readonly IAuditLogService _auditLogService;
        private ObservableCollection<AuditLog> _logs;
        private DateTime _startDate;
        private DateTime _endDate;
        private string _selectedEntityType = "All";
        private string _selectedAction = "All";
        private bool _isLoading;

        public AuditLogView(IAuditLogService auditLogService)
        {
            InitializeComponent();
            _auditLogService = auditLogService;
            _logs = new ObservableCollection<AuditLog>();
            _startDate = DateTime.Today.AddDays(-7);
            _endDate = DateTime.Today;

            // Initialize the DataContext
            this.DataContext = this;

            // Load initial data
            _ = LoadLogs();
        }

        public ObservableCollection<AuditLog> Logs
        {
            get => _logs;
            set
            {
                _logs = value;
                OnPropertyChanged(nameof(Logs));
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
                _ = LoadLogs();
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
                _ = LoadLogs();
            }
        }

        public string SelectedEntityType
        {
            get => _selectedEntityType;
            set
            {
                _selectedEntityType = value;
                OnPropertyChanged(nameof(SelectedEntityType));
                _ = LoadLogs();
            }
        }

        public string SelectedAction
        {
            get => _selectedAction;
            set
            {
                _selectedAction = value;
                OnPropertyChanged(nameof(SelectedAction));
                _ = LoadLogs();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        private async Task LoadLogs()
        {
            try
            {
                IsLoading = true;
                // Convert local dates to UTC for MongoDB query
                var startDateUtc = StartDate.ToUniversalTime();
                var endDateUtc = EndDate.ToUniversalTime();
                System.Diagnostics.Debug.WriteLine($"Querying logs from {startDateUtc} to {endDateUtc}");

                var logs = await _auditLogService.GetLogsByDateRangeAsync(startDateUtc, endDateUtc);
                System.Diagnostics.Debug.WriteLine($"Retrieved {logs.Count} logs from MongoDB.");

                // Apply filters
                if (SelectedEntityType != "All")
                {
                    logs = logs.Where(l => l.EntityType == SelectedEntityType).ToList();
                    System.Diagnostics.Debug.WriteLine($"Filtered by EntityType: {SelectedEntityType}, remaining logs: {logs.Count}");
                }
                if (SelectedAction != "All")
                {
                    logs = logs.Where(l => l.Action == SelectedAction).ToList();
                    System.Diagnostics.Debug.WriteLine($"Filtered by Action: {SelectedAction}, remaining logs: {logs.Count}");
                }

                Logs.Clear();
                foreach (var log in logs.OrderByDescending(l => l.Timestamp))
                {
                    Logs.Add(log);
                }
                System.Diagnostics.Debug.WriteLine($"Added {Logs.Count} logs to the UI.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading audit logs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadLogs();
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
} 