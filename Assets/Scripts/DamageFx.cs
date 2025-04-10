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
        //Si el objeto no es del jugador local, lo desactiva completamente asi no mostramos efectos en los demas players
        if (!photonView.IsMine)
        {
            gameObject.SetActive(false);
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
        //evita que el enemigo vea el flash de otro jugador
        if (!photonView.IsMine) return;

        //Si esta en curso la resetea
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);

        }

        flashCoroutine = StartCoroutine(Flash());
    }



    private IEnumerator Flash()
    {
        damageImage.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        damageImage.color = Color.clear;
    }
}
