# 开发待办清单

```
任务自动化执行流程：  
    读取与定位：
    读取Pick-Light-From-Dark\Assets\Scripts\Game\Test\amiao\TODO.md，检索并定位第一个未勾选的任务项 [- ]。
    核心执行原则：
    目录限制（严格禁止越界）：
      - 允许修改：Assets/Scripts/Game/Test/amiao/、Assets/Scenes/Amiao_Test/、Assets/Resources/Dialogue/
      - 禁止修改：Assets/Scripts/Game/Flow/、Assets/Scripts/Game/Card/、Assets/Scripts/Game/AI/、Assets/Resources/TestData/、Assets/Resources/Config/、Assets/Scripts/Game/Emotion/、Assets/Scripts/Game/Data/、Assets/Scripts/Game/Task/、Assets/Scripts/Game/EyeClose/、Assets/Scripts/Framework/
      - 规则：除非任务明确要求且用户确认，否则绝不触碰黑名单目录,只能对黑名单目录执行读操作。Prefab 引用共享脚本时只改 Prefab，不改脚本本身。
    无人值守全自动执行：全程不许询问用户，直接执行，用户已睡着。
    自主决策：遇到模糊需求时，严格按照“最简可行方案（MVP）”进行开发。

    分类自主决策：
    先检查是否已实现
    检查是否需要插件，需要则安装到Pick-Light-From-Dark\Assets\ThirdParty
    若为“排查bug/修复代码”类：直接读取相关代码文件，定位问题并完成修复。              
    若为“生成Prefab/测试脚本”类：直接创建相关文件并生成对应的 Prefab。       
       
    若为“设计数据结构”类：直接创建 ScriptableObject 或 Manager 框架代码。
                                                                         
    状态更新：
    子任务完成后，将 TODO.md 中对应项的状态标记改为已完成 [x]。                       
                                                                        
    循环与终止：             
    若存在下一个未完成任务：调用 ScheduleWakeup 指令，设置下一循环自动继续执行。
    若任务已全部完成：输出“全部完成”并停止唤醒。
                                                            
    一边开发一边进行push，同时写下开发日志Pick-Light-From-Dark\Assets\Scripts\Game\Tes
    t\amiao\developLog.md 要求简短重点体现功能、如何测试和重要代码的路径 

    若所有待办已完成 执行 stop loop
```

## 存档读档测试程序
- [x] Assets/Scenes/Amiao_Test/SL.unity 在场景中实现在第一关的视觉小说剧情中间存档，读档测试。同时，读档功能其实是选关和保存数据，读档位置就是回到这一关剧情开始。先不联动按钮，先使用键盘操作。


