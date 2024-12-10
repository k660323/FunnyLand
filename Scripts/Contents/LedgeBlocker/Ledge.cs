using System.Collections;
using UnityEngine;

public class Ledge : MonoBehaviour
{
    LedgeBlockerScene LBS;

    IEnumerator LedgeMove;

    public int ledgeSpeed;
    [HideInInspector]
    public int comboSpeed = 1;
    [HideInInspector]
    public int blockCount;
    [HideInInspector]
    public float blockSize;
    [HideInInspector]
    public int nowBlock;

    Block[] blocks;

    float zPos;

    object _lock = new object();

    void Start()
    {
        blocks = GetComponentsInChildren<Block>();
        zPos = transform.position.z;

        LBS = Managers.Game.ContentsScene as LedgeBlockerScene;
    }

    public void Align()
    {
        blockCount = blocks.Length;

        if (blockCount == 0)
        {
            Debug.Log("Not founded Blocks!");
            return;
        }
        blockSize = blocks[0].GetComponentInChildren<BoxCollider>().transform.localScale.z;
        for (short index = 0; index < blockCount; index++)
        {
            blocks[index].transform.Translate(0, 0, -1 * index * blockSize);
            blocks[index].Init();
        }
    }

    public void Select(int _selectType)
    {
        bool result = blocks[nowBlock].Check((characterType)_selectType);
        if (result)
        {
            lock(_lock)
            {
                zPos += 2;
                if (LedgeMove == null)
                {
                    LedgeMove = Move();
                    StartCoroutine(LedgeMove);
                }
                LBS.Success();
                nowBlock = (nowBlock + 1) % blockCount;
            }
        }
        else
        {
            LBS.Fail();
        }
    }

    IEnumerator Move()
    {
        while (transform.position.z < zPos)
        {
            yield return null;
           // float comboSpeed = GameManager.Instance.combo / 5;
            transform.Translate(0, 0, (ledgeSpeed + comboSpeed) * Time.deltaTime);
        }

        transform.position = Vector3.forward * zPos;
        LedgeMove = null;
    }
}
