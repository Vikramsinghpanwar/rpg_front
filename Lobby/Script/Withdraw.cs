// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using TMPro;
// using UnityEngine.UI;

// using UnityEngine.Networking;
// using System;
// public class Withdraw : MonoBehaviour
// {
//     public static Withdraw Instance;
//     public Error errorRef;
//     public TextMeshProUGUI withdrawlLimit_guildeLineTxt;
//     private bool LoginScreen;
//     public Image Bound;
//     public Image WithSc;
//     public GameObject[] swithcPanels;
//     public Button[] swithcButtons;
//     private int activePanelIndex = 1;
//     public Sprite normalSprite;
//     public Sprite highlightedSprite;


//     public Image HisWtran;
//     public Image HisWtrancolor;
//     private string ActiveBtn;
//     private string ActiveBtnH;

//     public TMP_InputField UpiAdressIn;
//     public TextMeshProUGUI UpiAdress;
//     public Text WithAble;
//     public TMP_InputField UPIHolder;
//     // public TMP_InputField BankName;
//     public TMP_Dropdown BankName;
//     public TMP_InputField AccountNu;
//     public TMP_InputField BankHolder;
//     public TMP_InputField IFSCCode;
//     public TMP_InputField WithAmount;

//     public Text BankName_Main;
//     public Text AccountNu_Main;
//     public Text IFSCCode_Main;
//     public Text HolderName_Main;

//     public Text ShowUpiBank;
//     private string UpiId;
//     private string Account;
//     private int upibank;

//     public GameObject rowPrefabSelfBonus;  // Reference to the row prefab
//     public Transform tableContainerSelfBonus;
//     public GameObject GameRecodPrefab;  // Reference to the row prefab
//     public Transform GameRecodsContainer;
//     public GameObject GameRechagePrefab;  // Reference to the row prefab
//     public Transform GameRechageContainer;

//     public GameObject LevelRewardPrefab;  // Reference to the row prefab
//     public Transform LevelRewardContainer;
//     public Transform randRewardPanel;

//     void Awake()
//     {
//         Instance = this;
//     }

//     void Start()
//     {
//         errorRef = FindObjectOfType<Error>();
//         ActiveBtn = "With-button-1";
//         ActiveBtnH = "Hisw-Button-1";

//         string Mobile = UserDetail.Mobile.ToString();
//         if (Mobile.Length <= 5)
//         { // Check the length of the string
//             LoginScreen = false;
//         }
//         else
//         {
//             LoginScreen = true;
//         }

//         float AmountWith = UserDetail.WinAmount;
//         WithAble.text = AmountWith.ToString();
//         WithdrawBalanceUpdate();
//         for(int i = 0; i< swithcButtons.Length; i++)
//         {
//             int index = i;
//             swithcButtons[i].onClick.AddListener(()=> Switch(index));
//             swithcButtons[i].GetComponent<Image>().sprite = normalSprite;
//         }
//         Switch(0);
//         GameRecod();
//         Rechage();
//         WithHistry();

//         // BankName.text = UserDetail.BankName;
//         AccountNu.text = UserDetail.Account;
//         BankHolder.text = UserDetail.HolderBank;
//         IFSCCode.text = UserDetail.IfscCode;

//         BankName_Main.text = UserDetail.BankName;
//         AccountNu_Main.text = UserDetail.Account;
//         IFSCCode_Main.text = UserDetail.IfscCode;
//         HolderName_Main.text = UserDetail.HolderBank;

//     }

//     public void Switch(int index)
//     {
//         if(index == activePanelIndex) return;
//         swithcPanels[activePanelIndex].SetActive(false);
//         swithcButtons[activePanelIndex].GetComponent<Image>().sprite = normalSprite;
//         activePanelIndex = index;
//         swithcButtons[activePanelIndex].GetComponent<Image>().sprite = highlightedSprite;
//         swithcPanels[activePanelIndex].SetActive(true);
//     }

//     public void OpenBound()
//     {
//         string Mobile = UserDetail.Mobile.ToString();
//         Debug.Log("MOBILE :  " + Mobile);
//         if (Mobile.Length <= 5)
//         { // Check the length of the string
//             Debug.Log("Mobile Withdraw " + Mobile);
//             LoginScreen = false;
//         }
//         else
//         {
//             Debug.Log("Mobile number " + Mobile);
//             LoginScreen = true;
//         }
//         if (LoginScreen)
//         {
//             WithSc.gameObject.SetActive(true);
//         }
//         else
//         {
//             Bound.gameObject.SetActive(true);
//         }
//     }


//     public void ChangeBtn(string NewBtn)
//     {
//         GameObject OldBtn = GameObject.Find(ActiveBtn);
//         Image OldButton = OldBtn.GetComponent<Image>();
//         OldButton.sprite = normalSprite;

//         GameObject NewBtnn = GameObject.Find(NewBtn);
//         Image NewButton = NewBtnn.GetComponent<Image>();
//         NewButton.sprite = highlightedSprite;
//         ActiveBtn = NewBtn;
//     }
//     public void ChangeBtnH(string NewBtnH)
//     {
//         GameObject OldBtnh = GameObject.Find(ActiveBtnH);
//         Image OldButtonh = OldBtnh.GetComponent<Image>();
//         OldButtonh.sprite = HisWtran.sprite;

//         GameObject NewBtnnh = GameObject.Find(NewBtnH);
//         Image NewButtonh = NewBtnnh.GetComponent<Image>();
//         NewButtonh.sprite = HisWtrancolor.sprite;
//         ActiveBtnH = NewBtnH;
//     }
//     public GameObject successfulSubmitObj;

//     public void SuccessOff()
//     {
//         successfulSubmitObj.SetActive(false);

//     }
//     public void BankAdd()
//     {
//             if (AccountNu.text.Length == 0)
//             {
//                 Logger.Instance.Warning("Please enter account number");
//                 return;
//             }
//             if (BankHolder.text.Length == 0)
//             {
//                 Logger.Instance.Warning("Please enter account holder name");
//                 return;
//             }
//             if (IFSCCode.text.Length == 0)
//             {
//                 Logger.Instance.Warning("Please enter IFSC code");
//                 return;
//             }
//             if (IFSCCode.text.Length < 11)
//             {
//                 Logger.Instance.Warning("Please enter valid IFSC code");
//                 return;
//             }
//             if (AccountNu.text.Length < 9 || AccountNu.text.Length > 18)
//             {
//                 Logger.Instance.Warning("Please enter valid account number");
//                 return;
//             }
//             if (BankHolder.text.Length > 50)
//             {
//                 Logger.Instance.Warning("Account holder name should be less than 50 characters");
//                 return;
//             }
//         if (BankName != null && AccountNu != null && BankHolder != null && IFSCCode != null)
//         {
//             string bankname = BankName.options[BankName.value].text;
//             string accountnum = AccountNu.text;
//             string bankholder = BankHolder.text;
//             string ifsccode = IFSCCode.text;
//             Login Upi = FindObjectOfType<Login>();
//             Upi.UpdateBankAdd(bankname, accountnum, bankholder, ifsccode);

//             // BankName.value = 0;
//             AccountNu.text = "";
//             IFSCCode.text = "";
//             BankHolder.text = "";
//             successfulSubmitObj.SetActive(true);
//             Invoke("SuccessOff", 2);
//         }
//     }

//     public void Submit_Withdrawal_Request()
//     {
//         string Account = UserDetail.Account;
//         if (UserDetail.Account == null || UserDetail.Account == "")
//         {
//             Logger.Instance.Warning("Please Add Bank Account");
//             return;
//         }



//         if (WithAmount.text.Length == 0)
//         {
//             Logger.Instance.Warning("Please enter amount");
//             return;
//         }
//         if (WithAmount.text != "")
//         {
//             int WithLimit = UserDetail.WithdrawalLimit;
//             float AmountWith = BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f;

//             if (int.TryParse(WithAmount.text, out int withAmount))
//             {
//                 if (WithLimit > withAmount)
//                 {
//                     Logger.Instance.Warning("Amount should be greater than " + WithLimit);
//                     return;
//                 }

//                 if(withAmount > UserDetail.withmax)
//                 {
//                     Logger.Instance.Warning("Amount should be less than " + UserDetail.withmax);
//                     return;
//                 }


//                 if (AmountWith < withAmount)
//                 {
//                     Logger.Instance.Warning("Insufficient funds");
//                     return;
//                 }
//                 if(!CheckAmount(withAmount)){
//                     Logger.Instance.Warning("Amount is rounded to the nearest 100");
//                     return;
//                 }
//                 if (WithLimit <= withAmount && AmountWith >= withAmount)
//                 {
//                     Debug.Log("withdrawal pass");
//                     Login Upi = FindObjectOfType<Login>();

//                     Upi.WithProcess(withAmount, upibank);
//                     WithAmount.text = "";
//                 }
//                 else
//                 {
//                     Debug.Log("withdrawal Failed");
//                 }
//             }
//             else
//             {
//                 Logger.Instance.Warning("Invalid amount");
//             }


//         }
//         else
//         {
//             Debug.Log("eHellow");
//         }
//     }

//     private bool CheckAmount(float value){
//         float x = value % 100;
//         if (x == 0) {
//             return true;
//         }
//         return false;
//     }
//     public void withdrawalresult(string resultt)
//     {
//         WithAble.text = resultt.ToString();
//     }

//     public void WithHistry()
//     {
//         Login WIth = FindObjectOfType<Login>();
//         WIth.WithHistryApi();
//     }
//     public void WithhistryApiResult(string resultt)
//     {
//         DataArray[] SelfBonusDate = JsonHelper.FromJson<DataArray>(resultt);
//         if (SelfBonusDate[0].error != "Data not found")
//         {
//             SelfBounsShow(SelfBonusDate);
//         }
//         else
//         {
//             DeleteAllEntries();
//         }
//     }

//     public void DeleteAllEntries()
//     {
//         for (int i = 0; i < tableContainerSelfBonus.transform.childCount; i++)
//         {
//             Destroy(tableContainerSelfBonus.transform.GetChild(i).gameObject);
//         }
//     }

//     [System.Serializable]
//     public class DataArray
//     {
//         public string array1;
//         public string array2;
//         public string array3;
//         public string array4;
//         public string array5;
//         public string array6;
//         public string error;
//     }

//     void SelfBounsShow(DataArray[] data)
//     {
//         for (int i = 0; i < tableContainerSelfBonus.transform.childCount; i++)
//         {
//             Destroy(tableContainerSelfBonus.transform.GetChild(i).gameObject);
//         }
//         foreach (DataArray rowData in data)
//         {
//             if (rowPrefabSelfBonus != null && tableContainerSelfBonus != null)
//             {
//                 GameObject newRow = Instantiate(rowPrefabSelfBonus, tableContainerSelfBonus);
//                 TextMeshProUGUI[] textComponents = newRow.GetComponentsInChildren<TextMeshProUGUI>();
//                 if (textComponents.Length >= 6)
//                 {
//                     textComponents[0].text = rowData.array1;
//                     textComponents[1].text = rowData.array2;
//                     textComponents[2].text = rowData.array3;
//                     textComponents[3].text = rowData.array4;
//                     textComponents[4].text = rowData.array5;
//                     textComponents[5].text = rowData.array6;
//                 }
//                 else
//                 {
//                     Debug.LogError("Not enough TextMeshProUGUI components in the row prefab");
//                 }
//             }
//             else
//             {
//                 Debug.LogError("rowPrefabBonus or tableContainerSelfBonus is not assigned");
//             }
//         }
//     }

//     public void GameRecod()
//     {
//         Login Re = FindObjectOfType<Login>();
//         Re.Gamerecodes();
//     }
//     public void LevelRewardApiCall()
//     {
//         Login Re = FindObjectOfType<Login>();
//         Re.LevelRewardApi();
//     }
//     public void GameRecodeResult(string data)
//     {
//         DataArray[] SelfBonusDate = JsonHelper.FromJson<DataArray>(data);
//         GameRecodeShow(SelfBonusDate);
//     }

//     void GameRecodeShow(DataArray[] data)
//     {
//         for (int i = 0; i < GameRecodsContainer.transform.childCount; i++)
//         {
//             Destroy(GameRecodsContainer.transform.GetChild(i).gameObject);
//         }
//         foreach (DataArray rowData in data)
//         {
//             if (GameRecodPrefab != null && GameRecodsContainer != null)
//             {
//                 GameObject newRow = Instantiate(GameRecodPrefab, GameRecodsContainer);
//                 TextMeshProUGUI[] textComponents = newRow.GetComponentsInChildren<TextMeshProUGUI>();
//                 if (textComponents.Length >= 4)
//                 {
//                     textComponents[0].text = rowData.array1;
//                     textComponents[1].text = rowData.array2;
//                     textComponents[2].text = rowData.array3;
//                     textComponents[3].text = rowData.array4;
//                 }
//                 else
//                 {
//                     Debug.LogError("Not enough TextMeshProUGUI components in the row prefab");
//                 }
//             }
//             else
//             {
//                 Debug.LogError("GameRecodPrefab or GameRecodsContainer is not assigned");
//             }
//         }
//     }

//     [System.Serializable]
//     public class DataArrayData
//     {
//         public string array1;
//         public string array2;
//         public string array3;
//         public string array4;
//         public string array5;
//         public string error;
//     }


//     [System.Serializable]
//     public class LevelRewardData
//     {
//         public int id;
//         public string name;
//         public int amount;
//         public int bonus;
//         public int collect;

//     }
//     public void Rechage()
//     {
//         Login ReLo = FindObjectOfType<Login>();
//         ReLo.RechageApi();
//     }
//     public void RechapiResult(string data)
//     {
//         DataArrayData[] SelfBonusDate = JsonHelper.FromJson<DataArrayData>(data);
//         if (SelfBonusDate[0].error != "Data not found")
//         {
//             GameRechageShow(SelfBonusDate);
//         }
//         else
//         {
//             DeleteAllEntries();
//         }
//     }


//     public void LevelReward(string data)
//     {
//         LevelRewardData[] SelfBonusDate = JsonHelper.FromJson<LevelRewardData>(data);
//         LevelRewardShow(SelfBonusDate);
//     }


//     void GameRechageShow(DataArrayData[] data)
//     {
//         for (int i = 0; i < GameRechageContainer.transform.childCount; i++)
//         {
//             Destroy(GameRechageContainer.transform.GetChild(i).gameObject);
//         }
//         foreach (DataArrayData rowData in data)
//         {
//             if (GameRechagePrefab != null && GameRechageContainer != null)
//             {
//                 GameObject newRow = Instantiate(GameRechagePrefab, GameRechageContainer);
//                 TextMeshProUGUI[] textComponents = newRow.GetComponentsInChildren<TextMeshProUGUI>();
//                 if (textComponents.Length >= 5)
//                 {
//                     textComponents[0].text = rowData.array1;
//                     textComponents[1].text = rowData.array2;
//                     textComponents[2].text = rowData.array3;
//                     textComponents[3].text = rowData.array4;
//                     textComponents[4].text = rowData.array5;
//                 }
//                 else
//                 {
//                     Debug.LogError("Not enough TextMeshProUGUI components in the row prefab");
//                 }
//             }
//             else
//             {
//                 Debug.LogError("GameRechagePrefab or GameRechageContainer is not assigned");
//             }
//         }
//     }

//     void LevelRewardShow(LevelRewardData[] data)
//     {
//         for (int i = 0; i < randRewardPanel.transform.childCount; i++)
//         {
//             Destroy(randRewardPanel.transform.GetChild(i).gameObject);
//         }


//         foreach (LevelRewardData rowData in data)
//         {
//             if (GameRechagePrefab != null && GameRechageContainer != null)
//             {
//                 GameObject newRow = Instantiate(LevelRewardPrefab, LevelRewardContainer);
//                 TextMeshProUGUI[] textComponents = newRow.GetComponentsInChildren<TextMeshProUGUI>();

//                 if (textComponents.Length >= 5)
//                 {
//                     textComponents[0].text = "" + rowData.id;
//                     textComponents[1].text = "" + rowData.name;
//                     textComponents[2].text = "" + rowData.bonus;
//                     textComponents[3].text = "" + rowData.amount;
//                     textComponents[4].text = "" + rowData.collect;
//                 }
//                 else
//                 {
//                     Debug.LogError("Not enough TextMeshProUGUI components in the row prefab");
//                 }
//             }
//             else
//             {
//                 Debug.LogError("GameRechagePrefab or GameRechageContainer is not assigned");
//             }
//         }
//     }

//     public void WithdrawBalanceUpdate()
//     {
//         withdrawlLimit_guildeLineTxt.text = UserDetail.WithdrawalCommision + "% of withdrawal amount will be charged as payment gateway fees";
//     }

//     public void Select_Withdrawal_Amount(int val)
//     {
//         WithAmount.text = val.ToString();
//     }

// }
