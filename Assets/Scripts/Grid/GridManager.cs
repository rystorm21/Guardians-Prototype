using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV 
{
    public class GridManager : MonoBehaviour
    {
        #region variables
        Node[,,] grid;
        [SerializeField]
        public float xzScale = 1.5f;
        [SerializeField]
        public float yScale = 2;
        [SerializeField]
        float rayDownDist = 1.3f;
        [SerializeField]
        float rayDownOrigin = .7f;
        [SerializeField]
        float collisionOffset;

        public Vector3 extends = new Vector3(.8f, .8f, .8f);

        Vector3 _minPos;
        int _maxX;
        int _maxZ;
        int _maxY;

        public bool visualizeCollisions;
        List<Vector3> _nodeViz = new List<Vector3>();
        float _offset = 0.5f;

        int _pos_x;
        int _pos_y;
        int _pos_z;

        #endregion

        public GameObject unit;
        public GameObject tileViz;
        public GameObject tileContainer;

        public void Init() 
        {
            tileContainer = new GameObject("tileContainer");
            ReadLevel();
        }

        void ReadLevel() 
        {
            GridPosition[] gridPosition = GameObject.FindObjectsOfType<GridPosition>();

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minZ = minX;
            float maxZ = maxX;
            float minY = minX;
            float maxY = maxX;

            for (int i = 0; i < gridPosition.Length; i++)
            {
                Transform t = gridPosition[i].transform;

                #region Read Positions
                if (t.position.x < minX)
                {
                    minX = t.position.x;
                }
                if (t.position.x > maxX)
                {
                    maxX = t.position.x;
                }

                if (t.position.z < minZ)
                {
                    minZ = t.position.z;
                }

                if (t.position.z > maxZ)
                {
                    maxZ = t.position.z;
                }

                if (t.position.y < minY)
                {
                    minY = t.position.y;
                }

                if (t.position.y > maxY)
                {
                    maxY = t.position.y;
                }

                #endregion
            }
            // Sets how many nodes we will have on the X and Z axes
            _pos_x = Mathf.FloorToInt((maxX - minX) / xzScale);
            _pos_z = Mathf.FloorToInt((maxZ - minZ) / xzScale);
            _pos_y = Mathf.FloorToInt((maxY - minY) / yScale);

            if (_pos_y == 0)
            {
                _pos_y = 1;
            }
           
            // Sets minimum position for x and z
            _minPos = Vector3.zero;
            _minPos.x = minX;
            _minPos.z = minZ;
            _minPos.y = minY;

            CreateGrid(_pos_x, _pos_z, _pos_y);
        }

        void CreateGrid(int pos_x, int pos_z, int pos_y)
        {
            grid = new Node[pos_x,pos_y, pos_z];

            for (int y = 0; y < pos_y; y++) 
            {
                for (int x = 0; x < pos_x; x++)
                {
                    for (int z = 0; z < pos_z; z++)
                    {
                        Node node = new Node();
                        node.x = x;
                        node.z = z;
                        node.y = y;

                        Vector3 tp = _minPos;
                        tp.x += x * xzScale + _offset;
                        tp.z += z * xzScale + _offset;
                        tp.y += y * yScale;

                        node.worldPosition = tp;

                        RaycastHit hit;
                        Vector3 origin = node.worldPosition;
                        origin.y += rayDownOrigin;

                        Debug.DrawRay(origin, Vector3.down * rayDownDist, Color.red, 1);
                        if (Physics.Raycast(origin, Vector3.down, out hit, rayDownDist))
                        {
                            GridObject gridObject = hit.transform.GetComponentInParent<GridObject>();
                            if (gridObject != null)
                            {
                                if (gridObject.isWalkable)
                                {
                                    node.isWalkable = true;
                                }
                            }
                            node.worldPosition = hit.point;
                        }

                        Vector3 collisionPosition = tp; // = tp + Vector3.up * .5f;
                        collisionPosition.y += collisionOffset;

                        // finds everything that collides with position of a node
                        Collider[] overlapNode = Physics.OverlapBox(collisionPosition, extends, Quaternion.identity);

                        if (overlapNode.Length > 0)
                        {
                            for (int i = 0; i < overlapNode.Length; i++) 
                            {
                                GridObject obj = overlapNode[i].transform.GetComponentInChildren<GridObject>();
                                if (obj != null)
                                {
                                    if (obj.isWalkable && node.obstacle == null) 
                                    { 
                                    }
                                    else 
                                    { 
                                        node.isWalkable = false; 
                                        node.obstacle = obj;
                                    }
                                }
                            }
                        }

                        if (node.isWalkable) 
                        {
                            GameObject go = Instantiate(tileViz, node.worldPosition + Vector3.one * .1f, Quaternion.identity) as GameObject;
                            node.tileViz = go;
                            go.transform.parent = tileContainer.transform;
                            go.SetActive(true);
                        }
                        else 
                        {
                            if (node.obstacle == null)
                            {
                                node.isAir = true;
                            }
                        }
                        _nodeViz.Add(collisionPosition);
                        grid[x, y, z] = node;
                    }
                }
            }
        }

        public Node GetNode(Vector3 worldPos)
        {
            Vector3 pos = worldPos - _minPos;
            int x = Mathf.RoundToInt(pos.x / xzScale);
            int y = Mathf.RoundToInt(pos.y / yScale);
            int z = Mathf.RoundToInt(pos.z / xzScale);

            return GetNode(x, y, z);
        }

        public Node GetNode(int x, int y, int z) 
        {
            if (x < 0 || x > _pos_x - 1 || y < 0 || y > _pos_y -1 || z < 0 || z > _pos_z - 1)
            {
                return null;
            }
            return grid[x, y, z];
        }

        private void OnDrawGizmos() 
        {
            if (visualizeCollisions) 
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < _nodeViz.Count; i++)
                {
                    Gizmos.DrawWireCube(_nodeViz[i], extends);
                }
            }
        }
    }
}
