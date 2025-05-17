using CustomerSalesSystem.Application.Features.Customers.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSalesSystem.Application.Features.Sales.Commands
{
    public class UpdateSaleCommandHandler :IRequestHandler<UpdateSaleCommand, bool>
    {
        private readonly ISalesRepository _repository;

    public UpdateSaleCommandHandler(ISalesRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await _repository.GetByIdAsync(request.Id);
        if (sale == null)
        {
            return false;
        }
        sale.CustomerId = request.CustomerId;
        sale.ProductId = request.ProductId;
        sale.SaleDate = request.SaleDate;
        sale.Quantity = request.Quantity;
        sale.TotalPrice = request.TotalPrice;

        await _repository.UpdateAsync(sale);
        return true;
    }
}

}
