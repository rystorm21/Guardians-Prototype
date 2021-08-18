using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EV
{
    public class HandleMouseInteractions : StateActions
    {
        GridCharacter previousCharacter;

        public override void Execute(StateManager states, SessionManager sessionManager, Turn turn)
        {
            if (sessionManager.turns[sessionManager.TurnIndex].name == "EnemyTurn")
                return;
            bool mouseClick = Input.GetMouseButtonDown(0); 

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if(Physics.Raycast(ray, out hit, 1000))                                 // if the raycast hits something
            {
                Node node = sessionManager.gridManager.GetNode(hit.point);          // get the node at the hit point

                IDetectable iDetect = hit.transform.GetComponentInParent<IDetectable>();
                if (iDetect != null)
                {
                    node = iDetect.OnRaycastHit();
                }

                if (mouseClick)
                {
                    if (!EventSystem.current.IsPointerOverGameObject()) // don't register the click if it's on a GUI object
                    {
                        if (SessionManager.currentGameState != GameState.Dialog)
                            sessionManager.DoAction(node, hit);
                    }
                }
                else
                {
                    sessionManager.OnActionTick(node, hit);
                }
            }
        }
    }
}

