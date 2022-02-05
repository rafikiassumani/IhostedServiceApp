using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder =  Host.CreateDefaultBuilder();
builder.ConfigureServices((hostContext, services) => {
    services.AddHostedService<BackgroundConsoleHostedService>();
    services.AddSingleton<FibNumberService>();
});

await builder.StartAsync();

public class BackgroundConsoleHostedService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _appLifeTime;
    private readonly FibNumberService _fibNumberService;

    public BackgroundConsoleHostedService(
             ILogger<BackgroundConsoleHostedService> logger, 
            IHostApplicationLifetime applicationLifetime,
            FibNumberService fibNumberService) 
    {

        _logger = logger;
        _appLifeTime = applicationLifetime;
        _fibNumberService = fibNumberService;
     }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _appLifeTime.ApplicationStarted.Register(async () => {
             _logger.LogInformation("Console Application Started");
            try {
                await _fibNumberService.FindFibSequence(1000);
            } catch(Exception e) {
               _logger.LogError(e, "Something bad happened");
            } finally {
                _appLifeTime.StopApplication();
            }     
        });
  
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _appLifeTime.ApplicationStopped.Register(() => {
              _logger.LogInformation("App shutting down");
        });

        return Task.CompletedTask;
    }
}

public class FibNumberService {
   private readonly ILogger _logger;
   public FibNumberService(ILogger<FibNumberService> logger) 
   {
       _logger = logger;
   }
    public Task FindFibSequence(int N) {
        var n1 = new BigInteger(0); 
        var n2 = new BigInteger(1);
        int count = 0;

        try {
                using var fs = new FileStream("fib.txt", FileMode.OpenOrCreate);
                using var sw = new StreamWriter(fs);
                sw.WriteLine(n1);
                sw.WriteLine(n2);
                while (count < N) {
                    BigInteger n3 = n1 + n2;
                    n1 = n2;
                    n2 = n3;
                    sw.WriteLine(n3);
                    count++; 
                }
                sw.Close();

        } catch (Exception ex) {
            _logger.LogError(ex, "Something bad happened");
        }
         return Task.CompletedTask;
       }
}