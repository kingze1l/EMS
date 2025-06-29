# Employee Management System (EMS)

## Project Overview
A comprehensive desktop-based Employee Management System built with WPF, C#, and MongoDB for New Zealand Finance and Trade Company (NZFTC). The system provides centralized HR processes, automated administrative tasks, and secure access to employee information.

## Technology Stack
- **Framework**: .NET 8.0
- **Language**: C# 12
- **Frontend**: WPF (Windows Presentation Foundation)
- **Database**: MongoDB
- **Authentication**: BCrypt password hashing
- **Architecture**: MVVM Pattern
- **UI Framework**: Material Design Themes

## Prerequisites
- Visual Studio 2022 Professional
- .NET 8.0 SDK
- MongoDB (local or cloud instance)
- Windows 10/11

## Installation & Setup

### 1. Clone the Repository
```bash
git clone https://github.com/kingze1l/EMS.git
cd EMS
```

### 2. Database Setup

#### Option A: Use Existing Cloud MongoDB (Recommended)
The application is already configured to use MongoDB Atlas cloud database. No changes needed to `appsettings.json`:

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb+srv://Kingzell:utqbm9GnwSQ99QDH@ems.m64qhyk.mongodb.net/EMS",
    "DatabaseName": "EMS"
  }
}
```

#### Option B: Use Local MongoDB
If you prefer to use a local MongoDB instance:

1. Install MongoDB Community Server locally
2. Update connection string in `appsettings.json`:
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "EMS"
  }
}
```

### 3. Build and Run
1. Open `EMS.sln` in Visual Studio 2022
2. Restore NuGet packages
3. Build the solution (Ctrl+Shift+B)
4. Run the application (F5)

## User Credentials

### Master Admin (Development Access)
- **Username**: `samiullah`
- **Password**: `samiullah`
- **Role**: Master Administrator
- **Permissions**: Full system access
- **Note**: This is a development bypass for testing

### Pre-configured Users
The system comes with seeded users for testing:

#### Admin User
- **Username**: `admin`
- **Password**: `admin123`
- **Role**: Administrator
- **Permissions**: Full HR management, payroll, employee management
- **Base Pay**: $120,000
- **Bonus**: $10,000
- **Deductions**: $2,000

#### Manager User
- **Username**: `manager`
- **Password**: `manager123`
- **Role**: Manager
- **Permissions**: View employees, edit employees, view reports
- **Base Pay**: $95,000
- **Bonus**: $7,000
- **Deductions**: $1,500

#### HR User
- **Username**: `hr`
- **Password**: `hr123`
- **Role**: HR
- **Permissions**: View employees, manage users
- **Base Pay**: $75,000
- **Bonus**: $4,000
- **Deductions**: $1,200

#### Regular Employee
- **Username**: `employee`
- **Password**: `employee123`
- **Role**: Employee
- **Permissions**: View employees (limited data), update own profile
- **Base Pay**: $55,000
- **Bonus**: $2,000
- **Deductions**: $800

## Database Seeding Information

### Automatic Seeding
The application automatically seeds the database with:
- **4 pre-configured users** (Admin, Manager, HR, Employee)
- **4 system roles** with appropriate permissions
- **Permission definitions** for all system features
- **Sample data** for testing all features

### Seeding Process
- Seeding occurs automatically on first run
- Located in `DatabaseSeeder.cs` (lines 1-294)
- Creates realistic salary data for testing payroll features
- Establishes role hierarchy and permissions

### Database Collections Created
1. **Employees** - User accounts and employee data
2. **roles** - Role definitions
3. **permissions** - Permission definitions
4. **leaveRequests** - Leave applications
5. **payrollRecords** - Payroll history
6. **auditLogs** - System activity logs
7. **notifications** - User notifications
8. **settings** - System settings

## Core Features

### 1. User Authentication & Security
- **BCrypt password hashing** for secure authentication
- **Role-based access control** with granular permissions
- **Biometric authentication** support (Windows Hello)
- **Session management** and secure logout

### 2. Employee Management
- **Full CRUD operations** for employee records
- **Role-based data access** (Admin sees all, others see limited data)
- **Search and filter** functionality
- **Profile management** with self-service capabilities

### 3. Leave Management
- **Leave request submission** by employees
- **Approval workflow** (Pending/Approved/Rejected)
- **Manager comments** and feedback
- **Leave history** tracking
- **Status notifications**

### 4. Payroll Management
- **Automated payroll calculations** (Base Pay + Bonus - Deductions)
- **Individual and bulk payroll** generation
- **Payroll history** and records
- **Audit trail** for all payroll operations
- **Employee self-service** payroll viewing

### 5. Role Management
- **Flexible role system** with custom permissions
- **Permission-based access control**
- **Role assignment** and management
- **System roles**: Admin, Manager, HR, Employee

### 6. Audit & Security
- **Comprehensive audit logging** with IP tracking
- **Real-time notifications** system
- **Data sanitization** for non-admin users
- **Secure data handling** and validation

### 7. User Interface
- **Modern WPF interface** with Material Design
- **Dark/Light theme** support
- **Responsive navigation** with animations
- **Professional dashboard** with charts and analytics
- **Intuitive user experience**

## Database Structure

### Collections
1. **Employees** - Employee records and user accounts
2. **leaveRequests** - Leave application and approval data
3. **payrollRecords** - Payroll history and calculations
4. **auditLogs** - System activity and security logs
5. **notifications** - User notifications and alerts
6. **roles** - Role definitions and permissions
7. **permissions** - Permission definitions
8. **settings** - System and user settings

### Key Relationships
- Employees ↔ Leave Requests (1:Many)
- Employees ↔ Payroll Records (1:Many)
- Employees ↔ Audit Logs (1:Many)
- Roles ↔ Permissions (Many:Many)

## Project Architecture

### MVVM Pattern Implementation
```
Views/          - WPF User Interfaces
ViewModels/     - Business Logic and Data Binding
Models/         - Data Models and Entities
Services/       - Business Services and Data Access
Converters/     - WPF Data Converters
Utils/          - Utility Classes
```

### Key Services
- `AuthenticationService` - User authentication and authorization
- `EmployeeService` - Employee CRUD operations
- `LeaveService` - Leave request management
- `PayrollService` - Payroll calculations and records
- `AuditLogService` - System audit logging
- `NotificationService` - User notifications
- `RoleService` - Role and permission management

## Important Files and Locations

### Core Application Files
- `App.xaml.cs` - Application startup and configuration
- `MainWindow.xaml` - Main application interface
- `LoginWindow.xaml` - Login screen
- `Program.cs` - Application entry point

### Database Files
- `DatabaseSeeder.cs` - Database initialization and seeding
- `MongoDbContext.cs` - Database connection and context
- `MongoDbSettings.cs` - Database configuration
- `appsettings.json` - Connection string and settings

### Key Models
- `Models/Employee.cs` - Employee data model
- `Models/LeaveRequest.cs` - Leave request model
- `Models/PayrollRecord.cs` - Payroll data model
- `Models/AuditLog.cs` - Audit logging model
- `Models/Notification.cs` - Notification model

### Services
- `Services/EmployeeService.cs` - Employee management
- `Services/PayrollService.cs` - Payroll operations
- `Services/LeaveService.cs` - Leave management
- `Services/AuditLogService.cs` - Audit logging
- `AuthenticationService.cs` - User authentication

## Development Features

### Code Quality
- **SOLID principles** implementation
- **Dependency injection** for loose coupling
- **Async/await** for responsive UI
- **Error handling** and validation
- **Clean code** practices

### Security Features
- **Password hashing** with BCrypt
- **Role-based access control**
- **Data sanitization** for sensitive information
- **Audit logging** for compliance
- **Input validation** and sanitization

## Testing the Application

### 1. Login Testing
1. Use any of the provided credentials above
2. Test biometric login (if Windows Hello is available)
3. Verify role-based access restrictions

### 2. Employee Management
1. **As Admin**: Add, edit, delete employees
2. **As Manager**: View and edit employee data
3. **As Employee**: Update own profile only

### 3. Leave Management
1. Submit leave requests as different users
2. Approve/reject requests as admin/manager
3. View leave history and status

### 4. Payroll Management
1. Generate individual payroll records
2. Perform bulk payroll operations
3. View payroll history and calculations

### 5. Role Management
1. Create custom roles with specific permissions
2. Assign roles to employees
3. Test permission-based access

## Quick Start Guide for Tutors

### First Time Setup
1. **No database setup required** - Cloud MongoDB is pre-configured
2. **Build and run** the application directly
3. **Login with master admin**: `samiullah` / `samiullah`

### Testing Different User Roles
1. **Admin Testing**: Use `admin` / `admin123`
   - Full access to all features
   - Can manage employees, payroll, roles
   
2. **Manager Testing**: Use `manager` / `manager123`
   - Can view/edit employees
   - Can approve leave requests
   
3. **HR Testing**: Use `hr` / `hr123`
   - Can view employees and manage users
   
4. **Employee Testing**: Use `employee` / `employee123`
   - Limited access to own data only

### Key Features to Demonstrate
1. **Dashboard** - Overview with charts and statistics
2. **Employee Management** - CRUD operations with role restrictions
3. **Leave Management** - Submit and approve leave requests
4. **Payroll Management** - Generate and view payroll records
5. **Audit Logs** - View system activity and security logs
6. **Settings** - Theme switching and system configuration

## Troubleshooting

### Common Issues

#### Database Connection
- **Cloud MongoDB**: Connection is pre-configured and should work immediately
- **Local MongoDB**: Ensure MongoDB service is running on localhost:27017
- Check network connectivity for cloud MongoDB
- Verify connection string in `appsettings.json`

#### Build Errors
- Restore NuGet packages
- Clean and rebuild solution
- Check .NET 8.0 SDK installation

#### Runtime Errors
- Check MongoDB connection
- Verify user permissions
- Review application logs

### Logs and Debugging
- Application logs are written to console
- MongoDB queries can be monitored in MongoDB Compass
- Use Visual Studio debugger for step-through debugging

## Project Documentation

### Additional Files
- `Project Proposal 106.docx` - Complete project proposal
- `presentation_preparation1.txt` - Presentation notes
- `DatabaseSeeder.cs` - Database initialization and seeding
- `EMS.csproj` - Project configuration and dependencies

### GitHub Repository
- **Repository**: https://github.com/kingze1l/EMS.git
- **Issues**: Tracked via GitHub Issues
- **Commits**: Regular commits with clear messages

## Team Information

### Development Team
- **Santosh Adhikari** - Scrum Master
- **Samiullah** - Developer/Team Member

### Methodology
- **Agile/Scrum** framework
- **3-week sprint** structure
- **13 user stories** implemented
- **Daily standups** via Microsoft Teams
- **GitHub** for version control
- **Trello** for task management

## Contact Information

For technical support or questions:
- **GitHub Issues**: https://github.com/kingze1l/EMS/issues
- **Team Communication**: Microsoft Teams

---

**Note**: This application is designed for educational purposes and demonstrates professional software development practices. All user data is for testing purposes only. 