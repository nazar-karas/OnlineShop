using Application.Common;
using Domain.Documents;
using Domain;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ProductRepository
    {
        private readonly IMongoCollection<Product> Products;

        public ProductRepository(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.Value.MongoDb.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.Value.MongoDb.Name);

            Products = mongoDatabase.GetCollection<Product>("products");

        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            var result = await Products.Find(Builders<Product>.Filter.Empty).ToListAsync();

            return result;
        }

        public async Task<Product?> GetAsync(string id) =>
            await Products.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Product product) =>
            await Products.InsertOneAsync(product);

        public async Task UpdateAsync(Product product) =>
            await Products.ReplaceOneAsync(x => x.Id == product.Id, product);

        public async Task DeleteAsync(string id) =>
            await Products.DeleteOneAsync(x => x.Id == id);
    }
}
