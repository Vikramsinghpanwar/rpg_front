using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Teenpatti;

public class ThunderUIManager : MonoBehaviour
{
    public static ThunderUIManager Instance;

    [System.Serializable]
    public class ThunderConnection
    {
        public int seatA;
        public int seatB;
        public GameObject thunderImage;
        public Animator animator;
    }

    public List<ThunderConnection> thunderConnections;

    public AudioClip thunderSFX;
    public AudioSource audioSource;

    private void Awake()
    {
        Instance = this;

        // Disable all thunder images at start
        foreach (var t in thunderConnections)
        {
            t.thunderImage.SetActive(false);
        }
    }

    public void PlayThunder(PlayerManager from, PlayerManager to)
    {
        Debug.Log("from player : " + from.myId + from.ToString());
        Debug.Log("to player : " + to.myId + to.ToString());
        Debug.Log("playing thunder for " + from.myIndex + "" + to.myIndex);
        
        int seat1 = from.myIndex;
        int seat2 = to.myIndex;

        StartCoroutine(ThunderRoutine(seat1, seat2));
    }

    IEnumerator ThunderRoutine(int seat1, int seat2)
    {
        ThunderConnection connection = GetConnection(seat1, seat2);

        if (connection == null)
            yield break;

        connection.thunderImage.SetActive(true);

        // Play animation
        if (connection.animator != null)
            connection.animator.SetTrigger("Play");

        // Sound
        if (thunderSFX)
            audioSource.PlayOneShot(thunderSFX);

        yield return new WaitForSeconds(0.5f);

        connection.thunderImage.SetActive(false);
    }

    ThunderConnection GetConnection(int seat1, int seat2)
    {
        // Make sure order doesn't matter (1,2 same as 2,1)
        int min = Mathf.Min(seat1, seat2);
        int max = Mathf.Max(seat1, seat2);

        foreach (var t in thunderConnections)
        {
            if (t.seatA == min && t.seatB == max)
                return t;
        }

        return null;
    }
}
