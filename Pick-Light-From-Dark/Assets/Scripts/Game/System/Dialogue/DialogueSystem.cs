using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueSystem : MonoBehaviour
{
    private static DialogueSystem _instance;
    public static DialogueSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DialogueSystem>();
                if (_instance == null)
                {
                    var go = new GameObject("[DialogueSystem]");
                    _instance = go.AddComponent<DialogueSystem>();
                    Debug.LogWarning("DialogueSystem 未在场景中配置，已自动创建（将从 Resources 加载依赖项）");
                }
            }
            return _instance;
        }
    }

    public enum DialogueMode
    {
        Gal,
        Game
    }

    [Header("配置")]
    public RoleConfig roleConfig;
    public BackgroundConfig backgroundConfig;

    [Header("Btns容器")]
    [SerializeField] private GameObject btnsPrefab;

    [Header("测试-快进间隔(秒)")]
    public float fastForwardInterval = 0.15f;

    private DialogueMode currentMode = DialogueMode.Gal;
    private List<DialogueLine> lines;
    private int lineIndex = 0;
    private bool isChoosing = false;

    private IDialoguePanel panelUI;
    private GameObject btnsObj;
    private BtnsUI btnsUI;

    // ===== 快进快退状态 =====
    private float ffCooldown = 0f;
    private List<int> lineIndexHistory = new List<int>();
    private bool isRewinding = false;
    private float originalTypingSpeed = 0.03f;

    private bool isFastForwarding = false;
    private bool isAutoRewinding = false;

    public bool IsFastForwarding => isFastForwarding;
    public bool IsAutoRewinding => isAutoRewinding;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 立即加载依赖（不能等 Start，因为回调可能在 Start 之前触发）
        if (btnsPrefab == null)
            btnsPrefab = Resources.Load<GameObject>("UI/Dialogue/Btns");
        if (roleConfig == null)
            roleConfig = Resources.Load<RoleConfig>("UI/Dialogue/RoleConfig");
        if (backgroundConfig == null)
            backgroundConfig = Resources.Load<BackgroundConfig>("UI/Dialogue/BackgroundConfig");

        if (backgroundConfig != null) backgroundConfig.Init();
        if (roleConfig != null) roleConfig.Init();
    }

    // =========================================
    // 绑定UI
    // =========================================
    public void BindPanel(IDialoguePanel panel)
    {
        panelUI = panel;
    }

    // =========================================
    // 启动对话
    // =========================================
    public void StartDialogue(TextAsset txt, DialogueMode mode)
    {
        if (panelUI == null)
        {
            Debug.LogError("没有绑定UI！请先调用 BindPanel");
            return;
        }

        if (txt == null)
        {
            Debug.LogError("对话文本为空！请检查 BeginPanel 上的 dialogueText 是否已赋值");
            return;
        }

        currentMode = mode;
        lines = DialogueParser.Parse(txt);
        lineIndex = 0;

        panelUI.Show();
        NextLine();
    }

    public bool CanRewind => lines != null && lineIndexHistory.Count >= 2;
    public bool CanFastForward => lines != null && lineIndex < lines.Count;

    void Update()
    {
        if (isChoosing) return;

        // 持续快进
        if (isFastForwarding)
        {
            ffCooldown -= Time.unscaledDeltaTime;
            if (ffCooldown <= 0f)
            {
                ffCooldown = fastForwardInterval;
                FastForwardTick();
            }
        }
        // 持续快退
        else if (isAutoRewinding)
        {
            ffCooldown -= Time.unscaledDeltaTime;
            if (ffCooldown <= 0f)
            {
                ffCooldown = fastForwardInterval;
                RewindTick();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            // Gal 模式下若正在打字，先跳过打字机
            if (panelUI is GalDialoguePanel galPanel && galPanel.IsTyping)
            {
                galPanel.SkipTyping();
                return;
            }

            NextLine();
        }
    }

    /// <summary>切换快进模式：持续自动推进</summary>
    public void ToggleFastForward()
    {
        isFastForwarding = !isFastForwarding;
        isAutoRewinding = false;
        ffCooldown = 0f;

        if (isFastForwarding && panelUI is GalDialoguePanel galPanel)
            galPanel.SetTypingSpeed(originalTypingSpeed * 0.05f);
        else if (panelUI is GalDialoguePanel galPanel2)
            galPanel2.SetTypingSpeed(originalTypingSpeed);
    }

    /// <summary>切换快退模式：持续自动回退</summary>
    public void ToggleRewind()
    {
        isAutoRewinding = !isAutoRewinding;
        isFastForwarding = false;
        ffCooldown = 0f;
    }

    void FastForwardTick()
    {
        if (panelUI is GalDialoguePanel galPanel)
        {
            if (galPanel.IsTyping)
            {
                galPanel.SkipTyping();
                return;
            }
        }

        if (!isChoosing)
            NextLine();
    }

    void RewindTick()
    {
        if (lines == null || lineIndexHistory.Count < 2)
        {
            isAutoRewinding = false;
            return;
        }

        isRewinding = true;
        lineIndexHistory.RemoveAt(lineIndexHistory.Count - 1);
        int prevIndex = lineIndexHistory[lineIndexHistory.Count - 1];
        lineIndex = prevIndex;
        NextLine();
    }

    // =========================================
    // 下一句
    // =========================================
    void NextLine()
    {
        if (btnsObj != null)
            btnsObj.SetActive(false);

        if (lines == null || lineIndex >= lines.Count)
        {
            isFastForwarding = false;
            EndDialogue();
            return;
        }

        DialogueLine line = lines[lineIndex];

        // 记录历史（快退时不重复记录）
        if (!isRewinding)
        {
            // 如果在历史中间，截断后面的记录
            if (lineIndexHistory.Count > 0 && lineIndexHistory[lineIndexHistory.Count - 1] != lineIndex)
            {
                // 正常推进，添加新记录
            }
            lineIndexHistory.Add(lineIndex);
        }
        isRewinding = false;

        lineIndex++;

        if (line.type == "选项")
        {
            if (currentMode == DialogueMode.Gal)
            {
                ShowChoice(line);
                return;
            }
            else
            {
                // Game模式下跳过选项行
                Debug.LogWarning("Game模式不支持选项，已跳过");
                NextLine();
                return;
            }
        }

        ShowLine(line);
        DispatchCommands(line);
    }

    // =========================================
    // 显示文本
    // =========================================
    void ShowLine(DialogueLine line)
    {
        if (line.type == "指令")
            return;

        string speaker = line.speaker;

        if (line.type == "旁白" || line.type == "场景")
            speaker = "陆萤";

        panelUI.SetContent(speaker, line.content);

        // [已注释] Fungus 桥接：同步显示到 Fungus SayDialog
        // FungusBridge.Instance?.ShowSay(speaker, line.content);

        // 黑屏检测：文本中出现"黑屏"时，所有层切纯黑背景
        if (!string.IsNullOrEmpty(line.content) && line.content.Contains("黑屏"))
        {
            var blackSprite = Resources.Load<Sprite>("UI/Background/Bg_Black");
            if (blackSprite != null)
            {
                panelUI.SetLayerBackground("bg", blackSprite, "fade");
                panelUI.SetLayerBackground("mg", blackSprite, "fade");
                panelUI.SetLayerBackground("fg", blackSprite, "fade");
            }
            else
                Debug.LogWarning("[DialogueSystem] 未找到黑屏背景资源 UI/Background/Bg_Black");
        }
    }

    // =========================================
    // 角色
    // =========================================
    private void AutoSetRole(DialogueLine line)
    {
        if (roleConfig == null) return;

        if (line.type == "对话" && currentMode == DialogueMode.Gal)
        {
            Sprite sprite = roleConfig.GetRoleSprite(line.speaker);
            panelUI.SetRole(sprite);
        }
        else
        {
            panelUI.SetRole(null);
        }
    }

    // =========================================
    // 显式指令分发（背景 / 音效 / BGM）
    // =========================================
    private void DispatchCommands(DialogueLine line)
    {
        // 分层背景处理
        if (line.isLayerCommand)
        {
            // [layer:...] 全换：显式指定的层才更新（null=未指定，保持原样；有值包括空字符串=更新）
            if (line.bg != null) DispatchLayerBg("bg", line.bg, line.transition);
            if (line.mg != null) DispatchLayerBg("mg", line.mg, line.transition);
            if (line.fg != null) DispatchLayerBg("fg", line.fg, line.transition);
        }
        else
        {
            // 单换指令：有值才更新
            if (!string.IsNullOrEmpty(line.bg)) DispatchLayerBg("bg", line.bg, line.transition);
            if (!string.IsNullOrEmpty(line.mg)) DispatchLayerBg("mg", line.mg, line.transition);
            if (!string.IsNullOrEmpty(line.fg)) DispatchLayerBg("fg", line.fg, line.transition);
        }

        if (!string.IsNullOrEmpty(line.solid))
        {
            Color c = ParseSolidColor(line.solid);
            panelUI.SetSolidBackground(c);
        }

        if (!string.IsNullOrEmpty(line.se))
        {
            MusicMgr.Instance.PlaySound(line.se);
            // [已注释] Fungus 桥接
            // FungusBridge.Instance?.PlaySound(line.se);
        }

        if (!string.IsNullOrEmpty(line.bgm))
        {
            MusicMgr.Instance.PlayBKMusic(line.bgm);
            // [已注释] Fungus 桥接
            // FungusBridge.Instance?.PlayBGM(line.bgm);
        }
    }

    private Sprite LoadBgSprite(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        Sprite s = Resources.Load<Sprite>("CG/" + name);
        if (s == null) s = Resources.Load<Sprite>("Backgrounds/" + name);
        if (s == null) s = Resources.Load<Sprite>("UI/Dialogue/Backgrounds/" + name);
        if (s == null && backgroundConfig != null) s = backgroundConfig.GetBg(name);

        return s;
    }

    private Color ParseSolidColor(string colorStr)
    {
        if (string.IsNullOrEmpty(colorStr)) return Color.clear;

        if (colorStr.StartsWith("#") && colorStr.Length == 7)
        {
            if (ColorUtility.TryParseHtmlString(colorStr, out Color c))
                return c;
        }

        switch (colorStr.ToLower())
        {
            case "black": return Color.black;
            case "white": return Color.white;
            case "red": return Color.red;
            case "green": return Color.green;
            case "blue": return Color.blue;
            case "yellow": return Color.yellow;
            case "cyan": return Color.cyan;
            case "magenta": return Color.magenta;
            case "gray": return Color.gray;
            case "grey": return Color.grey;
            case "clear":
            case "transparent": return Color.clear;
            default:
                Debug.LogWarning($"[DialogueSystem] 未知颜色名称: {colorStr}");
                return Color.clear;
        }
    }

    private void DispatchLayerBg(string layer, string spriteName, string transition)
    {
        Sprite s = LoadBgSprite(spriteName);
        if (s != null)
        {
            panelUI.SetLayerBackground(layer, s, transition);
        }
        else if (!string.IsNullOrEmpty(spriteName))
        {
            Debug.LogWarning($"[DialogueSystem] 背景未找到: {spriteName}（层: {layer}）。请将其放入 Resources/CG/、Resources/Backgrounds/ 或 BackgroundConfig 中。");
        }
        else
        {
            // spriteName 为空字符串 → 清空该层
            panelUI.SetLayerBackground(layer, null, transition);
        }
    }

    // =========================================
    // 选项
    // =========================================
    void ShowChoice(DialogueLine line)
    {
        isChoosing = true;

        CreateBtns();
        if (btnsObj == null || btnsUI == null)
        {
            Debug.LogError("选项按钮创建失败，跳过选项");
            isChoosing = false;
            NextLine();
            return;
        }

        btnsObj.SetActive(true);
        panelUI.SetContent("", "请选择：");

        // Btn1
        if (!string.IsNullOrEmpty(line.choice1))
        {
            btnsUI.btn1.gameObject.SetActive(true);
            btnsUI.btn1Text.text = line.choice1;
            btnsUI.btn1.onClick.RemoveAllListeners();
            btnsUI.btn1.onClick.AddListener(() => OnChoose1(line));
        }
        else
        {
            btnsUI.btn1.gameObject.SetActive(false);
        }

        // Btn2
        if (!string.IsNullOrEmpty(line.choice2))
        {
            btnsUI.btn2.gameObject.SetActive(true);
            btnsUI.btn2Text.text = line.choice2;
            btnsUI.btn2.onClick.RemoveAllListeners();
            btnsUI.btn2.onClick.AddListener(() => OnChoose2(line));
        }
        else
        {
            btnsUI.btn2.gameObject.SetActive(false);
        }
    }

    void CreateBtns()
    {
        if (btnsPrefab == null)
        {
            Debug.LogError("BtnsPrefab 没赋值！");
            return;
        }

        if (btnsObj == null)
        {
            if (panelUI is MonoBehaviour mb)
            {
                btnsObj = Instantiate(btnsPrefab, mb.transform);
                btnsUI = btnsObj.GetComponent<BtnsUI>();
            }
            else
            {
                Debug.LogError("panelUI 不是 MonoBehaviour，无法挂载按钮");
            }
        }
    }

    void OnChoose1(DialogueLine line)
    {
        isChoosing = false;
        if (btnsObj != null) btnsObj.SetActive(false);

        panelUI.SetContent("", line.choice1Result);
        Invoke(nameof(NextLine), 1f);
    }

    void OnChoose2(DialogueLine line)
    {
        isChoosing = false;
        if (btnsObj != null) btnsObj.SetActive(false);

        // 如果有 choice2Result，显示结果后继续
        if (!string.IsNullOrEmpty(line.choice2Result))
        {
            panelUI.SetContent("", line.choice2Result);
            Invoke(nameof(NextLine), 1f);
        }
        else
        {
            EndDialogue();
            SceneManager.LoadScene("GameScene");
        }
    }

    // =========================================
    // 结束
    // =========================================
    public void EndDialogue()
    {
        lines = null;
        isChoosing = false;
        isFastForwarding = false;
        isAutoRewinding = false;
        CancelInvoke();

        if (btnsObj != null)
        {
            Destroy(btnsObj);
            btnsObj = null;
            btnsUI = null;
        }

        if (panelUI != null)
        {
            panelUI.Hide();
        }

        // [已注释] Fungus 桥接：隐藏 SayDialog
        // FungusBridge.Instance?.HideSayDialog();
    }
}
