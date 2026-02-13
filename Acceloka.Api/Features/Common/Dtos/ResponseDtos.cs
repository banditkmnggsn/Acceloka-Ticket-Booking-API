namespace Acceloka.Api.Features.Common.Dtos
{
    public class TicketItemRequest
    {
        public string KodeTiket { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class AvailableTicketResponse
    {
        public string NamaKategori { get; set; } = string.Empty;
        public string KodeTiket { get; set; } = string.Empty;
        public string NamaTiket { get; set; } = string.Empty;
        public DateTime TanggalEvent { get; set; }
        public decimal Harga { get; set; }
        public int SisaQuota { get; set; }
    }

    public class BookedTicketResponse
    {
        public string NamaTiket { get; set; } = string.Empty;
        public string KodeTiket { get; set; } = string.Empty;
        public decimal Harga { get; set; }
        public Dictionary<string, decimal> TotalPerKategori { get; set; } = new();
        public decimal TotalSemua { get; set; }
    }

    public class BookedTicketDetailResponse
    {
        public string KodeTiket { get; set; } = string.Empty;
        public string NamaTiket { get; set; } = string.Empty;
        public DateTime TanggalEvent { get; set; }
        public int Quantity { get; set; }
    }

    public class RevokeTicketResponse
    {
        public string KodeTiket { get; set; } = string.Empty;
        public string NamaTiket { get; set; } = string.Empty;
        public string NamaKategori { get; set; } = string.Empty;
        public int SisaQuantity { get; set; }
    }

    public class EditBookedTicketResponse
    {
        public string KodeTiket { get; set; } = string.Empty;
        public string NamaTiket { get; set; } = string.Empty;
        public string NamaKategori { get; set; } = string.Empty;
        public int SisaQuantity { get; set; }
    }

    public class PaginationParams
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public int GetSkip() => (Page - 1) * PageSize;
    }
}
