using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using EMS.Models;

namespace EMS
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var settings = configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
            var client = new MongoClient(settings.ConnectionString);
            _database = client.GetDatabase(settings.DatabaseName);
        }

        public IMongoCollection<Employee> Employees => _database.GetCollection<Employee>("Employees");
        public IMongoCollection<Notification> Notifications => _database.GetCollection<Notification>("Notifications");
    }
}