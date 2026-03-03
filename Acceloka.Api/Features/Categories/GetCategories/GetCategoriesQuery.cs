using MediatR;

namespace Acceloka.Api.Features.Categories.GetCategories
{
    public class CategoryItemResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class GetCategoriesQuery : IRequest<List<CategoryItemResponse>>
    {
    }
}
