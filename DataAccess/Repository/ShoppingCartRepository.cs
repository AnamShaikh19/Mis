using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext _context;
        public ShoppingCartRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(ShoppingCart obj)
        {
            _context.ShoppingCarts.Update(obj);
        }
    }
}