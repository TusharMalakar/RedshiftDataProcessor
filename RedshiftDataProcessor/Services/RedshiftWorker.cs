using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RedshiftDataProcessor.Services
{
    public class RedshiftWorker : BackgroundService
    {
        private readonly IHostEnvironment _env;
        private readonly ILogger<RedshiftWorker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public RedshiftWorker(IHostEnvironment env, ILogger<RedshiftWorker> logger, IServiceProvider serviceProvider)
        {
            _env = env;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("**{method}: Starting RedShift Data Procesor for Environment {env}***", nameof(StartAsync), _env.EnvironmentName);
            _logger.LogInformation("**{source}: .Net Version {version}***", typeof(RedshiftWorker).Namespace, Environment.Version);
            await base.StartAsync(cancellationToken);   
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("**{method}: Stopping RedShift Data Procesor for Environment {env}***", nameof(StopAsync), _env.EnvironmentName);
            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            int.TryParse(Environment.GetEnvironmentVariable("Processor_Wait_Period_Seconds"), out var waitPeriodSeconds);
            if(waitPeriodSeconds == 0)
            {
                waitPeriodSeconds = 15;
            }

            while (!cancellationToken.IsCancellationRequested) {
                _logger.LogInformation("**BEGIN Processing CYCLE***");
                var loopStartTime = DateTime.Now;

                using (var scope = _serviceProvider.CreateScope())
                {
                    var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();
                    await orderService.GetOrderAsync();
                }

                var totalLoopTime = DateTime.Now - loopStartTime;
                _logger.LogInformation("**END Processing CYCLE | Total Time: {m:D2}:{s:D2} minutes***", totalLoopTime.Minutes, totalLoopTime.Seconds);
                _logger.LogInformation("**BEGINING {s} second wait period***", waitPeriodSeconds);

                await Task.Delay(waitPeriodSeconds * 1000, cancellationToken);
            }
        }
    }
}
