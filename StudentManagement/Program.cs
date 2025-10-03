
using StudentManagement;
using Microsoft.EntityFrameworkCore;

try
{
    // Use factory to build the context
    var factory = new SchoolContextFactory();
    using var context = factory.CreateDbContext(args);

    context.Database.Migrate();

    Console.WriteLine("✅ Database is ready with tables!");
}
catch (Exception ex)
{
    Console.WriteLine("❌ Error while setting up the database:");
    Console.WriteLine(ex.Message);
}
