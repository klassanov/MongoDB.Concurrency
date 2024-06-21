using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using OrderStatusApp.Interfaces;
using OrderStatusApp.Services;

namespace OrderStatusApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                              .ConfigureServices((context, services) =>
                                {
                                    var mongoConnectionString = context.Configuration["MongoDBSettings:AtlasURI"];
                                    services.AddTransient<IOrderStatusService, BasicOrderStatusService>();
                                    services.AddSingleton<IMongoClient, MongoClient>(serviceProvider =>
                                    {
                                        return new MongoClient(mongoConnectionString);
                                    });
                                });

            var host = builder.Build();          
            await Start(host);
        }

        static async Task Start(IHost host)
        {
            var orderStatusService = host.Services.GetService<IOrderStatusService>();
            var orders = await orderStatusService.GetOrdersInProgress();
            //Publish message in the ASB for each order in orders
            Console.WriteLine($"Orders in Progress Count: {orders.Count()}");
        }
    }
}
