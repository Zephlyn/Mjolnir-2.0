using BS;

namespace Mjolnir
{
    // This create an item module that can be referenced in the item JSON
    public class ItemModuleMjolnir : ItemModule
    {
        public float timeToCharge = 2f;
        public float explosionRadius = 12f;
        public float sWForceMultiplier = 50f;

        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<MjolnirShockwave>();
            item.gameObject.AddComponent<MjolnirReturning>();
        }
    }
}
