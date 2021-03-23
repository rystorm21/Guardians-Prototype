using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    [CreateAssetMenu(menuName = "Phases/Idle Phase")]
    public class IdlePhase : Phase
    {
        public override bool IsComplete(SessionManager sessionManager, Turn turn)
        {
            return false;
        }

        public override void OnStartPhase(SessionManager sessionManager, Turn turn)
        {
            if (isInit)
                return;
            isInit = true;

            Debug.Log("Idle Phase started");
        }

        public override void OnEndPhase(SessionManager sessionManager, Turn turn)
        {

        }
    }
}

