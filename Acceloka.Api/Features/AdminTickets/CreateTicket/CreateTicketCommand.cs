using MediatR;

namespace Acceloka.Api.Features.AdminTickets.CreateTicket
{
    public class CreateTicketRequest
    {
        public string TicketCode { get; set; } = string.Empty;
        public string TicketName { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public DateTime EventDate { get; set; }
        public decimal Price { get; set; }
        public int Quota { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
    }

    public class AdminTicketResponse
    {
        public Guid Id { get; set; }
        public string TicketCode { get; set; } = string.Empty;
        public string TicketName { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public decimal Price { get; set; }
        public int Quota { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
    }

    public class CreateTicketCommand : IRequest<AdminTicketResponse>
    {
        public string TicketCode { get; set; } = string.Empty;
        public string TicketName { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public DateTime EventDate { get; set; }
        public decimal Price { get; set; }
        public int Quota { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
    }
}
