using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepAlive : MonoBehaviour
{
    public GameObject keepAlive;
    List<GameObject> objectsToKeepAlive = new List<GameObject>();
    
    private void Start() 
    {
        keepAlive = GameObject.Find("KeepAlive");
        AddToList(keepAlive);
        DontDestroyList(objectsToKeepAlive);
    }

    void AddToList(GameObject objectToAdd)
    {
        objectsToKeepAlive.Add(objectToAdd);
    }

    void DontDestroyList(List<GameObject> dontDestroyList)
    {
        foreach (GameObject go in dontDestroyList)
        {
            DontDestroyOnLoad(go);
        }
    }
}
