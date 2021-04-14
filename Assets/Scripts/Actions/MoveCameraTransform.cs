using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace EV
{
    public class MoveCameraTransform : StateActions
    {
        TransformVariable cameraTransform;
        FloatVariable horizontal;
        FloatVariable vertical;

        VariablesHolder varHolder;

        public MoveCameraTransform(VariablesHolder holder) 
        {
            varHolder = holder;
            cameraTransform = varHolder.cameraTransform;
            horizontal = varHolder.horizontalInput;
            vertical = varHolder.verticalInput;
        } 

        public override void Execute(StateManager states, SessionManager sessionManager, Turn turn)
        {
            Vector3 targetPos = cameraTransform.value.forward * (vertical.value * varHolder.cameraMoveSpeed);
            targetPos += cameraTransform.value.right * (horizontal.value * varHolder.cameraMoveSpeed); 

            cameraTransform.value.position += targetPos * states.delta;
        }
    }
}
