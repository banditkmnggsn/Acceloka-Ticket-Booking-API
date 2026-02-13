using MediatR;
using Acceloka.Api.Features.Common.Dtos;

namespace Acceloka.Api.Features.GetAvailableTicket
{
    public class GetAvailableTicketQuery : IRequest<List<AvailableTicketResponse>>
    {
        public string? NamaKategori { get; set; }
        public string? KodeTiket { get; set; }
        public string? NamaTiket { get; set; }
        public decimal? HargaMaksimal { get; set; }
        public DateTime? TanggalEventMinimal { get; set; }
        public DateTime? TanggalEventMaksimal { get; set; }
        public string OrderBy { get; set; } = "KodeTiket";
        public string OrderState { get; set; } = "asc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public int GetSkip() => (Page - 1) * PageSize;
    }
}
