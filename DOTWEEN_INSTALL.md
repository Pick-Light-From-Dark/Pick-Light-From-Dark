# DOTween 安装指南

## 问题：Git URL 方式失败

`com.demigiant.dotween` 的 `[upm]` 分支不存在，导致 Git URL 安装失败。

---

## 解决方案（3 种方式，任选其一）

### 方案 1：通过 Unity Asset Store（推荐，最简单）

1. 打开 Unity
2. Window → Package Manager → 点击左上角 "+" → 选择 "Asset Store"
3. 搜索 "DOTween"
4. 点击 "Import" 或 "Download"
5. 等待导入完成

---

### 方案 2：通过 openupm 镜像（推荐）

修改 `Packages/manifest.json`，添加：

```json
"com.demigiant.dotween": "https://package.openupm.com/com.demigiant.dotween",
```

然后重启 Unity，它会自动从 openupm.com 下载。

---

### 方案 3：手动下载 .unitypackage

1. 访问 DOTween 官网：https://dotween.demigiant.com/
2. 下载最新版本的 DOTween Pro.unitypackage（免费版足够）
3. 在 Unity 中：Assets → Import Package → Custom Package
4. 选择下载的 .unitypackage 文件

---

## 验证安装

安装完成后，验证是否成功：

1. 打开任意脚本
2. 输入 `using DG.Tweening;`
3. 无红色波浪线 → 安装成功

---

## 下一步

安装完成后，`DOTweenAnimationMgr.cs` 中的代码会自动生效。

目前代码已使用 `using DG.Tweening;`，等 DOTween 安装完成即可使用。
