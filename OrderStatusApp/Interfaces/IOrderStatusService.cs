using OrderStatusApp.DbModels;

namespace OrderStatusApp.Interfaces
{
    public interface IOrderStatusService
    {
        Task<IEnumerable<Order>> GetOrdersInProgress();
    }
}
