using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
    public bool canTouch = true;
    private Element CloneBlock;
    public Element CloneBlockeasy, CloneBlockmiddle, CloneBlockhard; //복제할 블록
    public int WidthBlock = 10; //가로
    public int HeightBlock = 13; //세로
    public int CountOfMines = 10; //지뢰의 갯수
    public int RemainCountOfMines = 0; //남은 지뢰의 갯수
    public Text time;
    public Element[,] ElementArray = null;
    float xSize,ySize;

    public GameObject btns;

    public int easyX, easyY, easyMine;
    public int middleX, middleY, middleMine;
    public int hardX, hardY, hardMine;
    public GameObject SettingBtn;
    public Sprite ClearSprite;
    public Sprite DeadSprite;
   public void Easy()
   {
       CloneBlock = CloneBlockeasy;
       CountOfMines = easyMine;
       WidthBlock = easyX; 
       HeightBlock = easyY; 
       StartGame();
   }
   public void Middle()
   {
       CloneBlock = CloneBlockmiddle;
       CountOfMines = middleMine;
       WidthBlock = middleX; 
       HeightBlock = middleY; 
       StartGame();
   }
   public void Hard()
   {
       CloneBlock = CloneBlockhard;
       CountOfMines = hardMine;
       WidthBlock = hardX; 
       HeightBlock = hardY; 
       StartGame();
   }
   void StartGame()
   {
       StartCoroutine(timer());
       transform.position=Vector3.zero;
       SettingBtn.SetActive(true);
       btns.SetActive(false);
       ElementArray=new Element[WidthBlock,HeightBlock];
       xSize=GetSpriteSize(CloneBlock.gameObject).x;
       ySize=GetSpriteSize(CloneBlock.gameObject).y;
       transform.Translate(xSize*-0.5f*(WidthBlock-1),ySize*-0.5f*(HeightBlock+3),0);
       GeneratorMineSweeper();
   }
   Element CloneElement(int p_x,int p_y)
    {
        GameObject copyObj = GameObject.Instantiate(CloneBlock.gameObject);
        copyObj.transform.SetParent(this.transform);
        
        Vector2 tempPos = Vector2.zero;
        
        copyObj.transform.SetParent(this.transform);
        tempPos.Set(transform.position.x+p_x*xSize,transform.position.y+ p_y*ySize);
        copyObj.transform.position = tempPos;
        copyObj.name = "CloneBlock_" + p_x.ToString() + "_" + p_y.ToString();

        return copyObj.GetComponent<Element>();
    }
    void GeneratorMineSweeper()
    {
        for (int yy = 0; yy < HeightBlock; yy++)
        {
            for (int xx = 0; xx < WidthBlock; xx++)
            {
                ElementArray[xx, yy]=CloneElement(xx,yy);
            }
        }
        //지뢰세팅
        SetMineSetting();
        RemainCountOfMines = CountOfMines; //남은 지뢰갯수 넣음
    }

    public void SetFlag()
    {
        RemainCountOfMines--;
    }

    public void DeleteFlag()
    {
        RemainCountOfMines++;
    }
    List<Element> m_TempElementList=new List<Element>();
    void SetMineSetting()
    {
        foreach (var item in ElementArray)
       {
           m_TempElementList.Add(item); //배열에있는칸들을리스트로 옮김
       }

       int randomIndex = -1;
       for (int i = 0; i < CountOfMines; i++)
       {
           randomIndex = Random.Range(0, m_TempElementList.Count); //칸중에 하나고름
           m_TempElementList[randomIndex].SetElementDatas(true); //그칸을 지뢰로 만듬
           m_TempElementList.RemoveAt(randomIndex); //중복을 없애기 위해 이미 지뢰가 된 칸은 리스트에서 삭제
       }
    }

    public Vector3 GetSpriteSize(GameObject _target)
    {
        Vector3 worldSize = Vector3.zero;
        if(_target.GetComponent<SpriteRenderer>())
        {
            Vector2 spriteSize = _target.GetComponent<SpriteRenderer>().sprite.rect.size;
            Vector2 localSpriteSize = spriteSize / _target.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit;
            worldSize = localSpriteSize;
            worldSize.x *= _target.transform.lossyScale.x;
            worldSize.y *= _target.transform.lossyScale.y;
        }
        else
        {
            Debug.Log ("SpriteRenderer Null");
        }
        return worldSize;
    }

    bool GetMineAt(int p_x,int p_y)
    {
        if ((p_x >= 0 && p_x < WidthBlock)
            && (p_y >= 0 && p_y < HeightBlock))
        {
            return ElementArray[p_x, p_y].IsMine;
        }
        return false;
    }

    public bool IsFinished()
    {
        foreach (var item in ElementArray)
        {
            //아직 까지 않았는데 지뢰가 아닌게 하나도 없으면
            if (!item.IsMine&&item.GetComponent<SpriteRenderer>().sprite==item.ChangeSpriteArray[10])
                return false; //아직 클리어 아님
        }
        return true;
    }
    public void FFunCover(int p_x,int p_y, bool[,] p_visited)
    {
        StartCoroutine(delayFFunCover(p_x,p_y,p_visited));
    }

    public void Dead()
    {
        SettingBtn.GetComponent<Image>().sprite = DeadSprite;
    }
    IEnumerator delayFFunCover(int p_x,int p_y, bool[,] p_visited)
    {
        if ((p_x >= 0 && p_x < WidthBlock)
         && (p_y >= 0 && p_y < HeightBlock))
        {
            if (p_visited[p_x, p_y])
                yield break;
        
            int aroundCount = GetRoundMines(p_x, p_y);
            yield return new WaitForSeconds(0.02f);
            ElementArray[p_x,p_y].SetChangeTexture(aroundCount);
            if(aroundCount>0)
                yield break;

            p_visited[p_x, p_y] = true;
        
            FFunCover(p_x+1,p_y,p_visited);
            FFunCover(p_x-1,p_y,p_visited);
            FFunCover(p_x,p_y+1,p_visited);
            FFunCover(p_x,p_y-1,p_visited);
        }
        
    }
    public int GetRoundMines(int p_x, int p_y)
    {
        int outcount = 0;
        //상단
        if (GetMineAt(p_x - 1, p_y + 1)) outcount++;
        if (GetMineAt(p_x, p_y + 1)) outcount++;
        if (GetMineAt(p_x +1, p_y + 1)) outcount++;
        //가운데
        if (GetMineAt(p_x - 1, p_y)) outcount++;
        if (GetMineAt(p_x +1, p_y)) outcount++;
        //하단
        if (GetMineAt(p_x - 1, p_y - 1)) outcount++;
        if (GetMineAt(p_x, p_y - 1)) outcount++;
        if (GetMineAt(p_x +1, p_y - 1)) outcount++;
      
        return outcount;
    }

    public void ResetGame()
    {
        StartCoroutine(reset());
    }
    public void NodelayResetGame()
    {
        if(canTouch) 
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    IEnumerator reset()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void sunGlass()
    {
        SettingBtn.GetComponent<Image>().sprite = ClearSprite;
    }


    private void Update()
    {
        if (canTouch)
        {
            if(Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();
            if(Input.GetKeyDown(KeyCode.Space))
                StartGame();
                if (Input.GetMouseButtonUp(1))
            {
                ElementArray[
                    Mathf.RoundToInt((Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).x /
                                     xSize),
                    Mathf.RoundToInt((Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).y /
                                     ySize)].Flag();
            }
        }
    }

    IEnumerator timer()
    {
        int i = 0;
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (canTouch)
            {
                i++;
                time.text = i / 60 + " : " + i % 60;   
            }
        }
    }
}
