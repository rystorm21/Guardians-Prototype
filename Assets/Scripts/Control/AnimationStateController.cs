using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimationStateController : MonoBehaviour
{
    const float locomtionAnimationSmoothTime = .1f;
    public Animator animator;
    public NavMeshAgent playerNavMesh;

    private Vector3 lastPosition;

    // Start is called before the first frame update
    void Start()
    {
        animator.GetComponent<Animator>();
        playerNavMesh = gameObject.GetComponentInParent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        Locomotion();
    }

    void Locomotion() {
        float speedPercent = playerNavMesh.velocity.magnitude / playerNavMesh.speed;
        animator.SetFloat("percentSpeed", speedPercent, locomtionAnimationSmoothTime, Time.deltaTime);
    }
}
