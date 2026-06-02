using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosedCard : MonoBehaviour
{
    ConnectionRummy connectionRef;
    public ManagerRummy managerRef;
    public ManagerRummy2Player manager2PlayerRef;
    public InitialCardDistributor2Player initialCardDistributor2PlayerRef;
    public InitialCardDistributor initialCardDistributorRef;
    // Start is called before the first frame update
    void Start()
    {
        connectionRef = FindObjectOfType<ConnectionRummy>();
        initialCardDistributorRef = FindObjectOfType<InitialCardDistributor>();
        initialCardDistributor2PlayerRef = FindObjectOfType<InitialCardDistributor2Player>();
        managerRef = FindObjectOfType<ManagerRummy>();
        manager2PlayerRef = FindObjectOfType<ManagerRummy2Player>();
    }
    public void CloseCardPressed()
    {
        connectionRef.cardSound.Play();
        if (BootValueDecider.rummyPlayerCount == 2)
        {
            initialCardDistributor2PlayerRef.ClosedDeckCardRequest();

        }
        else
        {
            initialCardDistributorRef.ClosedDeckCardRequest();

        }

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
