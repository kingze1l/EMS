using System.Windows.Controls;
using EMS.Services;
using EMS.ViewModels;
using EMS.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Security;

namespace EMS.Views
{
    /// <summary>
    /// Interaction logic for EmployeeView.xaml
    /// </summary>
    public partial class EmployeeView : Page
    {
        private EmployeeViewModel _viewModel;

        public EmployeeView(IEmployeeService employeeService, IRoleService roleService, IAuditLogService auditLogService, UserRole currentUserRole)
        {
            InitializeComponent();
            _viewModel = new EmployeeViewModel(employeeService, roleService, auditLogService, currentUserRole);
            DataContext = _viewModel;

            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedEmployee != null && sender is PasswordBox passwordBox)
            {
                _viewModel.Password = passwordBox.SecurePassword;
            }
        }
    }
} 