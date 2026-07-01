namespace Api.Tests.AppHost;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        builder.AddSqlServer("bdserver").AddDatabase("bd");

        builder.Build().Run();
    }
}
