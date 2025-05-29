using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using EMS.Models;
using System;

namespace EMS
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly DatabaseSeeder _seeder;

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            if (settings.Value == null)
            {
                throw new ArgumentNullException(nameof(settings.Value), "MongoDbSettings are not configured.");
            }
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
            _seeder = new DatabaseSeeder(_database);
        }

        public IMongoCollection<Employee> Employees => _database.GetCollection<Employee>("Employees");
        public IMongoCollection<Notification> Notifications => _database.GetCollection<Notification>("Notifications");

        public async Task InitializeAsync()
        {
            await _seeder.SeedDataAsync();
        }

        public static MongoDbContext CreateFromConfig()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(System.AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            var settings = config.GetSection("MongoDbSettings").Get<MongoDbSettings>();
            
            if (settings == null)
            {
                 throw new InvalidOperationException("MongoDbSettings section not found in appsettings.json.");
            }

            var options = Options.Create(settings);
            return new MongoDbContext(options);
        }
    }
}