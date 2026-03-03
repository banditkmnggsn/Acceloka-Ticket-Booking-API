using MediatR;

namespace Acceloka.Api.Features.AdminAnalytics.GetTicketSalesAnalytics
{
    public class SalesSummaryDto
    {
        public int TotalOrders { get; set; }
        public int TotalTicketsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int UniqueBuyers { get; set; }
    }

    public class SalesTrendPointDto
    {
        public DateTime PeriodStart { get; set; }
        public int Orders { get; set; }
        public int TicketsSold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopSellingTicketDto
    {
        public Guid TicketId { get; set; }
        public string TicketCode { get; set; } = string.Empty;
        public string TicketName { get; set; } = string.Empty;
        public int SoldQuantity { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TicketSalesAnalyticsResponse
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string GroupBy { get; set; } = "day";
        public SalesSummaryDto Summary { get; set; } = new();
        public List<SalesTrendPointDto> Trend { get; set; } = new();
        public List<TopSellingTicketDto> TopTickets { get; set; } = new();
    }

    public class GetTicketSalesAnalyticsQuery : IRequest<TicketSalesAnalyticsResponse>
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string GroupBy { get; set; } = "day";
        public int Top { get; set; } = 10;
    }
}
