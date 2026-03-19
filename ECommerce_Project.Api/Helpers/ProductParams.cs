namespace ECommerce_Project.Api.Helpers
{
    public class ProductParams
    {
        public string? Search { get; set; }
        public Guid? CategoryId { get; set; }

        // Пагінація
        private const int MaxPageSize = 50;
        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10; // За замовчуванням 10 товарів на сторінку
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }
    }
}
