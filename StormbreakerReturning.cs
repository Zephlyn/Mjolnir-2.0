using System;
using BS;
using UnityEngine;

namespace Stormbreaker
{
    public class StormbreakerReturning : MonoBehaviour
    {
        protected Item item;
        private Transform hammer;
        private Rigidbody body;

        private bool isThrown;

        public float returnSpeed = 5f;
        public float fallReturnSpeed = 15f;
        public float minSpeedForReturn = 2f;
        public float returningBubble = 0.1f;
        public float speedZero = 0f;

        private PlayerHand playerHand;

        private Transform hammerTransform;
        private Transform targetTransform = null;

        private bool hammerFly = false;
        public bool hasTarget = false;
        private bool hammerTurn = false;

        private float detectionBubble = 20f;
        private float distanceToTarget = 0f;
        private float angleToTarget = 0f;
        private Vector3 vectorHammerToTarget;

        private Creature nearestTarget;

        public float explosionRadius = 5f;
        public float expForceMultiplier = 50f;
        public float rotationSpeed = 0.2f;

        private bool canReturn;
        private float timeLeft = 0.5f;
        private int pressed;
        private bool hasHeld;
        private bool epic;
        private bool isReturning;

        private void Awake()
        {
            item = this.GetComponent<Item>();
            item.OnGrabEvent += OnHandleGrabEvent;
            hammer = base.GetComponent<Transform>();
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
        void FallReturn()
        {
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
        void Update()
        {
            if (!item.IsHanded() && !item.IsTwoHanded())
            {
                if (item.rb.velocity.magnitude > minSpeedForReturn)
                {
                    canReturn = true;
                }
            }
            else
            {
                canReturn = false;
            }

            if (!isReturning && isThrown)
            {
                hammerFly = true;
            }

            if (isReturning || (canReturn = true && isThrown && PlayerControl.handLeft.usePressed || PlayerControl.handRight.usePressed) && (hasHeld = false))
            {
                Return();
            }
            if (isReturning || (canReturn = true && isThrown && PlayerControl.handLeft.usePressed || PlayerControl.handRight.usePressed) && Player.local.isFalling && (hasHeld = false))
            {
                FallReturn();
            }
            if (hammerFly)
            {
                HammerFly();
            }
        }
        void HammerFly()
        {
            if (!item.IsHanded() && !item.IsTwoHanded())
            {
                if (!hasTarget)
                {
                    SetTarget();
                }

                if (hasTarget && (targetTransform != null))
                {
                    distanceToTarget = Vector3.Distance(hammerTransform.position, targetTransform.position);
                    if (distanceToTarget <= detectionBubble)
                    {
                        hammerTurn = true;
                    }
                    else
                    {
                        hammerTurn = false;
                        DeleteTarget();
                    }
                }
                if (hammerTurn)
                {
                    Turn();
                }
            }
        }
        void SetTarget()
        {
            foreach (Creature target in Creature.list)
            {
                if (target != Creature.player)
                {
                    if (target.state != Creature.State.Dead)
                    {
                        if (Vector3.Distance(hammerTransform.position, target.transform.position) < detectionBubble)
                        {
                            vectorHammerToTarget = target.transform.position - hammerTransform.position;
                            vectorHammerToTarget.y = 0f;
                            angleToTarget = Vector3.Angle(hammerTransform.forward, vectorHammerToTarget);

                            if (angleToTarget < 90f)
                            {
                                nearestTarget = target;
                            }
                        }
                    }
                }
            }
            if (nearestTarget != null)
            {
                targetTransform = nearestTarget.transform;
                hasTarget = true;
            }
        }
        void DeleteTarget()
        {
            targetTransform = null;
            nearestTarget = null;
            hasTarget = false;
            angleToTarget = 0f;
            hammerTurn = false;
            hammerFly = false;
        }
        void Turn()
        {
            vectorHammerToTarget = targetTransform.position - hammerTransform.position;
            vectorHammerToTarget.y = 0f;

            angleToTarget = Vector3.SignedAngle(hammerTransform.forward, vectorHammerToTarget, hammerTransform.up);

            if (angleToTarget > 3f)
            {
                item.transform.Rotate(hammerTransform.up, rotationSpeed);
            }
            else if (angleToTarget < -3f)
            {
                item.transform.Rotate(hammerTransform.up, -1 * rotationSpeed);
            }
        }
    }
}