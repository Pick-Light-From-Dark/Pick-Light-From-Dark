using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DialogueSystem : BasePanel
{
    public static DialogueSystem Instance;

    public enum DialogueMode
    {
        Gal,
        Game
    }

    [Header("配置")]
    public RoleConfig roleConfig;
    public BackgroundConfig backgroundConfig;

    [Header("UI 预制体")]
    public GameObject galUIPrefab;       // 剧情用
    public GameObject gameUIPrefab;      // 游戏内用
    public Transform uiRoot;

    private DialogueMode currentMode;
    private List<DialogueLine> lines;
    private int lineIndex = 0;
    private bool isChoosing = false;
    private GameObject currentPanel;
    private DialoguePanelUI panelUI;

    [Header("Btns容器（新的）")]
    [SerializeField] private GameObject btnsPrefab;

    private GameObject btnsObj;
    private BtnsUI btnsUI;
    // 绑定的UI组件
    private TextMeshProUGUI contentText;
    private TextMeshProUGUI speakerText;
    private Image roleImage;
    private Image backgroundImage;       // Gal 全屏背景
    private Image dialogBoxBackground;   // 对话框背景（两种模式都用）

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    /// <summary>
    /// 启动剧情对话（Gal模式：含背景、立绘、选项）
    /// </summary>
    public void StartDialogue(TextAsset txt, DialogueMode mode)
    {
        currentMode = mode;  // 设置当前模式为 Game 或 Gal
        CreatePanelByMode(mode);  // 根据模式创建对应的UI面板

        lines = DialogueParser.Parse(txt);  // 解析对话文本
        lineIndex = 0;

        NextLine();  // 显示第一行对话
        panelUI.gameObject.SetActive(true);  // 显示对话框
    }

    /// <summary>
    /// 游戏内显示对话（含名字 + 对话框背景，动态创建）
    /// </summary>
    public void ShowGameDialogue(string speaker, string content)
    {
        // 创建Game模式的对话框
        CreatePanelByMode(DialogueMode.Game);

        // 强制显示名字与对话框
        speakerText.text = speaker;
        contentText.text = content;

        // 只显示背景，不显示角色图像
        dialogBoxBackground.gameObject.SetActive(true);
        speakerText.gameObject.SetActive(true);
        contentText.gameObject.SetActive(true);

        // 隐藏角色图像
        if (roleImage != null)
        {
            roleImage.gameObject.SetActive(false); // 隐藏角色图像
        }

        // 显示背景
        backgroundImage.gameObject.SetActive(true);

        // 隐藏选项按钮
        if (btnsObj != null)
        {
            btnsObj.SetActive(false);
        }
    }

    /// <summary>
    /// 按模式创建对应的面板
    /// </summary>
    void CreatePanelByMode(DialogueMode mode)
    {
        // 1. 先删除旧的
        if (currentPanel != null)
        {
            Destroy(currentPanel);
            currentPanel = null;
        }

        // 2. 选择 prefab
        GameObject prefab = null;

        if (mode == DialogueMode.Gal)
            prefab = galUIPrefab;
        else
            prefab = gameUIPrefab;

        // ❗防御
        if (prefab == null)
        {
            Debug.LogError("Prefab 没赋值！");
            return;
        }

        // 3. 实例化
        currentPanel = Instantiate(prefab, uiRoot);

        // ❗防御
        if (currentPanel == null)
        {
            Debug.LogError("实例化失败！");
            return;
        }

        // 4. 获取 UI 脚本（现在才安全）
        panelUI = currentPanel.GetComponent<DialoguePanelUI>();

        if (panelUI == null)
        {
            Debug.LogError("没找到 DialoguePanelUI！");
            return;
        }

        // 5. 绑定引用
        contentText = panelUI.contentText;
        speakerText = panelUI.speakerText;
        roleImage = panelUI.roleImage;
        backgroundImage = panelUI.backgroundImage;
    }

    private void Start()
    {
        if (backgroundConfig != null) backgroundConfig.Init();
        if (roleConfig != null) roleConfig.Init();
    }

    void Update()
    {
        if (isChoosing) return;
        if (Input.GetMouseButtonDown(0))
            NextLine();
    }

    void NextLine()
    {
        if (btnsObj != null)
            btnsObj.SetActive(false);

        if (lines == null || lineIndex >= lines.Count)
        {
            EndDialogue(); // 结束对话
            return;
        }

        DialogueLine line = lines[lineIndex];
        lineIndex++;

        // 只有 Game 模式能显示文本（没有选项）
        ShowLine(line); // 显示文本
        AutoSetRole(line); // 自动设置角色图像
        AutoSetBackground(line); // 自动设置背景
    }

    void ShowLine(DialogueLine line)
    {
        if (line != null)
        {
            panelUI.contentText.text = line.content;  // 设置对话内容
            panelUI.speakerText.text = line.speaker;  // 设置人物名字

            // 只在 Game 模式下显示文本，隐藏角色图像和选项
            AutoSetBackground(line);  // 设置背景
            AutoSetRole(line);         // 设置角色图像（在 Game 模式下可以选择是否隐藏）
        }
    }

    private void AutoSetRole(DialogueLine line)
    {
        if (roleConfig == null || roleImage == null)
        {
            Debug.LogError("角色配置或角色图像未正确赋值！");
            return;
        }

        if (line.type == "对话" && currentMode == DialogueMode.Gal) // 只有Gal模式才显示角色图像
        {
            Sprite sprite = roleConfig.GetRoleSprite(line.speaker);  // 获取角色立绘

            if (sprite != null)
            {
                roleImage.sprite = sprite;
                roleImage.gameObject.SetActive(true);  // 激活角色图像
            }
            else
            {
                roleImage.gameObject.SetActive(false);  // 如果没有找到图片，隐藏角色图像
                Debug.LogWarning($"角色立绘未找到：{line.speaker}");
            }
        }
        else
        {
            roleImage.gameObject.SetActive(false);  // 非对话状态下隐藏角色图像
        }
    }

    private void AutoSetBackground(DialogueLine line)
    {
        if (backgroundConfig == null || backgroundImage == null)
        {
            Debug.LogError("背景配置或背景图像未正确赋值！");
            return;
        }

        if (line.type == "场景" && currentMode == DialogueMode.Gal)  // 确保是场景类型的对话
        {
            string bgName = line.content;

            // 如果有描述，截取前半部分（比如背景名后有附加描述）
            if (bgName.Contains("，"))
                bgName = bgName.Split('，')[0];

            Sprite bg = backgroundConfig.GetBg(bgName);  // 获取背景图

            if (bg != null)
            {
                backgroundImage.sprite = bg;
            }
            else
            {
                Debug.LogWarning($"背景图片未找到：{bgName}");
            }
        }
    }

    #region 选项逻辑（仅 Gal）
    void ShowChoice(DialogueLine line)
    {
        isChoosing = true;

        CreateBtns();

        btnsObj.SetActive(true);

        contentText = panelUI.contentText;
        contentText.text = "请选择：";

        // ===== Btn1 =====
        if (!string.IsNullOrEmpty(line.choice1))
        {
            btnsUI.btn1.gameObject.SetActive(true);
            btnsUI.btn1Text.text = line.choice1;

            btnsUI.btn1.onClick.RemoveAllListeners();
            btnsUI.btn1.onClick.AddListener(() =>
            {
                OnChoose1(line);
            });
        }
        else
        {
            btnsUI.btn1.gameObject.SetActive(false);
        }

        // ===== Btn2 =====
        if (!string.IsNullOrEmpty(line.choice2))
        {
            btnsUI.btn2.gameObject.SetActive(true);
            btnsUI.btn2Text.text = line.choice2;

            btnsUI.btn2.onClick.RemoveAllListeners();
            btnsUI.btn2.onClick.AddListener(() =>
            {
                OnChoose2(line);
            });
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
            btnsObj = Instantiate(btnsPrefab, currentPanel.transform);

            btnsUI = btnsObj.GetComponent<BtnsUI>();

            if (btnsUI == null)
            {
                Debug.LogError("BtnsPrefab 上没有 BtnsUI！");
            }
        }
    }

    void OnChoose1(DialogueLine line)
    {
        isChoosing = false;

        if (btnsObj != null)
            btnsObj.SetActive(false);

        contentText.text = line.choice1Result;

        Invoke(nameof(NextLine), 1f);
    }

    void OnChoose2(DialogueLine line)
    {
        isChoosing = false;

        if (btnsObj != null)
            btnsObj.SetActive(false);

        EndDialogue();
        SceneManager.LoadScene("GameScene");
    }
    #endregion

    /// <summary>
    /// 结束对话，销毁面板（回到隐藏状态）
    /// </summary>
    public void EndDialogue()
    {
        if (currentPanel != null)
            Destroy(currentPanel);  // 销毁当前面板

        lines = null;
        isChoosing = false;
        CancelInvoke();

        // 隐藏面板
        if (panelUI != null)
        {
            panelUI.gameObject.SetActive(false);  // 隐藏面板
        }
    }

    public override void ShowMe() { }
    public override void HideMe() { }
    protected override void ClickBtn(string btnName) { }
}