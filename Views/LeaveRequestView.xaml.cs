using System.Windows.Controls;
using EMS.ViewModels;
using EMS.Services;
using EMS.Models;
using System;

namespace EMS.Views
{
    public partial class LeaveRequestView : UserControl
    {
        public LeaveRequestView(ILeaveService leaveService, IAuditLogService auditLogService, IEmployeeService employeeService, string currentUserId, UserRole currentUserRole)
        {
            InitializeComponent();
            DataContext = new LeaveRequestViewModel(leaveService, auditLogService, employeeService, currentUserId, currentUserRole);
        }
    }
} 