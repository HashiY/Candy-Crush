using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public int gridSizeX, gridSizeY; // o tamanho da grade
    //para nao ter um espaço entre as frutas (collider) para cair normalmente
    public float cellWidth = 1.1f; // largura da celula de caada grid
    public int matchMinimun = 3;
    public float delayBetweenMatches = 0.2f; // atraso entre cada match, para nao atroperar a operaçao de troca 

    private bool _canPlay;//verifica a interaçao do usuario enquanto o match estiver sendo calculado o usuario nao pode mexer nas peças

    private GameObject[] _fruits; //referencia para instanciar as frutas
    private GridItem[,] _items; // referencia para a posiçao dos itens
    private GridItem _selectedItem;

	void Start ()
    {
        _canPlay = true;
        GetFruits();
        CreateGrid();
        ClearGrid();
        GridItem.OnMouseOverItemEventHandler += OnMouseOverItem; //atribuindo um valor alem que existe na classe
	}

    private void OnDisable()
    {
        GridItem.OnMouseOverItemEventHandler -= OnMouseOverItem;
    }

    void CreateGrid()
    {
        _items = new GridItem[gridSizeX, gridSizeY]; // esta colocando as posiçoes

        for(int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                _items[x,y] = InstantiateFruit(x, y);
            }
        }
    }

    void ClearGrid()//vai limpar a lista da grade quando sumir as frutas da tela 
    {//percorre a grade , e verifica se a a fruta 
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                MatchInfo matchInfo = GetMatchInfo(_items[x, y]);//verifica se e um match ou nao
                if(matchInfo.IsMatchValid)//se representa
                {
                    Destroy(_items[x, y].gameObject);//destroi como gameobj
                    _items[x,y] =  InstantiateFruit(x, y);//instancia (substitui fisicamente e logicamente) uma nova fruta
                    y--;//para verificar novamente e ver se ocorreu corretamente 
                }
            }
        }
    }

    GridItem InstantiateFruit(int xPos, int yPos)
    {
        GameObject randomFruit = _fruits[Random.Range(0, _fruits.Length)];//achar um elememto aleatorio para instanciar
        //aleatorio, entre 0 e maximo
        //guardar o valor dessa instanciaçao - randomFruit, na posiçao 
        GridItem newFruit = ((GameObject)Instantiate(randomFruit, new Vector2(xPos * cellWidth, yPos), Quaternion.identity)).GetComponent<GridItem>();//pegar do griditem
        newFruit.OnItemPositionChanged(xPos, yPos);//colocar essa posiçao nesse metodo
        return newFruit; //para creatG
    }

    void OnMouseOverItem(GridItem item)// mesmo nome que o da outra classe
    {// para nao selecionar mais que um item quando selecionar um
        if (_selectedItem == item || _canPlay == false)  // se tem item e se a interaçao do usuario = falso
        {
            return;
        }

        if (_selectedItem == null) // se esta vazio
        {
            _selectedItem = item; // atribui o item selecionado
            _selectedItem.GetComponent<Animator>().SetTrigger("Select");
        }
        else
        {
            int xResult = Mathf.Abs(item.x - _selectedItem.x); // posiçao x dele - a posiçao anterior selecionada
            int yResult = Mathf.Abs(item.y - _selectedItem.y);//valor absoluto
            //assim nao consegue fazer a troca com outras frutas alem das q estao do lado


            if (xResult + yResult == 1)//para nao trocar com a diagonal
            {
                StartCoroutine(TryMatch(_selectedItem, item, 0.1f));//corrotina
            }

            _selectedItem.GetComponent<Animator>().SetTrigger("DeSelect");
            _selectedItem = null;
        }
    }

    IEnumerator TryMatch(GridItem a, GridItem b, float duration)//quase igual da swap
    {
        _canPlay = false;//para o usuario nao mexer nos itens e nao ter um erro de calculo
        yield return StartCoroutine(Swap(a, b, duration));//chama o swap

        MatchInfo matchInfoA = GetMatchInfo(a);
        MatchInfo matchInfoB = GetMatchInfo(b);

        if (!matchInfoA.IsMatchValid && !matchInfoB.IsMatchValid)//se nao e valido
        {
            yield return StartCoroutine(Swap(a, b, duration)); //desfazer esse swap
            _canPlay = true;//como nao e valido entao true
            yield break;
        }
        //destroi essas peças 
        if (matchInfoA.IsMatchValid)
        {
            yield return StartCoroutine(DestroyItems(matchInfoA.match));//todos os itens envolvido nesse match vao ser destruidos
            yield return new WaitForSeconds(delayBetweenMatches);//delay
            yield return StartCoroutine(UpdateGrid(matchInfoA));
        }
        //destroi essas peças 
        if (matchInfoB.IsMatchValid)
        {
            yield return StartCoroutine(DestroyItems(matchInfoB.match));
            yield return new WaitForSeconds(delayBetweenMatches);//delay
            yield return StartCoroutine(UpdateGrid(matchInfoB));
        }
        _canPlay = true;//como acabou o calculo
    }

    IEnumerator UpdateGrid(MatchInfo match)
    { // verificar se e um match horizontal ou vertical
        if(match.verticalMatchStart == match.verticalMatchEnd)//significa q e horizontal
        {
            for(int x = match.horizontalMatchStart; x <= match.horizontalMatchEnd; x++)//percorre o horizontal
            {
                for(int y = match.verticalMatchStart; y < gridSizeY -1; y++)//percorre a vertical ate 
                {
                    GridItem aboveItem = _items[x, y + 1];//o item q esta acima do match executado
                    GridItem currentItem = _items[x, y];//o item atual
                    _items[x, y] = aboveItem;//uma troca para cima 
                    _items[x, y + 1] = currentItem;//troca para desse
                    _items[x, y].OnItemPositionChanged(_items[x, y].x, _items[x, y].y - 1);//troca na logica,esta descendo(numeraçao)
                }
                _items[x, gridSizeY - 1] = InstantiateFruit(x, gridSizeY - 1);//instancia novos itens na posiçao de cima
                //fora da tela para cair ate o ponto q queremos
            }
        }// verificar se e um match horizontal ou vertical
        else if(match.horizontalMatchStart == match.horizontalMatchEnd)//significa q e vertival ,mesma coluna, mas a linha muda com os numeros
        {
            int height = 1 + (match.verticalMatchEnd - match.verticalMatchStart);//1+altura total ,ja q começa do 0

            for(int y = match.verticalMatchStart + height; y <= gridSizeY -1; y++)
            {
                GridItem belowItem = _items[match.horizontalMatchStart, y - height];//o item q esta embaixo
                GridItem current = _items[match.horizontalMatchStart, y];//atual
                _items[match.horizontalMatchStart, y - height] = current;//troca
                _items[match.horizontalMatchStart, y] = belowItem;//troca
            }
            //para nao ter um confrito de indices fazer separadamente
            for(int y = 0; y < gridSizeY - height; y++)
            {
                _items[match.horizontalMatchStart, y].OnItemPositionChanged(match.horizontalMatchStart, y);//troca logica
            }
            //instanciar os itens q foram destruidos para novas frutas
            for(int i = 0; i < match.match.Count;  i++)
            {
                _items[match.horizontalMatchStart, (gridSizeY - 1) - i] = InstantiateFruit(match.horizontalMatchStart, (gridSizeY - 1) - i);
            }
        }
        //mesmo q o gridClear
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                MatchInfo matchInfo = GetMatchInfo(_items[x, y]);
                if (matchInfo.IsMatchValid)
                {
                    yield return StartCoroutine(DestroyItems(matchInfo.match));//destroi
                    yield return new WaitForSeconds(delayBetweenMatches);//delay
                    yield return StartCoroutine(UpdateGrid(matchInfo));//chama novamente a funçao
                }
            }
        }
    }

    IEnumerator DestroyItems(List<GridItem> items)//vai destroie todos os itens da lista
    {
        foreach (GridItem i in items) //i em itens 
        {
            yield return StartCoroutine(i.transform.Scale(Vector3.zero, 0.05f));//a escala vai zerar
  /*eu*/   // yield return new WaitForSeconds(delayBetweenMatches);//delay
            Destroy(i.gameObject);
        }
    }
    //pode colocar funçoes de tempo
    IEnumerator Swap(GridItem a, GridItem b, float duration) // troca
    {
        ManagePhysics(false); // desliga a fisica

        Vector3 posA = a.transform.position; // para a posiçao da fruta que mudar
        Vector3 posB = b.transform.position;//nao ter um difrerença na posiçao apos o swap

        StartCoroutine(a.transform.Move(posB, duration));//corrotina move a funcao de extençao , vai a b
        StartCoroutine(b.transform.Move(posA, duration));//com o tempo de 0.1f

        SwapGridPos(a, b);

        yield return new WaitForSeconds(duration);//vai esperar exatamente 1s

        ManagePhysics(true); // a fisica volta

    }
    /*para conseguir fazer a troca com mais que um obj alem dos q estao ao lado
    sem essa funçao nao dava para fazer a continuaçao de trocas, so com os q estavao do lado*/
    void SwapGridPos(GridItem a, GridItem b)
    {
        GridItem tempA = _items[a.x, a.y];
        _items[a.x, a.y] = b;
        _items[b.x, b.y] = tempA;

        int aPosX = a.x, aPosY = a.y;
        //esta colocando dentro da propria funçao da classe GridItem
        a.OnItemPositionChanged(b.x, b.y);
        b.OnItemPositionChanged(aPosX, aPosY);
    }
    //verificar se tem o mesmo tipo de fruta na horizontal
    List<GridItem> CheckHorizontalMatches(GridItem item)
    {
        List<GridItem> horizontalMatches = new List<GridItem> { item };
        int left = item.x - 1, right = item.x + 1;

        while(left >= 0 && _items[left,item.y].id == item.id) //fazer a busca na esquerda
        {//se for maior ou igual a = 0 (nao ultrapassou o limite q e a ultima peça ) e o id for igual
            horizontalMatches.Add(_items[left, item.y]);
            left --;
        }

        while (right < gridSizeX && _items[right, item.y].id == item.id) //fazer a busca na direita
        {//nao pode superar a contagem de itens q temos
            horizontalMatches.Add(_items[right, item.y]);
            right++;
        }

        return horizontalMatches; // retorna a lista
    }
    //verificar se tem o mesmo tipo de fruta na vertical
    List<GridItem> CheckVerticalMatches(GridItem item)
    {
        List<GridItem> verticalMatches = new List<GridItem> { item };
        int down = item.y - 1, up = item.y + 1;

        while (down >= 0 && _items[item.x, down].id == item.id)
        {
            verticalMatches.Add(_items[item.x, down]);
            down--;
        }

        while (up < gridSizeY && _items[item.x, up].id == item.id)
        {
            verticalMatches.Add(_items[item.x, up]);
            up++;
        }

        return verticalMatches;
    }
    //coletar informaçoes
    MatchInfo GetMatchInfo(GridItem item) //Para usar as variaveis criado na outra classe MatchInfo
    {
        MatchInfo mInfo = new MatchInfo();
        mInfo.match = null; // a lista e nula, para atribuir os valores

        List<GridItem> horizontalMatch = CheckHorizontalMatches(item);// pegou a funçao
        List<GridItem> verticalMatch = CheckVerticalMatches(item);// pegou a funçao
        //se as peças > nas q tem na vertical e h >= ao minimo especificado
        if(horizontalMatch.Count > verticalMatch.Count && horizontalMatch.Count >= matchMinimun)
        {
            mInfo.horizontalMatchStart = GetHorizontalStart(horizontalMatch);
            mInfo.horizontalMatchEnd = GetHorizontalEnd(horizontalMatch);

            mInfo.verticalMatchStart = mInfo.verticalMatchEnd = horizontalMatch[0].y;//igual a 1 peça(a linha q estamos)

            mInfo.match = horizontalMatch;
        }
        else if(verticalMatch.Count >= matchMinimun)
        {
            mInfo.verticalMatchStart = GetVerticalStart(verticalMatch);
            mInfo.verticalMatchEnd = GetVerticalEnd(verticalMatch);

            mInfo.horizontalMatchStart = mInfo.horizontalMatchEnd = verticalMatch[0].x;//igual a 1 peça(a linha q estamos)

            mInfo.match = verticalMatch;
        }
        return mInfo;
    }
    //e criado essas funçoes para nao criar uma classe q faz isso e para retornao o menor e maior ...
    int GetHorizontalStart(List<GridItem> items) //pegando a peça com menor indice
    {
        float[] indexes = new float[items.Count];
        for(int i = 0; i < indexes.Length; i++)
        {
            indexes[i] = items[i].x;
        }
        return (int) Mathf.Min(indexes); // o menor float q encontrar no indice , cache para int
    }

    int GetHorizontalEnd(List<GridItem> items)//peça com maior indice
    {
        float[] indexes = new float[items.Count];
        for (int i = 0; i < indexes.Length; i++)
        {
            indexes[i] = items[i].x;
        }
        return (int)Mathf.Max(indexes);
    }

    int GetVerticalStart(List<GridItem> items)//pegando a peça com menor indice
    {
        float[] indexes = new float[items.Count];
        for (int i = 0; i < indexes.Length; i++)
        {
            indexes[i] = items[i].y;
        }
        return (int)Mathf.Min(indexes);
    }

    int GetVerticalEnd(List<GridItem> items)//peça com maior indice
    {
        float[] indexes = new float[items.Count];
        for (int i = 0; i < indexes.Length; i++)
        {
            indexes[i] = items[i].y;
        }
        return (int)Mathf.Max(indexes);
    }

    void GetFruits()
    {
        _fruits = Resources.LoadAll<GameObject>("Frutas"); // vai pegar os prefabs que estao nessa pasta
    }

    void ManagePhysics(bool state) //gerencia a fisica 
    {
        foreach(GridItem i in _items)//para cada item i dentro de item 
        {
            i.GetComponent<Rigidbody2D>().isKinematic = !state; // desliga a fisica
        }
    }
}
