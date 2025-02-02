
namespace Business_Logic
{
    public class Service
    {

        public int Id { get; set; }

        public string ServiceName { get; set; }

        public string ServiceType { get; set; }

        public decimal ServicePrice { get; set; }

        public TimeSpan ServiceExecutionTime { get; set; }

        public decimal GetDiscountedPrice(bool isNewCustomer)
        {
            decimal discountedPrice = ServicePrice;

            if (isNewCustomer)
            {
                discountedPrice *= 0.7m;
            }

            return discountedPrice;
        }
    }
}

