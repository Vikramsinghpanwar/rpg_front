using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeopleBetBac : MonoBehaviour
{
    public Animator coverImgAnim;
    public List<int> avoid;
    public float area;
    public List<GameObject> coinPrefabList, coinsMoved;
    public List<GameObject> destinationObject;
    public List<Vector3> destinationList;
    public float speed;
    public List<Transform> coinsToMoveList;
    public GameObject parentObject;
    public bool _isAnim;
    public Vector3 myPos;
    public float gapU = 0.5f;
    public float gapD = 0.5f;
    public int maxCoinsToInstantiate = 20;
    public AudioSource coinSound;
    // Start is called before the first frame update
    void Start()
    {
        coinSound = gameObject.GetComponent<AudioSource>();
        destinationList = new List<Vector3>();
        foreach (Transform child in transform)
        {
            destinationObject.Add(child.gameObject);
        }
        foreach (GameObject g in destinationObject)
        {
            destinationList.Add(g.GetComponent<Transform>().localPosition);
        }
        
        avoid = new List<int>();
        myPos = Vector3.zero;
        for (int i = 0; i < coinPrefabList.Count; i++)
        {
            InstantiatePrefabsInParent(coinPrefabList[i]);
        }
    }

    public void MoveAllcoinsBack()
    {
        foreach (GameObject child in coinsMoved)
        {
            Transform myTrnasform = child.GetComponent<Transform>();
            StartCoroutine(MoveObjectBack(myTrnasform.localPosition, myPos, myTrnasform));
        }
    }
    void InstantiatePrefabsInParent(GameObject prefabObject)
    {
        for (int i = 0; i < maxCoinsToInstantiate; i++)
        {
            GameObject instantiatedPrefab = Instantiate(prefabObject, parentObject.transform);
            instantiatedPrefab.GetComponent<Transform>().localPosition = Vector3.zero;
            coinsToMoveList.Add(instantiatedPrefab.GetComponent<Transform>());

        }
    }
    public IEnumerator AnimStart()
    {
        _isAnim = true;
        avoid.Clear();

        yield return new WaitForSeconds(1f);
        StartCoroutine(Wave());
    }

    public IEnumerator Wave()
    {
        int k = 0, d = 0, c_num = 0;
        d = Random.Range(0, destinationList.Count);
        c_num = Random.Range(0, coinsToMoveList.Count);
        while (avoid.Contains(c_num) && !(avoid.Count == coinPrefabList.Count))
        {
            c_num = Random.Range(0, coinsToMoveList.Count);
            if (avoid.Count == coinPrefabList.Count) break;
        }
        coinSound.Play();
        StartCoroutine(MoveObject(myPos, destinationList[d], coinsToMoveList[c_num]));
        coverImgAnim.SetBool("_is", true);
        avoid.Add(c_num);
        coinsMoved.Add(coinsToMoveList[c_num].gameObject);
        k++;
        yield return new WaitForSeconds(Random.Range(gapD, gapU));

        if (_isAnim && !(coinsMoved.Count == coinsToMoveList.Count))
        {
            StartCoroutine(Wave());
        }
        yield return new WaitForSeconds(0.5f);
        coverImgAnim.SetBool("_is", false);

    }

    public void AnimStop()
    {
        _isAnim = false;
    }

    IEnumerator MoveObject(Vector3 initialPos, Vector3 targetPos, Transform coinTransform)
    {
        Vector3 targetPosModified = targetPos + new Vector3(Random.Range(-area, area), Random.Range(-area, area), 0);

        float startTime = Time.time;
        float journeyLength = Vector3.Distance(initialPos, targetPosModified);

        while (Time.time - startTime < journeyLength / speed)
        {
            float fraction = (Time.time - startTime) / (journeyLength / speed);
            coinTransform.localPosition = Vector3.Lerp(initialPos, targetPosModified, fraction);
            yield return null;
        }
        coinTransform.localPosition = targetPosModified;
        yield return new WaitForSeconds(2f);
        coinTransform.localPosition = myPos;
    }

    IEnumerator MoveObjectBack(Vector3 initialPos, Vector3 targetPos, Transform coinTransform)
    {

        float startTime = Time.time;
        float journeyLength = Vector3.Distance(initialPos, targetPos);

        while (Time.time - startTime < journeyLength / speed)
        {
            float fraction = (Time.time - startTime) / (journeyLength / 0.1f);
            coinTransform.localPosition = Vector3.Lerp(initialPos, targetPos, fraction);
            yield return null;
        }
        coinTransform.localPosition = targetPos;

     

    }

}
