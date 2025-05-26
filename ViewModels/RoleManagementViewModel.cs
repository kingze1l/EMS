using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EMS.Models;
using System.Linq;
using System.Collections.Generic;
using EMS.ViewModels;

namespace EMS.ViewModels
{
    public class RoleManagementViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<UserRole> Roles { get; set; } = new();
        public ObservableCollection<Permission> AllPermissions { get; set; } = new();
        private UserRole? _selectedRole;
        public UserRole? SelectedRole
        {
            get => _selectedRole;
            set { _selectedRole = value; OnPropertyChanged(); }
        }

        public ICommand AddRoleCommand { get; }
        public ICommand SaveRoleCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand TogglePermissionCommand { get; }

        public RoleManagementViewModel()
        {
            // Example data
            AllPermissions = new ObservableCollection<Permission>((Permission[])System.Enum.GetValues(typeof(Permission)));
            Roles.Add(new UserRole { RoleID = 1, RoleName = "Admin", Permissions = AllPermissions.ToList() });
            Roles.Add(new UserRole { RoleID = 2, RoleName = "Manager", Permissions = new List<Permission> { Permission.ViewEmployees, Permission.EditEmployees, Permission.ViewReports } });
            Roles.Add(new UserRole { RoleID = 3, RoleName = "HR", Permissions = new List<Permission> { Permission.ViewEmployees, Permission.ManageUsers } });
            SelectedRole = Roles.FirstOrDefault();

            AddRoleCommand = new RelayCommand(AddRole);
            SaveRoleCommand = new RelayCommand(SaveRole, () => SelectedRole != null);
            CancelEditCommand = new RelayCommand(CancelEdit);
            TogglePermissionCommand = new RelayCommand<Permission>(TogglePermission);
        }

        private void AddRole()
        {
            var newRole = new UserRole { RoleID = Roles.Count + 1, RoleName = "New Role", Permissions = new List<Permission>() };
            Roles.Add(newRole);
            SelectedRole = newRole;
        }

        private void SaveRole()
        {
            // In a real app, save to DB here
            OnPropertyChanged(nameof(Roles));
        }

        private void CancelEdit()
        {
            // In a real app, reload from DB or revert changes
            SelectedRole = Roles.FirstOrDefault();
        }

        private void TogglePermission(Permission permission)
        {
            if (SelectedRole == null || SelectedRole.Permissions == null) return;
            if (SelectedRole.Permissions.Contains(permission))
                SelectedRole.Permissions.Remove(permission);
            else
                SelectedRole.Permissions.Add(permission);
            OnPropertyChanged(nameof(SelectedRole));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 