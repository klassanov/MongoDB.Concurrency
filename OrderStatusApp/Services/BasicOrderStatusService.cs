using MongoDB.Driver;
using OrderStatusApp.DbModels;
using OrderStatusApp.Interfaces;

namespace OrderStatusApp.Services
{
    public class BasicOrderStatusService : IOrderStatusService
    {
        private readonly IMongoClient client;

        public BasicOrderStatusService(IMongoClient client)
        {
            this.client = client;
        }

        public async Task<IEnumerable<Order>> GetOrdersInProgress()
        {
            var db = client.GetDatabase("orders-datastore");
            var ordersCollection = db.GetCollection<Order>("orders");
            var filterBuilder = Builders<Order>.Filter;
            var ordersInProgressFilter = filterBuilder.Eq(x => x.IsProcessing, false) & filterBuilder.Eq(x => x.Status, "InProgress");
            var orders = await ordersCollection.FindAsync(ordersInProgressFilter);
            List<Order> ordersInProgress = [];

            await orders.ForEachAsync(async order =>
            {
                order.IsProcessing = true;
                
                //Update succeeded
                if (await UpdateOrder(order, ordersCollection))
                {
                    ordersInProgress.Add(order);
                }
                else
                {
                    Console.WriteLine($"Concurrency conflict during updating order {order.Id}");
                }
            });

            return ordersInProgress;
        }

        private async Task<bool> UpdateOrder(Order order, IMongoCollection<Order> ordersCollection)
        {
            //UpdateFilter based on entity id and the original entity version needed for the concurrency check
            var updateFilter = Builders<Order>.Filter.And(
               Builders<Order>.Filter.Eq(o => o.Id, order.Id),
               Builders<Order>.Filter.Eq(o => o.Version, order.Version)
           );

            //UpdateBuilder: update IsProcessing flag and increment the version by 1
            var updateBuilder = Builders<Order>.Update
                .Set(o => o.IsProcessing, order.IsProcessing)
                .Inc(o => o.Version, 1);

            //Execute the update
            var result = await ordersCollection.UpdateOneAsync(updateFilter, updateBuilder);

            //Concurrency check: f the update has not succeeded due to version mismatch, return false
            return result.ModifiedCount == 1;
        }
    }
}
