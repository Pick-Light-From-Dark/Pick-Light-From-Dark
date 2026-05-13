# 开发待办清单

```
任务自动化执行流程：                                       
                                                                                
    读取与定位：                                                         
    读取Pick-Light-From-Dark\Assets\Scripts\Game\Test\amiao\TODO.md
    文件，检索并定位第一个未勾选的任务项 [- ]。                         
                                                                      
    分类自主决策：                                                              
    若为“排查bug/修复代码”类：直接读取相关代码文件，定位问题并完成修复。              
    若为“生成Prefab/测试脚本”类：直接创建相关文件并生成对应的 Prefab。          
    若为“设计数据结构”类：直接创建 ScriptableObject 或 Manager 框架代码。
                                                                         
    状态更新：                                                       
    子任务完成后，将 TODO.md 中对应项的状态标记改为已完成 [x]。                       
                                                                        
    循环与终止：                                                                      
    若存在下一个未完成任务：调用 ScheduleWakeup 指令，设置 60 秒后自动继续执行。
    若任务已全部完成：输出“全部完成”并停止唤醒。                                      
                                                                     
    核心执行原则：                                                                    
    全自动执行：全程无需询问用户，直接执行。                                          
    自主决策：遇到模糊需求时，严格按照“最简可行方案（MVP）”进行开发。
    一边开发一边进行push，同时写下开发日志Pick-Light-From-Dark\Assets\Scripts\Game\Tes
    t\amiao\developLog.md 要求简短重点体现功能、如何测试和重要代码的路径                    

    每执行一个阶段则push一次

    若所有待办已完成 执行 stop loop
```

## 2. 结局关卡测试 Prefab
- [x] 按照结局设计逻辑Pick-Light-From-Dark\Assets\Scripts\Game\Test\amiao\ending.md，生成可测试的结局关卡 Prefab
- [x] 设计 `EndingData` 数据结构（ScriptableObject 或字典）
- [x] 设计 `EndingUIController` / `EndingManager`
- [x] 实现结局面板：显示结局名称 + 描述 + 返回主界面/读取存档按钮
- [x] 生成测试 Prefab 验证结局流程

## 3. 存档系统测试 Prefab
- [x] 设计存档数据结构（每关剧情开始存档）
- [x] 保存结局分支的卡牌使用情况
- [x] 保存其他结局需要的数据
- [x] 生成测试 Prefab 验证存档/读档功能
- [x] 显示重要存档数据

## 4. Skip Button 不可见排查
- [x] 在 `BgFadeTest.cs` 中只看到存档按钮
- [x] 排查 `FungusVNController` 中跳过按钮未显示的原因
- [x] 修复并验证

## 5. 音效未加载排查
- [x] `Assets/Scenes/Amiao_Test` 中音效未加载
- [x] 类似图片未加载问题，检查 Resources 路径
- [x] 排查并修复

## 6. 演出效果增强
- [x] 不修改对话内容
- [x] 参考 `Dialogue1.txt` 的演出方式
- [x] 修改 Dialogue2/3/4 增强演出效果（Dialogue5 不存在，未创建）
- [x] 在 `Assets/Art` 下找图片素材
- [x] 在 `Assets/Audio` 下找声音素材

## 
- [x] SimpleSkipButton按钮为巨大白色且字体未显示 为方块 SkipButtonTest可以加载出ui字体正确的按钮但是要延迟加载 尝试中和两种问题
- [x] 修改Pick-Light-From-Dark\Assets\Resources\Dialogue\Dialogue5.txt 旁白、场景改为陆萤， 为这类对话加上（），表示心里话
- [x] 做一个占位文字功能的prefab测试 ，在需要展示图片、展示音效音乐的时候，素材缺失则在画面最前左上角显示占位文字”img missing”例如。

## 结局数据配置

| ID | 结局名称 | 结局描述 |
|---|---|---|
| 6001 | 【结局一：太阳照常升起】 | 薯片改变不了任何事，你也是。 |
| 6002 | 【结局二：莫比乌斯环】 | 一条走廊，离开起点之时，你就明白你终会回来。 |
| 6003 | 【结局三：人心不足蛇吞象】 | 得失荣枯总在天，机关用尽也徒然。 |
| 6004 | 【结局四：星垂之夜】 | 俯仰天地之间——无愧于人，无愧于心，无愧于己。 |
| 6005 | 【结局五：北极星】 | 我对你透露一个大秘密，这是人类最古老的玩笑——无论往哪走，都是向前走。 |
