using UnityEngine;
using System;
using BS;
using System.Collections.Generic;
using System.Collections;

namespace Mjolnir
{
    public class MjolnirShockwave : MonoBehaviour
    {
        protected Item item;
        protected ItemModuleMjolnir module;
        protected Transform pointingTransform;
        protected Transform bottomTransform;
        public Transform playerTransform;
        public Transform mjolnir;
        public Transform SFX;
        public Transform VFX;

        public bool changedToGreen = false;
        public bool isCharging = false;
        public bool isQuickCharging = false;
        public bool isCharged = false;
        public bool isFlyCharging = false;
        public float timeToCharge = 5f;
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

        public float minSpeedForFly;
        public float minForce = 500;
        public float maxForce = 1500;
        protected Interactor rightInteractor;
        protected Interactor leftInteractor;

        public float timeToSupercharge = 10f;
        public bool supercharged = false;
        public float superchargeTimer = 0f;
        bool called2 = false;
        bool called = false;
        bool isBeaming = false;

        protected void Awake()
        {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleMjolnir>();
            pointingTransform = item.definition.GetCustomReference("pointingTransform");
            bottomTransform = item.definition.GetCustomReference("bottomTransform");
            playerTransform = Creature.player.gameObject.transform;
            item.OnCollisionEvent += OnChargedCollisionEvent;
            item.OnHeldActionEvent += OnHeld;
            mjolnir = item.transform;
            SFX = mjolnir.Find("mjolnir").Find("SFX");
            VFX = mjolnir.Find("mjolnir").Find("VFX");
            VFX.Find("Charged").Find("Light").GetComponent<Light>().enabled = false;
        }
        void OnHeld(Interactor interactor, Handle handle, Interactable.Action action)
        {
            if(supercharged)
            {
                if(action == Interactable.Action.UseStart)
                {
                    ItsBeamTime();
                }
                if(action == Interactable.Action.UseStop)
                {
                    isBeaming = false;
                    called2 = true;
                    VFX.Find("Beam").Find("Lightning1").GetComponent<ParticleSystem>().Stop();
                    if(slowChargeAnimationPlaying)
                    {
                        VFX.Find("Long Charge").Find("Lightning 2").GetComponent<ParticleSystem>().Play();
                    }
                    if(isQuickCharging)
                    {
                        VFX.Find("Quick Charge").Find("Lightning 2").GetComponent<ParticleSystem>().Play();
                    }
                }
            }
        }
        void ItsBeamTime()
        {
            isBeaming = true;
            VFX.Find("Beam").Find("Lightning1").GetComponent<ParticleSystem>().Play();
            if(!called2)
            {
                SFX.Find("Clap").GetComponent<AudioSource>().Play();
                called2 = true;
            }
        }
        void OnChargedCollisionEvent(ref CollisionStruct collisionInstance)
        {
            if (isCharged && item.rb.velocity.magnitude >= 3f)
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
                            npc.ragdoll.SetState(Creature.State.Dead);
                        }
                        foreach (RagdollPart ragdollPart in npc.ragdoll.parts)
                        {
                            ragdollPart.rb.AddForce(sWForceDirection * sWForceMultiplier * sWReductor, ForceMode.Impulse);
                        }
                        foreach (Item item1 in Item.list)
                        {
                            item1.rb.AddForce(sWForceDirection * sWForceMultiplier * sWReductor, ForceMode.Impulse);
                        }
                        npc.health.Kill();
                    }
                }
            }
        }
        void ShockWaveFX()
        {
            VFX.Find("Explosion").Find("Embers").GetComponent<ParticleSystem>().Play();
            VFX.Find("Explosion").Find("Shockwave").GetComponent<ParticleSystem>().Play();
            VFX.Find("Charged").Find("Lightning").GetComponent<ParticleSystem>().Stop();
            VFX.Find("Charged").Find("Smoke").GetComponent<ParticleSystem>().Stop();
            VFX.Find("Charged").Find("Distortion").GetComponent<ParticleSystem>().Stop();
            VFX.Find("Long Charge").Find("Lightning 1").GetComponent<ParticleSystem>().Stop();
            VFX.Find("Long Charge").Find("Lightning 2").GetComponent<ParticleSystem>().Stop();
            VFX.Find("Quick Charge").Find("Lightning 1").GetComponent<ParticleSystem>().Stop();
            VFX.Find("Quick Charge").Find("Lightning 2").GetComponent<ParticleSystem>().Stop();
            VFX.Find("Quick Charge").Find("ChargedLightning").GetComponent<ParticleSystem>().Stop();
            VFX.Find("Long Charge").Find("cloud").GetComponent<ParticleSystem>().Stop();
            VFX.Find("Beam").Find("Lightning1").GetComponent<ParticleSystem>().Stop();
            VFX.Find("Charged").Find("Light").GetComponent<Light>().enabled = false;
            SFX.Find("Explode").GetComponent<AudioSource>().Play();
            supercharged = false;
            isBeaming = false;
            superchargeTimer = 0f;
        }
        protected void Start()
        {
            distance = Vector3.Distance(pointingTransform.position, bottomTransform.position);
            upCondition = distance / 1.4f;
        }
        protected void FixedUpdate()
        {
            bool playerHolding = false;
            using (List<Interactor>.Enumerator enumerator = this.item.handlers.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    Interactor interactor = enumerator.Current;
                    bool flag2 = interactor.bodyHand.body.creature == Creature.player;
                    playerHolding = flag2;
                }
            }
            if (playerHolding)
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

                if(slowChargeAnimationPlaying)
                {
                    superchargeTimer += Time.deltaTime;
                }
                if (superchargeTimer >= timeToSupercharge)
                {
                    supercharged = true;
                    if (!called)
                    {
                        SFX.Find("Clap2").GetComponent<AudioSource>().Play();
                        called = true;
                    }
                }
                else
                {
                    supercharged = false;
                    called = false;
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
                                VFX.Find("Charged").Find("Lightning").GetComponent<ParticleSystem>().Play();
                                VFX.Find("Charged").Find("Smoke").GetComponent<ParticleSystem>().Play();
                                VFX.Find("Charged").Find("Distortion").GetComponent<ParticleSystem>().Play();
                                VFX.Find("Long Charge").Find("Lightning 1").GetComponent<ParticleSystem>().Play();
                                VFX.Find("Long Charge").Find("Lightning 2").GetComponent<ParticleSystem>().Play();
                                VFX.Find("Long Charge").Find("cloud").GetComponent<ParticleSystem>().Play();
                                SFX.Find("ChargingLong").GetComponent<AudioSource>().Play();
                                SFX.Find("ChargedLong").GetComponent<AudioSource>().Play();
                                VFX.Find("Charged").Find("Light").GetComponent<Light>().enabled = true;
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
                    VFX.Find("Charged").Find("Smoke").GetComponent<ParticleSystem>().Play();
                    VFX.Find("Charged").Find("Distortion").GetComponent<ParticleSystem>().Play();
                    VFX.Find("Quick Charge").Find("Lightning 1").GetComponent<ParticleSystem>().Play();
                    VFX.Find("Quick Charge").Find("Lightning 2").GetComponent<ParticleSystem>().Play();
                    VFX.Find("Quick Charge").Find("ChargedLightning").GetComponent<ParticleSystem>().Play();
                    VFX.Find("Charged").Find("Light").GetComponent<Light>().enabled = true;
                    SFX.Find("QuickChargeStrike").GetComponent<AudioSource>().Play();
                    SFX.Find("ChargedQuick").GetComponent<AudioSource>().Play();
                    isCharged = true;
                    quickChargeTimer = 0f;
                }
            }
            else
            {
                NotCharging();
            }
            if(isBeaming)
            {
                VFX.Find("Quick Charge").Find("Lightning 2").GetComponent<ParticleSystem>().Stop();
                VFX.Find("Long Charge").Find("Lightning 2").GetComponent<ParticleSystem>().Stop();
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
                VFX.Find("Charged").Find("Lightning").GetComponent<ParticleSystem>().Stop();
                VFX.Find("Charged").Find("Smoke").GetComponent<ParticleSystem>().Stop();
                VFX.Find("Charged").Find("Distortion").GetComponent<ParticleSystem>().Stop();
                VFX.Find("Long Charge").Find("Lightning 1").GetComponent<ParticleSystem>().Stop();
                VFX.Find("Long Charge").Find("Lightning 2").GetComponent<ParticleSystem>().Stop();
                VFX.Find("Long Charge").Find("cloud").GetComponent<ParticleSystem>().Stop();
                VFX.Find("Quick Charge").Find("Lightning 1").GetComponent<ParticleSystem>().Stop();
                VFX.Find("Quick Charge").Find("Lightning 2").GetComponent<ParticleSystem>().Stop();
                VFX.Find("Quick Charge").Find("ChargedLightning").GetComponent<ParticleSystem>().Stop();
                VFX.Find("Charged").Find("Light").GetComponent<Light>().enabled = false;
                superchargeTimer = 0f;
                if (!isQuickCharging)
                {
                    SFX.Find("ChargedQuick").GetComponent<AudioSource>().Stop();
                }
                if (!isCharging)
                {
                    SFX.Find("ChargedLong").GetComponent<AudioSource>().Stop();
                    SFX.Find("ChargingLong").GetComponent<AudioSource>().Stop();
                }
            }
        }
    }
}