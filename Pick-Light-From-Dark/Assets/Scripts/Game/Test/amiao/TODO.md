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

    每完成一个阶段则push一次

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


## 
- [x] SimpleSkipButton按钮为巨大白色且字体未显示 为方块 SkipButtonTest可以加载出ui字体正确的按钮但是要延迟加载 尝试中和两种问题
- [x] 修改Pick-Light-From-Dark\Assets\Resources\Dialogue\Dialogue5.txt 旁白、场景改为陆萤， 为这类对话加上（），表示心里话
- [x] 做一个占位文字功能的prefab测试 ，在需要展示图片、展示音效音乐的时候，素材缺失则在画面最前左上角显示占位文字”img missing”例如。

##
- [x] Pick-Light-From-Dark\Assets\Resources\Dialogue下如果Dialogue1~5都是一关的剧情，可以分成两部分，比如说是上段~游玩~下段，1~5正确的文本。首先搜索整个游戏5个关卡连起来的逻辑在整个项目里是如何实现，按需对这5个文本进行分段，待到形如1-1的那些 TXT 文本上，得到分成两段的两个txt。
- [x] 把Pick-Light-From-Dark\Assets\Scripts\Game\Test\amiao\SimpleSkipButton.cs的实现复刻到Pick-Light-From-Dark\Assets\Scenes\Amiao_Test\FungusVN_1.prefab 1~4prefab上，使按钮可见。
- [x] 做个 FungusVN_5.prefab ，测试后可以的话可以再和结局分支合并起来。

##
- [x] Pick-Light-From-Dark\Pick-Light-From-Dark\As   sets\Resources\Dialogue\旧对话（未分段）\Dialogue5.txt   
  根据5的结局以及天台的结局 每段剧情分成各个prefab 
- [x] prefab存档和跳过的按钮位置偏右，存档两按钮已经超出了屏幕外，是分析是画幅出了问题还是什么问题？如果只是修改坐标就可以的话，那就修改坐标是往左一点，两个按钮
- [x]把1~4的prefab连在一起形成一个大的预制体，可不可以的话，我要测试一下剧情之间的变化，特别是要让第一关的按钮进入两个分支生效，先不管游玩的部分，就让剧情之间可以连在一起测试,形成一个完整的剧情demo
- [x] 站位符的功能应该是文字显示在画面的左上角，而不只是在控制台上，画面上并没有看到在预zhi体里面
- [x] fungus是否支持？第支持就是隐藏对话框的图片，然后把文字放大字号中间显示.那个”巡逻开始 第二夜”类似的描述在每个剧情的最后面出现，这样的描述代表着是即将进入游玩的部分，这部分将它就是隐去对话框居中大字
- [x] 第一关最后选项那里，”陆萤”应该显示出来

- [x] Pick-Light-From-Dark\Assets\Scenes\Amiao_Test\旧剧情prefab（路径改了不能直接跑）和这个不一样Pick-Light-From-Dark\Assets\Scenes\Amiao_Test 分段后的prefab里的字体 按照旧的prefab的字体移植过来
- [x] Pick-Light-From-Dark\Assets\Scenes\Amiao_Test\TestPrefabs\StoryChainTester.prefab day1-1.prefab中的选项点不了 不能进入分支剧情
- [x] Pick-Light-From-Dark\Assets\Resources\Dialogue\Dialogue5-1.txt 5-几系列中“[ 陆萤 ]”都换成“陆萤”。以及我修改了txt但是5-2的prefab未同步过去修改，是不是生成剧本方式和Pick-Light-From-Dark\Assets\Resources\Dialogue\旧对话（未分段）不一样了
- [x] 跳过的位置应该是跳到选项（第一关和第五关），或者跳到下一段剧情（即下一个prefab）
- [x] 看不到文字最可能的原因是 PlaceholderDisplay.cs 没有设置字体（第109-112行）：已在 PlaceholderDisplay.cs:113-114 修复，加载 Font/LXGWWenKaiScreen 或 Font/文软雅黑。
- [x] StoryChainTester.prefab 包含分支功能与结局判定
  - [x] 重构 StoryChainTestRunner：支持多关分支选项与第五关 prefab
  - [x] 结局画面集成：第一关/第五关结局自动触发 EndingManager
  - [x] 结局条件配置：Inspector 可手动编辑 EndingCondition
  - [x] 结局分歧点文档：写出各结局在操作中的触发位置
- [x]整体的所有的预制体中文本框中姓名的坐标向上一点，然后底下文本框的说话内容的坐标向下一点
- [x] The referenced script on this Behaviour (Game Object 'PlaceholderTester') is missing! 已修复 GUID 引用
- [x] 自动显示缺失素材示例文字（无需按键，运行后0.5秒自动显示）
- [x] 增加个快进键 开发中模式使用功能是？嗯，可以快速的推动剧情吗？把所有的那些文字一短短对话的加速，不用点击就通过，只要松开按键 fungus有类似功能吗 没有就自己实现下。可以建一个开发者功能父类，之后还有需要就添加进去，最后可以一次性取消这些操作，但是不会影响游戏其他部分，完全独立。
  - [x] 创建 `DevModeBase.cs` 开发者功能抽象基类
  - [x] 创建 `FastForwardDevMode.cs` 按住空格快进（松开停止）
  - [x] `DevModeBase.DisableAllDevModes()` 一键禁用所有开发模式
  - [x] 运行时自动查找 FungusVNController，完全独立不影响其他系统
- [x] 居中字体调小（72→48）
- [x] 给每关第一段剧情结尾加上居中大字（Dialogue3-1/4-1/5-1）
- [x] 存档任务 先优化 再编写测试prefab 只要看一下数据是否被记录
  - [x] `SaveLoadTestRunner.cs`：IMGUI 界面，支持模拟保存/读取/清除存档
  - [x] `SaveLoadTester.prefab`：挂载测试脚本的预制体
  - [x] 显示存档内容：关卡/时间/结局分支/卡牌使用/任务目标
  - [x] Pick-Light-From-Dark\Assets\Scenes\GameScene.unity 其中自己跳过过了第一句话 Pick-Light-From-Dark\Assets\Scenes\Amiao_Test\day1-1.prefab 无此问题 找找原因
  - [x] 做一个结局画面的功能prefab 主要是包含重新开始和返回主界面 其中死亡结局的重新开始是从本关的游玩部分开始 其他结局是从头开始 预留接口为后续的存档系统使用。Pick-Light-From-Dark\Assets\Scripts\Game\Test\amiao\需求\结局画面.log  结局文字在todo.md最底下  针对Pick-Light-From-Dark\Assets\Scenes\GameScene.unity 的结局部分进行优化 出一个prefab
  - [x] Pick-Light-From-Dark\Assets\Scripts\Game\Test\amiao\ending.md 按照这个设计一套结局分支系统 结合具体的游玩过程Pick-Light-From-Dark\Assets\Scenes\GameScene.unity 仅设计 还有设计留给后端调用的接口
  - [x] Pick-Light-From-Dark\Assets\Scenes\GameScene.unity游戏开始的时候 剧情到游玩部分 这些切换的时候会没有画面出现透明 而且会看到开始画面（还没消失）可以使用loading画面吗
  - [x] 做一个跳过功能的prefab 用以检查跳过的内容，跳过的时候跳到选项边，是不是显示的是选项之前的那个对话   同时这个跳过功能不应该能跳过选项
  - [x] 优化存档系统 在全局（视觉小说+游玩+结局） 分成两路存储 关卡数和其他数据 每次读档 按照关卡数回到对应的关卡的时间 而其他数据和结局相关Pick-Light-From-Dark\Logs\player_data.json 分析这个方案的可行性和优化方向  还是说原来的更好？ 然后做一个有剧情的prefab可以存档 Dialogue1~2 在2存档 再读档就是2剧情开始处；然后做一个Pick-Light-From-Dark\Assets\Scenes\GameScene.unity类似的prefab Dialogue1+level1+Dialogue1-1+Dialogue2 测试能不能读档到第二个
  - [ ] Pick-Light-From-Dark\Logs\05150420快进bug.log Assets/Scripts/Game/Test/amiao/FastForwardDevMode.cs   快进时遇到
  ```txt
    [hide_dialog]
    [wait:2]
    [show_dialog]

    [bg:light_room,fade]
    [se:DXH_SOUND/04.移动被子]
  ```
  之后只显示舍友，对话内容为妈，并且卡在这里无法推进 适配一下Pick-Light-From-Dark\Assets\Resources\Dialogue中的格式 让里边的内容、背景、音效都可以被跳过，尽量做
  -[ ]
```



## 结局数据配置

| ID | 结局名称 | 结局描述 |
|---|---|---|
| 6001 | 【结局一：太阳照常升起】 | 薯片改变不了任何事，你也是。 |
| 6002 | 【结局二：莫比乌斯环】 | 一条走廊，离开起点之时，你就明白你终会回来。 |
| 6003 | 【结局三：人心不足蛇吞象】 | 得失荣枯总在天，机关用尽也徒然。 |
| 6004 | 【结局四：星垂之夜】 | 俯仰天地之间——无愧于人，无愧于心，无愧于己。 |
| 6005 | 【结局五：北极星】 | 我对你透露一个大秘密，这是人类最古老的玩笑——无论往哪走，都是向前走。 |
