using MongoDB.Driver;
using System.Threading.Tasks;
using EMS.Models;

namespace EMS.Services
{
    public class AuthenticationService
    {
        private readonly IMongoCollection<Employee> _employeeCollection;
        private readonly FingerprintService _fingerprintService;
        private Employee? _currentUser;

        public AuthenticationService(MongoDbContext dbContext)
        {
            _employeeCollection = dbContext.Employees;
            _fingerprintService = new FingerprintService();
        }

        public Employee? CurrentUser => _currentUser;

        public async Task<bool> LoginAsync(string username, string password)
        {
            // Master admin bypass
            if (username == "samiullah" && password == "samiullah")
            {
                _currentUser = new Employee
                {
                    Id = "master_admin",
                    Name = "Samiullah",
                    Position = "Master Admin",
                    Contact = "N/A",
                    Username = "samiullah",
                    Password = "samiullah",
                    UserRole = new UserRole {
                        RoleID = 0,
                        RoleName = "Admin",
                        Permissions = new List<Permission> {
                            Permission.ViewEmployees,
                            Permission.EditEmployees,
                            Permission.ViewReports,
                            Permission.EditRoles,
                            Permission.ManageUsers,
                            Permission.ViewPayroll,
                            Permission.EditPayroll,
                            Permission.GeneratePayroll,
                            Permission.ViewPayrollHistory
                        }
                    },
                    DateOfBirth = DateTime.MinValue
                };
                return true;
            }

            var user = await _employeeCollection.Find(x => x.Username == username).FirstOrDefaultAsync();
            
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                _currentUser = user;
                return true;
            }
            
            return false;
        }

        public async Task<bool> LoginWithFingerprintAsync()
        {
            // Only allow fingerprint login for master admin
            bool isAuthenticated = await _fingerprintService.AuthenticateMasterAsync();
            
            if (isAuthenticated)
            {
                _currentUser = new Employee
                {
                    Id = "master_admin",
                    Name = "Samiullah",
                    Position = "Master Admin",
                    Contact = "N/A",
                    Username = "samiullah",
                    Password = "fingerprint_authenticated",
                    UserRole = new UserRole {
                        RoleID = 0,
                        RoleName = "Admin",
                        Permissions = new List<Permission> {
                            Permission.ViewEmployees,
                            Permission.EditEmployees,
                            Permission.ViewReports,
                            Permission.EditRoles,
                            Permission.ManageUsers,
                            Permission.ViewPayroll,
                            Permission.EditPayroll,
                            Permission.GeneratePayroll,
                            Permission.ViewPayrollHistory
                        }
                    },
                    DateOfBirth = DateTime.MinValue
                };
                return true;
            }
            
            return false;
        }

        public async Task<bool> IsFingerprintAvailableAsync()
        {
            return await _fingerprintService.IsFingerprintAvailableAsync();
        }

        public void Logout()
        {
            _currentUser = null;
        }

        public bool IsUserInRole(string roleName)
        {
            return _currentUser?.UserRole.RoleName == roleName;
        }

        public bool IsAuthenticated => _currentUser != null;

        public bool HasPermission(Permission permission)
        {
            return _currentUser?.UserRole.Permissions.Contains(permission) ?? false;
        }
    }
} 