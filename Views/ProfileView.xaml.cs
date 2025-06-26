using System.Windows.Controls;
using System.Windows;
using EMS.ViewModels;

namespace EMS.Views
{
    public partial class ProfileView : UserControl
    {
        public ProfileView()
        {
            InitializeComponent();
            // DataContext will be set from outside
        }

        private void OldPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProfileViewModel vm && sender is PasswordBox pb)
            {
                vm.OldPassword = pb.SecurePassword;
            }
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProfileViewModel vm && sender is PasswordBox pb)
            {
                vm.NewPassword = pb.SecurePassword;
            }
        }
    }
} 