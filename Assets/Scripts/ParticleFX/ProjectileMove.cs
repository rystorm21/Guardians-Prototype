using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMove : MonoBehaviour
{
    [SerializeField]
    private float speed = 50f;
    PlayerController controller;
    Vector3 target;
    Vector3 yOffset = new Vector3 (0,1f,0);

    // Start is called before the first frame update
    void Start()
    {
        controller = GameObject.Find("Player1").GetComponent<PlayerController>();
        target = controller.TargetedEnemy.playerGameObject.transform.position;
        Debug.Log(controller.TargetedEnemy.playerGameObject.name + " Pos: " + target);
    }

    // Update is called once per frame
    void Update()
    {   
        transform.LookAt(target);
        transform.position = Vector3.MoveTowards(gameObject.transform.position, target + yOffset, speed * Time.deltaTime);
    }
}
