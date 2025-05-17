namespace CustomerSalesSystem.Infrastructure.Repositories
{
    public class SaleRepository : ISalesRepository
    {
        private readonly AppDbContext _context;
        public SaleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Sale sale)
        {
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Sale sale)
        {
            _context.Sales.Update(sale);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale != null)
            {
                _context.Sales.Remove(sale);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Sale?> GetByIdAsync(int id)
        {
            return await _context.Sales
           .Include(s => s.Customer)
           .Include(s => s.Product)
           .FirstOrDefaultAsync(s => s.Id == id);
        }

    }
}
