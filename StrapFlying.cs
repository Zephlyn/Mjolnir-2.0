using UnityEngine;
using BS;

namespace Mjolnir
{
    // The item module will add a unity component to the item object. See unity monobehaviour for more information: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    // This component will apply a force on the player rigidbody to the direction of an item transform when the trigger is pressed (see custom reference in the item definition component of the item prefab)
    public class StrapFlying : MonoBehaviour
    {
        protected Item item;
        protected ItemModuleStrap module;

        protected Interactor rightInteractor;
        protected Interactor leftInteractor;
        protected Transform thrustTransform;

        public float minForce = 500;
        public float maxForce = 1500;

        protected void Awake()
        {
            item = this.GetComponent<Item>();
            item.OnHeldActionEvent += OnHeldAction;
            thrustTransform = item.definition.GetCustomReference("pointingTransform");
            module = item.data.GetModule<ItemModuleStrap>();
        }

        public void OnHeldAction(Interactor interactor, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.UseStart)
            {
                if (interactor.side == Side.Right)
                {
                    rightInteractor = interactor;
                }
                else
                {
                    leftInteractor = interactor;
                }
            }
            else if (action == Interactable.Action.UseStop || action == Interactable.Action.Ungrab)
            {
                if (interactor.side == Side.Right)
                {
                    rightInteractor = null;
                }
                else
                {
                    leftInteractor = null;
                }
            }
        }

        protected void FixedUpdate()
        {
            if (rightInteractor)
            {
                Player.local.locomotion.rb.AddForce(thrustTransform.forward * Mathf.Lerp(minForce, maxForce, PlayerControl.GetHand(rightInteractor.side).useAxis), ForceMode.Force);
            }
            if (leftInteractor)
            {
                Player.local.locomotion.rb.AddForce(thrustTransform.forward * Mathf.Lerp(minForce, maxForce, PlayerControl.GetHand(leftInteractor.side).useAxis), ForceMode.Force);
            }
        }
    }
}