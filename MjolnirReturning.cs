using System;
using BS;
using UnityEngine;
using UnityEngine.AI;

namespace Mjolnir
{
    public class MjolnirReturning : MonoBehaviour
    {
        public Item item;
        private Rigidbody body;

        private bool isThrown = false;

        public float returnSpeed = 5f;
        public float minSpeedForReturn = 2f;

        private PlayerHand playerHand;

        public bool canReturn;
        private bool hasHeld;
        public bool isReturning;

        private void Awake()
        {
            item = this.GetComponent<Item>();
            item.OnGrabEvent += OnHandleGrabEvent;
            body = base.GetComponent<Rigidbody>();

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
        void Return()
        {
            isReturning = true;
            float grabDistance = .3f;
            float returnSpeed = 5f;
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
        void Update()
        {
            if (!item.IsHanded() && !item.IsTwoHanded())
            {
                isThrown = true;
                if (item.rb.velocity.magnitude >= minSpeedForReturn)
                {
                    canReturn = true;
                    hasHeld = false;
                }
            }
            if (item.IsHanded() || item.IsTwoHanded())
            {
                isThrown = false;
                canReturn = false;
                hasHeld = true;
            }
            if (isReturning || (canReturn = true && isThrown && PlayerControl.handLeft.usePressed || PlayerControl.handRight.usePressed) && !hasHeld)
            {
                Return();
            }
        }
    }
}