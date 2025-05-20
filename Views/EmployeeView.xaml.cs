using System.Windows.Controls;
using EMS.ViewModels;
using EMS.Services;
using EMS.Models;

namespace EMS.Views
{
    public partial class EmployeeView : Page
    {
        public EmployeeView(IEmployeeService employeeService, UserRole currentUserRole)
        {
            InitializeComponent();
            DataContext = new EmployeeViewModel(employeeService, currentUserRole);
        }
    }
} 