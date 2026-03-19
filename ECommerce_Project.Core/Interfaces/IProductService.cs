using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce_Project.Api.DTOs.Product;

namespace ECommerce_Project.Core.Interfaces
{
    internal interface IProductService
    {
        Task<IEnumerable<ProductSummaryDto>> GetProductsAsync();
    }
}
