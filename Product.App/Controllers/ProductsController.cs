using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Product.App.Models;
using MongoDB.Driver;
using Shared.Events;
using Shared.Services.Abstractions;

namespace Product.App.Controllers
{
    public class ProductsController(IEventStoreService eventStoreService, IMongoDBService mongoDBService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var productCollection = mongoDBService.GetCollection<Shared.Models.Product>("Products");
            var products = await (await productCollection.FindAsync(_ =>true)).ToListAsync();
            return View(products);
        }
        public IActionResult Create()
        {
            return View();
        }
        public async Task<IActionResult> Edit(string id)
        {
            var productCollection = mongoDBService.GetCollection<Shared.Models.Product>("Products");
            var product = await (await productCollection.FindAsync(p => p.Id == Guid.Parse(id))).FirstOrDefaultAsync();
            EditProductVM editProduct = new()
            {
                Count = product.Count,
                IsAvailable = product.IsAvailable,
                Name = product.Name,
                Price = product.Price,
                Id = product.Id,
            };
            return View(editProduct);
        } 
        [HttpPost]
        public async Task <IActionResult> Create(CreateProductVM model)
        {
            ProductAddedEvent productAddedEvent = new()
            {
                Count = model.Count,
                Id = Guid.NewGuid(),
                IsAvailable = model.IsAvailable,
                Name = model.Name,
                Price = model.Price
            };
            await eventStoreService.AppendToStreamAsync("product-stream", new[]
            {
                eventStoreService.GenerateEventData(productAddedEvent)
            });
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditProductVM editProductVM)
        {
            ProductEditedEvent productEditedEvent = new()
            {
                Count = editProductVM.Count,
                Id = editProductVM.Id,
                Name = editProductVM.Name,
                Price = editProductVM.Price,
                IsAvailable = editProductVM.IsAvailable
            };

            await eventStoreService.AppendToStreamAsync("product-stream", new[]
            {
                eventStoreService.GenerateEventData(productEditedEvent)
            });
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            ProductDeletedEvent productDeletedEvent = new()
            {
                Id = Guid.Parse(id)
            };
            await eventStoreService.AppendToStreamAsync("product-stream", new[]
            {
                eventStoreService.GenerateEventData(productDeletedEvent)
            });

            return RedirectToAction("Index");
        }

    }
}
