using System.Windows.Controls;
using EMS.ViewModels;
using EMS.Services;
using EMS.Models;
using System;
using System.Windows;
using System.Windows.Input;

namespace EMS.Views
{
    public partial class LeaveRequestView : UserControl
    {
        public LeaveRequestView(ILeaveService leaveService, IAuditLogService auditLogService, IEmployeeService employeeService, string currentUserId, UserRole currentUserRole)
        {
            InitializeComponent();
            DataContext = new LeaveRequestViewModel(leaveService, auditLogService, employeeService, currentUserId, currentUserRole);
        }

        private void LeaveRequestsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid grid && grid.SelectedItem is LeaveRequest leaveRequest)
            {
                var detailsWindow = new LeaveRequestDetailsWindow(leaveRequest);
                detailsWindow.Owner = Window.GetWindow(this);
                detailsWindow.ShowDialog();
            }
        }
    }
} 