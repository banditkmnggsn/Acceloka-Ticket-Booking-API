namespace Acceloka.Api.Features.Users.Dtos
{
    public class OrderHistoryItemDto
    {
        public Guid Id { get; set; }
        public Guid BookedTicketId { get; set; }
        public string TicketCode { get; set; } = string.Empty;
        public string TicketName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime BookedAt { get; set; }
    }

    public class PaginationMeta
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    public class OrderHistoryResponse
    {
        public List<OrderHistoryItemDto> Items { get; set; } = new();
        public PaginationMeta Meta { get; set; } = new();
    }
}
