using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;

namespace GrpcGreeter
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;

        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"Received request from {request.Name}");
            return Task.FromResult(new HelloReply
            {
                Message = $"Hello, {request.Name}!"
            });
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var host = "localhost";
            var port = 50051;

            var server = new Server
            {
                Services = { Greeter.BindService(new GreeterService(new LoggerFactory().CreateLogger<GreeterService>())) },
                Ports = { new ServerPort(host, port, ServerCredentials.Insecure) }
            };
            server.Start();

            System.Console.WriteLine($"Server listening on {host}:{port}");
            System.Console.WriteLine("Press any key to stop the server...");
            System.Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}