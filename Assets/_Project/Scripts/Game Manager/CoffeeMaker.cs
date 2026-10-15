using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CoffeeMaker : MonoBehaviour, IInteractable
{
    [SerializeField] private string promptMessage = "Gunakan Mesin Kopi";
    [Header("Daftar Resep ScriptableObject")]
    [SerializeField] private List<Recipe> availableRecipes;

    private List<string> currentIngredients = new List<string>();
    private Recipe matchedRecipe = null;

    private bool lastPerfect;
    private bool isQteActive = false;
    private float qteProgress = 0f;
    private bool qteMovingRight = true;
    private float qteSpeed = 1.8f; 

   
    private GameObject canvasObj;
    private GameObject panelObj;
    private Slider sliderComp;
    private RectTransform perfectZoneRect;
    private RectTransform arrowRect;
    private TMP_Text infoText;
    private RectTransform goodZoneRect;
    [SerializeField] private TMP_FontAsset qteFont;

    private readonly List<SpriteRenderer> ingIcons = new List<SpriteRenderer>();
    private SpriteRenderer resultIcon;
    private TextMeshPro resultText;
    private float resultTimer;
    private Sprite whiteSp;

    [SerializeField] private float iconScale = 1.0f;
    private SpriteRenderer ingBubble;
    [SerializeField] private Sprite bubbleSprite;
    private Transform holder;
    [Header("Atur di Inspector")]
    [SerializeField] private Vector3 ingOffset = new Vector3(0f, 1.6f, 0f);
    [SerializeField] private Vector3 resultOffset = new Vector3(0f, 3f, 0f);
    [SerializeField] private float iconSpacing = 1.8f;
    [SerializeField] private float bubblePad = 0.5f;
    [SerializeField] private float bubbleHeight = 1.2f;
    [SerializeField] private Vector2 resultBubbleSize = new Vector2(3.6f, 2.8f);
    private SpriteRenderer resultBubble;

    private TextMeshPro hintText;

    private void SetHint(bool on)
    {
        if (hintText == null)
        {
            var o = new GameObject("HintText");
            o.transform.SetParent(GetHolder(), false);
            hintText = o.AddComponent<TextMeshPro>();
            hintText.alignment = TextAlignmentOptions.Center;
            hintText.fontSize = 10;
            hintText.sortingOrder = 212;
            hintText.color = new Color(1f, 0.92f, 0.45f);
            if (qteFont != null) hintText.font = qteFont;
        }
        hintText.text = "Tekan E";
        hintText.transform.localPosition = ingOffset + new Vector3(0f, bubbleHeight * 0.5f + 0.4f, 0f);
        hintText.gameObject.SetActive(on);
    }
    private Transform GetHolder()
    {
        if (holder == null)
        {
            holder = new GameObject("MachineUI").transform;
            holder.position = transform.position;
            holder.localScale = Vector3.one;
        }
        return holder;
    }

    private Sprite GetWhite()
    {
        if (whiteSp == null)
        {
            var t = new Texture2D(1, 1);
            t.filterMode = FilterMode.Point;
            t.SetPixel(0, 0, Color.white);
            t.Apply();
            whiteSp = Sprite.Create(t, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
        return whiteSp;
    }

    private SpriteRenderer MakeIcon(string n, int order)
    {
        var o = new GameObject(n);
        o.transform.SetParent(GetHolder(), false);
        var s = o.AddComponent<SpriteRenderer>();
        s.sortingOrder = order;
        o.transform.localScale = Vector3.one * iconScale;
        return s;
    }

    private void SetupBubble(ref SpriteRenderer b, string n, int order)
    {
        if (b != null) return;
        b = MakeIcon(n, order);
        b.transform.localScale = Vector3.one;
        if (bubbleSprite != null) { b.sprite = bubbleSprite; b.drawMode = SpriteDrawMode.Sliced; }
        else { b.sprite = GetWhite(); b.color = new Color(0.16f, 0.1f, 0.08f, 0.95f); }
    }

    private void RefreshIngredientIcons()
    {
        foreach (var s in ingIcons) if (s != null) Destroy(s.gameObject);
        ingIcons.Clear();
        SetupBubble(ref ingBubble, "IngBubble", 199);
        int n = currentIngredients.Count;
        ingBubble.gameObject.SetActive(n > 0);
        float w = n * iconSpacing + bubblePad;
        if (bubbleSprite != null) ingBubble.size = new Vector2(w, bubbleHeight);
        else ingBubble.transform.localScale = new Vector3(w, bubbleHeight, 1f);
        ingBubble.transform.localPosition = ingOffset;
        var disp = HeldItemDisplay.Instance;
        if (disp == null) return;
        float startX = -(n - 1) * 0.5f * iconSpacing;
        for (int i = 0; i < n; i++)
        {
            var s = MakeIcon("IngIcon", 200);
            s.sprite = disp.GetIcon(currentIngredients[i]);
            s.transform.localPosition = ingOffset + new Vector3(startX + i * iconSpacing, 0f, 0f);
            ingIcons.Add(s);
        }
    }

    private void ShowResult(string drink, bool perfect)
    {
        SetupBubble(ref resultBubble, "ResultBubble", 209);
        if (resultIcon == null) resultIcon = MakeIcon("ResultIcon", 210);
        if (resultText == null)
        {
            var o = new GameObject("ResultText");
            o.transform.SetParent(GetHolder(), false);
            resultText = o.AddComponent<TextMeshPro>();
            resultText.alignment = TextAlignmentOptions.Center;
            resultText.fontSize = 8f;
            resultText.sortingOrder = 211;
            if (qteFont != null) resultText.font = qteFont;
        }
        var disp = HeldItemDisplay.Instance;
        resultIcon.sprite = disp != null ? disp.GetIcon(drink) : null;
        Vector2 bs = resultBubbleSize;
        if (bubbleSprite != null) resultBubble.size = bs;
        else resultBubble.transform.localScale = new Vector3(bs.x, bs.y, 1f);
        resultBubble.transform.localPosition = resultOffset;
        resultText.transform.localPosition = resultOffset + new Vector3(0f, 0.45f, 0f);
        resultIcon.transform.localPosition = resultOffset + new Vector3(0f, -0.3f, 0f);
        resultText.text = perfect ? "PERFECT!" : "Good";
        resultText.color = perfect ? new Color(1f, 0.92f, 0.45f) : new Color(1f, 0.95f, 0.85f);
        resultBubble.gameObject.SetActive(true);
        resultIcon.gameObject.SetActive(true);
        resultText.gameObject.SetActive(true);
        resultTimer = 1.8f;
    }

    public string Prompt => promptMessage;

    public void Interact()
    {
        PlayerHand playerHand = FindObjectOfType<PlayerHand>();
        if (playerHand == null) return;

        if (isQteActive)
        {
            Debug.Log("QTE sedang berlangsung!");
            return;
        }

        if (playerHand.HasItem)
        {
            string itemInHand = playerHand.GetHeldItem();

            if (IsDrinkProduct(itemInHand))
            {
                Debug.Log("Tidak bisa memasukkan minuman jadi ke dalam mesin!");
                return;
            }

            currentIngredients.Add(itemInHand);
            playerHand.ClearHand();
            Debug.Log("Bahan masuk: " + itemInHand + ". Total bahan: " + currentIngredients.Count);
            RefreshIngredientIcons();
            EvaluateRecipe();
            return;
        }

        if (matchedRecipe != null)
        {
            StartAutoQTE();
        }
        else if (currentIngredients.Count > 0)
        {
            Debug.Log("Racikan sementara belum cocok. Tekan [R] untuk mereset.");
        }
        else
        {
            Debug.Log("Mesin kosong. Masukkan bahan dari stasiun.");
        }
    }

    private void Update()
    {
        if (resultTimer > 0f)
        {
            resultTimer -= Time.deltaTime;
            if (resultTimer <= 0f)
            {
                if (resultBubble != null) resultBubble.gameObject.SetActive(false);
                if (resultIcon != null) resultIcon.gameObject.SetActive(false);
                if (resultText != null) resultText.gameObject.SetActive(false);
            }
        }
        if (currentIngredients.Count > 0 && !isQteActive && Input.GetKeyDown(KeyCode.R))
        {
            ResetMachine();
        }

        
        if (isQteActive)
        {
            if (qteMovingRight)
            {
                qteProgress += qteSpeed * Time.deltaTime;
                if (qteProgress >= 1f) { qteProgress = 1f; qteMovingRight = false; }
            }
            else
            {
                qteProgress -= qteSpeed * Time.deltaTime;
                if (qteProgress <= 0f) { qteProgress = 0f; qteMovingRight = true; }
            }

            if (sliderComp != null) sliderComp.value = qteProgress;
            UpdateArrowPosition();

            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                EvaluateQTEInput();
            }
        }
    }

    private void EvaluateRecipe()
    {
        matchedRecipe = null;
        SetHint(false);
        var normalizedInput = currentIngredients.Where(i => !string.IsNullOrEmpty(i)).Select(i => i.Trim().ToLower()).OrderBy(i => i).ToList();

        foreach (Recipe recipe in availableRecipes)
        {
            if (recipe == null || !recipe.isUnlocked) continue;
            var normalizedRecipe = recipe.ingredients.Where(i => !string.IsNullOrEmpty(i)).Select(i => i.Trim().ToLower()).OrderBy(i => i).ToList();

            if (normalizedRecipe.SequenceEqual(normalizedInput))
            {
                matchedRecipe = recipe;
                SetHint(true);
                Debug.Log("Resep Valid Ditemukan: " + recipe.drinkName + "! Tekan interaksi lagi untuk meracik.");
                break;
            }
        }

        if (matchedRecipe == null)
        {
            Debug.Log("Racikan aktif belum cocok dengan resep manapun.");
        }
    }

    private bool IsDrinkProduct(string itemName)
    {
        return availableRecipes.Any(r => r != null && string.Equals(r.drinkName, itemName, System.StringComparison.OrdinalIgnoreCase));
    }

    
    private void StartAutoQTE()
    {
        isQteActive = true;
        SetHint(false);
        qteProgress = 0f;
        qteMovingRight = true;
        BuildQTEUI();
        qteSpeed = matchedRecipe.qteSpeed;
        goodZoneRect.sizeDelta = new Vector2(880f * matchedRecipe.perfectHalfWidth, 28f);
        perfectZoneRect.sizeDelta = new Vector2(880f * matchedRecipe.perfectHalfWidth * 0.4f, 28f);
        if (panelObj != null) panelObj.SetActive(true);
        if (infoText != null) infoText.text = matchedRecipe.drinkName + "\nTekan SPACE di zona emas!";
        Debug.Log("QTE DIMULAI untuk " + matchedRecipe.drinkName);
    }

    private RectTransform MakeBox(string n, Transform parent, Vector2 size, Vector2 pos, Color col)
    {
        var o = new GameObject(n);
        o.transform.SetParent(parent, false);
        var r = o.AddComponent<RectTransform>();
        r.sizeDelta = size; r.anchoredPosition = pos;
        o.AddComponent<Image>().color = col;
        return r;
    }

    private void BuildQTEUI()
    {
        if (canvasObj != null) return;

        canvasObj = new GameObject("AutoGenerated_QTE_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();

        var panelRect = MakeBox("QTE_Panel", canvasObj.transform, new Vector2(520, 150), new Vector2(0, -200), new Color(0.16f, 0.1f, 0.08f, 0.95f));
        panelObj = panelRect.gameObject;
        var ol = panelObj.AddComponent<Outline>();
        ol.effectColor = new Color(0.95f, 0.85f, 0.65f, 1f);
        ol.effectDistance = new Vector2(4, -4);

        GameObject textObj = new GameObject("InfoText");
        textObj.transform.SetParent(panelObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(480, 60);
        textRect.anchoredPosition = new Vector2(0, 38);
        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        if (qteFont != null) tmp.font = qteFont;
        tmp.fontSize = 26;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.95f, 0.85f);
        infoText = tmp;

        var barRect = MakeBox("ProgressBar", panelObj.transform, new Vector2(440, 28), new Vector2(0, -28), new Color(0.07f, 0.05f, 0.05f, 1f));
        var bol = barRect.gameObject.AddComponent<Outline>();
        bol.effectColor = new Color(0.95f, 0.85f, 0.65f, 1f);
        bol.effectDistance = new Vector2(2, -2);
        sliderComp = barRect.gameObject.AddComponent<Slider>();
        sliderComp.minValue = 0f; sliderComp.maxValue = 1f; sliderComp.value = 0f;
        sliderComp.transition = Selectable.Transition.None;

        goodZoneRect = MakeBox("GoodZone", barRect, new Vector2(88, 28), Vector2.zero, new Color(0.85f, 0.55f, 0.2f, 1f));
        perfectZoneRect = MakeBox("PerfectZone", barRect, new Vector2(35, 28), Vector2.zero, new Color(1f, 0.92f, 0.55f, 1f));

        arrowRect = MakeBox("IndicatorArrow", barRect, new Vector2(8, 44), Vector2.zero, new Color(0.98f, 0.96f, 0.9f, 1f));
        var aol = arrowRect.gameObject.AddComponent<Outline>();
        aol.effectColor = new Color(0.1f, 0.06f, 0.05f, 1f);
        aol.effectDistance = new Vector2(2, -2);
    }

    private void UpdateArrowPosition()
    {
        if (arrowRect == null || sliderComp == null) return;
        float sliderWidth = 440f;
        float posX = (qteProgress - 0.5f) * sliderWidth;
        arrowRect.anchoredPosition = new Vector2(posX, 0);
    }

    private void EvaluateQTEInput()
    {
        isQteActive = false;
        if (panelObj != null) panelObj.SetActive(false);

        float dist = Mathf.Abs(qteProgress - 0.5f);
        lastPerfect = dist <= matchedRecipe.perfectHalfWidth * 0.4f;

        if (dist <= matchedRecipe.perfectHalfWidth)
        {
            Debug.Log("QTE PERFECT! Berhasil meracik.");
            CompleteQTE();
        }
        else
        {
            Debug.Log("QTE GAGAL (Miss)! Minuman gagal.");
            ResetMachine();
        }
    }

    private void CompleteQTE()
    {
        PlayerHand playerHand = FindObjectOfType<PlayerHand>();

        if (playerHand != null && matchedRecipe != null)
        {
            string finalDrink = matchedRecipe.drinkName;
            currentIngredients.Clear();
            matchedRecipe = null;
            RefreshIngredientIcons();
            SetHint(false);
            playerHand.PickUpItem(finalDrink);
            playerHand.heldPerfect = lastPerfect;
            ShowResult(finalDrink, lastPerfect);
            Debug.Log("Berhasil! " + finalDrink + " masuk ke tangan.");
        }
    }

    private void ResetMachine()
    {
        Debug.Log("Mesin di-reset.");
        currentIngredients.Clear();
        matchedRecipe = null;
        RefreshIngredientIcons();
        SetHint(false);
        isQteActive = false;
        if (panelObj != null) panelObj.SetActive(false);
    }
}