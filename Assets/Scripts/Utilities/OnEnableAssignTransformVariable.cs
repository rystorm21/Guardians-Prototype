using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    public class OnEnableAssignTransformVariable : MonoBehaviour
    {
        public TransformVariable targetVariable;

        private void Awake() 
        {
            targetVariable.value = this.transform;
            Destroy(this);
        }
    }
}
