using System.Collections;
using System.Collections.Generic;
// Create this as: Assets/_Emergency/MiniCardSystem.cs
using UnityEngine;

[DisallowMultipleComponent]
public class MiniCardSystem : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            Debug.Log("EMERGENCY SYSTEM RESPONDING");
    }
}
