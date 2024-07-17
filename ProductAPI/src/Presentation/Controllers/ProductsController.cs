using Application.DTO;
using Application.Interfaces.Services;
using Domain.Common;
using Domain.Common.Query;
using Domain.Entities;
using Domain.Exceptions;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Presentation.Messages;
using Serilog;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly IProductService _productService;
        private readonly IFileService _fileService;
        private readonly IMemoryCache _memoryCache;


        public ProductsController(
            ILogger<ProductsController> logger, 
            IProductService productService, 
            IFileService fileService, 
            IMemoryCache memoryCache)
        {
            _logger = logger;
            _productService = productService;
            _fileService = fileService;
            _memoryCache = memoryCache;
        }

        [HttpGet(Name = "GetAllProducts")]
        [ProducesResponseType(typeof(IEnumerable<ProductReadDto>), 200)]
        public async Task<IActionResult> GetAllProducts([FromQuery] ProductParams query)
        {
            var products = await _productService.GetAll(query);

            Log.Logger.Information("Get All Products");

            return Ok(products);
        }

        [HttpGet("{productId}", Name = "GetProductById")]
        [ProducesResponseType(typeof(ProductReadDto), 200)]
        public async Task<IActionResult> GetProductById([FromRoute] Guid productId)
        {
            var product = await _productService.Get(productId);

            if (Request.Cookies.TryGetValue("clientId", out string clientId))
            {
                var clients = _memoryCache.Get<List<ClientReview>>("clients");

                var client = clients.FirstOrDefault(x => x.Id == clientId);

                client.ProductsIds.Add(product.Id);

                _memoryCache.Set<List<ClientReview>>("clients", clients);
            }
            
            return Ok(product);
        }

        [HttpPost(Name = "CreateOrUpdateProduct")]
        [ProducesResponseType(typeof(ProductReadDto), 201)]
        [ProducesResponseType(typeof(ProductReadDto), 200)]
        public async Task<IActionResult> CreateOrUpdateProduct([FromBody] ProductUpsertRequest request)
        {
            var categoriesWithSubcategories = (await _productService.GetCategoriesWithSubcategories()).ToList();

            // check category and subcategory

            if (!categoriesWithSubcategories.Any(x => x.Name == request.Product.Category))
            {
                return BadRequest($"The category with name '{request.Product.Category}' is not found.");
            }

            bool hasCorrectSubcategory = false;

            foreach (var category in categoriesWithSubcategories)
            {
                hasCorrectSubcategory = category.Subcategories.Any(y => y.Name == request.Product.Subcategory);

                if (hasCorrectSubcategory)
                {
                    break;
                }
            }

            if (!hasCorrectSubcategory)
            {
                return BadRequest($"The subcategory with name '{request.Product.Subcategory}' is not found.");
            }

            var groupWithFeatures = await _productService.GetFeatureGroupsWithFeatures();

            Guid featureGroupId = Guid.Empty;

            foreach (var group in groupWithFeatures)
            {
                var result = group.Features.Select(x => x.Id)
                    .Except(request.Product.Features.Select(f => f.FeatureId));

                if (result.IsNullOrEmpty())
                {
                    featureGroupId = group.Id;
                    break;
                }
            }

            if (featureGroupId == Guid.Empty)
            {
                return BadRequest($"You didn't provide all necessary features.");
            }

            if (request.Product.Id != Guid.Empty)
            {
                if (await _productService.UserWantsToChangeProductFeatures(request.Product.Id, featureGroupId))
                {
                    return BadRequest("The change of product features is not allowed.");
                }
            }

            string invalidValues = string.Empty;

            foreach (var group in groupWithFeatures)
            {
                foreach (var feature in group.Features)
                {
                    var itemToCheck = request.Product.Features.FirstOrDefault(z => z.FeatureId == feature.Id);
                    if (itemToCheck != null)
                    {
                        if (!feature.PossibleValues.IsNullOrEmpty() && !feature.PossibleValues.Contains(itemToCheck.Value))
                        {
                            invalidValues += $"Invalid feature value '{itemToCheck.Value}' from possible values [{string.Join(" ", feature.PossibleValues)}].\n";
                        }
                    }
                }
            }

            if (!invalidValues.IsNullOrEmpty())
            {
                return BadRequest(invalidValues);
            }

            // create product and link it with profile group

            var upsertedProduct = await _productService.CreateOrUpdateAsync(request.Product, featureGroupId);

            // create feature values and link product with them

            var upsertedFeatures = await _productService.CreateOrUpdate(request.Product.Features, upsertedProduct.Id);

            // attach files

            if (!request.FilesToAttach.IsNullOrEmpty())
            {
                var filesToAttach = request.FilesToAttach
                .Select(x => new AttachedFileDto() 
                { 
                    FileName = x.FileName, 
                    FileContents = x.FileContents 
                }).ToList();

                await _fileService.StoreFilesAsync(upsertedProduct.Id, filesToAttach);
            }

            upsertedProduct.Features = upsertedFeatures.ToList();
            
            if (request.Product.Id == upsertedProduct.Id)
            {
                return Ok(upsertedProduct);

            }

            return Created($"api/products/{upsertedProduct.Id}", upsertedProduct);
        }

        [HttpDelete(Name = "DeleteProduct")]
        public IActionResult DeleteProduct([FromQuery] string ids)
        {
            try
            {
                var preparedIds = ids.Split(",").Select(x => Guid.Parse(x));
                _productService.Delete(preparedIds);
                return Ok();
            }
            catch (NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("attach/files/{productId}")]
        public async Task<IActionResult> AttachFiles(Guid productId, [FromBody] IEnumerable<AttachFileRequest> request)
        {
            var filesToAttach = request
                .Select(x => new AttachedFileDto()
                {
                    FileName = x.FileName,
                    FileContents = x.FileContents
                }).ToList();

            await _fileService.StoreFilesAsync(productId, filesToAttach);

            return Ok();
        }

        [Authorize]
        [HttpPost("features")]
        [ProducesResponseType(typeof(FeatureGroupDto), 200)]
        public async Task<IActionResult> CreateOrUpdateFeaturesAsync([FromBody] FeatureGroupDto dto)
        {
            var result = await _productService.CreateOrUpdateAsync(dto);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("features")]
        [ProducesResponseType(typeof(IEnumerable<FeatureGroupDto>), 200)]
        public async Task<IActionResult> GetFeaturesAsync([FromQuery] string query)
        {
            var result = await _productService.GetFeatureGroupsWithFeatures();
            return Ok(result);
        }

        [HttpGet("recommendations")]
        public async Task<IActionResult> GetRecommendations()
        {
            if (Request.Cookies.TryGetValue("clientId", out string clientId))
            {
                var clients = _memoryCache.Get<List<ClientReview>>("clients");

                var client = clients.FirstOrDefault(x => x.Id == clientId);

                var result = await _productService.GetRecommendations(client.ProductsIds);

                return Ok(result);
            }

            return BadRequest();
        }
    }
}
