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
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public DateTime TanggalEvent { get; set; }
        public decimal Harga { get; set; }
        public int SisaQuota { get; set; }
    }

    public class BookedTicketDetailResponse
    {
        public string KodeTiket { get; set; } = string.Empty;
        public string NamaTiket { get; set; } = string.Empty;
        public DateTime TanggalEvent { get; set; }
        public int Quantity { get; set; }
    }

    // NEW: Untuk GET /get-booked-ticket/{id} — grouped by kategori
    public class BookedTicketItemDto
    {
        public string KodeTiket { get; set; } = string.Empty;
        public string NamaTiket { get; set; } = string.Empty;
        public DateTime TanggalEvent { get; set; }
        public int Quantity { get; set; }
    }

    public class BookedTicketCategoryGroupDto
    {
        public string NamaKategori { get; set; } = string.Empty;
        public List<BookedTicketItemDto> Items { get; set; } = new();
    }

    public class GetBookedTicketGroupedResponse
    {
        public Guid BookedTicketId { get; set; }
        public List<BookedTicketCategoryGroupDto> Categories { get; set; } = new();
    }

    // UPDATED: Untuk POST /book-ticket — dengan items list
    public class BookedTicketItemDetailDto
    {
        public string KodeTiket { get; set; } = string.Empty;
        public string NamaTiket { get; set; } = string.Empty;
        public string Kategori { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Harga { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class BookedTicketResponse
    {
        public Guid BookedTicketId { get; set; }
        public List<BookedTicketItemDetailDto> Items { get; set; } = new();
        public Dictionary<string, decimal> SubtotalPerKategori { get; set; } = new();
        public decimal GrandTotal { get; set; }
    }

    // LEGACY: Jika masih digunakan di tempat lain (untuk backward compatibility)
    public class BookedTicketResponseLegacy
    {
        public Guid BookedTicketId { get; set; }
        public string NamaTiket { get; set; } = string.Empty;
        public string KodeTiket { get; set; } = string.Empty;
        public decimal Harga { get; set; }
        public Dictionary<string, decimal> TotalPerKategori { get; set; } = new();
        public decimal TotalSemua { get; set; }
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
        public string? ImageUrl { get; set; }
        public int SisaQuantity { get; set; }
    }

    public class PaginationParams
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public int GetSkip() => (Page - 1) * PageSize;
    }
}
