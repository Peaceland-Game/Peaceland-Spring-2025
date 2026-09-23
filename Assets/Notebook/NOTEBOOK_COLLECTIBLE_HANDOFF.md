# Peaceland Notebook - 可收集物交接文档

## 不用读代码、不用改代码，往场景里放一个能收集的东西

---

## 📋 这份文档解决什么

以前往场景里加一个"捡起来会记进笔记本"的东西，要手动做四件事：挂 `NotebookCollectTrigger`、把笔记资产拖进它的列表、补一个碰撞体、再加发光。漏任何一步都不报错，只是**点了没反应**。

现在这四步合成了一个窗口。你要做的只有：

1. ✅ **写一条笔记**（填表，不写代码）
2. ✅ **同步一次数据库**（点一下菜单）
3. ✅ **在下拉里选中它，按一个按钮**

> **你不需要打开任何 `.cs` 文件。**
> 这份文档里出现的所有操作都在 Unity 编辑器菜单里。

---

## 🧠 先理解三个东西（只有三个）

| 名字 | 它是什么 | 你在哪碰它 |
|------|---------|-----------|
| **Notebook Entry** | 一条笔记本内容（标题 + 正文 + 配图） | 一个 `.asset` 文件，在 Inspector 里填 |
| **Notebook Database** | 全部笔记的总目录，游戏只认这里面的 | 一个 `.asset`，**用菜单同步，不要手填** |
| **Collectible** | 场景里那个被点了会解锁笔记的物体 | 用本文档的窗口生成 |

关系：**写 Entry → 同步进 Database → 在场景里生成 Collectible 指向它。**
三者缺一个，游戏里就是"点了没反应"。

---

## 🔧 操作步骤

### **第1步: 新建一条笔记**

```
在 Project 窗口里，进到 Assets/Notebook/Data
右键 → Create → Peaceland → Notebook → Entry
给文件起个名字，比如 NotebookEntry_FloristRose
```

选中它，在 Inspector 里填这几个字段。**其余字段留默认就行**：

```
NotebookEntryDefinition
┌────────────────────────────────────────────────────┐
│ Entry Id     → 全局唯一的英文 id，比如 florist-rose │
│                ⚠ 定了就别再改（存档按它认人）        │
│                                                     │
│ Section      → 这条笔记归到哪一页：                  │
│                Directory / Present /                │
│                Memory1 / Memory2 / HiddenStats      │
│                                                     │
│ Title        → 笔记本里显示的标题                    │
│ Body Text    → 笔记正文（多行）                      │
│ Image        → 配图 Sprite                           │
│                ⚠ 这张图同时是场景里那个物体的外观     │
└────────────────────────────────────────────────────┘
```

> **Image 留空也能跑**，只是场景里会生成一个看不见的方块（1×1 碰撞体）。做灰盒测试无所谓，正式内容记得给图。

---

### **第2步: 把它登记进数据库**

```
菜单栏 → Peaceland → Notebook → Sync Notebook Database From Assets
```

这一步把 `Assets/Notebook/Data` 里所有 Entry 扫一遍，写进 `NotebookDatabase`。

**每次新建 Entry 之后都要点一次。** 忘了点的话，第3步的窗口会直接黄字警告你，并给你一个按钮当场补上——所以忘了也不要紧。

---

### **第3步: 打开生成器窗口**

```
菜单栏 → Peaceland → Notebook → Add Collectible To Scene...
```

窗口长这样：

```
Add Collectible
┌──────────────────────────────────────────────────────┐
│ 1. Which note does this unlock?                      │
│    [ Present/florist-rose  —  A Pressed Rose      ▾] │
│                                                       │
│    ⚠ 如果这条还没进数据库，这里会出现一条黄色警告      │
│      和一个 [Sync Notebook Database From Assets] 按钮 │
│      点它，警告就消失                                  │
│                                                       │
│ 2. What kind of object?                              │
│    [ World Sprite                                 ▾] │
│                                                       │
│ 3. Options                                           │
│    Pulsing glow        [✓]                           │
│    Hide once collected [✓]                           │
│                                                       │
│            [      Add To Scene      ]                │
└──────────────────────────────────────────────────────┘
```

下拉里列的是**全项目**的笔记，格式是 `分区/id — 标题`，按分区排好序，不用自己找文件。

---

### **第4步: 选一种形态**

| 选项 | 生成什么 | 什么时候用 |
|------|---------|-----------|
| **World Sprite** | 世界里的一张图 + 碰撞体，鼠标点它就收集 | 默认选这个。场景里的花、信、物件 |
| **UI Button** | Canvas 下的一个 UI 图片，点它就收集 | 笔记本界面上、HUD 上的按钮 |
| **Attach To Selection** | 不新建物体，把收集行为加到你**当前选中**的物体上 | 你已经摆好了一个美术资源，只想让它能被捡 |

**Attach To Selection 要先在 Hierarchy 里选中一个物体**，没选中时按钮是灰的。

两个开关：

```
Pulsing glow        → 加一个呼吸式高亮，提示玩家"这个能点"
                      （只对有 SpriteRenderer 的物体生效，UI Button 没有）

Hide once collected → 收集后物体自动隐藏，不会被重复点
                      关掉的话，收集过的物体会一直留在场景里
```

---

### **第5步: 按 Add To Scene**

按下去之后，场景里就有了：

```
Collectible - florist-rose
├─ SpriteRenderer          (图 = Entry 的 Image)
├─ BoxCollider2D           (大小按图自动算好)
├─ NotebookCollectTrigger  (Entries 里已经填好你选的那条)
├─ NotebookCollectableGlowView
└─ Collect Glow            (发光子物体，运行时自动生成，编辑器里看不到)
```

物体会落在**你当前 Scene 视图的正中间**，直接拖到想要的位置就行。

Console 里会打印一行确认：

```
Collectible ready: 'Collectible - florist-rose' unlocks 'florist-rose'.
```

**Ctrl+Z 可以撤销**，新建的物体和加上去的组件都在撤销范围里（可能要连按几下）。

> 选 **UI Button** 时，如果场景里还没有 Canvas，窗口会**自动建一个**（1920×1080，Screen Space Overlay），并补上 GraphicRaycaster 和 EventSystem——没有这两个东西，UI 按钮点了不会有任何反应。

---

## 🎮 运行时会发生什么

```
玩家点击物体
   ↓
NotebookCollectTrigger.Collect()
   ↓
查这条 Entry 是不是已经收集过了
   ├─ 已收集 → 什么都不做
   └─ 没收集 → 写进存档（场景里有提示条的话，顺带弹一行提示）
                 ↓
              Hide once collected 开着的话，物体隐藏
```

场景里**不需要**提前放 `NotebookController`。找得到就用，找不到就自动走全局存档通道。两种情况都能正常收集。

---

## 🐛 常见问题

### **1. 点了没反应**

按这个顺序查：

```
① 这条 Entry 在 NotebookDatabase 里吗？
   → Peaceland → Notebook → Sync Notebook Database From Assets

② 之前是不是已经收集过了？
   → 存档里记着呢，第二次点当然没反应
   → Peaceland → Save → Reset All Progress  然后重新测

③ World Sprite: 场景里有相机吗？碰撞体被别的东西挡住了吗？
④ UI Button: 场景里有 EventSystem 吗？
   → 新版会自动补。老场景手动补的话：
     GameObject → UI → Event System
```

### **2. 场景里只有一个看不见的小方块**

Entry 的 **Image 字段是空的**。回第1步补图，然后**重新生成一次**（旧的删掉）。

### **3. 生成的物体太大 / 太小**

Sprite 的 **Pixels Per Unit** 决定的，不是这个窗口决定的。改 Sprite 的导入设置，或者直接在场景里缩放物体。

### **4. 我改了 Entry 的 Entry Id，之前的收集记录没了**

正常。存档按 `Entry Id` 认人，改 id 等于换了一条新笔记。**Entry Id 定了就别改。**

### **5. 我想一个物体解锁好几条笔记**

生成完之后，在 Inspector 里选中那个物体，`NotebookCollectTrigger` 的 **Entries** 列表可以直接加。这是普通的 Unity 列表，拖 Entry 资产进去就行。

---

## 📁 文件清单

### 你会碰的：

| 路径 | 干什么 |
|------|-------|
| `Assets/Notebook/Data/*.asset` | 你写的笔记 |
| `Assets/Notebook/Data/NotebookDatabase.asset` | 总目录（**用菜单同步，别手改**） |
| 你自己的场景 | 生成物落在这里 |

### 你不需要碰的：

```
Assets/Notebook/Scripts/     ← 全部代码，一行都不用看
Assets/Notebook/Prefabs/     ← 笔记本界面本体
Assets/Peaceland/            ← 存档系统
```

---

## 🚀 快速验收清单

新建一条笔记到能在游戏里收集，完整走一遍：

```
□ 1. Create → Peaceland → Notebook → Entry
□ 2. 填 Entry Id / Section / Title / Body Text / Image
□ 3. Peaceland → Notebook → Sync Notebook Database From Assets
□ 4. Peaceland → Notebook → Add Collectible To Scene...
□ 5. 下拉里选中它（确认没有黄色警告）
□ 6. 选 World Sprite，按 Add To Scene
□ 7. 把物体拖到想要的位置，Ctrl+S 存场景
□ 8. 按 Play，点它 → Console 出现收集日志，笔记本里出现这条
□ 9. 再点一次 → 没反应（正确，已经收集过了）
```

全过程 **0 行代码**。

如果第 6 步生成的东西看起来不对，先跑一次 **Peaceland → Notebook → Harness → Verify Collectible Spawner**：它会在一个临时场景里把三种类型各生成一遍再撤销掉，Console 里出 `[SpawnerHarness] PASS` 就说明工具本身是好的，问题出在 Entry 或场景上。

---

## ⚠️ 明确不要做的事

```
✗ 不要手改 NotebookDatabase.asset  → 用 Sync 菜单，手改会被下次同步覆盖
✗ 不要改已经上线的 Entry Id        → 存档会认不出来
✗ 不要改 Assets/Notebook/Scripts/  → 有需求找 Yu，不要自己改
```

---

## 📞 有问题找谁

这套工具和笔记本系统由 **Yu Ma** 维护。
上面第9步跑不通、或者你需要一种这个窗口不支持的形态，直接说，不要自己改脚本。

---

*文档对应版本：`Peaceland/Notebook/Add Collectible To Scene...`（2026-09-23）*
