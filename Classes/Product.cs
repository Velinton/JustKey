using System.Collections.Generic;

namespace JustKey.Classes
{
    public class Product
    {
        public int ID { get; set; }

        public List<GameImage> Images { get; set; }

        public string GameName { get; set; }

        public int Price { get; set; }

        public int Count { get; set; }

        public string GameDescription { get; set; }

        public string SystemRequirements { get; set; }
    }
}
