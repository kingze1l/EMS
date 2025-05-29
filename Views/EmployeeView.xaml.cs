using System.Windows.Controls;
using EMS.Services;
using EMS.ViewModels;
using EMS.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace EMS.Views
{
    /// <summary>
    /// Interaction logic for EmployeeView.xaml
    /// </summary>
    public partial class EmployeeView : Page
    {
        public EmployeeView(IEmployeeService employeeService, UserRole currentUserRole)
        {
            InitializeComponent();
            var app = Application.Current as App;
            // Pass the current user role to the EmployeeViewModel
            if (app?.ServiceProvider != null)
            {
                 this.DataContext = new EmployeeViewModel(employeeService, app.ServiceProvider.GetRequiredService<IRoleService>(), currentUserRole);
            } else
            {
                // Handle the case where ServiceProvider is not available, perhaps show an error or log it.
                // For now, we'll just set DataContext to null or a default ViewModel state.
                 this.DataContext = null; // Or a ViewModel indicating an error
                 MessageBox.Show("Application ServiceProvider is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
} 