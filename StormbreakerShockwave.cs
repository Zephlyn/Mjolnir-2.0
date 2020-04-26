using UnityEngine;
using System;
using BS;
using System.Collections.Generic;

namespace Stormbreaker
{
    public class StormbreakerShockwave : MonoBehaviour
    {
        protected Item item;
        protected ItemModuleStormbreaker module;
        protected Transform pointingTransform;
        protected Transform bottomTransform;
        public Transform playerTransform;
        public Transform stormbreaker;

        public bool changedToGreen = false;
        public bool isCharging = false;
        public bool isQuickCharging = false;
        public bool isCharged = false;
        public bool isBounceCharging = false;
        public float timeToCharge = 2f;
        public float timeToQuickCharge = 0.4f;
        public float chargeTimer = 0f;
        public float quickChargeTimer = 0f;

        public float quickChargeExpires = 0.25f;
        public float quickChargeCountdown = 0f;
        public bool isLowEnough = false;

        private bool slowChargeAnimationPlaying = false;

        public float explosionRadius = 12f;
        public float distNPC;
        public float sWReductor;
        public Vector3 sWForceDirection;
        public float sWForceMultiplier = 50f;

        public float distance;
        public float upCondition;
        public float headHight;

        protected void Awake()
        {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleStormbreaker>();
            pointingTransform = item.definition.GetCustomReference("pointingTransform");
            bottomTransform = item.definition.GetCustomReference("bottomTransform");
            playerTransform = Creature.player.gameObject.transform;
            item.OnCollisionEvent += OnChargedCollisionEvent;
            timeToCharge = module.timeToCharge;
            explosionRadius = module.explosionRadius;
            sWForceMultiplier = module.sWForceMultiplier;
            stormbreaker = base.GetComponent<Transform>();
        }

        void OnChargedCollisionEvent(ref CollisionStruct collisionInstance)
        {
            if (isCharged)
            {
                isCharged = false;
                ShockWave(explosionRadius);
            }
        }

        void ShockWave(float rad)
        {
            ShockWaveFX();
            foreach (Creature npc in Creature.list)
            {
                if (npc != Creature.player)
                {
                    distNPC = Vector3.Distance(npc.gameObject.transform.position, pointingTransform.position);
                    Debug.Log(distNPC);
                    if (distNPC < rad)
                    {
                        sWReductor = (rad - distNPC) / rad;
                        sWForceDirection = npc.gameObject.transform.position - pointingTransform.position;
                        sWForceDirection.Normalize();
                        sWForceDirection += Vector3.up;
                        sWForceDirection.Normalize();
                        if (npc.state != Creature.State.Dead)
                        {
                            npc.ragdoll.SetState(Creature.State.Destabilized);
                        }
                        foreach (RagdollPart ragdollPart in npc.ragdoll.parts)
                        {
                            ragdollPart.rb.AddForce(sWForceDirection * sWForceMultiplier * sWReductor, ForceMode.Impulse);
                        }
                        npc.health.Kill();
                    }
                }
            }
        }

        void ShockWaveFX()
        {
            stormbreaker.Find("Hit").Find("Strikes").GetComponent<ParticleSystem>().Play();
            stormbreaker.Find("Hit").Find("EarthShatter").GetComponent<ParticleSystem>().Play();
            stormbreaker.Find("Long Charge").Find("Lightning").GetComponent<ParticleSystem>().Stop();
            stormbreaker.Find("Quick Charge").Find("Lightning").GetComponent<ParticleSystem>().Stop();
            stormbreaker.Find("Long Charge").Find("Lightning 1").GetComponent<ParticleSystem>().Stop();
            stormbreaker.Find("Long Charge").Find("Lightning 2").GetComponent<ParticleSystem>().Stop();
            stormbreaker.Find("Quick Charge").Find("Lightning 1").GetComponent<ParticleSystem>().Stop();
            stormbreaker.Find("Quick Charge").Find("Lightning 2").GetComponent<ParticleSystem>().Stop();
            stormbreaker.Find("Long Charge").Find("cloud").GetComponent<ParticleSystem>().Stop();
            stormbreaker.Find("Quick Charge").Find("cloud").GetComponent<ParticleSystem>().Stop();
            stormbreaker.Find("Explode").GetComponent<AudioSource>().Play();
            stormbreaker.Find("Explosion1").GetComponent<AudioSource>().Play();
        }
        protected void Start()
        { 
            distance = Vector3.Distance(pointingTransform.position, bottomTransform.position);
            upCondition = distance / 1.4f;
        }
        protected void FixedUpdate()
        {
            bool flag = false;
            using (List<Interactor>.Enumerator enumerator = this.item.handlers.GetEnumerator())
            {
                bool flag2 = enumerator.MoveNext();
                if (flag2)
                {
                    Interactor interactor = enumerator.Current;
                    bool flag3 = interactor.bodyHand.body.creature == Creature.player;
                    flag = flag3;
                }
            }
            bool flag4 = flag;
            if(flag4 && !isBounceCharging)
            {
                headHight = pointingTransform.position.y - bottomTransform.position.y;
                if (bottomTransform.position.y < Player.local.waist.gameObject.transform.position.y)
                {
                    isLowEnough = true;
                    quickChargeCountdown = 0f;
                }

                if (isLowEnough)
                {
                    quickChargeCountdown += Time.deltaTime;
                }

                if (quickChargeCountdown >= quickChargeExpires)
                {
                    quickChargeCountdown = 0f;
                    isLowEnough = false;
                }

                if ((headHight > upCondition) && !isCharged)
                {
                    if (bottomTransform.position.y > Player.local.head.gameObject.transform.position.y)
                    {
                        if (isLowEnough)
                        {
                            isQuickCharging = true;
                        }
                        else
                        {
                            isCharging = true;

                            if (!slowChargeAnimationPlaying && !isQuickCharging)
                            {
                                stormbreaker.Find("Long Charge").Find("Lightning").GetComponent<ParticleSystem>().Play();
                                stormbreaker.Find("Long Charge").Find("Lightning 1").GetComponent<ParticleSystem>().Play();
                                stormbreaker.Find("Long Charge").Find("Lightning 2").GetComponent<ParticleSystem>().Play();
                                stormbreaker.Find("Long Charge").Find("cloud").GetComponent<ParticleSystem>().Play();
                                stormbreaker.Find("ChargingLong").GetComponent<AudioSource>().Play();
                                stormbreaker.Find("ChargedLong").GetComponent<AudioSource>().Play();
                                slowChargeAnimationPlaying = true;
                            }
                        }
                    }
                    else
                    {
                        NotCharging();
                    }
                }
                else
                {
                    NotCharging();
                }

                if (isCharging && !isCharged)
                {
                    chargeTimer += Time.deltaTime;
                }
                if (isQuickCharging && !isCharged)
                {
                    quickChargeTimer += Time.deltaTime;
                }

                if (timeToCharge <= chargeTimer)
                {
                    isCharged = true;
                    chargeTimer = 0f;
                }
                if (timeToQuickCharge <= quickChargeTimer)
                {
                    stormbreaker.Find("Quick Charge").Find("Lightning").GetComponent<ParticleSystem>().Play();
                    stormbreaker.Find("Quick Charge").Find("Lightning 1").GetComponent<ParticleSystem>().Play();
                    stormbreaker.Find("Quick Charge").Find("Lightning 2").GetComponent<ParticleSystem>().Play();
                    stormbreaker.Find("Quick Charge").Find("cloud").GetComponent<ParticleSystem>().Play();
                    stormbreaker.Find("QuickChargeStrike").GetComponent<AudioSource>().Play();
                    stormbreaker.Find("ChargedQuick").GetComponent<AudioSource>().Play();
                    isCharged = true;
                    quickChargeTimer = 0f;
                }
            }
            else
            {
                NotCharging();
            }
        }
        void NotCharging()
        {
            isCharging = false;
            isQuickCharging = false;
            chargeTimer = 0f;
            quickChargeTimer = 0f;
            slowChargeAnimationPlaying = false;

            if (!isCharged)
            {
                stormbreaker.Find("Long Charge").Find("Lightning").GetComponent<ParticleSystem>().Stop();
                stormbreaker.Find("Long Charge").Find("Lightning 1").GetComponent<ParticleSystem>().Stop();
                stormbreaker.Find("Long Charge").Find("Lightning 2").GetComponent<ParticleSystem>().Stop();
                if(!isQuickCharging)
                {
                    stormbreaker.Find("ChargedQuick").GetComponent<AudioSource>().Stop();
                }
                if(!isCharging)
                {
                    stormbreaker.Find("ChargedLong").GetComponent<AudioSource>().Stop();
                    stormbreaker.Find("ChargingLong").GetComponent<AudioSource>().Stop();
                }
            }
        }
    }
}