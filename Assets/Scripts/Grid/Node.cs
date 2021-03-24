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
        public GridCharacter character;

        public float hCost;
        public float gCost;
        public Node parentNode;

        public float fCost
        {
            get { return gCost + hCost; }
        }
    }
}
