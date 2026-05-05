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

    private DialogueMode currentMode;
    private List<DialogueLine> lines;
    private int lineIndex = 0;
    private bool isChoosing = false;

    private IDialoguePanel panelUI;
    private GameObject btnsObj;
    private BtnsUI btnsUI;

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

    void Update()
    {
        if (isChoosing) return;

        if (Input.GetMouseButtonDown(0))
        {
            NextLine();
        }
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
            EndDialogue();
            return;
        }

        DialogueLine line = lines[lineIndex];
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
        AutoSetRole(line);
        AutoSetBackground(line);
    }

    // =========================================
    // 显示文本
    // =========================================
    void ShowLine(DialogueLine line)
    {
        panelUI.SetContent(line.speaker, line.content);
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
    // 背景
    // =========================================
    private void AutoSetBackground(DialogueLine line)
    {
        if (backgroundConfig == null) return;

        if (line.type == "场景")
        {
            string bgName = line.content;

            if (bgName.Contains("，"))
                bgName = bgName.Split('，')[0];

            Sprite bg = backgroundConfig.GetBg(bgName);
            panelUI.SetBackground(bg);
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
    }
}
