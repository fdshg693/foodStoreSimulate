using static foodStoreSimulate.Components.Pages.Queue;
using foodStoreSimulate.Components.Services;
using foodStoreSimulate.Components.Services.Entities;
using Microsoft.VisualBasic;

namespace foodStoreSimulate.Components.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly TextLogger logger;
        private readonly CustomerQueueService customerQueueService;
        private readonly Queue<CookedFood> cookedFoodQueue = new();
        private readonly Queue<Garbage> garbageQueue = new();
        private int maxFoodId = 0;
        private UIStateService uIStateService;

        public RestaurantService(TextLogger logger, CustomerQueueService customerQueueService, UIStateService uIStateService)
        {
            this.logger = logger;
            this.customerQueueService = customerQueueService;
            this.uIStateService = uIStateService;
        }

        public IReadOnlyCollection<Customer> Customers => customerQueueService.Customers;
        public IReadOnlyCollection<CookedFood> Foods => cookedFoodQueue;
        public IReadOnlyCollection<Garbage> Garbages => garbageQueue;

        public void EnqueueCustomer()
        {
            customerQueueService.EnqueueCustomer();
            TryServe();
        }

        public void EnqueueFood(MenuItem orderedFood)
        {
            maxFoodId++;
            var cookedFood = new CookedFood { CookedItemId = maxFoodId, MenuItem = orderedFood };
            cookedFoodQueue.Enqueue(cookedFood);
            logger.AddLog($"食品が準備されました: {cookedFood.MenuItem.Food.DishName}");
            TryServe();
        }

        public void DisposeExpiredFood()
        {
            while (cookedFoodQueue.Any() && cookedFoodQueue.Peek().IsExpired())
            {
                var cookedFood = cookedFoodQueue.Dequeue();
                var garbage = Garbage.AutoGenereateGarbage(cookedFood);
                garbageQueue.Enqueue(garbage);
                logger.AddLog($"{garbage.DishName} は賞味期限切れのため廃棄しました。");
            }
        }

        public async Task CookAsync(int SelectedValue)
        {
            MenuItem? orderedFood = MenuService.GetMenuItem(SelectedValue);
            if (orderedFood == null)
            {
                return;
            }
            uIStateService.SetCooking(true);
            await Task.Delay(orderedFood.CookingTime  * 1000);
            EnqueueFood(orderedFood);
            uIStateService.SetCooking(false);
        }

        private void TryServe()
        {
            DisposeExpiredFood();
            while (customerQueueService.Any() && cookedFoodQueue.Any())
            {
                var customer = customerQueueService.DequeueCustomer();
                var cookedFood = cookedFoodQueue.Dequeue();
                logger.AddLog($"{customer.Name} に {cookedFood.MenuItem.Food.DishName} を提供しました。");
            }
        }
    }

}
