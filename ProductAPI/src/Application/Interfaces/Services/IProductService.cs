using Application.DTO;
using Domain.Common;
using Domain.Common.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<ProductReadDto> Get(Guid id);
        Task<PagedCollection<ProductReadDto>> GetAll(ProductParams query);
        Task<ProductReadDto> CreateOrUpdateAsync(ProductDto dto, Guid featureGroupId);
        Task Delete(IEnumerable<Guid> ids);
        Task<FeatureGroupDto> CreateOrUpdateAsync(FeatureGroupDto dto);
        Task<FeatureGroupDto> CreateAsync(FeatureGroupCreateDto dto);
        Task<IEnumerable<CategoryDto>> GetCategoriesWithSubcategories();
        Task<IEnumerable<FeatureGroupDto>> GetFeatureGroupsWithFeatures();
        Task<IEnumerable<FeatureValueReadDto>> CreateOrUpdate(IEnumerable<FeatureValueUpsertDto> items, Guid productId);
        Task<bool> UserWantsToChangeProductFeatures(Guid productId, Guid featureGroupId);
        Task<List<ProductReadDto>> GetRecommendations(List<Guid> ids);
    }
}
