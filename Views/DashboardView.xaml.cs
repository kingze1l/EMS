using System.Windows.Controls;
using EMS.Services;
using EMS.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace EMS.Views
{
    public partial class DashboardView : UserControl
    {
        private DashboardViewModel _viewModel;

        public DashboardView()
        {
            InitializeComponent();
            
            // Get services from the application's service provider
            var app = System.Windows.Application.Current as App;
            if (app?.ServiceProvider != null)
            {
                var employeeService = app.ServiceProvider.GetRequiredService<EmployeeService>();
                var leaveService = app.ServiceProvider.GetRequiredService<LeaveService>();
                var payrollService = app.ServiceProvider.GetRequiredService<PayrollService>();
                var authService = app.ServiceProvider.GetRequiredService<AuthenticationService>();

                _viewModel = new DashboardViewModel(employeeService, leaveService, payrollService, authService);
                DataContext = _viewModel;
            }
        }
    }
} 