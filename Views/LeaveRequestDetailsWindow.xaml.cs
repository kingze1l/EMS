using System.Windows;
using EMS.Models;

namespace EMS.Views
{
    public partial class LeaveRequestDetailsWindow : Window
    {
        public LeaveRequestDetailsWindow(LeaveRequest leaveRequest)
        {
            InitializeComponent();
            DataContext = leaveRequest;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement actual approve logic and notification
            this.Close();
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement actual reject logic and notification
            this.Close();
        }
    }
} 