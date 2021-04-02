using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV 
{
    public class Node
    {
        public int x;
        public int y;
        public int z;

        public bool isAir;
        public bool isWalkable;
        public Vector3 worldPosition;
        public GridObject obstacle;
        public GameObject tileViz;
        public Renderer tileRenderer;
        public TextMesh dddText;
        public GridCharacter character;

        int _steps;

        public float hCost;
        public float gCost;
        public Node parentNode;

        public int Steps {
            get { return _steps; }
            set { _steps = value; SetText();}
        }

        public float fCost
        {
            get { return gCost + hCost; }
        }

        public void SetText() 
        {
                dddText.text = _steps.ToString(); 
                dddText.gameObject.SetActive(true);
        }
    }
}
