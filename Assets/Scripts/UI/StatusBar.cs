using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EV
{
    public class StatusBar : MonoBehaviour
    {
        public GameObject imageTemplate;
        public GridCharacter thisCharacter;

        List<GameObject> imageList = new List<GameObject>();
        GameObject image;
        SessionManager sessionManager;
        EV.Characters.Character character;

        private void OnEnable()
        {
            sessionManager = GameObject.Find("Grid Manager").GetComponent<SessionManager>();
            thisCharacter = this.transform.parent.gameObject.transform.parent.gameObject.GetComponent<GridCharacter>();
            
            if (sessionManager.currentCharacter == null)
                return;
            character = sessionManager.currentCharacter.character;

            if (sessionManager != null && sessionManager.currentCharacter != null)
            {
                for (int i = 0; i < thisCharacter.character.appliedStatus.Count; i++)
                {
                        image = Instantiate(imageTemplate, transform);
                        image.GetComponent<Image>().color = thisCharacter.character.appliedStatus[i].statusColor;
                        imageList.Add(image);
                }
            }
        }

        private void OnDisable()
        {
            if (imageList != null)
            {
                foreach (var image in imageList)
                {
                    Destroy(image);
                }
            }
        }
    }
}
