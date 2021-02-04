using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//usado para ser um codigo de extensao para o gameManager
public static class Extensions // precisa ser statico
{
    //pode colocar funçoes de tempo , para o swap/ esta referenciando esta transforme, posiçao , duraçao
    public static IEnumerator Move(this Transform t, Vector3 pos, float duration)
    {                                 //posi inicial     posi final
        Vector3 direction = pos - t.position; // direçao do move 
        float distance = direction.magnitude;//valor mediano de todos os eixos
        direction.Normalize();// pra nenhum numero ser maior que 1 pois e so a direçao

        float startTime = 0; // time q inclementa com os acontecimentos

        while(startTime < duration) // se for menor continua 
        {
            float remainingDistance = (distance * Time.deltaTime) / duration;//cresa de acordo com
            t.position += direction * remainingDistance; // 
            startTime += Time.deltaTime;//avançar com o tempo
            yield return null;
        }
        t.position = pos;//depois q fez a animaçao pega o valor extaco q foi criado
    }
    //pode colocar funçoes de tempo
    public static IEnumerator Scale(this Transform t, Vector3 scale, float duration)
    {
        Vector3 direction = scale - t.localScale; //direçao para onde vai escalar
        float size = direction.magnitude;//tamanho q vai escalar
        direction.Normalize();

        float startTime = 0;

        while (startTime < duration)
        {
            float remainingDistance = (size * Time.deltaTime) / duration;
            t.localScale += direction * remainingDistance;
            startTime += Time.deltaTime;
            yield return null;
        }
        t.localScale = scale;
    }

}
