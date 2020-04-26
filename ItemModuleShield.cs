using BS;

namespace Shield
{
    public class ItemModuleShield : ItemModule
    {
        public bool canHome;
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ShieldReturning>();
            item.gameObject.AddComponent<ShieldHoming>();
        }
    }
}
