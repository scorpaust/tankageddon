using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lifetime : MonoBehaviour
{
    [SerializeField]
    private float lifetime = 1f;

    // Start is called before the first frame update
    private void Start()
    {
        Destroy(this.gameObject, lifetime);
    }
}
