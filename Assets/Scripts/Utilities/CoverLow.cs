using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    public enum CoverType
    {
        highCover,
        lowCover
    }

    public class CoverLow : MonoBehaviour
    {
        [SerializeField]
        SessionManager sessionManager;
        int cover = ((int)CoverType.lowCover);
        bool coverHit;

        private void OnTriggerEnter(Collider other)
        {
            coverHit = true;
        }

        public int GetCoverType()
        {
            return cover;
        }

        public bool CoverHit
        {
            get
            {
                return coverHit;
            }
            set
            {
                this.coverHit = value;  
            }
        }
    }

}
