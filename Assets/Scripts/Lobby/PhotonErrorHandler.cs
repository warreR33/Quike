using UnityEngine;

public class PhotonErrorHandler : MonoBehaviour
{
    private static PhotonErrorHandler instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject); 
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            Application.logMessageReceived -= HandleLog;
            instance = null;
        }
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            if (logString.Contains("Operation SetProperties") && logString.Contains("client is not connected or not ready"))
            {
                Debug.LogWarning("Sincronizando: Se Intento modificar propiedades cuando el cliente ya estaba saliendo de la sala.");
                return;
            }
        }
    }
}
