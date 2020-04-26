using System;
using UnityEngine;
using BS;

namespace Mjolnir
{
    public class MjolnirNPCGrab : MonoBehaviour
    {
        protected Item item;

        void Awake()
        {
            item = base.GetComponent<Item>();
            item.OnGrabEvent += OnGrab;
        }

        void OnGrab(Handle handle, Interactor interactor)
        {
            if(interactor != Creature.player)
            {
                item.rb.isKinematic = true;
            }
            else
            {
                item.rb.isKinematic = false;
            }
            if(interactor == null)
            {
                item.rb.isKinematic = false;
            }
        }
    }
}
