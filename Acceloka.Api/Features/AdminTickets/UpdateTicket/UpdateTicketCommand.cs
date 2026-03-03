using MediatR;
using Acceloka.Api.Features.AdminTickets.CreateTicket;

namespace Acceloka.Api.Features.AdminTickets.UpdateTicket
{
    public class UpdateTicketRequest
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

    public class UpdateTicketCommand : IRequest<AdminTicketResponse>
    {
        public string CurrentTicketCode { get; set; } = string.Empty;
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
