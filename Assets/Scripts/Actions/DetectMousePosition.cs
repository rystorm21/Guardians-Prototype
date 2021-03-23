using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    public class HandleMouseInteractions : StateActions
    {

        GridCharacter previousCharacter;

        public override void Execute(StateManager states, SessionManager sessionManager, Turn turn)
        {
            bool mouseClick = Input.GetMouseButtonDown(0);

            if (previousCharacter != null) 
                previousCharacter.OnDeHighlight(states.playerHolder);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if(Physics.Raycast(ray, out hit, 1000))
            {
                Node n = sessionManager.gridManager.GetNode(hit.point);
                IDetectable detectable = hit.transform.GetComponent<IDetectable>();

                if (detectable != null) // then we probably hit a character or something sitting on a node
                {
                    n = detectable.OnDetect(); // makes it so you can hover the mouse over the collider instead of only the tile to detect a character
                }
                
                if (n != null) 
                {
                    if (n.character != null)
                    {
                        // you highlighted your own unit
                        if (n.character.owner == states.playerHolder)
                        {
                            n.character.OnHighlight(states.playerHolder);
                            previousCharacter = n.character;
                            sessionManager.ClearPath(states);
                        }
                        else // you highlighted an enemy unit
                        {

                        }
                    }

                    if (states.currentCharacter != null && n.character == null) 
                    {
                        if (mouseClick)
                        {
                            if (states.currentCharacter.currentPath != null || states.currentCharacter.currentPath.Count > 0)
                            {
                                states.SetState("moveOnPath");
                            }
                        }
                        else 
                        {
                            PathDetection(states, sessionManager, n);
                        }    
                    }
                    else // no character selected
                    {
                        if (mouseClick)
                        {
                            if (n.character != null) 
                            {
                                if (n.character.owner == states.playerHolder)
                                {
                                    n.character.OnSelect(states.playerHolder);
                                    states.prevNode = null;
                                    sessionManager.ClearPath(states);
                                }
                            }
                        }
                    }
                }
            }
        }

        void PathDetection(StateManager states, SessionManager sessionManager, Node node)
        {
            states.currentNode = node;

            if (states.currentNode != null)
            {
                if (states.currentNode != states.prevNode || states.prevNode == null)
                {
                    states.prevNode = states.currentNode;
                    sessionManager.PathfinderCall(states.currentCharacter, states.currentNode);
                }
            }
        }
    }
}

