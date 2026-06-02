using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public float selectedPosY;
    public float displacementY = 80f;
    ConnectionRummy connectionRef;
    public Transform tmpTransform;
    public bool _isSelected = false;
    public AlignchildrenToCenter blockAlignerScriptRef;
    public GameObject mainBlock;
    public GameObject newBlockPrefab;
    bool _isDragging, _isHolding;
    private RectTransform rectTransform;
    private Canvas canvas;  // Add a reference to the Canvas component
    private CanvasGroup canvasGroup;
    private Vector3 initialPosition; // Store the initial position of the item
    int cValue, tmp;
    public GameObject tmpCardHolder;
    bool _dropped = true;
    public ManagerRummy2Player managerScript2Player;
    public ManagerRummy managerScript;
    public InitialCardDistributor initialCardDistributorRef;
    public InitialCardDistributor2Player initialCardDistributor2PlayerRef;
    public bool _isDragged;
    private void OnEnable()
    {
        connectionRef = FindObjectOfType<ConnectionRummy>();
        blockAlignerScriptRef = FindAnyObjectByType<AlignchildrenToCenter>();
        mainBlock = GameObject.FindGameObjectWithTag("MainBlock");
        tmpCardHolder = GameObject.FindGameObjectWithTag("TmpCardHolder");
    }
    private void Start()
    {
        initialCardDistributorRef = FindObjectOfType<InitialCardDistributor>();
        initialCardDistributor2PlayerRef = FindObjectOfType<InitialCardDistributor2Player>();
        _isHolding = false;
        _isDragging = false;
        rectTransform = GetComponent<RectTransform>();
        selectedPosY = rectTransform.anchoredPosition.y + displacementY;

        canvas = GetComponentInParent<Canvas>();  // Retrieve the Canvas component
        canvasGroup = GetComponent<CanvasGroup>();
        initialPosition = rectTransform.position; // Store the initial position
        managerScript = FindObjectOfType<ManagerRummy>();
        managerScript2Player = FindObjectOfType<ManagerRummy2Player>();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        connectionRef.draggedCard = gameObject;
        _dropped = false;
        tmpTransform = transform.parent.transform;
        _isDragged = true;
        gameObject.transform.parent = tmpCardHolder.transform;
        _isDragging = true;
        _isHolding = true;
        //canvasGroup.blocksRaycasts = false;
        if (BootValueDecider.rummyPlayerCount == 2)
        {
            if (initialCardDistributor2PlayerRef.mainBlock.transform.childCount < 6)
            {
                connectionRef.AddBlockObj.SetActive(true);

            }
        }
        else
        {
            if (initialCardDistributorRef.mainBlock.transform.childCount < 6)
            {
                connectionRef.AddBlockObj.SetActive(true);
            }
        }

    }


    public bool IsOverFinishSlot()
    {

        // Get the position of the mouse click
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        // Raycast to find all the UI elements under the mouse click
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        // Check if any of the UI elements have the target script
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.tag == "FinishSlot")
            {
                //
                //yes over Add new Block;
                return true;


            }
            DropTarget targetScript = result.gameObject.GetComponent<DropTarget>();

        }

        return false;

    }
    
    
    
    public bool IsOverOpenDeck()
    {

        // Get the position of the mouse click
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        // Raycast to find all the UI elements under the mouse click
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        // Check if any of the UI elements have the target script
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.tag == "openDeck")
            {
                //
                //yes over Add new Block;
                return true;


            }
            DropTarget targetScript = result.gameObject.GetComponent<DropTarget>();

        }

        return false;

    }


    public bool IsOverAddNewBlock()
    {
        /*if (EventSystem.current.IsPointerOverGameObject())
        {*/

        // Get the position of the mouse click
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        // Raycast to find all the UI elements under the mouse click
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        // Check if any of the UI elements have the target script
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.tag == "AddNewBlock")
            {
                //
                //yes over Add new Block;
                return true;


            }
            DropTarget targetScript = result.gameObject.GetComponent<DropTarget>();

        }

        return false;

    }
    public void OnDrag(PointerEventData eventData)
    {

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
    private void Update()
    {
        if (_isDragging)
        {

        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        connectionRef.cardSound.Play();
        if (_isHolding)
        {
            _isDragging = false;
            //canvasGroup.blocksRaycasts = true;
            // Check if the item is over the correct drop target
            if (IsOverAddNewBlock())
            {
                if (mainBlock.transform.childCount < 6)
                {

                    GameObject newBlock = Instantiate(newBlockPrefab, mainBlock.transform);
                    gameObject.transform.parent = newBlock.transform.GetChild(0).transform;
                    _dropped = true;
                    if (_isSelected)
                    {
                        if (BootValueDecider.rummyPlayerCount == 2)
                        {
                            managerScript2Player.selectedCardsList.Clear();
                        }
                        else
                        {
                            Debug.Log(managerScript.selectedCardsList.Count);
                            managerScript.selectedCardsList.Remove(gameObject);
                            Debug.Log("block removed successfully");
                            Debug.Log(managerScript.selectedCardsList.Count);

                        }
                    }

                    _isSelected = false;
                    if (tmpTransform.childCount == 0)
                    {
                        Debug.Log("zero ho gaya re baba ");
                        Destroy(tmpTransform.parent.gameObject);
                    }
                }
            }
            else if (IsOverOpenDeck())
            {
                if (BootValueDecider.rummyPlayerCount == 2)
                {
                    if (managerScript2Player._isMyChance && managerScript2Player._isTakenCard)
                    {
                        Debug.Log("radhey");
                        Debug.Log("child count : " + tmpTransform.childCount);
                        if (tmpTransform.childCount == 0)
                        {
                            Debug.Log("zero ho gaya re baba ");
                            Destroy(tmpTransform.parent.gameObject);
                        }
                        connectionRef.Discard(gameObject);

                    }
                    else { ReturnToOriginalPosition(); }
                }
                else
                {
                    if (managerScript._isMyChance && managerScript._isTakenCard)
                    {
                        connectionRef.Discard(gameObject);
                        if (tmpTransform.childCount == 0)
                        {
                            Debug.Log("zero ho gaya re baba ");
                            Destroy(tmpTransform.parent.gameObject);
                        }
                    }
                    else { ReturnToOriginalPosition(); }

                }

            }
            else if (IsOverFinishSlot())
            {
                if (BootValueDecider.rummyPlayerCount == 2)
                {
                    if (managerScript2Player._isMyChance)
                    {
                        ReturnToOriginalPosition();
                        initialCardDistributor2PlayerRef.cardSprites.Remove(gameObject.GetComponent<Image>().sprite);
                        if (!_isSelected)
                        {
                            SelectFinish();
                        }

                        connectionRef.FinishBtn();

                    }
                    else
                    {
                        ReturnToOriginalPosition();
                    }
                }
                else
                {
                    initialCardDistributorRef.cardSprites.Remove(gameObject.GetComponent<Image>().sprite);

                    if (managerScript._isMyChance)
                    {
                        ReturnToOriginalPosition();

                        if (!_isSelected)
                        {
                            SelectFinish();
                        }
                        connectionRef.FinishBtn();

                    }
                    else
                    {
                        ReturnToOriginalPosition();

                    }
                }

            }


            else if (IsOverDropTarget())
            {
                _dropped = true;
                if (tmpTransform.childCount == 0)
                {
                    Destroy(tmpTransform.parent.gameObject);

                }
                if (cValue <= tmp)
                {
                    // Implement the logic to handle the drop
                    rectTransform.position = eventData.position;
                    //gameObject.GetComponent<DraggableItem>().enabled = false;
                    if (BootValueDecider.rummyPlayerCount == 2)
                    {
                        initialCardDistributor2PlayerRef.ScoreUpdate();
                    }
                    else
                    {
                        initialCardDistributorRef.ScoreUpdate();
                    }
                   /* if (_isSelected)
                    {
                        if (BootValueDecider.rummyPlayerCount == 2)
                        {
                            managerScript2Player.selectedCardsList.Clear();

                        }
                        else
                        {
                            managerScript.selectedCardsList.Remove(gameObject);

                        }
                    }
                    _isSelected = false;*/


                }
                else
                {
                    ReturnToOriginalPosition();
                }


            }
            else
            {
                // Return to the initial position if not dropped at the correct location
                ReturnToOriginalPosition();
            }
        }
        else
        {
        }
        blockAlignerScriptRef.AlignChildren();
        _isDragged = false;



        connectionRef.AddBlockObj.SetActive(false);


    }

    // Check if the item is over the correct drop target




    private bool IsOverDropTarget()
    {
        
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        // Raycast to find all the UI elements under the mouse click
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        Vector2 localPoint;

        // Check if any of the UI elements have the target script
        foreach (RaycastResult result in results)
        {
            DropTarget targetScript = result.gameObject.GetComponent<DropTarget>();
            if (targetScript != null)
            {
                float partitionWidth;
                if (targetScript.transform.childCount != 0)
                {
                    partitionWidth = (targetScript.gameObject.GetComponent<RectTransform>().rect.width - targetScript.transform.GetChild(0).GetComponent<RectTransform>().rect.width) / targetScript.transform.childCount;
                }
                else
                {
                    partitionWidth = 1f;
                }

                RectTransformUtility.ScreenPointToLocalPointInRectangle(result.gameObject.GetComponent<RectTransform>(),
                    pointerData.position, pointerData.pressEventCamera, out localPoint);
                Debug.Log("local point : " + localPoint);

                float destance =  (0 - targetScript.GetComponent<RectTransform>().rect.width / 2f) - localPoint.x;

                if(destance < 0)
                {
                    destance *= -1f;
                }
                int k = (int) (destance / partitionWidth);
                if (k < 0)
                {
                    k = 0; 
                }
                Debug.Log("partition width is : " + partitionWidth + "\n destance is : " + destance + "\n k : " + k + "\n tartet's localposition : " + targetScript.transform.position.x);
                int closestSlotIndex = 0;
                float closestDistance = float.MaxValue;

                for (int i = 0; i < result.gameObject.transform.childCount; i++)
                {
                    float distance = Vector2.Distance(localPoint, result.gameObject.transform.GetChild(i).gameObject.GetComponent<RectTransform>().anchoredPosition);
                    if (distance < closestDistance + 40f)
                    {
                        closestDistance = distance;
                        closestSlotIndex = i;
                    }
                }

                // The item is dropped over a target with the target script
                gameObject.transform.SetParent(result.gameObject.transform);// = result.gameObject.transform;
                gameObject.transform.SetSiblingIndex(k);

                return true;

            }
        }

        return false;
    }


    // Method to return the item to its initial position
    public void ReturnToOriginalPosition()
    {
        Debug.LogWarning("return to original position");
        gameObject.transform.SetParent(tmpTransform);
    }


    public void SelectFinish()
    {
        Debug.Log("selecting to finishs");
        if (BootValueDecider.rummyPlayerCount == 2)
        {
            managerScript2Player.selectedCardsList.Clear();

            if (_isSelected)
            {
                _isSelected = false;
                transform.position += new Vector3(0, -displacementY, 0);
                managerScript2Player.selectedCardsList.Remove(gameObject);

            }
            else if (!_isSelected)
            {
                _isSelected = true;
                transform.position += new Vector3(0, displacementY, 0);
                managerScript2Player.selectedCardsList.Add(gameObject);

            }


        }
        else
        {
            managerScript.selectedCardsList.Clear();



            if (_isSelected)
            {

                _isSelected = false;
                transform.position += new Vector3(0, -displacementY, 0);
                managerScript.selectedCardsList.Remove(gameObject);

            }
            else if (!_isSelected)
            {
                _isSelected = true;
                transform.position += new Vector3(0, displacementY, 0);
                managerScript.selectedCardsList.Add(gameObject);
                Debug.Log("added successfully" + managerScript.selectedCardsList.Count);
            }


        }
    }

    public void SelectMe()
    {
        if (!_isDragged)
        {

            connectionRef.cardSound2.Stop();
            connectionRef.cardSound2.Play();
            if (BootValueDecider.rummyPlayerCount == 2)
            {


                if (_isSelected)
                {
                    _isSelected = false;
                    transform.localPosition += new Vector3(0, -displacementY, 0);
                    managerScript2Player.selectedCardsList.Remove(gameObject);

                }
                else if (!_isSelected)
                {
                    _isSelected = true;
                    transform.localPosition += new Vector3(0, displacementY, 0);
                    managerScript2Player.selectedCardsList.Add(gameObject);

                }

                if (managerScript2Player.selectedCardsList.Count > 1)
                {
                    managerScript2Player.GroupBtnOjb.SetActive(true);
                }
                else
                {
                    managerScript2Player.GroupBtnOjb.SetActive(false);
                }

                if (managerScript2Player.selectedCardsList.Count == 1)
                {
                    if (managerScript2Player._isTakenCard)
                    {
                        initialCardDistributor2PlayerRef.dropPanel.SetActive(true);
                    }
                }
                else
                {
                    initialCardDistributor2PlayerRef.dropPanel.SetActive(false);
                }
            }
            else
            {



                if (_isSelected)
                {

                    _isSelected = false;
                    transform.localPosition += new Vector3(0, -displacementY, 0);
                    managerScript.selectedCardsList.Remove(gameObject);

                }
                else if (!_isSelected)
                {
                    _isSelected = true;
                    transform.localPosition += new Vector3(0, displacementY, 0);
                    managerScript.selectedCardsList.Add(gameObject);

                }

                if (managerScript.selectedCardsList.Count > 1)
                {
                    if (mainBlock.transform.childCount < 6)
                    {
                        managerScript.GroupBtnOjb.SetActive(true);
                    }
                }
                else
                {

                    managerScript.GroupBtnOjb.SetActive(false);
                }

                if (managerScript.selectedCardsList.Count == 1)
                {

                    if (managerScript._isTakenCard)
                    {

                        initialCardDistributorRef.dropPanel.SetActive(true);
                    }
                }
                else
                {

                    initialCardDistributorRef.dropPanel.SetActive(false);
                }
            }


        }
    }
}
