# Notebook Prefab 使用指南

本指南适用于 `D:\Peaceland\_migration-notebook-clean`。Notebook 的 UI 来源只有一个：

- `Assets/Notebook/Prefabs/NotebookProductionSceneUI.prefab`
- 测试工具来源：`Assets/Notebook/Prefabs/NotebookTestSceneControls.prefab`

不要在 Scene 中手工复制 Notebook Canvas，也不要让 runtime script 创建 Notebook UI。

## 1. Production Scene 接入

1. 打开目标 Scene。
2. 将 `NotebookProductionSceneUI.prefab` 拖到 Scene 根节点。
3. 确认 Scene 中只有：
   - 1 个 `NotebookController`
   - 1 个 `NotebookUIShellReferences`
   - 1 个 `NotebookOpenButton`
   - 1 个 Notebook Canvas
4. 如果 Scene 没有 `EventSystem`，创建一个，并使用项目当前 Input System 对应的 UI Input Module。
5. 保存 Scene，进入 Play Mode：
   - 点击左上角 Notebook icon。
   - 打开、切换 section、翻页、关闭。
   - 离开并重新进入 Scene，确认收集状态一致。

也可以使用：

`Peaceland > Notebook > Author Open UI In Active Scene`

该命令只放置和绑定 prefab，不再动态生成 UI。

## 2. Notebook 测试 Scene 接入

测试 Scene 应同时包含：

- `NotebookProductionSceneUI.prefab`
- `NotebookTestSceneControls.prefab`
- Scene 自己的真实 collectible、minigame 或 interaction adapter

不要把 `NotebookTestSceneControls.prefab` 放进 production Scene。

批量准备 5 个 Notebook 测试 Scene：

`Peaceland > Notebook > Prefabs > Migrate All Notebook Test Scenes`

这个命令会：

1. 重建 `NotebookTestSceneControls.prefab`。
2. 清理旧 bootstrap 动态 UI。
3. 在每个测试 Scene 放置 production UI prefab。
4. 放置 test-controls prefab。
5. 绑定 controller、shell、open button 与 harness。
6. 保存 Scene。

## 3. 修改 Notebook 外观

打开 `NotebookProductionSceneUI.prefab` 的 Prefab Mode 修改：

- notebook icon
- book background
- directory
- bookmark tabs
- entry template
- record-choice panel
- page-turn controls
- toast/notification

不要直接在每个 Scene 重做相同修改。Scene override 只用于：

- Canvas sorting order 的特殊冲突
- 该 Scene 明确需要的可见性差异
- 与 Scene-specific UI 的小范围位置协调

如果多个 Scene 都需要同一 override，应把修改应用回 prefab。

## 4. 添加可收集 Note

1. 在 `Assets/Notebook/Data` 创建或复制一个 `NotebookEntryDefinition`。
2. 设置唯一 `entryId`。
3. 设置 section、title、body、image、sort order。
4. 将该 Entry 加入 `NotebookDatabase.asset`。
5. 在真实可交互对象上添加对应 Notebook collectible/adapter。
6. 在 Inspector 中引用该 Entry asset，不要在代码中写死 Entry 文本。
7. Play Mode 验证：
   - interaction 前未收集
   - interaction 后出现 detected/updated 提示
   - Notebook 中出现正确图片、标题和自适应正文
   - 重复 interaction 不重复添加

## 5. 添加 Minigame 完成收集

Minigame Scene 保留自己的完成条件。完成时由 Scene adapter 调用 Notebook 收集入口：

1. Adapter 引用 `NotebookEntryDefinition`。
2. 只在 minigame 成功完成时 collect。
3. 不要在 Notebook UI 内判断 minigame 规则。
4. 如果 Entry 需要 interpretation，启用 record-choice 配置。
5. 再次打开 Notebook 时确认选择面板出现。

Notebook、minigame 和 Scene 的职责：

- Minigame：判断完成。
- Adapter：把完成结果转换为 Entry collect。
- Notebook：显示待 interpretation 状态并保存选择。
- Stat system：应用该选择配置的 stat delta。

## 6. 配置 Interpretation 与 Stats

在 `NotebookEntryDefinition` 中：

1. 启用需要 record choice 的选项。
2. 配置 2–4 个 interpretation choices。
3. 每个 choice 使用稳定的 choice id。
4. 设置要改变的 `PeacelandStatId` 与 delta。
5. 不要把 stat 名称或数值写进 Scene button 代码。

测试闭环：

1. 完成 minigame 并 collect。
2. 关闭后再次打开 Notebook。
3. 选择 interpretation。
4. 检查 stat 只变化一次。
5. Save。
6. 切换 Scene。
7. Load 或重启 Play Mode。
8. 确认 choice、Entry 和 stat 都恢复。
9. 再次打开，不得重复应用 delta。

## 7. `NotebookTestSceneBootstrap` 的新职责

该类型仅为旧 Scene/序列化引用保留。它现在只会：

- 查找已有 prefab instance
- 把 `NotebookUIShellReferences` 应用到 `NotebookController`
- 配置 `NotebookOpenButton`
- 配置可选 `NotebookTestHarness`

它不会：

- 创建 Canvas/EventSystem
- 创建 Button/TMP/entry template
- 创建 Entry asset 或 Database
- collect dummy entries
- 修复或重写 RectTransform

新 Scene 通常不需要添加该 component。

## 8. Inspector Debug Checklist

- [ ] Scene 中只有一个 production Notebook prefab instance
- [ ] Production Scene 中没有 `NotebookTestHarness`
- [ ] 测试 Scene 中存在一个 test-controls prefab instance
- [ ] `NotebookController.Database` 非空
- [ ] `NotebookUIShellReferences` 的关键引用非空
- [ ] `NotebookOpenButton` 指向当前 controller
- [ ] 没有 `Test Tools (Runtime)` 或动态生成 Canvas
- [ ] Console 没有 missing script、duplicate EventSystem 或 duplicate controller
- [ ] 16:9、16:10 与窗口缩放下，icon/toast 保持在左上安全区
- [ ] Entry 正文高度随文字变化，图片不会挤压正文

## 9. Migration Checklist

- [ ] 先在 duplicate 中执行迁移菜单
- [ ] 检查 5 个 Notebook test Scenes
- [ ] 检查 production prefab 没有 test-only component
- [ ] 检查 production Scenes 没有被批量迁移命令修改
- [ ] Unity Console 0 个 compile error
- [ ] 运行 content Harness
- [ ] 运行 save/load closed loop
- [ ] 完成人工 Play Mode 清单
- [ ] 审查 Git diff 后再决定是否 stage/commit

## 10. 回滚

所有工作必须在 `_migration-notebook-clean` 中进行。

回滚单个文件或 Scene 时，只恢复明确目标，不要 reset 整个 dirty worktree。迁移前可复制目标 Scene，或先创建本地 checkpoint commit；未经确认不要 push。

