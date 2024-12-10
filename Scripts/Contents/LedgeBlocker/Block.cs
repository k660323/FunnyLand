using System.Collections;
using UnityEngine;

public enum characterType
{
    Yellow,
    Mint,
    Purple,
}

public class Block : MonoBehaviour
{
    public Rigidbody[] characters;
    public characterType curType;

    Ledge ledge;
    void Start()
    {
        ledge = GetComponentInParent<Ledge>();
    }

    void LateUpdate()
    {
        if (transform.position.z >= 4)
        {
            transform.Translate(0, 0, -1 * ledge.blockCount * ledge.blockSize);
            Init();
        }
    }

    public void Init()
    {
        curType = (characterType)Random.Range(0, characters.Length);

        for (int index = 0; index < characters.Length; index++)
        {
            characters[index].gameObject.SetActive(curType == (characterType)index);
        }
        StartCoroutine(InitPhysics());
    }

    IEnumerator InitPhysics()
    {
        characters[(int)curType].GetComponent<Rigidbody>().isKinematic = true;
        yield return new WaitForFixedUpdate();
        characters[(int)curType].velocity = Vector3.zero;
        characters[(int)curType].angularVelocity = Vector3.zero;
        yield return new WaitForFixedUpdate();
        characters[(int)curType].transform.localPosition = Vector3.zero;
        characters[(int)curType].transform.rotation = Quaternion.identity;
    }

    public bool Check(characterType _selectType)
    {
        bool result = (curType == _selectType);

        if (result)
            StartCoroutine(Hit());

        return result;
    }

    IEnumerator Hit()
    {
        characters[(int)curType].GetComponent<Rigidbody>().isKinematic = false;
        yield return new WaitForFixedUpdate();
        
        int ran = Random.Range(0, 2);
        Vector3 forceVec;
        Vector3 torqueVec;
        switch (ran)
        {
            case 0:
                forceVec = (Vector3.right + Vector3.up) * 4f;
                torqueVec = (Vector3.forward + Vector3.down) * 4f;
                characters[(int)curType].AddForce(forceVec, ForceMode.Impulse);
                characters[(int)curType].AddTorque(torqueVec, ForceMode.Impulse);
                break;

            case 1:
                forceVec = (Vector3.left + Vector3.up) * 4f;
                torqueVec = (Vector3.back + Vector3.up) * 4f;
                characters[(int)curType].AddForce(forceVec, ForceMode.Impulse);
                characters[(int)curType].AddTorque(torqueVec, ForceMode.Impulse);
                break;
        }
    }
}
