# 开发待办清单

```
任务自动化执行流程：  
    读取与定位：
    读取Pick-Light-From-Dark\Assets\Scripts\Game\Test\amiao\TODO.md，检索并定位第一个未勾选的任务项 [- ]。

    分类自主决策：
    先检查是否已实现
    检查是否需要插件，需要则安装到Pick-Light-From-Dark\Assets\ThirdParty
    若为“排查bug/修复代码”类：直接读取相关代码文件，定位问题并完成修复。              
    若为“生成Prefab/测试脚本”类：直接创建相关文件并生成对应的 Prefab。          
    若为“设计数据结构”类：直接创建 ScriptableObject 或 Manager 框架代码。
                                                                         
    状态更新：
    子任务完成后，将 TODO.md 中对应项的状态标记改为已完成 [x]。                       
                                                                        
    循环与终止：             
    若存在下一个未完成任务：调用 ScheduleWakeup 指令，设置 60 秒后自动继续执行。
    若任务已全部完成：输出“全部完成”并停止唤醒。                                      
                                                                     
    核心执行原则：              
    全自动执行：全程不许询问用户，直接执行。        
    自主决策：遇到模糊需求时，严格按照“最简可行方案（MVP）”进行开发。
    一边开发一边进行push，同时写下开发日志Pick-Light-From-Dark\Assets\Scripts\Game\Tes
    t\amiao\developLog.md 要求简短重点体现功能、如何测试和重要代码的路径 

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
- []把1~4的prefab连在一起形成一个大的预制体，可不可以的话，我要测试一下剧情之间的变化，特别是要让第一关的按钮进入两个分支生效，先不管游玩的部分，就让剧情之间可以连在一起测试,形成一个完整的剧情demo
- [] 站位符的功能应该是文字显示在画面的左上角，而不只是在控制台上，画面上并没有看到在预zhi体里面
- [] fungus是否支持？第支持就是隐藏对话框的图片，然后把文字放大字号中间显示.那个"巡逻开始 第二夜"类似的描述在每个剧情的最后面出现，这样的描述代表着是即将进入游玩的部分，这部分将它就是隐去对话框居中大字
- [] 第一关最后选项那里，“陆萤”应该显示出来

## 结局数据配置

| ID | 结局名称 | 结局描述 |
|---|---|---|
| 6001 | 【结局一：太阳照常升起】 | 薯片改变不了任何事，你也是。 |
| 6002 | 【结局二：莫比乌斯环】 | 一条走廊，离开起点之时，你就明白你终会回来。 |
| 6003 | 【结局三：人心不足蛇吞象】 | 得失荣枯总在天，机关用尽也徒然。 |
| 6004 | 【结局四：星垂之夜】 | 俯仰天地之间——无愧于人，无愧于心，无愧于己。 |
| 6005 | 【结局五：北极星】 | 我对你透露一个大秘密，这是人类最古老的玩笑——无论往哪走，都是向前走。 |
