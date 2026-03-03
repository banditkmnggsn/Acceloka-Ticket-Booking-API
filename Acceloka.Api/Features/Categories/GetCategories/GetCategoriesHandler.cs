using MediatR;
using Microsoft.EntityFrameworkCore;
using Acceloka.Api.Infrastructure.Persistence;

namespace Acceloka.Api.Features.Categories.GetCategories
{
    public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, List<CategoryItemResponse>>
    {
        private readonly AccelokaDbContext _context;

        public GetCategoriesHandler(AccelokaDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryItemResponse>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new CategoryItemResponse
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync(cancellationToken);
        }
    }
}
