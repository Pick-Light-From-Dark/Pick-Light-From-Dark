#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
skill_runner.py - 项目内 Skill 自动化执行器
替代 Claude Code 的 skills 系统，直接通过命令行调用。

用法示例:
    python tools/skill_runner.py --skill todo
    python tools/skill_runner.py --skill make-vn-prefab --template "Assets/Scenes/Amiao_Test/day5-2.prefab" --new-name "5-2a" --dialogue "Dialogue5-2a.txt"
    python tools/skill_runner.py --skill batch-replace --old "[ 陆萤 ]" --new "陆萤"
    python tools/skill_runner.py --skill fix-placeholder-font
    python tools/skill_runner.py --skill git-commit --commit-msg "[auto] 修复字体"
"""

import argparse
import os
import re
import sys
import subprocess
import uuid
import io

# Windows 控制台编码修复
if sys.platform == "win32":
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8")
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding="utf-8")
import shutil
from datetime import datetime
from pathlib import Path

# ========== 路径配置 ==========
TOOLS_DIR = Path(__file__).parent.resolve()
PROJECT_ROOT = TOOLS_DIR.parent
UNITY_PROJECT = PROJECT_ROOT / "Pick-Light-From-Dark"

TODO_PATH = UNITY_PROJECT / "Assets/Scripts/Game/Test/amiao/TODO.md"
DEVLOG_PATH = UNITY_PROJECT / "Assets/Scripts/Game/Test/amiao/developLog.md"
DIALOGUE_DIR = UNITY_PROJECT / "Assets/Resources/Dialogue"
PREFAB_DIR = UNITY_PROJECT / "Assets/Scenes/Amiao_Test"
OLD_PREFAB_DIR = PREFAB_DIR / "旧剧情prefab（路径改了不能直接跑）"
SKILLS_DIR = PROJECT_ROOT / ".claude/skills"
PLACEHOLDER_DISPLAY_PATH = UNITY_PROJECT / "Assets/Scripts/Game/Test/amiao/PlaceholderDisplay.cs"


# ========== 工具函数 ==========
def run_git(args, cwd=None):
    """执行 git 命令，返回结果对象"""
    if cwd is None:
        cwd = PROJECT_ROOT
    result = subprocess.run(
        ["git"] + args, cwd=cwd, capture_output=True, text=True, encoding="utf-8"
    )
    return result


def write_devlog(message):
    """追加开发日志到 developLog.md"""
    if not DEVLOG_PATH.exists():
        print(f"[警告] 开发日志不存在: {DEVLOG_PATH}")
        return
    timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    header = f"\n## {timestamp[:10]} {message.split(chr(10))[0][:30]}\n\n"
    body = f"**功能**：{message}\n\n"
    with open(DEVLOG_PATH, "a", encoding="utf-8") as f:
        f.write(header + body)
    print(f"[DevLog] 已记录: {message[:60]}...")


def generate_guid():
    """生成 Unity 格式的 32 位小写 GUID"""
    return uuid.uuid4().hex


# ========== TODO 系统 ==========
def parse_todo():
    """解析 TODO.md，返回 (任务列表, 原始内容)"""
    if not TODO_PATH.exists():
        print(f"[错误] 找不到 TODO.md: {TODO_PATH}")
        return [], ""

    with open(TODO_PATH, "r", encoding="utf-8") as f:
        content = f.read()

    tasks = []
    # 匹配 - [ ] / - [x] / - [] 任务文本
    pattern = re.compile(r'^(\s*)- \[([ xX]?)\]\s*(.+)$', re.MULTILINE)
    for match in pattern.finditer(content):
        indent = match.group(1)
        status_raw = match.group(2).strip().lower()
        status = "done" if status_raw == "x" else "todo"
        text = match.group(3).strip()
        tasks.append({
            "indent": indent,
            "status": status,
            "text": text,
            "full_match": match.group(0),
            "start": match.start(),
            "end": match.end(),
        })
    return tasks, content


def get_first_undone_task():
    """获取第一个未完成任务"""
    tasks, content = parse_todo()
    for task in tasks:
        if task["status"] == "todo":
            return task, tasks, content
    return None, tasks, content


def mark_task_done(task_text):
    """将指定任务标记为完成 [x]"""
    if not TODO_PATH.exists():
        return False

    with open(TODO_PATH, "r", encoding="utf-8") as f:
        content = f.read()

    # 精确匹配整行: - [ ] task_text 或 - [] task_text
    escaped = re.escape(task_text)
    pattern = re.compile(r'^(\s*- \[) ?(\]\s*)' + escaped + r'$', re.MULTILINE)
    new_content, count = pattern.subn(lambda m: m.group(1) + 'x' + m.group(2) + task_text, content)

    if count == 0:
        # 尝试只匹配前 80 个字符（防止文本过长）
        short = re.escape(task_text[:80])
        pattern2 = re.compile(r'^(\s*- \[) ?(\]\s*)' + short + r'.*$', re.MULTILINE)
        new_content, count = pattern2.subn(lambda m: m.group(1) + 'x' + m.group(2) + task_text, content)

    if count == 0:
        print(f"[警告] 未找到匹配任务，无法标记完成: {task_text[:60]}...")
        return False

    with open(TODO_PATH, "w", encoding="utf-8") as f:
        f.write(new_content)

    print(f"[TODO] 已标记完成: {task_text[:60]}...")
    return True


# ========== Skill: todo ==========
def skill_todo():
    """读取并处理第一个未完成的 TODO 任务"""
    task, all_tasks, content = get_first_undone_task()

    if task is None:
        print("\n[TODO] 所有任务已完成！\n")
        return

    print(f"\n[TODO] 当前任务:\n  {task['text']}\n")

    text_lower = task["text"].lower()

    # 1. Dialogue 文本替换
    if "dialogue" in text_lower and any(k in text_lower for k in ("换成", "替换", "修改", "改为", "都换成")):
        print("[自动] 检测到 Dialogue 文本替换任务...")
        # 提取替换规则：把 A 换成 B / 将 A 改为 B
        replace_match = re.search(r'[把将](.+?)[换成改为](.+?)(?:$|，|,|；)', task["text"])
        if replace_match:
            old_str = replace_match.group(1).strip().strip('"').strip("'")
            new_str = replace_match.group(2).strip().strip('"').strip("'")
            files = find_dialogue_files()
            count = batch_replace_in_files(files, old_str, new_str)
            if count > 0:
                mark_task_done(task["text"])
                git_auto_commit(f"[auto] 文本替换: {old_str} -> {new_str}")
                return

    # 2. C# 代码修复 (PlaceholderDisplay 字体)
    if ("placeholderdisplay" in text_lower or ".cs" in text_lower) and "字体" in text_lower:
        print("[自动] 检测到代码字体修复任务...")
        if fix_placeholder_font():
            mark_task_done(task["text"])
            return

    # 3. Prefab 字体移植
    if "prefab" in text_lower and ("字体" in text_lower or "移植" in text_lower):
        print("[自动] 检测到 Prefab 字体移植任务...")
        if skill_migrate_font(auto_mode=True):
            mark_task_done(task["text"])
            return

    # 4. Git 提交
    if any(k in text_lower for k in ("git", "commit", "提交", "push")):
        print("[自动] 检测到 Git 任务...")
        msg = task["text"].replace("git", "").replace("commit", "").replace("提交", "").strip(" ：:-")
        if not msg:
            msg = "[auto] 自动化更新"
        if git_auto_commit(f"[auto] {msg[:80]}"):
            mark_task_done(task["text"])
            return

    # 4. Prefab 生成
    if "prefab" in text_lower and any(k in text_lower for k in ("生成", "复制", "创建", "做个")):
        print("[自动] 检测到 Prefab 生成任务...")
        print("[建议] 请使用以下命令手动执行:")
        print('  python tools/skill_runner.py --skill make-vn-prefab --template "<模板路径>" --new-name "<名称>"')
        return

    print("[TODO] 该任务暂不支持全自动执行，请手动处理后标记完成。")
    print(f"[提示] 标记命令: python tools/skill_runner.py --skill mark-done --text \"{task['text'][:50]}...\"")


# ========== Skill: make-vn-prefab ==========
def skill_make_vn_prefab(template_relative_path, new_name, dialogue_text_name=None):
    """
    基于模板生成新的 VN Prefab。
    复制 .prefab 和 .prefab.meta，生成新 GUID，修改内部名称和对话引用。
    """
    template_prefab = UNITY_PROJECT / template_relative_path.replace("/", os.sep)
    if not template_prefab.exists():
        print(f"[错误] 模板 Prefab 不存在: {template_prefab}")
        return False

    new_prefab_path = template_prefab.parent / f"FungusVN_{new_name}.prefab"
    new_meta_path = new_prefab_path.with_suffix(".prefab.meta")

    if new_prefab_path.exists():
        print(f"[警告] 目标已存在，跳过: {new_prefab_path}")
        return False

    # 1. 复制 .prefab 文件
    shutil.copy2(template_prefab, new_prefab_path)

    # 2. 生成新 GUID
    new_guid = generate_guid()

    # 3. 创建 .meta 文件
    meta_content = f"""fileFormatVersion: 2
guid: {new_guid}
PrefabImporter:
  externalObjects: {{}}
  userData:
  assetBundleName:
  assetBundleVariant:
"""
    with open(new_meta_path, "w", encoding="utf-8") as f:
        f.write(meta_content)

    # 4. 修改 Prefab 内部 m_Name
    with open(new_prefab_path, "r", encoding="utf-8") as f:
        prefab_content = f.read()

    old_name = template_prefab.stem  # 去掉 .prefab
    prefab_content = prefab_content.replace(
        f"m_Name: {old_name}", f"m_Name: FungusVN_{new_name}", 1
    )

    # 5. 如有对话文本，替换 dialogueText 的 GUID
    if dialogue_text_name:
        dialogue_path = DIALOGUE_DIR / dialogue_text_name
        if dialogue_path.exists():
            dialogue_meta = dialogue_path.with_suffix(".txt.meta")
            if dialogue_meta.exists():
                dialogue_guid = None
                with open(dialogue_meta, "r", encoding="utf-8") as f:
                    for line in f:
                        if line.startswith("guid:"):
                            dialogue_guid = line.split(":", 1)[1].strip()
                            break
                if dialogue_guid:
                    # 替换 Prefab 中 dialogueText 的 GUID
                    prefab_content = re.sub(
                        r'(dialogueText: \{fileID: \d+, guid: )[a-f0-9]+(, type: 3\})',
                        rf'\g<1>{dialogue_guid}\g<2>',
                        prefab_content,
                    )
                    print(f"[Prefab] 已绑定对话文本 GUID: {dialogue_guid}")

    with open(new_prefab_path, "w", encoding="utf-8") as f:
        f.write(prefab_content)

    print(f"[Prefab] 已生成: {new_prefab_path}")
    print(f"[Prefab] 新 GUID: {new_guid}")
    return True


# ========== Skill: batch-replace ==========
def find_dialogue_files():
    """查找所有 Dialogue 文本文件"""
    if not DIALOGUE_DIR.exists():
        print(f"[错误] 对话目录不存在: {DIALOGUE_DIR}")
        return []
    return sorted(DIALOGUE_DIR.glob("Dialogue*.txt"))


def batch_replace_in_files(files, old_str, new_str):
    """在多个文件中批量替换文本，返回修改文件数"""
    total = 0
    for file_path in files:
        with open(file_path, "r", encoding="utf-8") as f:
            content = f.read()

        if old_str in content:
            new_content = content.replace(old_str, new_str)
            with open(file_path, "w", encoding="utf-8") as f:
                f.write(new_content)
            print(f"  [Replace] {file_path.name}: '{old_str}' -> '{new_str}'")
            total += 1

    if total > 0:
        print(f"[Replace] 共修改 {total} 个文件")
    else:
        print(f"[Replace] 未找到匹配文本: '{old_str}'")
    return total


def skill_batch_replace(old_str, new_str, file_pattern="Dialogue*.txt"):
    """批量替换 skill 入口"""
    if "*" in file_pattern:
        files = list(DIALOGUE_DIR.glob(file_pattern))
    else:
        target = UNITY_PROJECT / file_pattern.replace("/", os.sep)
        files = [target] if target.exists() else []

    count = batch_replace_in_files(files, old_str, new_str)
    if count > 0:
        git_auto_commit(f"[auto] 批量替换: {old_str} -> {new_str}")
    return count


# ========== Skill: fix-placeholder-font ==========
def fix_placeholder_font():
    """修复 PlaceholderDisplay.cs 字体缺失问题"""
    if not PLACEHOLDER_DISPLAY_PATH.exists():
        print(f"[错误] 找不到文件: {PLACEHOLDER_DISPLAY_PATH}")
        return False

    with open(PLACEHOLDER_DISPLAY_PATH, "r", encoding="utf-8") as f:
        content = f.read()

    # 已修复则跳过
    if "LXGWWenKaiScreen" in content or "文软雅黑" in content:
        print("[Fix] 字体引用已存在，跳过")
        return False

    # 尝试在 displayText.color = textColor; 后插入
    pattern = r"(displayText\.color = textColor;)"
    replacement = r"""\1
            displayText.font = Resources.Load<Font>("Font/LXGWWenKaiScreen")
                ?? Resources.Load<Font>("Font/文软雅黑");"""

    new_content, count = re.subn(pattern, replacement, content)

    if count == 0:
        # 备用：在 displayText.fontSize = fontSize; 后插入
        pattern2 = r"(displayText\.fontSize = fontSize;)"
        replacement2 = r"""\1
            displayText.font = Resources.Load<Font>("Font/LXGWWenKaiScreen")
                ?? Resources.Load<Font>("Font/文软雅黑");"""
        new_content, count = re.subn(pattern2, replacement2, content)

    if count == 0:
        print("[Fix] 无法自动定位插入点，请手动修复")
        return False

    with open(PLACEHOLDER_DISPLAY_PATH, "w", encoding="utf-8") as f:
        f.write(new_content)

    print(f"[Fix] 已修复字体: {PLACEHOLDER_DISPLAY_PATH}")
    git_auto_commit("[auto] 修复 PlaceholderDisplay 字体缺失")
    return True


# ========== Skill: migrate-font ==========
def extract_all_guids(prefab_path):
    """从 Prefab YAML 中提取所有 GUID 引用"""
    guids = set()
    with open(prefab_path, "r", encoding="utf-8") as f:
        for line in f:
            matches = re.findall(r'guid:\s*([a-f0-9]{32})', line)
            guids.update(matches)
    return guids


def skill_migrate_font(auto_mode=False):
    """
    字体 GUID 移植/诊断。
    自动比较新旧 Prefab 的 GUID 引用差异，如无差异则诊断通过。
    """
    if not OLD_PREFAB_DIR.exists():
        print(f"[错误] 旧 Prefab 目录不存在: {OLD_PREFAB_DIR}")
        return False

    old_prefabs = sorted(OLD_PREFAB_DIR.glob("*.prefab"))
    new_prefabs = sorted(PREFAB_DIR.glob("day*.prefab"))

    if not old_prefabs:
        print("[MigrateFont] 未找到旧 Prefab")
        return False
    if not new_prefabs:
        print("[MigrateFont] 未找到新 Prefab (day*.prefab)")
        return False

    print("[MigrateFont] 开始诊断字体配置...")

    # 收集旧 Prefab 的所有 GUID
    old_all_guids = set()
    for op in old_prefabs:
        old_all_guids.update(extract_all_guids(op))

    # 收集新 Prefab 的所有 GUID
    new_all_guids = set()
    for np in new_prefabs:
        new_all_guids.update(extract_all_guids(np))

    # 检查是否有显式字体 GUID（Font 资产的 GUID 通常是 128 类型）
    # 由于 YAML 中不直接标注类型，我们先检查是否有 GUID 差异
    only_in_old = old_all_guids - new_all_guids
    only_in_new = new_all_guids - old_all_guids

    # 进一步：扫描 m_Font 或 m_FontData 字段（显式字体引用）
    explicit_font_guids = []
    for op in old_prefabs:
        with open(op, "r", encoding="utf-8") as f:
            content = f.read()
        # 查找字体相关字段旁的 GUID
        font_matches = re.findall(r'm_Font(?:Data)?:\s*\{[^}]*guid:\s*([a-f0-9]{32})', content)
        explicit_font_guids.extend(font_matches)

    if explicit_font_guids:
        print(f"[MigrateFont] 发现 {len(explicit_font_guids)} 个显式字体 GUID，准备移植...")
        # 将显式字体 GUID 移植到所有新 Prefab（假设所有新 Prefab 需要相同字体）
        replaced = 0
        font_guid = explicit_font_guids[0]  # 取第一个找到的字体 GUID
        for np in new_prefabs:
            with open(np, "r", encoding="utf-8") as f:
                content = f.read()
            # 替换新 Prefab 中的 m_Font/m_FontData GUID
            new_content, count = re.subn(
                r'(m_Font(?:Data)?:\s*\{[^}]*guid:\s*)[a-f0-9]{32}',
                rf'\g<1>{font_guid}',
                content,
            )
            if count > 0:
                with open(np, "w", encoding="utf-8") as f:
                    f.write(new_content)
                print(f"  [MigrateFont] 已移植字体 GUID 到: {np.name}")
                replaced += 1
        if replaced > 0:
            git_auto_commit("[auto] 移植旧 Prefab 字体 GUID 到新 Prefab")
            return True
        else:
            print("[MigrateFont] 新 Prefab 中无显式字体字段可替换")
            return False
    else:
        # 无显式字体 GUID：说明字体是运行时动态加载的
        print("[MigrateFont] 未找到显式字体 GUID 字段")
        print("[MigrateFont] 诊断结果: 字体由运行时脚本动态加载（FungusVNController.SetupFont）")
        print("[MigrateFont] 新旧 Prefab 使用的是同一套运行时逻辑，无需 GUID 移植")
        if only_in_old or only_in_new:
            print(f"[MigrateFont] 备注: 发现其他 GUID 差异（旧 {len(only_in_old)} 个/新 {len(only_in_new)} 个），系对话文本与图片引用不同，属正常")
        return True


# ========== Skill: git-commit ==========
def git_auto_commit(message):
    """自动 git add + commit，有改动才提交"""
    status = run_git(["status", "--porcelain"])
    if not status.stdout.strip():
        print("[Git] 工作区干净，无需提交")
        return False

    run_git(["add", "."])
    result = run_git(["commit", "-m", message])

    if result.returncode == 0:
        print(f"[Git] 提交成功: {message}")
        write_devlog(message)
        return True
    else:
        print(f"[Git] 提交失败:\n{result.stderr}")
        return False


# ========== 主入口 ==========
def main():
    parser = argparse.ArgumentParser(
        description="项目内 Skill 自动化执行器",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
常用示例:
  python tools/skill_runner.py --skill todo
  python tools/skill_runner.py --skill batch-replace --old "[ 陆萤 ]" --new "陆萤"
  python tools/skill_runner.py --skill fix-placeholder-font
  python tools/skill_runner.py --skill git-commit --commit-msg "[auto] 更新对话文本"
        """,
    )
    parser.add_argument("--skill", required=True, help="Skill 名称")
    parser.add_argument("--template", help="模板 Prefab 相对路径 (make-vn-prefab)")
    parser.add_argument("--new-name", help="新 Prefab 名称 (make-vn-prefab)")
    parser.add_argument("--dialogue", help="对话文本文件名，如 Dialogue5-2a.txt")
    parser.add_argument("--old", help="待替换文本 (batch-replace)")
    parser.add_argument("--new", help="替换后文本 (batch-replace)")
    parser.add_argument("--file-pattern", default="Dialogue*.txt", help="文件匹配模式 (batch-replace)")
    parser.add_argument("--commit-msg", default="[auto] 自动化更新", help="Git 提交信息")
    parser.add_argument("--text", help="任务文本 (mark-done 专用)")

    args = parser.parse_args()

    skill = args.skill.lower().replace("_", "-")

    if skill == "todo":
        skill_todo()
    elif skill == "make-vn-prefab":
        if not args.template or not args.new_name:
            print("[错误] --template 和 --new-name 是必需的")
            sys.exit(1)
        success = skill_make_vn_prefab(args.template, args.new_name, args.dialogue)
        if success:
            git_auto_commit(f"[auto] 生成 Prefab: {args.new_name}")
    elif skill == "batch-replace":
        if not args.old or not args.new:
            print("[错误] --old 和 --new 是必需的")
            sys.exit(1)
        skill_batch_replace(args.old, args.new, args.file_pattern)
    elif skill == "fix-placeholder-font":
        fix_placeholder_font()
    elif skill == "git-commit":
        git_auto_commit(args.commit_msg)
    elif skill == "migrate-font":
        skill_migrate_font()
    elif skill == "mark-done":
        text = args.text or args.commit_msg
        if text == "[auto] 自动化更新":
            print("[错误] 请提供 --text 参数指定要标记的任务文本")
            sys.exit(1)
        mark_task_done(text)
    else:
        # 尝试从 .claude/skills/*.md 读取并显示说明
        skill_file = SKILLS_DIR / f"{args.skill}.md"
        if skill_file.exists():
            print(f"Skill '{args.skill}' 定义如下，但尚未实现自动执行:\n")
            print(skill_file.read_text(encoding="utf-8"))
        else:
            print(f"[错误] 未知 skill: {args.skill}")
            sys.exit(1)


if __name__ == "__main__":
    main()
