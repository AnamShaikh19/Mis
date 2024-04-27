using Models;

namespace DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);
        void UpdateStatus(int id, string OrderStatus,  string? PaymentStatus = null);
        void UpdateStripePaymentID(int id, string SessionId, string PaymentIntentId);



    }
}