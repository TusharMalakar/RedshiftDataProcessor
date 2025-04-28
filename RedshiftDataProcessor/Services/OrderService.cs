using Microsoft.Extensions.Logging;
using System.Data.Odbc;
using System.Text.Json;

namespace RedshiftDataProcessor.Services
{
    public class OrderService
    {
        private readonly ILogger<OrderService> _logger;
        private readonly AppConfiguration _appConfig;

        public OrderService(ILogger<OrderService> logger, AppConfiguration appConfig)
        {
            _logger = logger;
            _appConfig = appConfig;
        }

        public async Task GetOrderAsync()
        {
            try
            {
                var query = "Select top 1 * from order;";
                var builder = new OdbcConnectionStringBuilder
                {
                    Driver = _appConfig.RedShiftDriver
                };
                builder.Add("HostName", _appConfig.RedShiftHost);
                builder.Add("PortNumber", _appConfig.RedShiftPort);
                builder.Add("Database", _appConfig.RedShiftDatabase);
                builder.Add("UID", _appConfig.RedShiftUserName);
                builder.Add("PWD", _appConfig.RedShiftPassword);

                using (var connection = new OdbcConnection(builder.ConnectionString))
                {
                    var command = new OdbcCommand(query, connection);
                    connection.Open();
                    Console.WriteLine("Redshift Connetion was opened successfully.");

                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine(JsonSerializer.Serialize(reader));
                            Console.WriteLine("\n");
                        }
                    }
                    reader.Close();
                    command.Dispose();
                    await Task.FromResult(Task.CompletedTask);
                }
            }
            catch (Exception ex) { 
                _logger.LogError("{method}:Error to fetch order: {err}", nameof(GetOrderAsync), ex.StackTrace);
            }  
        }
    }
}
