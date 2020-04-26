using BS;
using UnityEngine;
using UnityEngine.AI;
namespace Mjolnir
{
    public class MjolnirHoming : MonoBehaviour
    {
        protected Item item;
        public Creature target;
        public Transform headTransform;
        public ItemModuleMjolnir module;
        public MjolnirReturning returning;
        public NavMeshAgent agent;
        public float turn = 20f;
        public float speed = 20f;
        public bool canHome = true;
        public bool isHoming = false;

        public float minSpeedForHome = 4f;

        protected void Awake()
        {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleMjolnir>();
            agent = item.gameObject.GetComponent<NavMeshAgent>();
            returning = item.gameObject.GetComponent<MjolnirReturning>();
            item.OnGrabEvent += HammerGrab;
            item.OnTeleGrabEvent += HammerTeleGrab;

            headTransform = item.definition.GetCustomReference("pointingTransform");
        }

        void HammerGrab(Handle handle, Interactor interactor)
        {
            target = null;
            isHoming = false;
        }

        void HammerTeleGrab(Handle handle, Telekinesis teleGrabber)
        {
            target = null;
            isHoming = false;
        }

        void Update()
        {
            if(item.rb.isKinematic)
            {
                item.rb.isKinematic = false;
            }
            if (!item.IsHanded() && !item.IsTwoHanded() && (canHome = true))
            {
                if (item.rb.velocity.magnitude >= minSpeedForHome)
                {
                    if (!target)
                    {
                        SetTarget();
                    }
                    else
                    {
                        var rotation = Quaternion.LookRotation(target.transform.position - transform.position);
                        item.rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotation, turn));

                        returning.canReturn = false;
                        isHoming = true;
                    }
                }
            }
            if(target = null)
            {
                returning.canReturn = true;
                isHoming = false;
            }
        }

        void SetTarget()
        {
            item.rb.velocity = transform.forward * speed;
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