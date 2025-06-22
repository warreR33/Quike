using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  
public class DeadUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI killerNameText;



    public void Init(string killerName)
    {
        if (killerNameText != null)
            killerNameText.text = $"Eliminated by: {killerName}";
     
    }

  
}

