using Application.DTO;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Common;
using Domain.Common.Query;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        public async Task<FeatureGroupDto> CreateOrUpdateAsync(FeatureGroupDto dto)
        {
            var result = await _productRepository.CreateOrUpdateAsync(dto);
            return result;
        }

        public async Task<ProductReadDto> CreateOrUpdateAsync(ProductDto dto, Guid featureGroupId)
        {
            var result = await _productRepository.CreateOrUpdate(dto, featureGroupId);
            return result;
        }

        public async Task Delete(IEnumerable<Guid> ids)
        {
            await _productRepository.Delete(ids);
        }

        public async Task<ProductReadDto> Get(Guid id)
        {
            var product = await _productRepository.Get(id);
            return product;
        }

        public async Task<PagedCollection<ProductReadDto>> GetAll(ProductParams query)
        {
            var products = await _productRepository.GetAll(query);
            return products;
        }

        public Task<FeatureGroupDto> CreateAsync(FeatureGroupCreateDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesWithSubcategories()
        {
            var dtos = await _productRepository.GetCategoriesWithSubcategories();
            return dtos;
        }

        public async Task<IEnumerable<FeatureGroupDto>> GetFeatureGroupsWithFeatures()
        {
            var dtos = await _productRepository.GetFeatureGroupsWithFeatures();
            return dtos;
        }

        public async Task<IEnumerable<FeatureValueReadDto>> CreateOrUpdate(IEnumerable<FeatureValueUpsertDto> items, Guid productId)
        {
            var dtos = await _productRepository.CreateOrUpdate(items, productId);
            return dtos;
        }

        public async Task<bool> UserWantsToChangeProductFeatures(Guid productId, Guid featureGroupId)
        {
            var result = await _productRepository.UserWantsToChangeProductFeatures(productId, featureGroupId);
            return result;
        }

        public async Task<List<ProductReadDto>> GetRecommendations(List<Guid> ids)
        {
            var result = await _productRepository.ApplyPerceptron(ids);
            return result;
        }
    }
}
