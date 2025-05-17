using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSalesSystem.Application.Features.Sales.Queries
{
    public class GetAllSalesQuery:IRequest<IEnumerable<SaleDto>>
    {
    }
}
