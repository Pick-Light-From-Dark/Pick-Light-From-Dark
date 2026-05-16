using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 局内剧情对话管理器 — 管理 PlayerGameDialogue / OtherGameDialogue 两个面板
/// </summary>
public class GameDialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject playerDialoguePrefab;
    [SerializeField] private GameObject otherDialoguePrefab;

    private GameObject playerInstance;
    private GameObject otherInstance;
    private TextMeshProUGUI playerText;
    private TextMeshProUGUI playerSpeakerNameText;
    private TextMeshProUGUI otherText;
    private TextMeshProUGUI otherSpeakerNameText;

    private List<DialogueLine> lines;
    private int lineIndex;
    private bool isActive;

    private RectTransform thinkBubbleRt;

    public bool IsDialogueActive => isActive;

    /// <summary>
    /// 初始化：加载预制体并定位到思考框同一父级下
    /// </summary>
    public void Initialize(RectTransform thinkBubble)
    {
        thinkBubbleRt = thinkBubble;
        LoadAndPositionPrefabs();
    }

    private void LoadAndPositionPrefabs()
    {
        if (thinkBubbleRt == null) return;

        if (playerDialoguePrefab == null)
            playerDialoguePrefab = Resources.Load<GameObject>("UI/Dialogue/PlayerGameDialogue");
        if (otherDialoguePrefab == null)
            otherDialoguePrefab = Resources.Load<GameObject>("UI/Dialogue/OtherGameDialogue");

        // 实例化到思考框的同一父级，确保 anchoredPosition 坐标系一致
        Transform parent = thinkBubbleRt.parent;

        if (playerDialoguePrefab != null)
        {
            playerInstance = Instantiate(playerDialoguePrefab, parent);
            playerInstance.SetActive(false);
            playerText = playerInstance.transform.Find("PlayerDialogueText")?.GetComponent<TextMeshProUGUI>();
            playerSpeakerNameText = playerInstance.transform.Find("PlayerSpeakerNameImgBk/PlayerSpeakerNameText")?.GetComponent<TextMeshProUGUI>();
            CopyRectTransform(playerInstance, mirror: false);
        }

        if (otherDialoguePrefab != null)
        {
            otherInstance = Instantiate(otherDialoguePrefab, parent);
            otherInstance.SetActive(false);
            otherText = otherInstance.transform.Find("OtherDialogueText")?.GetComponent<TextMeshProUGUI>();
            otherSpeakerNameText = otherInstance.transform.Find("OtherSpeakerNameImgBk/OtherSpeakerNameText")?.GetComponent<TextMeshProUGUI>();
            CopyRectTransform(otherInstance, mirror: true);
        }
    }

    /// <summary>
    /// 复制思考框的 RectTransform 设置，Player=原位，Other=关于Y轴对称
    /// </summary>
    private void CopyRectTransform(GameObject panel, bool mirror)
    {
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = thinkBubbleRt.anchorMin;
        rt.anchorMax = thinkBubbleRt.anchorMax;
        rt.pivot = thinkBubbleRt.pivot;
        rt.sizeDelta = thinkBubbleRt.sizeDelta;

        Vector2 pos = thinkBubbleRt.anchoredPosition;
        if (mirror) pos.x = -pos.x;
        rt.anchoredPosition = pos;
    }

    void Update()
    {
        if (!isActive) return;
        if (Input.GetMouseButtonDown(0))
            NextLine();
    }

    /// <summary>
    /// 启动对话（由 EventCenter GameDialogueStart 触发）
    /// </summary>
    public void StartDialogue(string resourcePath)
    {
        var text = Resources.Load<TextAsset>(resourcePath);
        if (text == null)
        {
            Debug.LogError($"[GameDialogueManager] 对话文本未找到: Resources/{resourcePath}");
            EndDialogue();
            return;
        }

        lines = DialogueParser.Parse(text);
        lineIndex = 0;
        isActive = true;

        Debug.Log($"[GameDialogueManager] 对话开始, 共 {lines.Count} 行");
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (lines == null || lineIndex >= lines.Count)
        {
            EndDialogue();
            return;
        }

        var line = lines[lineIndex];

        // 跳过纯指令行（如 [bg:...]），避免空白面板卡住
        if (line.type == "指令" || string.IsNullOrEmpty(line.speaker))
        {
            lineIndex++;
            ShowCurrentLine();
            return;
        }

        bool isPlayer = line.speaker == "陆萤";

        if (playerInstance != null) playerInstance.SetActive(isPlayer);
        if (otherInstance != null) otherInstance.SetActive(!isPlayer);

        if (isPlayer)
        {
            if (playerText != null) playerText.text = line.content;
            if (playerSpeakerNameText != null) playerSpeakerNameText.text = line.speaker;
        }
        else
        {
            if (otherText != null) otherText.text = line.content;
            if (otherSpeakerNameText != null) otherSpeakerNameText.text = line.speaker;
        }

        Debug.Log($"[GameDialogueManager] 第{lineIndex}行: [{line.speaker}] {line.content}");
    }

    private void NextLine()
    {
        lineIndex++;
        ShowCurrentLine();
    }

    private void EndDialogue()
    {
        isActive = false;
        if (playerInstance != null) playerInstance.SetActive(false);
        if (otherInstance != null) otherInstance.SetActive(false);

        EventCenter.Instance.EventTrigger(E_EventType.GameDialogueEnd);
        Debug.Log("[GameDialogueManager] 对话结束");
    }
}
