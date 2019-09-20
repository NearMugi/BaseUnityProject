using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtController : MonoBehaviour
{
    [SerializeField]
    GameObject target;
    [SerializeField]
    GameObject targetChild;
    Vector3 pos;
    Animator animator;
    
    void Start()
    {
        this.animator = GetComponent<Animator>();

    }

    private void Update()
    {
        target.GetComponent<Transform>().localRotation = EventTrigger_mpu6050.Instance.TargetQuaternion;
        pos = targetChild.GetComponent<Transform>().position;
    }


    private void OnAnimatorIK(int layerIndex)
    {
        this.animator.SetLookAtWeight(1.0f, 0.8f, 1.0f, 0.0f, 0f);
        this.animator.SetLookAtPosition(pos);
    }
}