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
                // Assign default role if none is provided
                if (employee.UserRole == null)
                {
                    employee.UserRole = new UserRole
                    {
                        RoleID = 3, // Assuming 3 is the ID for 'Employee'
                        RoleName = "Employee",
                        Permissions = new List<Permission> { Permission.ViewEmployees }
                    };
                }
                // Password is already hashed in the ViewModel
                await _employees.InsertOneAsync(employee);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateEmployeeAsync(Employee updatedEmployee, UserRole currentUserRole, string? oldPassword = null)
        {
            // Admin can update any employee without old password
            if (currentUserRole.RoleName == "Admin")
            {
                try
                {
                    var result = await _employees.ReplaceOneAsync(e => e.Id == updatedEmployee.Id, updatedEmployee);
                    return result.ModifiedCount > 0;
                }
                catch
                {
                    return false;
                }
            }

            // Manager cannot update any employee info
            if (currentUserRole.RoleName == "Manager")
                return false;

            // Employee can only update their own info (contact/password)
            if (currentUserRole.RoleName == "Employee")
            {
                try
                {
                    var existingEmployee = await _employees.Find(e => e.Id == updatedEmployee.Id).FirstOrDefaultAsync();
                    if (existingEmployee == null || existingEmployee.Username != updatedEmployee.Username)
                        return false; // Cannot change username or update others

                    // Check if password is being changed
                    bool isPasswordChange = !string.IsNullOrEmpty(updatedEmployee.Password) && updatedEmployee.Password != existingEmployee.Password;
                    
                    if (isPasswordChange)
                    {
                        // Require old password to match
                        if (string.IsNullOrEmpty(oldPassword) || !BCrypt.Net.BCrypt.Verify(oldPassword, existingEmployee.Password))
                            return false;
                    }
                    else
                    {
                        // Keep existing password if not changing
                        updatedEmployee.Password = existingEmployee.Password;
                    }

                    // Only allow updating contact info and password
                    existingEmployee.Contact = updatedEmployee.Contact;
                    existingEmployee.Password = updatedEmployee.Password;
                    
                    // Preserve all other fields from existing employee
                    updatedEmployee.Name = existingEmployee.Name;
                    updatedEmployee.Position = existingEmployee.Position;
                    updatedEmployee.Username = existingEmployee.Username;
                    updatedEmployee.UserRole = existingEmployee.UserRole;
                    updatedEmployee.DateOfBirth = existingEmployee.DateOfBirth;
                    updatedEmployee.BasePay = existingEmployee.BasePay;
                    updatedEmployee.Bonus = existingEmployee.Bonus;
                    updatedEmployee.Deductions = existingEmployee.Deductions;
                    updatedEmployee.BiometricEnabled = existingEmployee.BiometricEnabled;
                    updatedEmployee.BiometricUserId = existingEmployee.BiometricUserId;
                    updatedEmployee.BiometricEnrolledDate = existingEmployee.BiometricEnrolledDate;

                    var result = await _employees.ReplaceOneAsync(e => e.Id == updatedEmployee.Id, updatedEmployee);
                    return result.ModifiedCount > 0;
                }
                catch
                {
                    return false;
                }
            }

            return false;
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