namespace LunchOrderingSystem.Shared
{
    public class MenuItem
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Price { get; set; }

        public bool HasBeenPick { get; set; }

        public bool PickByMyself { get; set; }
    }
}
