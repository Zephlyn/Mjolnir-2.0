using BS;
using UnityEngine;

namespace Shield
{
    public class ShieldReturning : MonoBehaviour
    {
        protected Item item;
        private Rigidbody body;

        private bool isThrown = false;

        public float returnSpeed = 4f;
        public float minSpeedForReturn = 4f;

        private PlayerHand playerHand;

        public bool canReturn;
        private bool hasHeld;
        private bool leftThrown;
        private bool rightThrown;
        public bool isReturning;
        private bool canCollide;

        private void Awake()
        {
            item = this.GetComponent<Item>();
            item.OnGrabEvent += OnHandleGrabEvent;
            body = base.GetComponent<Rigidbody>();
            item.OnCollisionEvent += OnCollisionEvent;

            foreach (Collider col in item.GetComponentsInChildren<Collider>())
            {
                col.material.frictionCombine = 0f;
                col.material.dynamicFriction = 0f;
                col.material.staticFriction = 0f;
            }
        }
        void OnCollisionEvent(ref CollisionStruct collisionInstance)
        {
            if(canCollide == true)
            {
                Return();
            }
        }
        void OnHandleGrabEvent(Handle handle, Interactor interactor)
        {
            hasHeld = true;
            playerHand = interactor.playerHand;
            if(interactor.playerHand == Player.local.handLeft)
            {
                leftThrown = true;
            }
            if (interactor.playerHand == Player.local.handRight)
            {
                rightThrown = true;
            }
        }
        void Return()
        {
            float grabDistance = 1f;
            float returnSpeed = 4f;
            isReturning = true;
            if (Vector3.Distance(item.transform.position, playerHand.transform.position) < grabDistance)
            {
                if(rightThrown == true)
                {
                    playerHand.bodyHand.interactor.TryRelease();
                    playerHand.bodyHand.interactor.Grab(item.mainHandleRight);
                    rightThrown = false;
                }
                if (leftThrown == true)
                {
                    playerHand.bodyHand.interactor.TryRelease();
                    playerHand.bodyHand.interactor.Grab(item.mainHandleLeft);
                    leftThrown = false;
                }
                isReturning = false;
            }
            else
            {
                body.velocity = (playerHand.transform.position - body.position) * returnSpeed;
            }
        }
        void FixedUpdate()
        {
            if(!item.IsHanded() && !item.IsTwoHanded())
            {
                if(item.rb.velocity.magnitude >= minSpeedForReturn)
                {
                    canCollide = true;
                }
                else
                {
                    canCollide = false;
                }
            }
        }
    }
}