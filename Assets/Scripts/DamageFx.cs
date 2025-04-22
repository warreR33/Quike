using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


public class DamageFx : MonoBehaviourPun
{
    public Image damageImage;

    public float flashDuration = 0.2f;
    public Color flashColor = new Color(1f, 0f, 0f, 0.4f);



    private Coroutine flashCoroutine;



    private void Start()
    {
        
        //Si no es el jugador local no hacemos nada
        if (!photonView.IsMine)
        {
            return;
        }
        //Si es el nuestro reseteamos el color de la imagen por las
        else if (damageImage != null)
        {
            damageImage.color = Color.clear;
        }
    }

    //la llamamos cuando recibimos daño desde PlayerHealth
    public void ShowDamage()
    {
        if (!photonView.IsMine) return;

        if (damageImage == null || !damageImage.gameObject.activeInHierarchy)
        {
            return;
        }

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(Flash());
    }



    private IEnumerator Flash()
    {
        damageImage.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        damageImage.color = Color.clear;
    }
}
