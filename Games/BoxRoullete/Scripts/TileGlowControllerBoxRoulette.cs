using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TileGlowControllerBoxRoullete : MonoBehaviour
{
    public AudioSource watchCountDownSound;
    public bool _tile;
    public Image[] tiles;
    public List<AudioSource> tilesAudio;
    public AudioClip tapSound;
    public float glowDuration = 1f;
    public ManagerBRoullete managerScript;
    //public AudioSource tickSound;
    //public TextMeshProUGUI timerTxt;
    public TextMeshProUGUI timerTxt;
    public float s = 0;


    Color transparentColor;
    public Color transparentColor1;
    public Color transparentColor2;
    public Color transparentColor3;
    Color OpaqueColor;
    public SymbolIndex[] symbolIndexList = new SymbolIndex[8];


    private void Start()
    {
        managerScript = FindObjectOfType<ManagerBRoullete>();
        foreach(Image i in tiles)
        {
            tilesAudio.Add(i.gameObject.GetComponent<AudioSource>());
        }
        foreach(AudioSource a in tilesAudio)
        {
            a.clip = tapSound;
        }

        transparentColor.a = 0f;
        OpaqueColor = Color.white;
    }

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            s += 0.5f;
        }
    }
    public void TileStart(int stopAt, int randVal)
    {
        _tile = true;
        StartCoroutine(StartTileGlow(symbolIndexList[stopAt].index[0], randVal, s));
    }


    IEnumerator StartTileGlow(int stopAtIndex,int randVal, float val = 0)
    {
        int currentIndex = 0;
        if (val == 0)
        {
            Invoke("ToolFalse", Random.Range(9f, 10f));
        }
        else
        {
            Invoke("ToolFalse", 8 + val);
        }
        for (int i = 0; i < 5; i++)
        {
            yield return StartCoroutine(GlowTile(currentIndex, 0.3f));
            currentIndex = (currentIndex + 1) % tiles.Length;
        }
        currentIndex = (currentIndex + 4) % tiles.Length;

        Invoke("ToolFalse", Random.Range(11f, 12f));
 
        int rand = Random.Range(80, 120);
        for (int i = 0; i < rand; i++)
        {
            yield return StartCoroutine(GlowTile(currentIndex, 0.03f, true));
            currentIndex = (currentIndex + 1) % tiles.Length;
        }
        int ss = stopAtIndex;
        if(stopAtIndex < 7)
        {
            ss += 28;
        }
        do
        {
            yield return StartCoroutine(GlowTile(currentIndex, 0.05f, true));
            currentIndex = (currentIndex + 1) % tiles.Length;
        }
        while (ss != currentIndex + 7);


        int k = 7 + randVal;
        for (int i = 0; i < k; i++)
        {
            yield return StartCoroutine(GlowTile(currentIndex, 0.2f, true));
            currentIndex = (currentIndex + 1) % tiles.Length;
        }

        tiles[currentIndex].color = Color.yellow;
        StartCoroutine(managerScript.WinnerDeclare(currentIndex));

    }


    public void ToolFalse()
    {
        _tile = false;
    }
    public IEnumerator CountDown(int duration)
    {
        for(int i = 0; i<=duration; i++)
        {
            yield return new WaitForSeconds(1);

            timerTxt.text = "Roullete " + (duration - i).ToString();
            if(duration-1 < 5)
            {
                watchCountDownSound.Play();
            }
        }


        timerTxt.text = "Roullete";
        yield return new WaitForSeconds(1f);
        duration = 13;
        for (int i = 0; i <= duration; i++)
        {
            yield return new WaitForSeconds(1);

            timerTxt.text = "Drawing " + (duration - i).ToString();
            if (duration - 1 < 5)
            {
                watchCountDownSound.Play();
            }
        }
        timerTxt.text = "Roullete";
    }





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
                    image1 = tiles[28 - 1];
                    image2 = tiles[28 - 2];
                    image3 = tiles[28 - 3];
                    break;
                case 1:
                    image1 = tiles[index - 1];
                    image2 = tiles[28 - 2];
                    image3 = tiles[28 - 3];
                    break;
                case 2:
                    image1 = tiles[index - 1];
                    image2 = tiles[index - 2];
                    image3 = tiles[28 - 3];
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
        tilesAudio[index].Play();
        yield return new WaitForSeconds(glowDuration);
        // Reset to the original color
        image.color = transparentColor;
        if (_makeTrail)
        {
            image1.color = transparentColor;
            image2.color = transparentColor;
            image3.color = transparentColor;
        }
        tilesAudio[index].Stop();


    }

}
