# ICE 汉化术语表（zh-CN）

> **权威来源**：`https://github.com/RedAsteroid/Ices-Cosmic-Exploration`（`Develop` 分支，完整汉化版）。
> 本表术语一律以该仓库为准；该仓库没有的概念才自行拟定，并标注 `〔拟〕`。
> 本文件不参与编译。分支：`localization/zh-CN` · 仅简体中文 · 不含 Debug 窗口。

---

## 0. 翻译操作规则（SOP 摘要）

1. **只翻译 `##` / `###` 之前的可见文本**，后缀 ID 原样保留。
2. **原本无 `##`、译后可能与同一 ID 作用域内其它交互控件重名**时，补唯一后缀。
   - 交互控件：`Button` / `Checkbox` / `Selectable` / `RadioButton` / `CollapsingHeader` /
     `BeginCombo` / `InputText` / `Input*` / `Slider*` / `Drag*` / `TreeNode` / `BeginTabItem` /
     `MenuItem` / `BeginChild`(名字) / `Window`(名字)。
   - 纯显示无 ID，可安全翻译：`Text` / `TextWrapped` / `TextDisabled` / `SetTooltip`。
3. **动态字符串**：保留插值与格式串，只译固定片段。
4. **换行**：手工 `\n` 段落按中文重新断行。
5. **标点**：本插件统一使用全角（`，。：！？（）`、`、`），与参考仓库的混用风格不同，此处取全角以求一致。
6. 参考仓库对应元素存在时，**优先采用其原文措辞**。

---

## 1. 核心概念（对齐参照仓库）

| 英文 | 中文 | 来源 |
|---|---|---|
| Cosmic Exploration | 宇宙探索 | 参照 |
| Cosmic Helper | 宇宙助手 | 参照 |
| Mission | 任务 | 参照 |
| Cosmic Agenda | 宇宙日程 | 〔拟〕 |
| Expedition Log | 探索日志 | 〔拟〕 |
| Relic | 宇宙工具 | 参照 |
| Relic research data | 研究数据 / 宇宙工具研究数据 | 参照 |
| Relic Grind | 宇宙工具研究数据刷取模式 | 参照 |
| Leveling Grind | 练级模式 | 参照 |
| Standard | 标准模式 | 参照 |
| Completion | 完成情况 | 参照 |
| Gold Completion Grind | 金星完成模式 | 〔拟〕 |
| Agenda Mode | 日程模式 | 〔拟〕 |
| Score | 技巧点 | 参照 |
| Cosmic credits | 宇宙信用点 | 参照 |
| Planetary credits | 行星信用点 | 参照 |
| Hub | 基地 | 参照 |
| Hub Activities | 基地活动 | 参照 |
| Overlay | 悬浮窗 | 参照 |
| Profile（Gathering） | 采集配置 | 参照 |
| Mission Priority | 任务优先级 | 参照 |
| Mission Settings | 任务设置 | 参照 |
| Stop When... | 停止条件 | 参照 |
| Misc Settings | 杂项设置 | 参照 |
| All Settings | 所有设置 | 参照 |
| Planet Selection | 地图选择 | 参照 |
| Class Selection | 职业选择 | 参照 |
| Tool Relic XP | 宇宙研究数据 | 参照 |
| Help | 帮助 | 参照 |
| Requirements | 插件需求 | 参照 |
| Plugins Log | Ice 日志 | 参照 |

## 2. 任务类型 / 等级

| 英文 | 中文 | 来源 |
|---|---|---|
| Critical / Red Alert | 紧急探索（任务） | 参照 |
| Provisional | 临时性任务 | 参照 |
| Sequence | 连续任务 | 参照 |
| Weather | 天气限定（任务） | 参照 |
| Timed | 时间限定（任务） | 参照 |
| Master | 超难任务 | 用户确认 |
| Rank A / B / C / D | A 类任务 / B 类任务 / C 类任务 / D 类任务 | 参照 |
| Gold / Silver / Bronze | 金星 / 银星 / 铜星 | 参照 |
| Gold rating | 金星评价 | 参照 |
| Turn-in | 汇报 / 提交 | 参照 |

## 3. 设置与操作

| 英文 | 中文 | 来源 |
|---|---|---|
| Start / Stop | 开始 / 停止 | 参照 |
| Stop after current mission | 当前任务结束后停止 | 参照 |
| Stop at threshold | 达到阈值时停止 | 参照 |
| Stop once relic finished | 宇宙工具可报告时（停止） | 参照 |
| Turnin if relic is complete | 宇宙工具可报告时提交 | 参照 |
| Remove Mission Upon Gold | 金星完成时移除任务 | 参照 |
| Allow All Classes（provisional） | 允许执行所有临时性任务 | 参照 |
| Relic Mode: Only Enabled | 仅限启用的任务 | 参照 |
| Ignore manual missions | 忽略手动模式任务 | 参照 |
| Show selected job only | 只显示选择的职业 | 参照 |
| Save Current Mission Preset | 保存当前任务为预设 | 参照 |
| View All Presets | 查看所有预设 | 参照 |
| Load Mission Preset | 加载任务预设 | 参照 |
| Playlist Name | 预设名称 | 参照 |
| Save New List | 保存 | 参照 |
| Clear All | 全部清空 | 参照 |
| Import Missions | 导入任务 | 参照 |
| Remove from list | 移除 | 参照 |
| Swap jobs when turning in relic | 提交宇宙工具时切换职业 | 参照 |
| Use Stylist to re-equip tools | 使用 Stylist 插件重新装备工具 | 参照 |
| Repair | 修理 | 参照 |
| Mount | 坐骑 | 参照 |
| Stellar Sprint | 宇宙冲刺 | 参照 |
| Crafters | 能工巧匠 | 参照 |
| Gatherers | 大地使者 | 参照 |
| planet token | 行星票据 | 参照 |
| Credit Shopping | 信用点购物 | 参照 |
| Gambling Settings | 宇宙好运道设置 | 参照 |
| Dronebit Settings | 无人机晶片设置 | 参照 |
| random mount | 随机坐骑 | 参照 |
| Reset to default | 重置为默认 | 参照 |

## 4. 表格 / 统计

| 英文 | 中文 | 来源 |
|---|---|---|
| Mission Name | 任务名称 | 参照 |
| Score | 技巧点 | 参照 |
| Sort / Sort by | 排序方式 | 参照 |
| Map position | 地图位置 | 参照 |
| Class score | 职业技巧点 | 参照 |
| Class exp | 职业经验值 | 参照 |
| Research data I..VI | 研究数据 I … 研究数据 VI | 参照 |
| Amount Enabled | 启用任务数量 | 参照 |
| Gold requirement | 金星需求 | 参照 |

## 5. 职业

**能工巧匠（制作，11）**

| 英文 | 缩写 | 中文 |
|---|---|---|
| Carpenter | CRP | 刻木匠 |
| Blacksmith | BSM | 锻铁匠 |
| Armorer | ARM | 铸甲匠 |
| Goldsmith | GSM | 雕金匠 |
| Leatherworker | LTW | 制革匠 |
| Weaver | WVR | 裁衣匠 |
| Alchemist | ALC | 炼金术士 |
| Culinarian | CUL | 烹调师 |

**大地使者（采集，3）**

| Miner | MIN | 采矿工 |
| Botanist | BTN | 园艺工 |
| Fisher | FSH | 捕鱼人 |

**战斗职业（21）**：骑士 / 战士 / 暗黑骑士 / 绝枪战士 / 白魔法师 / 学者 / 占星术士 / 贤者 /
武僧 / 龙骑士 / 忍者 / 武士 / 钐镰客 / 蝰蛇剑士 / 吟游诗人 / 机工士 / 舞者 /
黑魔法师 / 召唤师 / 赤魔法师 / 绘灵法师

## 6. 星球

| 英文 | 中文 | 来源 |
|---|---|---|
| Sinus Ardorum | 憧憬湾 | 参照 |
| Phaenna | 法恩娜行星 | 参照 |
| Oizys | 俄匊斯行星 | 参照 |
| Auxesia | 奥克塞西亚 | — |

## 7. 遗物经验面板（Relic_XP）

| 英文 | 中文 | 来源 |
|---|---|---|
| Stage | 阶段 | 参照 |
| [MAX] | [最高] | 参照 |
| Type | 类型 | 参照 |
| Score | 技巧点 | 参照 |

> 参考仓库中 `阶段` / `类型` 用半角冒号（`阶段: `），本插件统一用全角。

## 8. 禁止翻译清单

- `/ice` 子命令关键字比较：`add` / `remove` / `toggle` / `only` / `flag` / `stop` / `start` / `help` / `debug` / `settings` / `overlay`。
- 仅作 ID 的字符串：`PushID("...")`、`Child("...")`、`Window("...###...")`、`TableSetupColumn("##...")`、以 `##`/`###` 开头的纯 ID、弹窗 ID。
- Config 字段名、序列化键、游戏 sheet 名、资源路径、URL、图标资源名。
- 游戏 Excel 表来源的任务名 / 物品名 / 地名。
- 窗口标题品牌名 `Ice's Cosmic Exploration`（参照仓库亦保留英文）。

## 9. 待确认项（用户手改 vs 参照的差异）

| 位置 | 用户手改 | 参照仓库 | 当前采用 |
|---|---|---|---|
| 侧边栏任务页 | 任务列表 | 标准 | 任务列表（保留用户手改） |
| 任务设置按钮 | 任务列表 | 任务设置 | 任务设置（对齐参照） |
| A–D 级筛选按钮 | A 级 … | A 类任务 … | A 类任务 …（对齐参照） |
| Expedition Log | 探索日志 | （无对应，参照为“完成情况”） | 探索日志 |
