﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMeInvisibleOnStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>().enabled =false;
    }

}
