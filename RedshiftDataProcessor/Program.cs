using Serilog;
using dotenv.net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using RedshiftDataProcessor.Services;
using RedshiftDataProcessor;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        DotEnv.Load(options: new DotEnvOptions(probeForEnv: true, probeLevelsToSearch: 7));
        builder.Configuration.AddEnvironmentVariables();

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message} \n")
            .CreateLogger();
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(dispose: true);

        builder.Services.AddHostedService<RedshiftWorker>();
        builder.Services.AddScoped(x => new AppConfiguration
        {
            RedShiftDriver = GetRequiredVariable("redshift_driver"),
            RedShiftHost = GetRequiredVariable("redshift_host"),
            RedShiftPort = GetRequiredVariable("redshift_port"),
            RedShiftDatabase = GetRequiredVariable("redshift_database"),
            RedShiftUserName = GetRequiredVariable("redshift_user"),
            RedShiftPassword = GetRequiredVariable("redshift_secret")
        });
        builder.Services.AddScoped<OrderService>();
        builder.Services.AddHealthChecks();

        var host = builder.Build();
        host.MapHealthChecks("/health");
        host.Run();
    }

    static string GetRequiredVariable(string variableName)
    {
        var value = Environment.GetEnvironmentVariable(variableName);
        return string.IsNullOrWhiteSpace(value) ? throw new Exception($"Missing env variable {variableName}") : value;
    }
}