using EMS.Models;
using EMS.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EMS.ViewModels
{
    public class RoleManagementViewModel : INotifyPropertyChanged
    {
        private readonly IRoleService _roleService;
        private ObservableCollection<Role> _roles;
        private Role? _selectedRole;
        private ObservableCollection<PermissionGroup> _permissionGroups;
        private bool _isEditing;
        private string _searchText = string.Empty;

        public RoleManagementViewModel(IRoleService roleService)
        {
            _roleService = roleService;
            _roles = new ObservableCollection<Role>();
            _selectedRole = null;
            _permissionGroups = new ObservableCollection<PermissionGroup>();
            
            LoadRolesCommand = new RelayCommand(() => LoadRoles());
            AddRoleCommand = new RelayCommand(AddRole);
            EditRoleCommand = new RelayCommand(EditRole, () => SelectedRole != null);
            SaveRoleCommand = new RelayCommand(async () => await SaveRole(), () => IsEditing && SelectedRole != null);
            CancelEditCommand = new RelayCommand(CancelEdit, () => IsEditing);
            DeleteRoleCommand = new RelayCommand(async () => await DeleteRole(), () => CanDeleteRole && SelectedRole != null);
            SearchCommand = new RelayCommand(() => FilterRoles());

            LoadRoles();
        }

        public ObservableCollection<Role> Roles
        {
            get => _roles;
            set
            {
                _roles = value;
                OnPropertyChanged();
            }
        }

        public Role? SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanDeleteRole));
                OnPropertyChanged(nameof(CanEditRole));
                if (value != null)
                {
                    LoadRolePermissions();
                }
                else
                {
                    PermissionGroups.Clear();
                }
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ObservableCollection<PermissionGroup> PermissionGroups
        {
            get => _permissionGroups;
            set
            {
                _permissionGroups = value;
                OnPropertyChanged();
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsViewing));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsViewing => !IsEditing;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterRoles();
            }
        }

        public bool CanDeleteRole => SelectedRole != null && !SelectedRole.IsSystemRole;
        public bool CanEditRole => SelectedRole != null && !SelectedRole.IsSystemRole;

        public ICommand LoadRolesCommand { get; }
        public ICommand AddRoleCommand { get; }
        public ICommand EditRoleCommand { get; }
        public ICommand SaveRoleCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand DeleteRoleCommand { get; }
        public ICommand SearchCommand { get; }

        private async void LoadRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            Roles = new ObservableCollection<Role>(roles);
            SelectedRole = null;
            PermissionGroups.Clear();
        }

        private void AddRole()
        {
            SelectedRole = new Role
            {
                RoleName = "New Role",
                Type = RoleType.Employee,
                Description = "Enter role description",
                Permissions = new List<string>()
            };
            IsEditing = true;
            LoadRolePermissions();
        }

        private void EditRole()
        {
            if (SelectedRole != null && !SelectedRole.IsSystemRole)
            {
                IsEditing = true;
                LoadRolePermissions();
            }
        }

        private async Task SaveRole()
        {
            if (SelectedRole == null) return;

            try
            {
                SelectedRole.Permissions = PermissionGroups
                    .SelectMany(pg => pg.Permissions)
                    .Where(p => p.IsSelected)
                    .Select(p => p.Name)
                    .ToList();

                if (string.IsNullOrEmpty(SelectedRole.Id))
                {
                    await _roleService.CreateRoleAsync(SelectedRole);
                }
                else
                {
                    await _roleService.UpdateRoleAsync(SelectedRole);
                }
                LoadRoles();
                CancelEdit();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving role: {ex.Message}");
            }
        }

        private void CancelEdit()
        {
            IsEditing = false;
            LoadRoles();
        }

        private async Task DeleteRole()
        {
            if (SelectedRole == null || SelectedRole.IsSystemRole) return;

            try
            {
                await _roleService.DeleteRoleAsync(SelectedRole.Id);
                LoadRoles();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting role: {ex.Message}");
            }
        }

        private async void LoadRolePermissions()
        {
            if (SelectedRole == null) return;

            var groups = new ObservableCollection<PermissionGroup>();
            var categories = typeof(PermissionDefinition.Categories).GetFields()
                .Where(f => f.IsLiteral && !f.IsInitOnly)
                .Select(f => f.GetValue(null)?.ToString())
                .Where(name => !string.IsNullOrEmpty(name));

            foreach (var categoryName in categories)
            {
                var permissions = await _roleService.GetPermissionsByCategoryAsync(categoryName!);
                var group = new PermissionGroup
                {
                    Category = categoryName!,
                    Permissions = new ObservableCollection<PermissionItem>(
                        permissions.Select(p => new PermissionItem
                        {
                            Name = p.Name,
                            Description = p.Description,
                            IsSelected = SelectedRole.Permissions.Contains(p.Name)
                        })
                    )
                };
                groups.Add(group);
            }
            PermissionGroups = groups;
        }

        private void FilterRoles()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadRoles();
                return;
            }

            var filteredRoles = Roles.Where(r =>
                r.RoleName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                r.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                r.Type.ToString().Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            ).ToList();

            Roles = new ObservableCollection<Role>(filteredRoles);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PermissionGroup
    {
        public string Category { get; set; } = string.Empty;
        public ObservableCollection<PermissionItem> Permissions { get; set; } = new ObservableCollection<PermissionItem>();
    }

    public class PermissionItem
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
} 