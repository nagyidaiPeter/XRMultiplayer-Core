using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerData : MonoBehaviour
{

    public Quaternion DefaultRotation = new Quaternion();

    void Start()
    {
        DefaultRotation = transform.localRotation;
    }

}
