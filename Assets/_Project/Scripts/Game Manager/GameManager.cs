using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class DayData
    {
        public int dayNumber;
        public string dayTitle;
        [TextArea] public string openingNarration;
        public List<Recipe> unlockedRecipesForThisDay;
        public int totalOrdersInDay = 5;

        public string storyNpcName;
        public Recipe storyRecipe;
        public DialogueData storyIntro;
        public DialogueData storyOrder;
        public NPCVariant storyVariant;
    }

    public class ActiveOrder
    {
        public Recipe recipe;
        public GameObject uiElement;
        public Image timerBarFill;
        public string customerName;
        public NPCController npcController;
    }

    [Header("Sistem Keuangan & Hari")]
    [SerializeField] private int currentDay = 1;
    [SerializeField] private int playerMoney = 0;
    [SerializeField] private List<DayData> campaignDays;

    [Header("Titik Navigasi & Prefab")]
    [SerializeField] private Transform doorSpawnPoint;
    [SerializeField] private Transform counterSpawnPoint;
    [SerializeField] private List<Transform> seatPoints;
    private Dictionary<Transform, NPCController> seatUsers = new Dictionary<Transform, NPCController>();
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private List<NPCVariant> randomVariants;

    [Header("Waktu & Durasi")]
    [SerializeField] private float initialDelay = 3.0f;
    [SerializeField] private float minSpawnTime = 25f;
    [SerializeField] private float maxSpawnTime = 45f;

    private int spawnedOrdersCount = 0;
    private float spawnTimer = 0f;
    private float nextSpawnInterval = 6f;
    private bool isDayStarted = false;
    private bool isDayFinished = false;
    private int servedCount, perfectCount, lostCount, dayEarnings;

    private List<ActiveOrder> activeOrders = new List<ActiveOrder>();
    private Canvas worldCanvas;

    private int trashLeft;

    [SerializeField] private TMP_Text missionText;
    private int trashTotal;

    private DialogueManager dialogueMgr;

    [SerializeField] private TMP_Text dayBanner;

    [SerializeField] private DialogueData introInside;

    [SerializeField] private GameObject finchIntro;

    [SerializeField] private TMP_Text endBanner;

    [SerializeField] private Image fadeImage;
    [SerializeField] private TMP_FontAsset orderFont;
    public TMP_FontAsset OrderFont => orderFont;
    [SerializeField] private GameObject fridgeStation;
    [SerializeField] private Transform eveSpawnPoint;

    private IEnumerator ShowEndBanner(string text)
    {
        if (endBanner == null) yield break;
        endBanner.text = text;
        endBanner.enableAutoSizing = true;
        endBanner.fontSizeMin = 20;
        endBanner.fontSizeMax = 48;
        endBanner.enableWordWrapping = false;
        Color c = endBanner.color;
        for (float t = 0f; t < 1f; t += Time.deltaTime) { c.a = t; endBanner.color = c; yield return null; }
        c.a = 1f; endBanner.color = c;
    }

    private IEnumerator Fade(float from, float to, float dur)
    {
        if (fadeImage == null) yield break;
        Color c = fadeImage.color;
        for (float t = 0f; t < dur; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(from, to, t / dur);
            fadeImage.color = c;
            yield return null;
        }
        c.a = to; fadeImage.color = c;
    }

    private IEnumerator GoNextDay()
    {
        yield return new WaitForSeconds(4f);

        bool hasNext = false;
        foreach (var d in campaignDays)
            if (d.dayNumber == currentDay + 1) hasNext = true;
        if (!hasNext)
        {
            yield return Fade(0f, 1f, 1.5f);
            if (endBanner != null)
            {
                endBanner.text = "Thanks for playing our demo";
                Color ec = endBanner.color; ec.a = 1f; endBanner.color = ec;
            }
            yield return new WaitForSeconds(4f);
            MainMenu.hasSession = false;
            MainMenu.savedDay = 0;
            MainMenu.savedMoney = 0;
            SceneManager.LoadScene("MainMenu");
            yield break;
        }

        yield return Fade(0f, 1f, 1f);

        if (endBanner != null)
        {
            Color c = endBanner.color; c.a = 0f; endBanner.color = c;
        }

        var p = FindObjectOfType<PlayerController>();
        if (p != null && eveSpawnPoint != null) p.transform.position = eveSpawnPoint.position;

        yield return new WaitForSeconds(0.5f);

        currentDay++;
        UpdateFridgeLock();
        spawnedOrdersCount = 0;
        servedCount = 0; perfectCount = 0; lostCount = 0; dayEarnings = 0;
        spawnTimer = 0f;
        nextSpawnInterval = 6f;
        isDayStarted = false;
        isDayFinished = false;
        StartCoroutine(StartDayRoutine());

        yield return Fade(1f, 0f, 1f);
    }

    private IEnumerator ShowBanner(string text)
    {
        if (dayBanner == null) yield break;
        dayBanner.text = text;
        Color c = dayBanner.color;
        for (float t = 0f; t < 1f; t += Time.deltaTime) { c.a = t; dayBanner.color = c; yield return null; }
        c.a = 1f; dayBanner.color = c;
        yield return new WaitForSeconds(1.5f);
        for (float t = 1f; t > 0f; t -= Time.deltaTime) { c.a = t; dayBanner.color = c; yield return null; }
        c.a = 0f; dayBanner.color = c;
    }

    private void SetMission(string text)
    {
        if (missionText == null) return;
        missionText.gameObject.SetActive(true);
        missionText.text = text;
    }

    private bool StoryBusy()
    {
        if (dialogueMgr != null && dialogueMgr.IsRunning) return true;
        foreach (var n in seatUsers.Values)
            if (n != null && n.introDialogue != null &&
                (n.currentState == NPCController.NPCState.WalkingToCounter || n.currentState == NPCController.NPCState.WaitingForInteraction))
                return true;
        return false;
    }
    private void OnEnable() { Trash.OnTrashCleaned += HandleTrashCleaned; }
    private void OnDisable() { Trash.OnTrashCleaned -= HandleTrashCleaned; }

    private void HandleTrashCleaned()
    {
        trashLeft--;
        if (missionText != null)
            missionText.text = trashLeft > 0 ? "Bersihkan sampah (" + (trashTotal - trashLeft) + "/" + trashTotal + ")" : "Bersihkan sampah (" + trashTotal + "/" + trashTotal + ") - selesai!";
    }

    public void SaveSession()
    {
        MainMenu.hasSession = true;
        MainMenu.sessionScene = SceneManager.GetActiveScene().name;
        MainMenu.savedDay = currentDay;
        MainMenu.savedMoney = playerMoney;
    }

    private void Start()
    {
        if (MainMenu.hasSession && MainMenu.savedDay > 0)
        {
            currentDay = MainMenu.savedDay;
            playerMoney = MainMenu.savedMoney;
        }
        dialogueMgr = FindObjectOfType<DialogueManager>();
        SetupWorldCanvas();
        UpdateFridgeLock();
        StartCoroutine(StartDayRoutine());
    }

    private IEnumerator StartDayRoutine()
    {
        DayData currentDayData = GetCurrentDayData();
        if (currentDayData != null)
        {
            Debug.Log("[NARASI Day " + currentDay + "]: " + currentDayData.openingNarration);
        }

        if (currentDay == 1 && !MainMenu.hasSession && introInside != null && dialogueMgr != null)
        {
            if (missionText != null) missionText.gameObject.SetActive(false);

            bool introDone = false;
            dialogueMgr.StartDialogue(introInside.lines, () => introDone = true);
            while (!introDone) yield return null;

            yield return Fade(0f, 1f, 1f);
            if (finchIntro != null) finchIntro.SetActive(false);
            yield return new WaitForSeconds(0.5f);
            yield return Fade(1f, 0f, 1f);
        }

        if ((currentDay > 1 || MainMenu.hasSession) && finchIntro != null) finchIntro.SetActive(false);

        yield return ShowBanner("DAY " + currentDay);

        trashLeft = FindObjectsOfType<Trash>().Length;
        trashTotal = trashLeft;
        if (trashTotal > 0) SetMission("Bersihkan sampah (0/" + trashTotal + ")");
        while (trashLeft > 0) yield return null;
        yield return new WaitForSeconds(1.5f);
        SetMission("Layani pelanggan pertama");
        yield return new WaitForSeconds(initialDelay);
        isDayStarted = true;
        Debug.Log("Kafe Finch's Resmi Dibuka!");
    }
    private void Update()
    {
        if (!isDayStarted || isDayFinished) return;

        DayData currentDayData = GetCurrentDayData();
        if (currentDayData == null) return;

        if (spawnedOrdersCount < currentDayData.totalOrdersInDay)
        {
            if (StoryBusy()) spawnTimer = 0f;
            else if (!QueueIsFull()) spawnTimer += Time.deltaTime;
            if (spawnTimer >= nextSpawnInterval)
            {
                spawnTimer = 0f;
                nextSpawnInterval = Random.Range(minSpawnTime, maxSpawnTime);
                SpawnNewCustomer(currentDayData);
            }
        }

        if (spawnedOrdersCount >= currentDayData.totalOrdersInDay && activeOrders.Count == 0 && !AnyCustomerLeft())
        {
            isDayFinished = true;
            SetMission("Hari selesai! Total: $" + playerMoney);
            StartCoroutine(ShowEndBanner("DAY " + currentDay + " COMPLETE\nServed: " + servedCount + "   Perfect: " + perfectCount + "   Left: " + lostCount + "\nEarned today: $" + dayEarnings + "\nTotal: $" + playerMoney));
            StartCoroutine(GoNextDay());
            Debug.Log("DAY " + currentDay + " SELESAI! Total Uang: $" + playerMoney);

        }
    }

    private DayData GetCurrentDayData()
    {
        foreach (var day in campaignDays)
        {
            if (day.dayNumber == currentDay) return day;
        }
        return null;
    }

    public List<Recipe> GetTodayRecipes()
    {
        var d = GetCurrentDayData();
        return d != null ? d.unlockedRecipesForThisDay : null;
    }

    private void UpdateFridgeLock()
    {
        if (fridgeStation == null) return;
        fridgeStation.SetActive(currentDay >= 2);
    }

    private bool AnyCustomerLeft()
    {
        foreach (var n in seatUsers.Values)
            if (n != null) return true;
        return false;
    }

 
    private bool QueueIsFull()
    {
        int c = 0;
        foreach (var n in seatUsers.Values)
            if (n != null && (n.currentState == NPCController.NPCState.WalkingToCounter || n.currentState == NPCController.NPCState.WaitingForInteraction)) c++;
        return c >= 3;
    }

    private void SetupWorldCanvas()
    {
        GameObject canvasObj = new GameObject("OrderWorldCanvas");
        worldCanvas = canvasObj.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.sortingOrder = 50;
        canvasObj.AddComponent<CanvasScaler>();

        RectTransform rect = canvasObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 200);
        rect.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        if (counterSpawnPoint != null)
        {
            canvasObj.transform.position = counterSpawnPoint.position + new Vector3(0f, 1.5f, 0f);
        }
    }

    private Transform GetFreeSeat()
    {
        var free = new List<Transform>();
        foreach (var s in seatPoints)
            if (s != null && (!seatUsers.ContainsKey(s) || seatUsers[s] == null)) free.Add(s);
        return free.Count > 0 ? free[Random.Range(0, free.Count)] : null;
    }

    private void SpawnNewCustomer(DayData dayData)
    {
        if (dayData.unlockedRecipesForThisDay == null || dayData.unlockedRecipesForThisDay.Count == 0) return;
        if (npcPrefab == null || doorSpawnPoint == null) return;

        Transform seat = GetFreeSeat();
        if (seat == null)
        {
            Debug.LogWarning("Spawn batal: nggak ada kursi kosong. Jumlah Seat Points: " + seatPoints.Count);
            return;
        }

        Recipe targetRecipe;
        string customerName;
        bool isStory = spawnedOrdersCount == 0 && !string.IsNullOrEmpty(dayData.storyNpcName) && dayData.storyRecipe != null;

        if (isStory)
        {
            customerName = dayData.storyNpcName;
            targetRecipe = dayData.storyRecipe;
        }
        else
        {
            customerName = "Warga " + Random.Range(10, 99);
            targetRecipe = dayData.unlockedRecipesForThisDay[Random.Range(0, dayData.unlockedRecipesForThisDay.Count)];
        }

        spawnedOrdersCount++;

        GameObject npcObj = Instantiate(npcPrefab, doorSpawnPoint.position, Quaternion.identity);
        NPCController npcCtrl = npcObj.GetComponent<NPCController>();
        if (npcCtrl != null)
        {
            npcCtrl.Initialize(doorSpawnPoint, counterSpawnPoint, seat, targetRecipe, customerName, this);
            seatUsers[seat] = npcCtrl;

            if (isStory && dayData.storyVariant != null && dayData.storyVariant.idle != null)
                npcCtrl.ApplyVariant(dayData.storyVariant);
            else if (!isStory && randomVariants != null && randomVariants.Count > 0)
                npcCtrl.ApplyVariant(randomVariants[Random.Range(0, randomVariants.Count)]);

            if (isStory)
            {
                npcCtrl.introDialogue = dayData.storyIntro;
                npcCtrl.orderDialogue = dayData.storyOrder;
            }
        }
        else
        {
            Debug.LogError("Prefab NPC belum memiliki komponen NPCController!");
        }
    }

  
    public void TriggerOrderBubble(NPCController npcCtrl, Recipe targetRecipe, string customerName)
    {
        GameObject orderCard = new GameObject("BubbleOrder_" + targetRecipe.drinkName);
        orderCard.transform.SetParent(worldCanvas.transform, false);
        RectTransform cardRect = orderCard.AddComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(640, 110);
        Image cardBg = orderCard.AddComponent<Image>();
        cardBg.color = new Color(0.16f, 0.1f, 0.08f, 0.95f);
        var ol = orderCard.AddComponent<Outline>();
        ol.effectColor = new Color(0.95f, 0.85f, 0.65f, 1f);
        ol.effectDistance = new Vector2(4, -4);

        GameObject textObj = new GameObject("DrinkTitle");
        textObj.transform.SetParent(orderCard.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(16, 8); textRect.offsetMax = new Vector2(-16, -8);
        TextMeshProUGUI t = textObj.AddComponent<TextMeshProUGUI>();
        if (orderFont != null) t.font = orderFont;
        t.enableAutoSizing = true; t.fontSizeMin = 40; t.fontSizeMax = 64;
        t.enableWordWrapping = false;
        t.alignment = TextAlignmentOptions.Center;
        t.color = new Color(1f, 0.95f, 0.85f);
        t.text = customerName + ": " + targetRecipe.drinkName;

        ActiveOrder newOrder = new ActiveOrder
        {
            recipe = targetRecipe,
            uiElement = orderCard,
            customerName = customerName,
            npcController = npcCtrl
        };

        activeOrders.Add(newOrder);
        RelayoutOrders();
    }

    public bool CheckAndCompleteOrder(string deliveredDrinkName, NPCController target)
    {
        for (int i = 0; i < activeOrders.Count; i++)
        {
            if (activeOrders[i].npcController == target && string.Equals(activeOrders[i].recipe.drinkName, deliveredDrinkName, System.StringComparison.OrdinalIgnoreCase))
            {
                ActiveOrder order = activeOrders[i];
                Debug.Log("Kopi diantar ke " + order.customerName);

                Destroy(order.uiElement);
                order.npcController.ReceiveCoffee();
                SetMission("Dilayani: " + order.customerName + ". Tunggu pengunjung berikutnya");

                activeOrders.RemoveAt(i);
                RelayoutOrders();
                return true;
            }
        }
        return false;
    }

    public void CompletePaymentAndLeave(NPCController npcCtrl, Recipe recipe)
    {
        int earnings = recipe.price;
        if (npcCtrl.gotPerfect) earnings += Mathf.CeilToInt(recipe.price * 0.5f);
        playerMoney += earnings;
        servedCount++;
        dayEarnings += earnings;
        if (npcCtrl.gotPerfect) perfectCount++;
        Debug.Log("Pembayaran otomatis diterima! Uang bertambah +$" + earnings + ". Total Saldo: $" + playerMoney);
        
    }

    public void CancelOrderFor(NPCController npc)
    {
        for (int i = 0; i < activeOrders.Count; i++)
        {
            if (activeOrders[i].npcController == npc)
            {
                Destroy(activeOrders[i].uiElement);
                activeOrders.RemoveAt(i);
                break;
            }
        }
        lostCount++;
        RelayoutOrders();
        SetMission(npc.CustomerName + " pergi karena kelamaan menunggu");
    }
    private void RelayoutOrders()
    {
        for (int i = 0; i < activeOrders.Count; i++)
        {
            var rt = activeOrders[i].uiElement.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0f, i * 130f);
        }
    }
}