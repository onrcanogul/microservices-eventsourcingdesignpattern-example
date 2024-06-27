namespace Product.App.Models
{
    public class EditProductVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
    }
}
