using System.Threading.Tasks;
using EMS;

class Program
{
    static async Task Main()
    {
        var context = MongoDbContext.CreateFromConfig();
        await context.InitializeAsync();
        System.Console.WriteLine("Database seeded. You can now log in with the default admin user.");
    }
}