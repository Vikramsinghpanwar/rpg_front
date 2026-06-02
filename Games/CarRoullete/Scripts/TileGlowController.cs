using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
[System.Serializable]
public class SymbolIndex
{
    public string sybmol;
    public int[] index;
}
public class TileGlowController : MonoBehaviour
{
    public bool _tile;
    public Image[] tiles;
    public CarRoulleteManager managerScript;
    //public AudioSource tickSound;
    public AudioSource wheelSound;
    public TextMeshProUGUI timerTxt;
    public Animator bgAnimation;
    int currentIndex;
    Color transparentColor;
    public Color transparentColor1;
    public Color transparentColor2;
    public Color transparentColor3;
    Color OpaqueColor;
    public SymbolIndex[] symbolIndexList = new SymbolIndex[8] ;

    private void Start()
    {
        managerScript = FindObjectOfType<CarRoulleteManager>();
        transparentColor.a = 0f;       
        OpaqueColor = new Color(255f, 255f, 255f, 255f);
    }
    public void TileStart(int stopAt)
    {
        _tile = true;
        wheelSound.Play();
        StartCoroutine(StartTileGlow(stopAt));
    }

    public void ToolFalse()
    {
        _tile = false;
    }
    public IEnumerator CountDown(int duration)
    {
        int myDuration = duration;
        for(int i = 0; i<=duration; i++)
        {
            timerTxt.text = myDuration.ToString();
            myDuration = duration - i;
            yield return new WaitForSeconds(1);
        }
        timerTxt.text = "";
    }
    IEnumerator StartTileGlow(int stopAtIndexArray)
    {
        Debug.Log("'ll stop at : " + stopAtIndexArray);
        bgAnimation.SetBool("_is", true);
        currentIndex = 0;
        //
        for(int i = 0; i< 5; i++)
        {
            yield return StartCoroutine(GlowTile(currentIndex, 0.3f));
            currentIndex = (currentIndex + 1) % tiles.Length;
        }
        currentIndex = (currentIndex + 4) % tiles.Length;

        //StartCoroutine(GlowDurationChanger());
        Invoke("ToolFalse", Random.Range(11f, 12f));
        /*  while (glowDuration != 0)
          {
              yield return StartCoroutine(GlowTile(currentIndex));
              currentIndex = (currentIndex + 1) % tiles.Length;
          }*/
        int rand = Random.Range(80, 120);
        for (int i = 0; i < rand; i++)
        {
            yield return StartCoroutine(GlowTile(currentIndex, 0.05f, true));
            currentIndex = (currentIndex + 1) % tiles.Length;
        }
       
        do
        {
            yield return StartCoroutine(GlowTile(currentIndex, 0.05f, true));
            currentIndex = (currentIndex + 1) % tiles.Length;
            Debug.Log("i'm at " + (currentIndex + 7));
        }
        while (stopAtIndexArray != ( currentIndex + 7));
      


        for (int i = 0; i < 7; i++)
        {
            yield return StartCoroutine(GlowTile(currentIndex, 0.2f));
            currentIndex = (currentIndex + 1) % tiles.Length;
        }

     
        tiles[currentIndex].color = Color.yellow;
        StartCoroutine(managerScript.WinnerDeclare(currentIndex));
        wheelSound.Stop();
        bgAnimation.SetBool("_is", false);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            currentIndex = 3;
        }
    }


    /*IEnumerator GlowDurationChanger()
    {
        glowDuration = .5f;
        yield return new WaitForSeconds(1);
        glowDuration = .25f;
        yield return new WaitForSeconds(1);
        glowDuration = .125f;
        yield return new WaitForSeconds(1);
        glowDuration = .05f;
        yield return new WaitForSeconds(Random.Range(0.5f,1.5f));
        glowDuration = .025f;
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        glowDuration = .0125f;
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        glowDuration = .025f;
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        glowDuration = .05f;
        yield return new WaitForSeconds(0.5f);
        glowDuration = .125f;
        yield return new WaitForSeconds(0.5f);
        glowDuration = .2f;

        yield return new WaitForSeconds(0.5f);
        glowDuration = .3f;

        yield return new WaitForSeconds(0.4f);
        glowDuration = .4f;

        yield return new WaitForSeconds(0.3f);
        glowDuration = .5f;

        yield return new WaitForSeconds(0.2f);
        glowDuration = .75f;
        yield return new WaitForSeconds(0.5f);
        glowDuration = 0f;


    }*/




    IEnumerator GlowTile(int index, float glowDuration, bool _makeTrail = false)
    {
        Image image = tiles[index];
        Image image1 = null;
        Image image2 = null;
        Image image3 = null;
        if (index < 3)
        {
            switch (index)
            {
                case 0:
                    image1 = tiles[24 - 1];
                    image2 = tiles[24 - 2];
                    image3 = tiles[24 - 3];
                    break;
                case 1:
                    image1 = tiles[index - 1];
                    image2 = tiles[24 - 2];
                    image3 = tiles[24 - 3];
                    break;
                case 2:
                    image1 = tiles[index - 1];
                    image2 = tiles[index - 2];
                    image3 = tiles[24 - 3];
                    break;
            }
        }
        else
        {
            image1 = tiles[index - 1];
            image2 = tiles[index - 2];
            image3 = tiles[index - 3];
        }
       
  

        if (_makeTrail)
        {            
            image1.color = transparentColor1;
            image2.color = transparentColor2;
            image3.color = transparentColor3;          
        }
  
        image.color = OpaqueColor;
        yield return new WaitForSeconds(glowDuration);
        // Reset to the original color
        image.color = transparentColor;
        if (_makeTrail)
        {
            image1.color = transparentColor;
            image2.color = transparentColor;
            image3.color = transparentColor;
        }

    }
}
