using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Manager : MonoBehaviour
{

    public static UI_Manager instance;

    private void Start()
    {

        if (instance != null)
            Destroy(gameObject);

        instance = this;

    }

}
