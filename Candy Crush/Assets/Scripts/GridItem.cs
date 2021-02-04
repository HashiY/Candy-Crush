using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//usado para os items se encaixarem em uma grade
public class GridItem : MonoBehaviour
{ // para outras classes nao alterar os int x e y
    public int x //encapsulada
    {
        get; // pc para mostrar o valor da variada para quem consultar
        private set; // palavra cheve para ser modificada
    }

    public int y
    {
        get;
        private set;
    }

    public int id; // identificar os tipos diferentes de frutas q esta sendo trabalhado

    //metodo de chamada de fora de algo especifico q ocorrera em um momento
    public void OnItemPositionChanged(int newX, int newY)//qunado a posiçao mudar ,a classe vai gerenciar a grade e as regras
    {
        x = newX;
        y = newY;
        gameObject.name = string.Format("Sprite [{0}] [{1}]", x, y);//nome,x=0,y=1
        //para ter um fidback visual da grade e qual elemento esta sendo especificicado
    }

    private void OnMouseDown()//clica o obj e os eventos sao chamados
    {
        if(OnMouseOverItemEventHandler != null)
            OnMouseOverItemEventHandler(this);//recebe uma variavel do tipo item q e ela mesmo
    }
    //tipo especifico q pode receber informaçoes de outros metodos/ quando o mause estiver sobre o item
    public delegate void OnMouseOverItem(GridItem item);//tipo de obj q aceita a ser guardado um metodo especifico
    public static event OnMouseOverItem OnMouseOverItemEventHandler;
    //estancia e acessado por outras classes com evento para receber informaçoes de outros metodos  
    //gerenciador de evento EventHandler para colocar metodos dentro
}
