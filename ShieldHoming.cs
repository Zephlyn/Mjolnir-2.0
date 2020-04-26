using BS;
using UnityEngine;
namespace Shield
{
    public class ShieldHoming : MonoBehaviour
    {
        protected Item item;
        public Creature target;
        public Transform headTransform;
        public ItemModuleShield module;
        public ShieldReturning returning;

        public float minSpeedForHome = 4f;

        protected void Awake()
        {

            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleShield>();
            returning = item.GetComponent<ShieldReturning>();
            item.OnGrabEvent += ShieldGrab;
            item.OnTeleGrabEvent += ShieldTeleGrab;

            headTransform = item.definition.GetCustomReference("pointingTransform");
        }

        void ShieldGrab(Handle handle, Interactor interactor)
        {
            target = null;
        }

        void ShieldTeleGrab(Handle handle, Telekinesis teleGrabber)
        {
            target = null;
        }

        void Update()
        {
            if (!item.IsHanded() && !item.IsTwoHanded() && (module.canHome = true))
            {
                if (item.rb.velocity.magnitude >= minSpeedForHome)
                {
                    if (!target && item.rb.velocity.magnitude >= 3f)
                    {
                        if (returning.isReturning != true)
                        {
                            SetTarget();
                        }
                        if (returning.isReturning == true)
                        {
                            target = null;
                        }
                    }

                    if (target)
                    {
                        if (item.rb.velocity.magnitude < 3f)
                        {
                            target = null;
                        }
                        else
                        {
                            float initVelocity = item.rb.velocity.magnitude;
                            item.transform.LookAt(target.body.headBone);
                            item.rb.velocity = Vector3.zero;
                            item.rb.AddForce(item.transform.forward.normalized * initVelocity, ForceMode.VelocityChange);
                        }
                    }
                }
            }
        }

        void SetTarget()
        {
            float minAngle = 30;
            target = null;
            if (Creature.list.Count > 0)
            {
                foreach (Creature npc in Creature.list)
                {
                    if (npc != Creature.player && npc.state != Creature.State.Dead)
                    {
                        if (npc.body)
                        {
                            if (npc.body.headBone)
                            {
                                if (Vector3.Angle(headTransform.position - item.transform.position, npc.body.headBone.position - item.transform.position) < minAngle)
                                {
                                    target = npc;
                                    minAngle = Vector3.Angle(headTransform.position - item.transform.position, target.body.headBone.position - item.transform.position);
                                }
                            }
                        }

                    }
                }
            }

        }

    }
}