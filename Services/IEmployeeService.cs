using System.Threading.Tasks;
using System.Collections.Generic;
using EMS.Models;

namespace EMS.Services
{
    public interface IEmployeeService
    {
        Task<IEnumerable<Employee>> SearchEmployeesAsync(string searchTerm, UserRole currentUserRole);
        Task<Employee?> GetEmployeeByIdAsync(string id, UserRole currentUserRole);
        Task<IEnumerable<Employee>> GetAllEmployeesAsync(UserRole currentUserRole);
        Task<bool> AddEmployeeAsync(Employee employee);
        Task<bool> UpdateEmployeeAsync(Employee employee, UserRole currentUserRole);
        Task<bool> DeleteEmployeeAsync(string id, UserRole currentUserRole);
    }
}