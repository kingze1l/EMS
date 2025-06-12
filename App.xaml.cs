using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using EMS.Services;
using EMS.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using EMS.Views;

namespace EMS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;
        public IServiceProvider ServiceProvider => _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Configure MongoDB
            services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));

            // Register MongoDB services
            services.AddSingleton<MongoDbContext>();
            services.AddSingleton<IMongoDatabase>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                var client = new MongoClient(settings.ConnectionString);
                return client.GetDatabase(settings.DatabaseName);
            });

            // Register application services
            services.AddSingleton<IEmployeeService, EmployeeService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<AuthenticationService>();
            services.AddSingleton<IRoleService, RoleService>();
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<IAuditLogService, AuditLogService>();
            services.AddSingleton<ILeaveService, LeaveService>();
            services.AddSingleton<IPayrollService, PayrollService>();
            services.AddTransient<PayrollView>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<LoginWindow>();
            services.AddSingleton<DashboardView>();
            services.AddSingleton<EmployeeView>();
            services.AddTransient<SettingsView>();
            services.AddSingleton<RoleManagementView>();
            services.AddSingleton<NotificationView>();
            services.AddSingleton<AuditLogView>();
            services.AddTransient<LeaveRequestView>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize the database seeder
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
                await dbContext.InitializeAsync();
            }

            var loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }
}
