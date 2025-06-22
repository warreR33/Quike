using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Musical : MonoBehaviour
{
    public static Musical instance;

    private void Awake()
    {
        // Singleton para evitar duplicados
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // Mantiene la música entre escenas
    }
}
