using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class StoryManager : MonoBehaviour
{
    [Header("Story Data")]
    [SerializeField] private StoryData storyData;

    [Header("Settings")]
    [SerializeField] private float slideDuration = 1.2f;
    [SerializeField] private float typewriterSpeed = 30f;
    [SerializeField] private bool showSkipButton = true;
    [SerializeField] private string nextSceneName = "";

    [Header("UI References (optional)")]
    [SerializeField] private Canvas targetCanvas;

    private int currentSlide;
    private float slideWidth;
    private RectTransform contentContainer;
    private RectTransform borderContainer;
    private Canvas canvasRef;
    private GameObject skipButton;
    private bool isTransitioning;

    private List<RectTransform> slidePanels = new List<RectTransform>();
    private List<Image> slideImages = new List<Image>();
    private List<GameObject> dotIndicators = new List<GameObject>();

    private GameObject textPanel;
    private Text speakerText;
    private Text dialogueText;
    private Coroutine typewriterCoroutine;
    private string fullDialogueText;

    private const float BORDER_THICKNESS = 6f;
    private const float DIALOGUE_HEIGHT_RATIO = 0.28f;

    void Start()
    {
        if (storyData == null || storyData.slides.Length == 0)
        {
            Debug.LogError("StoryManager: No story data assigned or empty.");
            return;
        }
        SetupCanvas();
        CreateLayout();
        ShowSlide(0);
    }

    void Update()
    {
        if (isTransitioning) return;

        bool advance = Input.GetKeyDown(KeyCode.Space) ||
                       Input.GetKeyDown(KeyCode.Return) ||
                       Input.GetMouseButtonDown(0);

        if (!advance) return;

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
            dialogueText.text = fullDialogueText;
            return;
        }

        Advance();
    }

    private void SetupCanvas()
    {
        canvasRef = targetCanvas;
        if (canvasRef == null)
        {
            GameObject canvasGO = new GameObject("StoryCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasRef = canvasGO.GetComponent<Canvas>();
            canvasRef.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasRef.sortingOrder = 100;

            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject bg = new GameObject("BackgroundOverlay", typeof(Image));
            bg.transform.SetParent(canvasRef.transform, false);
            Image bgImg = bg.GetComponent<Image>();
            bgImg.color = new Color(0.05f, 0.05f, 0.05f, 1f);
            bgImg.rectTransform.anchorMin = Vector2.zero;
            bgImg.rectTransform.anchorMax = Vector2.one;
            bgImg.rectTransform.sizeDelta = Vector2.zero;
        }
    }

    private void CreateLayout()
    {
        slideWidth = Screen.width;

        GameObject borderHolder = new GameObject("BorderContainer", typeof(Image));
        borderHolder.transform.SetParent(canvasRef.transform, false);
        borderContainer = borderHolder.GetComponent<RectTransform>();
        borderContainer.anchorMin = new Vector2(0, DIALOGUE_HEIGHT_RATIO);
        borderContainer.anchorMax = new Vector2(1, 1);
        borderContainer.offsetMin = new Vector2(20, 10);
        borderContainer.offsetMax = new Vector2(-20, -10);

        Image borderImage = borderHolder.GetComponent<Image>();
        borderImage.color = Color.black;

        GameObject panelMask = new GameObject("PanelMask", typeof(RectTransform), typeof(Image), typeof(Mask));
        panelMask.transform.SetParent(borderContainer, false);
        RectTransform maskRect = panelMask.GetComponent<RectTransform>();
        maskRect.anchorMin = Vector2.zero;
        maskRect.anchorMax = Vector2.one;
        maskRect.offsetMin = new Vector2(BORDER_THICKNESS, BORDER_THICKNESS);
        maskRect.offsetMax = new Vector2(-BORDER_THICKNESS, -BORDER_THICKNESS);
        panelMask.GetComponent<Image>().color = Color.white;
        panelMask.GetComponent<Mask>().showMaskGraphic = false;

        GameObject container = new GameObject("ContentContainer", typeof(RectTransform));
        container.transform.SetParent(panelMask.transform, false);
        contentContainer = container.GetComponent<RectTransform>();
        contentContainer.anchorMin = new Vector2(0, 0);
        contentContainer.anchorMax = new Vector2(0, 1);
        contentContainer.pivot = new Vector2(0, 0.5f);

        int total = storyData.slides.Length;
        float totalWidth = slideWidth * total;
        contentContainer.sizeDelta = new Vector2(totalWidth, 0);

        for (int i = 0; i < total; i++)
        {
            CreateSlidePanel(i, total);
        }

        CreateDialogueBox();
        CreateProgressDots(total);
        CreateSkipButton();
    }

    private void CreateSlidePanel(int index, int total)
    {
        GameObject panel = new GameObject("Slide_" + index, typeof(RectTransform), typeof(Image));
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.SetParent(contentContainer, false);
        rect.pivot = new Vector2(0, 0.5f);
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 1);
        rect.sizeDelta = new Vector2(slideWidth, 0);
        rect.anchoredPosition = new Vector2(slideWidth * index, 0);

        Image img = panel.GetComponent<Image>();
        img.preserveAspect = true;

        StorySlide data = storyData.slides[index];
        if (data.panelImage != null)
        {
            img.sprite = data.panelImage;
            img.color = data.panelTint;
        }
        else
        {
            img.color = new Color(0.1f, 0.1f, 0.15f);
        }

        slidePanels.Add(rect);
        slideImages.Add(img);
    }

    private void CreateDialogueBox()
    {
        textPanel = new GameObject("DialoguePanel", typeof(Image));
        textPanel.transform.SetParent(canvasRef.transform, false);
        textPanelRect = textPanel.GetComponent<RectTransform>();
        textPanelRect.anchorMin = new Vector2(0.05f, 0.01f);
        textPanelRect.anchorMax = new Vector2(0.95f, DIALOGUE_HEIGHT_RATIO - 0.01f);
        textPanelRect.offsetMin = Vector2.zero;
        textPanelRect.offsetMax = Vector2.zero;

        Image bg = textPanel.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.85f);

        GameObject speakerGO = new GameObject("SpeakerText", typeof(Text));
        speakerGO.transform.SetParent(textPanel.transform, false);
        speakerText = speakerGO.GetComponent<Text>();
        speakerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        speakerText.fontStyle = FontStyle.Bold;
        speakerText.fontSize = 28;
        speakerText.color = new Color(0.8f, 0.4f, 0.1f);
        speakerText.alignment = TextAnchor.LowerLeft;
        speakerText.rectTransform.anchorMin = new Vector2(0.02f, 0.5f);
        speakerText.rectTransform.anchorMax = new Vector2(0.3f, 0.98f);
        speakerText.rectTransform.offsetMin = Vector2.zero;
        speakerText.rectTransform.offsetMax = Vector2.zero;

        GameObject dialogueGO = new GameObject("DialogueText", typeof(Text));
        dialogueGO.transform.SetParent(textPanel.transform, false);
        dialogueText = dialogueGO.GetComponent<Text>();
        dialogueText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        dialogueText.fontSize = 24;
        dialogueText.color = Color.white;
        dialogueText.alignment = TextAnchor.UpperLeft;
        dialogueText.rectTransform.anchorMin = new Vector2(0.02f, 0.02f);
        dialogueText.rectTransform.anchorMax = new Vector2(0.98f, 0.55f);
        dialogueText.rectTransform.offsetMin = Vector2.zero;
        dialogueText.rectTransform.offsetMax = Vector2.zero;
    }

    private RectTransform textPanelRect;

    private void CreateProgressDots(int total)
    {
        GameObject dotsHolder = new GameObject("ProgressDots", typeof(RectTransform));
        dotsHolder.transform.SetParent(canvasRef.transform, false);
        RectTransform dotsRect = dotsHolder.GetComponent<RectTransform>();
        dotsRect.anchorMin = new Vector2(0.5f, DIALOGUE_HEIGHT_RATIO - 0.04f);
        dotsRect.anchorMax = new Vector2(0.5f, DIALOGUE_HEIGHT_RATIO - 0.04f);
        dotsRect.sizeDelta = new Vector2(total * 30f, 10f);

        HorizontalLayoutGroup layout = dotsHolder.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 8f;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        for (int i = 0; i < total; i++)
        {
            GameObject dot = new GameObject("Dot_" + i, typeof(Image));
            dot.transform.SetParent(dotsHolder.transform, false);
            Image dotImg = dot.GetComponent<Image>();
            dotImg.rectTransform.sizeDelta = new Vector2(10, 10);
            dotImg.color = i == 0 ? Color.white : new Color(0.4f, 0.4f, 0.4f);
            dotIndicators.Add(dot);
        }
    }

    private void CreateSkipButton()
    {
        if (!showSkipButton) return;

        skipButton = new GameObject("SkipButton", typeof(Image), typeof(Button));
        skipButton.transform.SetParent(canvasRef.transform, false);
        RectTransform rect = skipButton.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.92f, 0.92f);
        rect.anchorMax = new Vector2(0.98f, 0.98f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = skipButton.GetComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);

        GameObject skipText = new GameObject("SkipLabel", typeof(Text));
        skipText.transform.SetParent(skipButton.transform, false);
        Text st = skipText.GetComponent<Text>();
        st.text = "SKIP >>";
        st.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        st.fontSize = 18;
        st.fontStyle = FontStyle.Bold;
        st.color = Color.white;
        st.alignment = TextAnchor.MiddleCenter;
        st.rectTransform.anchorMin = Vector2.zero;
        st.rectTransform.anchorMax = Vector2.one;
        st.rectTransform.sizeDelta = Vector2.zero;

        Button btn = skipButton.GetComponent<Button>();
        btn.onClick.AddListener(SkipStory);
    }

    private void ShowSlide(int index)
    {
        currentSlide = index;
        UpdateDialogue(index);
        UpdateDots(index);
    }

    private void UpdateDialogue(int index)
    {
        StorySlide data = storyData.slides[index];

        if (string.IsNullOrEmpty(data.speaker))
        {
            speakerText.text = "";
            dialogueText.rectTransform.anchorMin = new Vector2(0.02f, 0.02f);
            dialogueText.rectTransform.anchorMax = new Vector2(0.98f, 0.95f);
        }
        else
        {
            speakerText.text = data.speaker;
            dialogueText.rectTransform.anchorMin = new Vector2(0.02f, 0.02f);
            dialogueText.rectTransform.anchorMax = new Vector2(0.98f, 0.55f);
        }

        fullDialogueText = data.text;
        dialogueText.text = "";
        dialogueText.color = data.textColor;
        dialogueText.alignment = data.textAlignment;

        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = StartCoroutine(TypewriterEffect());
    }

    private IEnumerator TypewriterEffect()
    {
        dialogueText.text = "";
        foreach (char c in fullDialogueText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(1f / typewriterSpeed);
        }
        typewriterCoroutine = null;
    }

    private void UpdateDots(int activeIndex)
    {
        for (int i = 0; i < dotIndicators.Count; i++)
        {
            Image dot = dotIndicators[i].GetComponent<Image>();
            if (i == activeIndex)
                dot.color = Color.white;
            else if (i < activeIndex)
                dot.color = new Color(0.6f, 0.6f, 0.6f);
            else
                dot.color = new Color(0.4f, 0.4f, 0.4f);
        }
    }

    public void Advance()
    {
        if (currentSlide >= storyData.slides.Length - 1)
        {
            EndStory();
            return;
        }
        StartCoroutine(SlideTo(currentSlide + 1));
    }

    private IEnumerator SlideTo(int target)
    {
        isTransitioning = true;
        float startX = contentContainer.anchoredPosition.x;
        float endX = -slideWidth * target;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / slideDuration);
            float x = Mathf.Lerp(startX, endX, t);
            contentContainer.anchoredPosition = new Vector2(x, contentContainer.anchoredPosition.y);
            yield return null;
        }

        contentContainer.anchoredPosition = new Vector2(endX, contentContainer.anchoredPosition.y);
        ShowSlide(target);
        isTransitioning = false;
    }

    public void SkipStory()
    {
        StopAllCoroutines();
        isTransitioning = false;
        EndStory();
    }

    private void EndStory()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            if (canvasRef != null && canvasRef.gameObject.name == "StoryCanvas")
                Destroy(canvasRef.gameObject);
            Destroy(gameObject);
        }
    }
}
