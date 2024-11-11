using Application.Common;
using Application.Interfaces.Repositories;
using Domain;
using Domain.Documents;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class OrderRepository : IRepository<Order>
    {
        private readonly IMongoCollection<Order> Orders;

        public OrderRepository(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(
                databaseSettings.Value.MongoDb.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                databaseSettings.Value.MongoDb.Name);

            Orders = mongoDatabase.GetCollection<Order>("orders");

        }

        public async Task<IEnumerable<Order>> GetAllAsync(FilterField[] filterFields, Sorting sorting)
        {
            var filters = new List<FilterDefinition<Order>>();

            foreach (var filterField in filterFields)
            {
                var propertyInfo = typeof(Order).GetProperty(filterField.Name);

                if (propertyInfo != null)
                {
                    var filterToAdd = Builders<Order>.Filter.Eq(x => filterField.Name, filterField.Value);
                    filters.Add(filterToAdd);
                }
            }

            var filter = Builders<Order>.Filter.And(filters.ToArray());

            var orders = Orders.Find(filter);

            if (sorting.Direction == Domain.Constants.SortDirection.Ascending)
            {
                if (string.Equals(sorting.Field, "firstName", StringComparison.OrdinalIgnoreCase))
                {
                    orders = orders.SortBy(x => x.FirstName);
                }
                else if (string.Equals(sorting.Field, "lastName", StringComparison.OrdinalIgnoreCase))
                {
                    orders = orders.SortBy(x => x.LastName);
                }
                else if (string.Equals(sorting.Field, "IsShipped", StringComparison.OrdinalIgnoreCase))
                {
                    orders = orders.SortBy(x => x.IsShipped);
                }
                else
                {
                    orders = orders.SortBy(x => x.Id);
                }
            }
            else
            {
                if (string.Equals(sorting.Field, "firstName", StringComparison.OrdinalIgnoreCase))
                {
                    orders = orders.SortByDescending(x => x.FirstName);
                }
                else if (string.Equals(sorting.Field, "lastName", StringComparison.OrdinalIgnoreCase))
                {
                    orders = orders.SortByDescending(x => x.LastName);
                }
                else if (string.Equals(sorting.Field, "IsShipped", StringComparison.OrdinalIgnoreCase))
                {
                    orders = orders.SortByDescending(x => x.IsShipped);
                }
                else
                {
                    orders = orders.SortByDescending(x => x.Id);
                }
            }

            var result = await orders.ToListAsync();

            return result;
        }

        public async Task<Order?> GetAsync(string id) =>
            await Orders.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Order order) =>
            await Orders.InsertOneAsync(order);

        public async Task UpdateAsync(Order entity) =>
            await Orders.ReplaceOneAsync(x => x.Id == entity.Id, entity);

        public async Task DeleteAsync(string id) =>
            await Orders.DeleteOneAsync(x => x.Id == id);
    }
}
