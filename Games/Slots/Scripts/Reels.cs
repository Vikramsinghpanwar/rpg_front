using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Reels : MonoBehaviour
{
    public bool _ManualSetup;
    public int reelNum;
    public Reel myReel;
    public GameObject startPosO;// Define your start position
    public GameObject endPosO; // Define your end position
    public Transform[] objectsToSpin = new Transform[4] ; // Array of objects to spin
    public float speed = 1000f;
    Vector3 startPos;
    Vector3 endPos;
    public List<GameObject> slotObjectsList;
    public List<Vector3> slotPos;
    public bool _spinning;
    public SlotMachine sMachine;

    private float[] probabilities;
    public List<ReelImages> randomList;
    ManagerSlot managerRef;

    public void RegenerateAll()
    {
        for (int i = 0; i < 4; i++)
        {
            objectsToSpin[i].GetComponent<Image>().sprite = randomList[Random.Range(0, randomList.Count)].sprite;
        }
        
    }
    public void RandomListGenerator()
    {
        randomList = new List<ReelImages>();
        for(int i = 0;i < myReel.slotImages.Count; i++)
        {
            for(int j =0; j<(myReel.slotImages[i].frequency * 300); j++)
            randomList.Add(myReel.slotImages[i]);
        }
    }

    public void SlotImageGenerator()
    {
        myReel.slotImages.Clear();

        foreach (ReelImages s in managerRef.slotElementsList)
        {
            if(reelNum == 1)
            {
                if(s.sprite.name == "wild")
                {
                    continue;
                }
            }
            if(reelNum == 4|| reelNum == 5)
            {
                if (s.sprite.name == "scatter")
                {
                    continue;
                }
            }
            myReel.slotImages.Add(s);
        }
    }

    void WarmUpCoroutines()
    {
        StartCoroutine(DummyCoroutine());
    }

    IEnumerator DummyCoroutine()
    {
        yield return null;
    }


    private void Start()
    {
        WarmUpCoroutines();
        managerRef = FindObjectOfType<ManagerSlot>();
        if (!_ManualSetup)
        {
            SlotImageGenerator();

        }
        RandomListGenerator();

        sMachine = FindObjectOfType<SlotMachine>();

         startPos = startPosO.GetComponent<Transform>().localPosition;// Define your start position
        endPos = endPosO.GetComponent<Transform>().localPosition; ; // Define your end position

        myReel.spot1Img = myReel.spot1.GetComponent<Image>();
        myReel.spot1Pos = myReel.spot1.GetComponent<Transform>().localPosition;
        myReel.spot2Img = myReel.spot2.GetComponent<Image>();
        myReel.spot2Pos = myReel.spot2.GetComponent<Transform>().localPosition;
        myReel.spot3Img = myReel.spot3.GetComponent<Image>();
        myReel.spot3Pos = myReel.spot3.GetComponent<Transform>().localPosition;
        myReel.spot4Img = myReel.spot4.GetComponent<Image>();
        myReel.spot4Pos = myReel.spot4.GetComponent<Transform>().localPosition;
        myReel.spot1Img.sprite = randomList[Random.Range(0, randomList.Count)].sprite;
        myReel.spot2Img.sprite = randomList[Random.Range(0, randomList.Count)].sprite;
        myReel.spot3Img.sprite = randomList[Random.Range(0, randomList.Count)].sprite;
        myReel.spot4Img.sprite = randomList[Random.Range(0, randomList.Count)].sprite;

        objectsToSpin = new Transform[] { myReel.spot1.GetComponent<Transform>(), myReel.spot2.GetComponent<Transform>(), myReel.spot3.GetComponent<Transform>(), myReel.spot4.GetComponent<Transform>()}; 
    
        for(int i = 0; i< slotObjectsList.Count; i++)
        {
            slotPos.Add(slotObjectsList[i].GetComponent<Transform>().localPosition);
        }

        for (int i = 0; i < 4; i++)
        {
            objectsToSpin[i].GetComponent<Image>().sprite = myReel.slotImages[Random.Range(2, myReel.slotImages.Count - 1)].sprite;

        }
    }

    public void SpinDummy(float delay, int[] finalValuesArray = null)
    {
        StartCoroutine(SpinEDummy(delay, finalValuesArray));

    }
    public void Spin(float delay, int[] finalValuesArray = null)
    {
        StartCoroutine(SpinE(delay, finalValuesArray));
    }
    IEnumerator SpinE(float delay, int[] finalValuesArray = null)
    {
        yield return new WaitForSeconds(delay);
        _spinning = true;
        if(finalValuesArray != null)
        {
            for (int i = 0; i < 4; i++)
            {
                StartCoroutine(SpinObjectsCoroutine(objectsToSpin[i], endPos, speed, i, finalValuesArray[i]));
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                StartCoroutine(SpinObjectsCoroutine(objectsToSpin[i], endPos, speed, i));
            }
        }     
    }

    IEnumerator SpinObjectsCoroutine(Transform gTransform, Vector3 targetPos, float speed, int pos, int val = -11)
    {
        int l = 0;
        bool canBrek = false;
        Vector3 currentPos;

        float distance;
        float duration;
        float timeElapsed;
        do
        {
            l++;
            currentPos = gTransform.localPosition;
            distance = Vector3.Distance(currentPos, targetPos);
            duration = distance / speed;
            timeElapsed = 0;
            do
            {
                timeElapsed += Time.deltaTime;
                float k = Mathf.Clamp01(timeElapsed / duration);
                gTransform.localPosition = Vector3.Lerp(currentPos, targetPos, k);
                if (l >= 7)
                {
                    if (Vector3.Distance(gTransform.localPosition, slotPos[pos]) < 50f)
                    {
                        canBrek = true;
                        gTransform.localPosition = slotPos[pos];
                        break;
                    }
                    if (pos == 0)
                    {
                        canBrek = true;
                        gTransform.localPosition = slotPos[pos];
                        break;
                    }
                }
                yield return null;
                if (canBrek)
                {
                    break;
                }
            }
            while (timeElapsed < duration);
            if (canBrek)
            {
                gTransform.localPosition = slotPos[pos];
                break;
            }
            else
            {
                gTransform.localPosition = startPos;
                gTransform.GetComponent<Image>().sprite = randomList[Random.Range(0, randomList.Count)].sprite;
            }
        }
        while (true);


        if(val != -11)
        {
            gTransform.GetComponent<Image>().sprite = myReel.slotImages[val].sprite;
        }




        ////////////////////

        _spinning = false;
        if(pos == 3)
        {
            sMachine.ActivateSpin();
        }
    }


    IEnumerator SpinEDummy(float delay, int[] finalValuesArray = null)
    {
        yield return new WaitForSeconds(delay);
        _spinning = true;
        if (finalValuesArray != null)
        {
            for (int i = 0; i < 4; i++)
            {
                StartCoroutine(SpinObjectsCoroutineDummy(objectsToSpin[i], endPos, speed, i, finalValuesArray[i]));
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                StartCoroutine(SpinObjectsCoroutineDummy(objectsToSpin[i], endPos, speed, i));
            }
        }

    }

    IEnumerator SpinObjectsCoroutineDummy(Transform gTransform, Vector3 targetPos, float speed, int pos, int val = -11)
    {
        Vector3 originalPos = gTransform.localPosition;
        speed = 100000;
        int l = 0;
        bool canBrek = false;
        Vector3 currentPos;

        float distance;
        float duration;
        float timeElapsed;

        for(int i = 0; i< 4; i++)
        {
            l++;
            currentPos = gTransform.localPosition;
            distance = Vector3.Distance(currentPos, targetPos);
            duration = distance / speed;
            timeElapsed = 0;
            do
            {
                timeElapsed += Time.deltaTime;
                float k = Mathf.Clamp01(timeElapsed / duration);
                
                Vector3 ps = Vector3.Lerp(currentPos, targetPos, k);
                gTransform.position = ps;
                if (l >= 4)
                {
                    if (Vector3.Distance(gTransform.localPosition, slotPos[pos]) < 50f)
                    {
                        canBrek = true;
                        gTransform.localPosition = slotPos[pos];
                        break;
                    }
                    if (pos == 0)
                    {
                        canBrek = true;
                        gTransform.localPosition = slotPos[pos];
                        break;
                    }
                }
                yield return null;
                if (canBrek)
                {
                    break;
                }
            }
            while (timeElapsed < duration);
            gTransform.localPosition = originalPos;
            gTransform.GetComponent<Image>().sprite = randomList[Random.Range(0, randomList.Count)].sprite;
        }
      

        Debug.Log("pos is : " + pos);
        if (val != -11)
        {
            gTransform.GetComponent<Image>().sprite = myReel.slotImages[val].sprite;
        }




        ////////////////////

        _spinning = false;
        if (pos == 3)
        {
            sMachine._isSpinning = false;
        }
    }



}

