using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberScript : MonoBehaviour {
    public int ButtonIndex;
    TextMesh TextMesh;
    private void Awake()
    {
        TextMesh = GetComponentInChildren<TextMesh>();
    }
}
