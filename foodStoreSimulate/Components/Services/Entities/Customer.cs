namespace foodStoreSimulate.Components.Services.Entities
{
    public class Customer
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required DateTime CreatedAt { get; set; }
        public static Customer AutoGenereateCustomer(int Id)
        {
            return new Customer { Id = Id, Name = $"customer {Id}", CreatedAt = DateTime.Now };
        }
        public TimeSpan GetWaitingTime()
        {
            return DateTime.Now - this.CreatedAt; 
        }
    }
}
