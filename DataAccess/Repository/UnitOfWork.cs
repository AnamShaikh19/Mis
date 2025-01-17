using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public ICategoryRepository category {get; private set;}
        public ICompanyRepository Company { get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IProductRepository Product {get; private set;}
        public IOrderHeaderRepository OrderHeader { get; private set; }
        public IOrderDetailRepository OrderDetail { get; private set; }


        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;

            category = new CategoryRepository(_context);
            ShoppingCart = new ShoppingCartRepository(_context);
            Product = new ProductRepository(_context);
            Company = new CompanyRepository(_context);
            ApplicationUser = new ApplicationUserRepository(_context);
            OrderHeader = new OrderHeaderRepository(_context);
            OrderDetail = new OrderDetailRepository(_context);




        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}