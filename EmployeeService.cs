using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using EMS.Models;
using BCrypt.Net;

namespace EMS.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IMongoCollection<Employee> _employees;

        public EmployeeService(MongoDbContext context)
        {
            _employees = context.Employees;
        }

        public async Task<IEnumerable<Employee>> SearchEmployeesAsync(string searchTerm, UserRole currentUserRole)
        {
            var filter = Builders<Employee>.Filter.Or(
                Builders<Employee>.Filter.Regex(e => e.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<Employee>.Filter.Regex(e => e.Contact, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );
            var employees = await _employees.Find(filter).ToListAsync();
            return FilterEmployeesByRole(employees, currentUserRole);
        }

        public async Task<Employee?> GetEmployeeByIdAsync(string id, UserRole currentUserRole)
        {
            var employee = await _employees.Find(e => e.Id == id).FirstOrDefaultAsync();
            if (employee == null) return null;
            
            return currentUserRole.RoleName == "Admin" ? employee : SanitizeEmployeeData(employee);
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync(UserRole currentUserRole)
        {
            var employees = await _employees.Find(_ => true).ToListAsync();
            return FilterEmployeesByRole(employees, currentUserRole);
        }

        public async Task<bool> AddEmployeeAsync(Employee employee)
        {
            try
            {
                // Hash the password before storing
                employee.Password = BCrypt.Net.BCrypt.HashPassword(employee.Password);
                await _employees.InsertOneAsync(employee);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateEmployeeAsync(Employee employee, UserRole currentUserRole)
        {
            if (currentUserRole.RoleName != "Admin")
                return false;

            try
            {
                // If password is being updated, hash it
                if (!string.IsNullOrEmpty(employee.Password))
                {
                    employee.Password = BCrypt.Net.BCrypt.HashPassword(employee.Password);
                }
                else
                {
                    // Keep the existing password
                    var existingEmployee = await GetEmployeeByIdAsync(employee.Id, currentUserRole);
                    if (existingEmployee != null)
                    {
                        employee.Password = existingEmployee.Password;
                    }
                    else
                    {
                        return false;
                    }
                }

                var result = await _employees.ReplaceOneAsync(e => e.Id == employee.Id, employee);
                return result.ModifiedCount > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteEmployeeAsync(string id, UserRole currentUserRole)
        {
            if (currentUserRole.RoleName != "Admin")
                return false;

            var result = await _employees.DeleteOneAsync(e => e.Id == id);
            return result.DeletedCount > 0;
        }

        private IEnumerable<Employee> FilterEmployeesByRole(IEnumerable<Employee> employees, UserRole currentUserRole)
        {
            if (currentUserRole.RoleName == "Admin")
                return employees;

            return employees.Select(e => SanitizeEmployeeData(e));
        }

        private Employee SanitizeEmployeeData(Employee employee)
        {
            // Return limited data for non-admin users
            return new Employee
            {
                Id = employee.Id,
                Name = employee.Name,
                Position = employee.Position,
                Contact = employee.Contact,
                Username = employee.Username,
                Password = employee.Password,
                UserRole = employee.UserRole,
                DateOfBirth = employee.DateOfBirth
            };
        }
    }
} 