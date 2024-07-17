using Application.DTO;
using Domain.Common;
using Domain.Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<ProductReadDto> Get(Guid id);
        Task<ProductReadDto> CreateOrUpdate(ProductDto dto, Guid featureGroupId);
        Task<FeatureGroupDto> CreateOrUpdateAsync(FeatureGroupDto dto);
        Task<PagedCollection<ProductReadDto>> GetAll(ProductParams query);
        Task Delete(IEnumerable<Guid> ids);
        Task<IEnumerable<CategoryDto>> GetCategoriesWithSubcategories();
        Task<IEnumerable<FeatureGroupDto>> GetFeatureGroupsWithFeatures();
        Task<IEnumerable<FeatureValueReadDto>> CreateOrUpdate(IEnumerable<FeatureValueUpsertDto> items, Guid productId);
        Task<bool> UserWantsToChangeProductFeatures(Guid productId, Guid featureGroupId);
        Task<List<ProductReadDto>> GetRecommendations(List<Guid> products);

        Task<List<ProductReadDto>> ApplyPerceptron(List<Guid> ids);

        //Task ClearUnlinkedFilesAsync();
    }
}
