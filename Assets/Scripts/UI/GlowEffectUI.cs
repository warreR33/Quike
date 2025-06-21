using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlowEffectUI : MonoBehaviour
{
    public Image image; 
    public float minAlpha = 0.6f;
    public float maxAlpha = 1.0f;
    public float speed = 2f;

    void Update()
    {
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(Time.time * speed, 1));
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }
}
