using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Data;
using Application.DTO;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop.Infrastructure;
using System.Net;
using System.Collections;
using Domain.Common.Query;
using Domain.Common;

namespace Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        private IMapper _mapper;

        public ProductRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ProductReadDto> CreateOrUpdate(ProductDto dto, Guid featureGroupId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == dto.Id);
            ProductReadDto result = null;

            if (product == null)
            {
                // create
                var productToCreate = new Product()
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Category = dto.Category,
                    Subcategory = dto.Subcategory,
                    Description = dto.Description,
                    Price = dto.Price,
                    Stock = dto.Stock,
                    FeatureGroupId = featureGroupId
                };

                await _context.AddAsync(productToCreate);
                result = _mapper.Map<ProductReadDto>(productToCreate);
            }
            else
            {
                // update
                product.Name = dto.Name;
                product.Category = dto.Category;
                product.Subcategory = dto.Subcategory;
                product.Description = dto.Description;
                product.Price = dto.Price;
                product.Stock = dto.Stock;

                _context.Update(product);
                result = _mapper.Map<ProductReadDto>(product);
            }

            await _context.SaveChangesAsync();
            return result;
        }

        public async Task Delete(IEnumerable<Guid> ids)
        {
            var products = await _context.Products
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();

            if (products == null)
            {
                throw new Domain.Exceptions.NotFoundException("The products with the given ids not found.");
            }

            _context.RemoveRange(products);
            await _context.SaveChangesAsync();
        }

        public async Task<ProductReadDto> Get(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.FeatureGroup)
                .ThenInclude(g => g.Features)
                .ThenInclude(v => v.FeatureValues)
                .FirstOrDefaultAsync(x => x.Id == id);

            var dto = _mapper.Map<ProductReadDto>(product);
            return dto;
        }

        public async Task<PagedCollection<ProductReadDto>> GetAll(ProductParams query)
        {
            var dos = await _context.Products
                .Include(p => p.FeatureGroup)
                .ThenInclude(g => g.Features)
                .ThenInclude(v => v.FeatureValues)
                .ToListAsync();

            if (string.Equals(query.SortBy, "name", StringComparison.OrdinalIgnoreCase))
            {
                dos = query.SortType == Domain.Constants.SortingType.Ascending ?
                    dos.OrderBy(x => x.Name).ToList()
                    : dos.OrderByDescending(x => x.Name).ToList();
            }
            else if (string.Equals(query.SortBy, "category", StringComparison.OrdinalIgnoreCase))
            {
                dos = query.SortType == Domain.Constants.SortingType.Ascending ?
                    dos.OrderBy(x => x.Category).ToList()
                    : dos.OrderByDescending(x => x.Category).ToList();
            }
            else if (string.Equals(query.SortBy, "price", StringComparison.OrdinalIgnoreCase))
            {
                dos = query.SortType == Domain.Constants.SortingType.Ascending ?
                    dos.OrderBy(x => x.Price).ToList()
                    : dos.OrderByDescending(x => x.Price).ToList();
            }
            else if (string.Equals(query.SortBy, "stock", StringComparison.OrdinalIgnoreCase))
            {
                dos = query.SortType == Domain.Constants.SortingType.Ascending ?
                    dos.OrderBy(x => x.Stock).ToList()
                    : dos.OrderByDescending(x => x.Stock).ToList();
            }
            else
            {
                dos = query.SortType == Domain.Constants.SortingType.Ascending ?
                    dos.OrderBy(x => x.Id).ToList()
                    : dos.OrderByDescending(x => x.Id).ToList();
            }

            if (!query.Name.IsNullOrEmpty())
            {
                dos = dos.Where(x => x.Name.Contains(query.Name)).ToList();
            }
            if (!query.Category.IsNullOrEmpty())
            {
                dos = dos.Where(x => x.Category.Contains(query.Category)).ToList();
            }
            if (!query.Subcategory.IsNullOrEmpty())
            {
                dos = dos.Where(x => x.Subcategory.Contains(query.Subcategory)).ToList();
            }
            if (query.InStock != null)
            {
                dos = dos.Where(x => query.InStock.Value ? x.Stock > 0 : x.Stock == 0).ToList();
            }

            var dtos = _mapper.Map<IEnumerable<ProductReadDto>>(dos);

            var result = DataHelper.ToPagedCollection(dtos, query.Page, query.Number);
            return result;
        }

        public async Task<FeatureGroupDto> CreateOrUpdateAsync(FeatureGroupDto dto)
        {
            FeatureGroupDto result = null;
            var groupDao = await _context.FeatureGroups.Include(x => x.Features).FirstOrDefaultAsync(x => x.Id == dto.Id);

            // create
            if (groupDao == null)
            {
                var group = new FeatureGroup()
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Features = new List<Feature>()
                };

                foreach (var feature in dto.Features)
                {
                    group.Features.Add(new Feature()
                    {
                        Id = Guid.NewGuid(),
                        FeatureGroupId = group.Id,
                        Name = feature.Name,
                        PossibleValues = ProductHelper.Convert<string>(feature.PossibleValues)
                    });
                }

                await _context.AddAsync(group);
                result = _mapper.Map<FeatureGroupDto>(group);
            }
            else
            {
                // update
                groupDao.Name = dto.Name;

                foreach (var featureDto in dto.Features)
                {
                    var feature = groupDao.Features.FirstOrDefault(x => x.Id == featureDto.Id);

                    if (feature == null)
                    {
                        var featureToCreate = new Feature();

                        featureToCreate.Id = Guid.NewGuid();
                        featureToCreate.Name = featureDto.Name;
                        featureToCreate.PossibleValues = ProductHelper.Convert<string>(featureDto.PossibleValues);
                        
                        await _context.AddAsync(featureToCreate);
                    }
                    else
                    {
                        feature.Name = featureDto.Name;
                        feature.PossibleValues = ProductHelper.Convert<string>(featureDto.PossibleValues);
                    }
                }

                _context.Update(groupDao);

                result = _mapper.Map<FeatureGroupDto>(groupDao);
            }

            await _context.SaveChangesAsync();

            return result;
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesWithSubcategories()
        {
            var categoriesWithSubcategories = await _context.Categories.Include(x => x.Subcategories).ToListAsync();
            var dtos = _mapper.Map<IEnumerable<CategoryDto>>(categoriesWithSubcategories);
            return dtos;
        }

        public async Task<IEnumerable<FeatureGroupDto>> GetFeatureGroupsWithFeatures()
        {
            var groupWithFeatures = await _context.FeatureGroups.Include(x => x.Features).ToListAsync();
            var dtos = _mapper.Map<IEnumerable<FeatureGroupDto>>(groupWithFeatures);
            return dtos;
        }

        public async Task<IEnumerable<FeatureValueReadDto>> CreateOrUpdate(IEnumerable<FeatureValueUpsertDto> items, Guid productId)
        {
            var features = await _context.FeaturesValues.ToListAsync();
            var upsertedFeatures = new List<FeatureValue>();

            foreach (var item in items)
            {
                var existingFeature = features.FirstOrDefault(f => f.FeatureId == item.FeatureId && f.ProductId == productId);

                if (existingFeature == null)
                {
                    // create
                    existingFeature = new FeatureValue();
                    existingFeature.Id = Guid.NewGuid();
                    existingFeature.Value = item.Value;
                    existingFeature.FeatureId = item.FeatureId;
                    existingFeature.ProductId = productId;
                    
                    await _context.AddAsync(existingFeature);
                }
                else
                {
                    // update
                    existingFeature.Value = item.Value;
                    _context.Update(existingFeature);
                }

                upsertedFeatures.Add(existingFeature);
            }
            await _context.SaveChangesAsync();

            var results = _mapper.Map<IEnumerable<FeatureValueReadDto>>(upsertedFeatures.AsEnumerable());
            return results;
        }

        public async Task<bool> UserWantsToChangeProductFeatures(Guid productId, Guid featureGroupId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == productId);

            return product.FeatureGroupId == featureGroupId;
        }

        public async Task<List<ProductReadDto>> ApplyPerceptron(List<Guid> ids)
        {
            List<(Guid, double)> highestRecommendations = new List<(Guid, double)>();

            var allProducts = await _context.Products
                .Include(p => p.FeatureGroup)
                .ThenInclude(g => g.Features)
                .ThenInclude(v => v.FeatureValues)
                .ToListAsync();

            var viewedProducts = allProducts.Where(x => ids.Contains(x.Id)).ToList();

            // Initialize Perceptron
            double threshold = 3;
            double learningRate = 0.9;
            var perceptron = new Perceptron(3, threshold, learningRate);

            // Prepare training data
            var trainingData = new List<(List<double> inputs, int label)>();

            foreach (var viewed in viewedProducts)
            {
                foreach (var item in allProducts)
                {
                    double priceIsSimilar = (viewed.Price - item.Price <= 100) ? 1 : 0;
                    double categoryIsSimilar = (viewed.Category == item.Category) ? 1 : 0;
                    double featureGroupIsSimilar = (viewed.FeatureGroup == item.FeatureGroup) ? 1 : 0;

                    List<double> inputs = new List<double> { priceIsSimilar, categoryIsSimilar, featureGroupIsSimilar };
                    int label = (viewed.Id == item.Id) ? 1 : 0; // Positive example if it's the same product

                    trainingData.Add((inputs, label));
                }
            }

            // Train the Perceptron
            perceptron.Train(trainingData);

            // Generate recommendations

            foreach (var viewed in viewedProducts)
            {
                foreach (var item in allProducts)
                {
                    double priceIsSimilar = (viewed.Price - item.Price <= 100) ? 1 : 0;
                    double categoryIsSimilar = (viewed.Category == item.Category) ? 1 : 0;
                    double featureGroupIsSimilar = (viewed.FeatureGroup == item.FeatureGroup) ? 1 : 0;

                    List<double> inputs = new List<double> { priceIsSimilar, categoryIsSimilar, featureGroupIsSimilar };
                    int prediction = perceptron.Predict(inputs);

                    if (prediction == 1 && !ids.Contains(item.Id))
                    {
                        double sum = inputs.Zip(perceptron.Weights, (input, weight) => input * weight).Sum();
                        highestRecommendations.Add((item.Id, sum));
                    }
                }
            }

            List<Guid> topRecommendations = highestRecommendations.OrderBy(x => x.Item2).Take(5).Select(x => x.Item1).ToList();

            var chosenProducts = allProducts.Where(x => topRecommendations.Contains(x.Id)).ToList();

            var dtos = _mapper.Map<IEnumerable<ProductReadDto>>(chosenProducts);

            return dtos.ToList();
        }

        public async Task<List<ProductReadDto>> GetRecommendations(List<Guid> ids)
        {
            List<(Guid, double)> highestRecommendations = new List<(Guid, double)>();

            var allProducts = await _context.Products
                .Include(p => p.FeatureGroup)
                .ThenInclude(g => g.Features)
                .ThenInclude(v => v.FeatureValues)
                .ToListAsync();

            var viewedProducts = allProducts.Where(x => ids.Contains(x.Id)).ToList();

            double threshold = 1.5;

            double priceIsSimilar;
            double categoryIsSimilar;
            double featureGroupIsSimilar;

            double weightForPrice = 0.7;
            double weightForCategory = 0.5;
            double weightForFeatureGroup = 0.8;

            List<double> weights = new List<double>()
            {
                weightForPrice,
                weightForCategory,
                weightForFeatureGroup
            };

            foreach (var viewed in viewedProducts)
            {
                foreach (var item in allProducts)
                {
                    priceIsSimilar = (viewed.Price - item.Price <= 100) ? 1 : 0;
                    categoryIsSimilar = (viewed.Category == item.Category) ? 1 : 0;
                    featureGroupIsSimilar = (viewed.FeatureGroup == item.FeatureGroup) ? 1 : 0;

                    List<double> inputs = new List<double>()
                    {
                        priceIsSimilar,
                        categoryIsSimilar,
                        featureGroupIsSimilar
                    };

                    double sum = 0;

                    for (int i = 0; i < inputs.Count; i++)
                    {
                        sum += inputs[i] * weights[i];
                    }

                    if (sum > threshold)
                    {
                        highestRecommendations.Add((item.Id, sum));
                    }
                }
            }

            highestRecommendations = highestRecommendations.Where(x => !ids.Contains(x.Item1)).ToList();

            List<Guid> topRecommendations = highestRecommendations.OrderBy(x => x.Item2).Take(5).Select(x => x.Item1).ToList();

            var chosenProducts = allProducts.Where(x => topRecommendations.Contains(x.Id)).ToList();

            var dtos = _mapper.Map<IEnumerable<ProductReadDto>>(chosenProducts);

            return dtos.ToList();
        }

        public class Perceptron
        {
            public List<double> Weights { get; private set; }
            public double Threshold { get; private set; }
            private double LearningRate;

            public Perceptron(int inputSize, double threshold, double learningRate)
            {
                Weights = new List<double>();
                Random rand = new Random();

                for (int i = 0; i < inputSize; i++)
                {
                    Weights.Add(rand.NextDouble());
                }

                Threshold = threshold;
                LearningRate = learningRate;
            }

            public int Predict(List<double> inputs)
            {
                double sum = 0;

                for (int i = 0; i < inputs.Count; i++)
                {
                    sum += inputs[i] * Weights[i];
                }

                return sum >= Threshold ? 1 : 0;
            }

            public void Train(List<(List<double> inputs, int label)> trainingData)
            {
                bool errorsPresent = true;

                int num = 0;
                while (errorsPresent)
                {
                    if (num > 15)
                    {
                        break;
                    }
                    num++;
                    errorsPresent = false;

                    foreach (var data in trainingData)
                    {
                        var inputs = data.inputs;
                        var label = data.label;

                        int prediction = Predict(inputs);
                        int error = label - prediction;

                        if (error != 0)
                        {
                            errorsPresent = true;

                            for (int i = 0; i < Weights.Count; i++)
                            {
                                Weights[i] += LearningRate * error * inputs[i];
                            }

                            Threshold -= LearningRate * error;
                        }
                    }
                }
            }
        }

    }
}
