using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSalesSystem.Application.Features.Customers.Queries
{
    public class GetAllFilterCustomersQuery:IRequest<PagedResult<CustomerDto>>
    {
        public List<AIFieldFilter> Filters { get; set; } = new();
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
