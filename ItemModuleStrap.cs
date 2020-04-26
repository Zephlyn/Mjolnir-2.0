using BS;

namespace Mjolnir
{
    // This create an item module that can be referenced in the item JSON
    public class ItemModuleStrap : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<StrapFallReturn>();
        }
    }
}
