using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject handPointer;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private GameObject buyButton;
    [Inject] private TutorialProgressService _tutorialProgressService;
    [Inject] private BlockManager _blockManager;
    [Inject] private WaveManager _waveManager;
    [Inject] private PlayerRuntimeManager _playerRuntimeManager;
    [Inject] private TileManager _tileManager;
    [Inject] private BoardManager _boardManager;

    private RectTransform overlay;
    private RectTransform safeArea;
    private RectTransform pointer;
    private RectTransform destination;
    private TMP_Text title;
    private TMP_Text body;
    private BlockParent first;
    private BlockParent second;
    private BlockParent merged;
    private int step;
    private float stepStarted;
    private bool running;
    public bool IsRunning => running;
    public event System.Action OnStateChanged;
    public int CurrentStep => step;
    public bool CanBuyBlocks => !running || step == 0 || step == 2;

    private void OnDisable()
    {
        BlockDragController.OnDragFinished -= HandleDragFinished;
        _blockManager.OnBlockCreated -= HandleBlockCreated;
        _waveManager.OnWaveCompleted -= HandleWaveCompleted;
    }

    public void StartTutorial()
    {
        if (!_tutorialProgressService.ShouldPlayTutorial || running) return;
        tutorialPanel.SetActive(false);
        instructionText.gameObject.SetActive(false);
        BuildGuidance();
        running = true;
        BlockDragController.OnDragFinished += HandleDragFinished;
        _blockManager.OnBlockCreated += HandleBlockCreated;
        _waveManager.OnWaveCompleted += HandleWaveCompleted;
        EnsureCoins();
        ShowStep(0);
    }

    private void BuildGuidance()
    {
        overlay = RuntimeUI.Overlay("Tutorial guidance", transform, 80);
        safeArea = RuntimeUI.Rect("Safe area", overlay, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var card = RuntimeUI.Rect("Instruction card", safeArea, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(1000f, 190f), new Vector2(0f, 118f));
        var background = card.gameObject.AddComponent<Image>();
        background.color = new Color(0.07f, 0.12f, 0.17f, 0.96f);
        background.raycastTarget = false;
        title = RuntimeUI.Text("Step", card, "", 31f, instructionText.font);
        title.rectTransform.anchorMin = new Vector2(0.03f, 0.62f);
        title.rectTransform.anchorMax = new Vector2(0.77f, 0.96f);
        title.alignment = TextAlignmentOptions.Left;
        title.color = new Color(0.65f, 0.95f, 0.25f);
        body = RuntimeUI.Text("Instruction", card, "", 33f, instructionText.font);
        body.rectTransform.anchorMin = new Vector2(0.03f, 0.06f);
        body.rectTransform.anchorMax = new Vector2(0.97f, 0.61f);
        body.alignment = TextAlignmentOptions.Left;
        var skipRect = RuntimeUI.Rect("Skip tutorial", card, new Vector2(0.87f, 0.8f), new Vector2(0.87f, 0.8f),
            new Vector2(200f, 54f), Vector2.zero);
        RuntimeUI.Button("Skip", skipRect, "Skip", instructionText.font, SkipTutorial);
        pointer = handPointer.GetComponent<RectTransform>();
        pointer.SetParent(overlay, false);
        pointer.localRotation = Quaternion.identity;
        pointer.localScale = Vector3.one;
        pointer.sizeDelta = new Vector2(95f, 116f);
        pointer.pivot = new Vector2(0.365f, 0.99f);
        foreach (var graphic in pointer.GetComponentsInChildren<Graphic>()) graphic.raycastTarget = false;
        destination = RuntimeUI.Rect("Drop target", overlay, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(130f, 110f), Vector2.zero);
        var image = destination.gameObject.AddComponent<Image>();
        image.color = new Color(0.65f, 1f, 0.15f, 0.22f);
        image.raycastTarget = false;
        var outline = destination.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.75f, 1f, 0.2f, 1f);
        outline.effectDistance = new Vector2(4f, 4f);
        pointer.SetAsLastSibling();
    }

    private void ShowStep(int index)
    {
        step = index;
        stepStarted = Time.unscaledTime;
        string[] headings = { "1 / 6   BUY AMMO", "2 / 6   BUILD YOUR SUPPLY", "3 / 6   BUY A MATCH", "4 / 6   MERGE", "5 / 6   MORE AMMO", "6 / 6   DEFEND" };
        string[] instructions = {
            "Tap the green price button to buy an ammo block.",
            "Drag the block from the lower shelf onto the orange grid.",
            "Buy one more block of the same rank. Matching blocks can merge.",
            "Drag the block from the grid onto the matching block on the shelf.",
            "Drag the upgraded block onto the grid. It now makes more ammo.",
            "Blocks feed the conveyor. Turrets fire automatically. Clear the first wave!"
        };
        title.text = headings[step];
        body.text = instructions[step];
        if (step == 2) EnsureCoins();
        if (step == 3)
        {
            first.CanDrag = true;
            second.CanDrag = false;
        }
        if (step == 5)
        {
            ReleaseBlocks();
            _waveManager.StartWaves();
        }
        OnStateChanged?.Invoke();
    }

    private void EnsureCoins()
    {
        float missing = _blockManager.BlockPrice - _playerRuntimeManager.GetCurrency(CurrencyType.Coins);
        if (missing > 0f) _playerRuntimeManager.AddCurrency(CurrencyType.Coins, missing);
    }

    private void HandleBlockCreated(BlockParent block)
    {
        if (!running) return;
        if (step == 0)
        {
            first = block;
            ShowStep(1);
        }
        else if (step == 2)
        {
            second = block;
            ShowStep(3);
        }
    }

    private void HandleDragFinished(DragResult result)
    {
        if (!running) return;
        if (step == 1 && result.Action == DragActionType.PlaceOnBoard && result.Block1 == first)
        {
            first.CanDrag = false;
            ShowStep(2);
        }
        else if (step == 3 && result.Action == DragActionType.Merge &&
            ((result.Block1 == first && result.Block2 == second) || (result.Block1 == second && result.Block2 == first)))
        {
            merged = _blockManager.LastCreatedBlock;
            merged.CanDrag = true;
            ShowStep(4);
        }
        else if (step == 4 && result.Action == DragActionType.PlaceOnBoard && result.Block1 == merged)
            ShowStep(5);
    }

    private void HandleWaveCompleted(int wave)
    {
        if (running && step == 5) EndTutorial();
    }

    private void LateUpdate()
    {
        if (!running || overlay == null) return;
        Rect safe = Screen.safeArea;
        safeArea.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
        safeArea.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
        bool visible = Time.timeScale > 0f;
        safeArea.gameObject.SetActive(visible);
        pointer.gameObject.SetActive(visible && step < 5 && !Input.GetMouseButton(0));
        destination.gameObject.SetActive(visible && step < 5);
        if (!visible || step == 5) return;
        Vector2 from;
        Vector2 to;
        if (step == 0 || step == 2)
        {
            var rect = buyButton.GetComponent<RectTransform>();
            var canvas = rect.GetComponentInParent<Canvas>();
            var eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null :
                canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
            from = RectTransformUtility.WorldToScreenPoint(eventCamera,
                rect.TransformPoint(rect.rect.center));
            to = from;
        }
        else
        {
            var block = step == 1 || step == 3 ? first : merged;
            if (block == null || Camera.main == null) return;
            from = Camera.main.WorldToScreenPoint(block.transform.position + Vector3.up * 0.8f);
            Vector3 target;
            if (step == 3) target = second.transform.position + Vector3.up * 0.8f;
            else if (!TryGetBoardPlacement(block, out target, out _)) return;
            to = Camera.main.WorldToScreenPoint(target);
        }
        float cycle = (Time.unscaledTime - stepStarted) % 2f;
        float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((cycle - 0.3f) / 1.2f));
        var screen = Vector2.Lerp(from, to, progress);
        if (step == 0 || step == 2) screen += Vector2.up * (12f * Mathf.Sin(Time.unscaledTime * 4f));
        RectTransformUtility.ScreenPointToLocalPointInRectangle(overlay, screen, null, out var local);
        pointer.anchoredPosition = local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(overlay, to, null, out local);
        destination.anchoredPosition = local;
    }

    public bool TryGetBoardPlacement(BlockParent block, out Vector3 position, out Vector2Int topLeft)
    {
        position = default;
        topLeft = default;
        int height = block.shape.Count;
        int width = block.shape[0].row.Length;
        float best = float.MaxValue;
        for (int x = 0; x <= _tileManager.rows - width; x++)
            for (int y = 0; y <= _tileManager.columns - height; y++)
            {
                bool valid = true;
                for (int r = 0; r < height; r++)
                    for (int c = 0; c < width; c++)
                        if (block.shape[r].row[c] == 1 && !_tileManager.GetTileAt(new Vector2Int(x + c, y + r)).isAvailable)
                            valid = false;
                float score = new Vector2(x + width * 0.5f - _tileManager.rows * 0.5f,
                    y + height * 0.5f - _tileManager.columns * 0.5f).sqrMagnitude;
                if (!valid || score >= best) continue;
                best = score;
                topLeft = new Vector2Int(x, y);
            }
        if (best == float.MaxValue) return false;
        position = _tileManager.TileToWorld(topLeft) + new Vector3((width - 1) * 0.5f, 0f, (height - 1) * 0.5f) * _tileManager.CellSize;
        return true;
    }

    public void SkipTutorial()
    {
        if (!running) return;
        if (_boardManager.Blocks.Count == 0)
        {
            var block = merged != null ? merged : first;
            if (block == null)
            {
                _blockManager.CreateBlock(true);
                block = _blockManager.LastCreatedBlock;
            }
            if (block != null && TryGetBoardPlacement(block, out var position, out var index))
            {
                var tiles = new System.Collections.Generic.List<Tile>();
                for (int r = 0; r < block.shape.Count; r++)
                    for (int c = 0; c < block.shape[r].row.Length; c++)
                        if (block.shape[r].row[c] == 1) tiles.Add(_tileManager.GetTileAt(index + new Vector2Int(c, r)));
                if (block.blockData.platform >= 0)
                    FindAnyObjectByType<MergePlatformsManager>().SetPlatformFree(block.blockData.platform);
                block.PlaceOnBoard(tiles, position);
                block.blockData.topLeftIndex = index;
                _boardManager.RegisterBlock(block);
            }
        }
        _waveManager.Unfreeze();
        _waveManager.StartWaves();
        EndTutorial();
    }

    private void ReleaseBlocks()
    {
        if (first != null) first.CanDrag = true;
        if (second != null) second.CanDrag = true;
        if (merged != null) merged.CanDrag = true;
    }

    private void EndTutorial()
    {
        _tutorialProgressService.CompleteTutorial();
        HideTutorial();
    }

    public void HideTutorial()
    {
        running = false;
        OnStateChanged?.Invoke();
        ReleaseBlocks();
        if (overlay != null) overlay.gameObject.SetActive(false);
        handPointer.SetActive(false);
        instructionText.gameObject.SetActive(false);
        tutorialPanel.SetActive(false);
        enabled = false;
    }
}
