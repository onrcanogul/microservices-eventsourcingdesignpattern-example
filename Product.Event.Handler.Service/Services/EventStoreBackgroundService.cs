using Microsoft.EntityFrameworkCore;
using Shared.Events;
using Shared.Services.Abstractions;
using System.Reflection;
using System.Text.Json;
using MongoDB.Driver;

namespace Product.Event.Handler.Service.Services
{
    public class EventStoreBackgroundService(IMongoDBService mongoDBService, IEventStoreService eventStoreService, IServiceProvider service) : BackgroundService
    {
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await eventStoreService.SubscribeToStreamAsync("product-stream", async (streamSubscription, resolvedEvent, cancellationToken) =>
            {    

                    string eventType = resolvedEvent.Event.EventType;
                    object @event = JsonSerializer.Deserialize(
                        resolvedEvent.Event.Data.ToArray(),
                        Assembly.Load("Shared").GetTypes().FirstOrDefault(x => x.Name == eventType)
                    );
                    var productCollection = mongoDBService.GetCollection<Shared.Models.Product>("Products");
                    Shared.Models.Product? product = null;
                    switch (@event)
                    {
                        case ProductAddedEvent e:
                            var hasProduct = await (await productCollection.FindAsync(p => p.Id == e.Id)).AnyAsync();
                            if (!hasProduct)
                                await productCollection.InsertOneAsync(new()
                                {
                                    Id = e.Id,
                                    Name = e.Name,
                                    Count = e.Count,
                                    IsAvailable = e.IsAvailable,
                                    Price = e.Price
                                });
                            break;
                        case ProductEditedEvent e:
                            product = await (await productCollection.FindAsync(p => p.Id == e.Id)).FirstOrDefaultAsync();
                            if (product != null)
                            {
                                product.Count = e.Count;
                                product.IsAvailable = e.IsAvailable;
                                product.Price = e.Price;
                                product.Name = e.Name;
                                await productCollection.FindOneAndReplaceAsync(p => p.Id == e.Id, product);
                            }
                            break;
                        case ProductDeletedEvent e:
                            product = await (await productCollection.FindAsync(p => p.Id == e.Id)).FirstOrDefaultAsync();
                            if (product != null)         
                                await productCollection.FindOneAndDeleteAsync(p => p.Id == e.Id);                      
                            break;      
                    }
                
            });
        }
    }
}
