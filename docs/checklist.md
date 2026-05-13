# 项目检查清单

当被要求"检查"时，逐项执行以下清单。

---

## 1. 场景跳转是否使用 SceneMgr 包装

**规则：** 所有场景跳转必须通过 `SceneMgr.LoadScene` / `SceneMgr.LoadSceneAsyn`，禁止直接调用 `SceneManager.LoadScene` / `SceneManager.LoadSceneAsync`。

**原因：** SceneMgr 是框架层的统一入口，负责在跳转前做清理工作（如回收音效对象）。直接调用会绕过这些清理，导致 MissingReferenceException 等问题。

**检查方法：**
```bash
# 在 Pick-Light-From-Dark/Assets/Scripts/ 下搜索（排除 ThirdParty）
grep -rn "SceneManager\.Load" --include="*.cs" --exclude-dir=ThirdParty
```

**允许的例外：**
- `ThirdParty/` 目录下的第三方代码
- `SceneMgr.cs` 自身的实现

**不合规示例：**
```csharp
// TipPanel.cs — 直接调用
SceneManager.LoadScene("GameScene");
// EndingContentPanel.cs — 直接调用
SceneManager.LoadScene("Level2");
// BeginPanel.cs — 直接调用
SceneManager.LoadScene("Level1");
```

**合规示例：**
```csharp
SceneMgr.Instance.LoadScene("Level2");
SceneMgr.Instance.LoadSceneAsyn("GameScene");
```
