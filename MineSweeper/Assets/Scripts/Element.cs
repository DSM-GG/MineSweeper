using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Element : MonoBehaviour
{
    public Sprite[] ChangeSpriteArray = null;

    private SpriteRenderer spr;
    private GridManager GridLinkManager= null;

    public bool IsMine; 

    public void SetElementDatas(bool p_ismine)
    {
        IsMine = p_ismine;
    }
    private void Start()
    {
        spr = GetComponent<SpriteRenderer>();
        GridLinkManager = FindObjectOfType<GridManager>();
    }

    public void SetChangeTexture(int p_index)
    {
        spr.sprite = ChangeSpriteArray[p_index];
    }
    void OnMouseDown()
    {
        if (GridLinkManager.canTouch)
        {
            if(GetComponent<SpriteRenderer>().sprite==ChangeSpriteArray[10]) 
                SetChangeTexture(0);   
        }
    }
    

    void OnMouseUp()
    {
        if (GridLinkManager.canTouch)
        {
            if (IsMine)
            {
                GridLinkManager.Dead();
                GridLinkManager.canTouch = false;
                SetChangeTexture(9);
                GridLinkManager.ResetGame();
            }
            else
            {
                //주변 지뢰갯수 찾기
                int x = Mathf.RoundToInt(transform.localPosition.x / GetSpriteSize(gameObject).x);
                int y = Mathf.RoundToInt(transform.localPosition.y / GetSpriteSize(gameObject).y);

                //지뢰갯수만큼 텍스처 변경
                SetChangeTexture(GridLinkManager.GetRoundMines(x, y));

                //재귀함수로 지뢰가 주변 두칸이내로 없는칸 없앰
                GridLinkManager.FFunCover(x, y, new bool[GridLinkManager.WidthBlock, GridLinkManager.HeightBlock]);

                if (GridLinkManager.IsFinished())
                {
                    GridLinkManager.sunGlass();
                    GridLinkManager.canTouch = false;
                    GridLinkManager.ResetGame();
                }
            }
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
    
    void OnDrawGizmos()
    {
        if (IsMine)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(this.transform.position,new Vector3(0.1f,0.1f,0.5f));
        }
    }

    public void Flag()
    {
        if (GetComponent<SpriteRenderer>().sprite == ChangeSpriteArray[11])
                SetChangeTexture(10);
        else if (GetComponent<SpriteRenderer>().sprite == ChangeSpriteArray[10])
            SetChangeTexture(11);
    }
}
