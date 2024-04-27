using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderHeaderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }


        public void Update(OrderHeader obj)
        {
            _context.OrderHeaders.Update(obj);
        }

        public void UpdateStatus(int id, string OrderStatus, string? PaymentStatus = null)
        {
            var orderFromDb= _context.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if(orderFromDb != null)
            {
                orderFromDb.OrderStatus = OrderStatus;
                if (!string.IsNullOrEmpty(PaymentStatus))
                {
                    orderFromDb.PaymentStatus = PaymentStatus;
                }
                {

                }

            }
        }

        public void UpdateStripePaymentID(int id, string SessionId, string PaymentIntentId)
        {
            var orderFromDb = _context.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if (!string.IsNullOrEmpty(SessionId))
            {
                orderFromDb.SessionId = SessionId;
            }
            if(!string.IsNullOrEmpty(PaymentIntentId)) 
            {
                orderFromDb.PaymentIntentId = PaymentIntentId;
                orderFromDb.PaymentDate = DateTime.Now;
            }
               
            }
    }
}