# 商店页面商品分类功能 — 设计文档

日期：2026-09-15

## 背景与目标

当前 Demo 商店页面（`ShopView`）把所有平台/渠道的商品混在一个列表里展示，商品数量增多后不易查找。本次需求：在商店入口新增一个"分类选择页"，按发行平台/渠道对商品做分组展示，用户可以先选分类，再看该分类下的商品列表。

## 现状（调研结论）

- UI 框架为 UGUI，单 Scene + Prefab 面板堆叠架构，无场景路由。
- 面板统一继承 `View<T>`（`Assets/Scripts/Foundations/Types/View.cs`），通过 `[ViewPrefab("Prefabs/xxx")]` 特性从 `Resources` 加载 prefab；`UIController`（`Assets/Scripts/Components/Controllers/UIController.cs`）是静态路由入口，每个面板一对 `ShowXxxView()/HideXxxView()`。
- 商店页面：`ShopView.prefab` → `ProductPanel(ScrollRect)` → `Content`（挂 `GridLayoutGroup`，`cellSize=(100,130)`，`spacing=(30,20)`，即现有的"九宫格"效果）→ 由 `ProductManager`（挂在 `Content` 节点）在 `Start()` 里拉取商品数据、循环 `AppendProductView` 生成 `ProductView` 实例。
- 商品数据结构 `ListProduct`（`Assets/Scripts/GameClient.cs:65-76`）只有 4 个字段：`productId`、`productName`（商品名称）、`price`、`iconUrl`，**没有独立的"描述"或"分类"字段**。经确认，用户所说的"商品描述"实际指 `productName`。
- 商店入口：`Game.cs:61` 调用 `UIController.ShowShopView()`。

## 需求确认（与用户逐项核对的结论）

1. **分类数组**：写死在代码中，值为 `app_store`、`android`（后续如需增删平台，直接改代码）。
2. **匹配规则**：`productName.StartsWith(分类值)`，纯字符串前缀匹配，无需分隔符或正则。未匹配任何分类前缀的商品，只会出现在"全部商品"分组中，不影响其可购买性。
3. **页面流程**：分类选择页作为商店的新入口（替换 `Game.cs:61` 原本直接进商品列表的调用）。
4. **分类九宫格内容**：分类数组每一项 + 额外一个"全部商品"项，纯文字按钮格子（不复用商品卡片的图片/边框样式），布局参数与现有商品网格一致（`cellSize=(100,130)`，`spacing=(30,20)`）。
5. **商品列表页**：复用现有 `ShopView` 样式，新增顶部标题文字，显示当前分类名称（如"app_store 商品"/"全部商品"），点击"全部商品"进入时展示效果与当前商店页面完全一致（不过滤）。
6. **返回逻辑**：
   - 商品列表页返回按钮 → 回到分类选择页（而不是像现状那样直接回游戏首页）。
   - 分类选择页返回按钮 → 回到游戏首页（沿用现有 `ShopView` 的"返回首页"行为，迁移到分类页）。
7. **Prefab 搭建方式**：由于本环境无法打开 Unity 编辑器可视化搭建，新增/修改的 prefab（`CategoryView.prefab`、`CategoryItemView.prefab`，以及 `ShopView.prefab` 新增标题节点）将直接手写 `.prefab`（YAML）文件，参照现有 `ShopView.prefab`/`ProductView.prefab` 的结构和序列化格式。这部分风险高于常规代码改动，需要用户在 Unity 编辑器中打开实际验证效果（层级、引用绑定、布局是否符合预期）。

## 架构设计

### 数据层（不改动数据结构，仅改渲染侧过滤逻辑）

- `ListProduct` 保持不变。
- 分类判断逻辑：`productInfo.productName.StartsWith(categoryPrefix)`，在 `ProductManager` 拉取到全量数据后本地过滤，不新增后端接口。

### 新增：分类选择页（CategoryView）

- `Assets/Scripts/Components/Views/CategoryView.cs`：仿照 `ShopView.cs`，继承 `View<CategoryView>`，`[ViewPrefab("Prefabs/CategoryView")]`，持有 `homeBtn` + `Action OnGoHome`（返回游戏首页）。
- `Assets/Scripts/Components/Controllers/CategoryManager.cs`：挂在 `CategoryView.prefab` 的 `Content` 节点（结构比照 `ShopView.prefab` 的 `ProductPanel/Viewport/Content`），职责：
  - 顶部定义 `private static readonly string[] Categories = { "app_store", "android" };`
  - `Start()` 中依次为 `Categories` 的每一项 + "全部商品" 实例化一个 `CategoryItemView`，设置文字和点击回调。
  - 点击回调：具体分类 → `UIController.ShowShopView(category, category + " 商品")`；"全部商品" → `UIController.ShowShopView(null, "全部商品")`。
- `Assets/Scripts/Components/Views/CategoryItemView.cs`：仿照 `ProductView.cs` 精简为纯文字格子，继承 `View<CategoryItemView>`，`[ViewPrefab("Prefabs/CategoryItemView")]`，持有 `Text titleTxt`、`Button btn`，暴露 `SetTitle(string)` 和点击事件回调（`Action OnClick`，仿 `ShopView` 的 `SetGoHomeCallback` 风格）。

### 改造：商品列表页（ShopView / ProductManager）

- `ShopView.cs`：新增 `public Text titleTxt;` 和 `public void SetTitle(string title)`；`SetGoHomeCallback` 语义不变，但调用方（`UIController`）传入的回调从"直接 Destroy"改为"Destroy 后回到分类页"。
- `ProductManager.cs`：新增 `public string categoryFilter;`（默认为 `null`，表示不过滤）。`Start()` 中过滤：
  ```csharp
  GameClient.GetListProduct(data =>
  {
      var filtered = string.IsNullOrEmpty(categoryFilter)
          ? data
          : data.Where(p => p.productName.StartsWith(categoryFilter)).ToArray();
      foreach (var productInfo in filtered) { ... }
  });
  ```
  （`System.Linq` 已在文件头部引入，无需新增 using。）
- `categoryFilter` 需要在 `ProductManager.Start()` 执行前被设置，由 `UIController.ShowShopView` 在 `Instantiate()` 之后、`Show()` 之前通过 `shopView.GetComponentInChildren<ProductManager>()` 赋值，符合 Unity `Awake`/`Start` 时序（同一帧内 `Instantiate` 后立即赋值一定早于下一帧的 `Start`）。

### 改造：路由入口（UIController / Game.cs）

- `UIController.cs`：
  - `ShowShopView()` 改为 `ShowShopView(string categoryPrefix, string categoryTitle)`：设置标题、设置 `categoryFilter`、`SetGoHomeCallback` 回调改为 `shopView.Destroy(); ShowCategoryView();`。
  - 新增 `ShowCategoryView()`：`CategoryView.DestroyAll()` → `Instantiate()` → `SetGoHomeCallback(() => categoryView.Destroy())`（回游戏首页） → `Show()`。
- `Game.cs:61`：`UIController.ShowShopView()` 改为 `UIController.ShowCategoryView()`。

### Prefab 改动清单

- 新建 `Assets/Resources/Prefabs/CategoryView.prefab`：结构比照 `ShopView.prefab`（`Canvas` 根节点、`homeBtn`、`ProductPanel(ScrollRect)/Viewport/Content` 挂 `GridLayoutGroup` 沿用相同参数、`ContentSizeFitter`），`Content` 节点挂 `CategoryManager`。
- 新建 `Assets/Resources/Prefabs/CategoryItemView.prefab`：一个简单的 `Button` + 居中 `Text` 子节点，尺寸与 `ProductView.prefab` 的 `cellSize`（100×130）匹配。
- 修改 `Assets/Resources/Prefabs/ShopView.prefab`：在 `ProductPanel` 上方新增一个标题 `Text` 节点，并在 `ShopView` 组件的序列化字段里新增对该节点的引用（`titleTxt`）。

## 影响范围 / 不改动的部分

- 不改动后端接口、`ListProduct` 数据结构、购买流程（`ProductManager.OnPurchase` 等）。
- 不引入分类的可配置化（Inspector/配置文件），分类数组是代码常量。
- 不改动限购商品逻辑（`limit_` 前缀判断）。

## 验证方式

本环境无法运行 Unity 编辑器/播放模式，实现完成后需要用户在 Unity 编辑器中打开验证：
1. 游戏首页进商店 → 是否先看到分类九宫格（app_store / android / 全部商品）。
2. 点击具体分类 → 商品列表是否按 `productName` 前缀正确过滤，标题是否正确。
3. 点击"全部商品" → 是否展示与改动前一致的全量商品列表。
4. 商品列表页返回 → 是否回到分类页；分类页返回 → 是否回到游戏首页。
5. Prefab 引用绑定是否完整（无 Missing Reference 报错）。
