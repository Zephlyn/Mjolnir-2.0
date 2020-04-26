using System;
using BS;
using UnityEngine;

namespace Mjolnir
{
    public class StrapFallReturn : MonoBehaviour
    {
        protected Item item;
        private Rigidbody body;

        private bool isThrown = false;

        public float returnSpeed = 5f;
        public float fallReturnSpeed = 15f;
        public float minSpeedForReturn = 2f;
        public float returningBubble = 0.1f;
        public float speedZero = 0f;

        private PlayerHand playerHand;

        public float explosionRadius = 5f;
        public float expForceMultiplier = 50f;
        public float rotationSpeed = 0.2f;

        public bool canReturn;
        private bool hasHeld;
        private bool isReturning;

        protected void Awake()
        {
            item = this.GetComponent<Item>();
            body = base.GetComponent<Rigidbody>();
            item.OnGrabEvent += OnHandleGrabEvent;

            foreach (Collider col in item.GetComponentsInChildren<Collider>())
            {
                col.material.frictionCombine = 0f;
                col.material.dynamicFriction = 0f;
                col.material.staticFriction = 0f;
            }
        }
        void OnHandleGrabEvent(Handle handle, Interactor interactor)
        {
            hasHeld = true;
            playerHand = interactor.playerHand;
        }
        private void Update()
        {
            if (isReturning && (canReturn = true && isThrown && PlayerControl.handLeft.usePressed || PlayerControl.handRight.usePressed) && Player.local.isFalling)
            {
                FallReturn();
            }
            if(Player.local.isFalling)
            {
                canReturn = true;
            }
            else
            {
                canReturn = false;
            }
            if (!item.IsHanded() && !item.IsTwoHanded())
            {
                isThrown = true;
                if (item.rb.velocity.magnitude >= minSpeedForReturn)
                {
                    hasHeld = false;
                    item.rb.useGravity = false;
                }
            }
            if (item.IsHanded() || item.IsTwoHanded())
            {
                isThrown = false;
                canReturn = false;
                hasHeld = true;
                item.rb.useGravity = true;
            }
        }
        void FallReturn()
        {
            isReturning = true;
            float grabDistance = 1f;
            float returnSpeed = 10f;
            if (Vector3.Distance(item.transform.position, playerHand.transform.position) < grabDistance)
            {
                playerHand.bodyHand.interactor.TryRelease();
                playerHand.bodyHand.interactor.Grab(item.mainHandleRight);
                isReturning = false;
            }
            else
            {
                body.velocity = (playerHand.transform.position - body.position) * returnSpeed;
            }
        }
    }
}
