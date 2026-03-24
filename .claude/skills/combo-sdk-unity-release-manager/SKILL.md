---
name: combo-sdk-unity-release-manager
description: 当用户需要发布新版本、更新版本号、生成 CHANGELOG、Bump Version、Prepare 新版本、更新版本字段时使用此 skill。支持 Bump（直接指定版本号）和 Prepare（准备新版本，自动递增 minor 版本）两种模式。
type: skill
---

# Combo SDK for Unity Release Manager

## 功能说明

用于 Combo SDK for Unity 项目发版时自动更新版本号和生成 CHANGELOG。支持两种模式：

- **Bump 模式**：直接更新到用户指定的版本号（如 `2.19.0`）
- **Prepare 模式**：准备下一个版本，如果用户未指定版本号，则自动递增 minor 版本并添加 `-dev` 后缀（`x.x.x` → `x.(x+1).0-dev`）

---

## 执行步骤

### 步骤一：判断模式并获取版本号

#### 1. 判断用户请求的模式

| 模式 | 触发关键词 |
|------|-----------|
| **Bump 模式** | "Bump"、"更新到"、"发布"、"发布新版本"等 |
| **Prepare 模式** | "Prepare"、"准备新版本"、"下一个版本"等 |
| **默认** | 未明确指定时，默认为 **Bump 模式** |

#### 2. 获取版本号

**Bump 模式**：
- 检查用户是否已提供目标版本号（格式：`x.x.x`）
- 如果用户已提供版本号：直接使用该版本号
- 如果用户未提供版本号：
  1. 读取当前 `Combo/Version.swift` 中的 `SDKVersion`
  2. 若版本号包含 `-dev` 后缀，则自动移除该后缀
  3. 使用移除 `-dev` 后的版本号作为本次发布版本号

**Prepare 模式**：
- 如果用户已指定目标版本号，使用用户指定的版本号（自动添加 `-dev` 后缀，如用户输入 `2.20.0` 则使用 `2.20.0-dev`）
- 如果用户未指定版本号：
  1. 读取当前 `Combo/Version.swift` 中的 `SDKVersion` 字段，获取当前版本号
  2. 解析当前版本号 `x.y.z`，计算新版本号为 `x.(y+1).0-dev`
  3. 例如：当前版本是 `2.18.0`，则新版本号为 `2.19.0-dev`

#### 3. 确定版本号

将确定的版本号记为 `NEW_VERSION`，后续统一使用。

---

### 步骤二：更新版本字段

找到并修改以下文件：

| 文件路径 | 字段 |
|----------|------|
| `package.json` | `version` |
| `Packages/combo-sdk-unity/Combo/Plugins/Windows/ComboSDK/Runtime/Version.cs` | `SDKVersion` |
| `Packages/combo-sdk-unity/Combo/Plugins/Windows/SeayooAccount/Runtime/Constants.cs` | `Version` |
| `Packages/combo-sdk-unity/Combo/Runtime/Version.cs` | `SDKVersion` |

将上述字段值均更新为 `NEW_VERSION`。

---

### 步骤三：处理 CHANGELOG（仅 Bump 模式需要）

> **注意**：Prepare 模式不需要生成 CHANGELOG，直接跳过此步骤。

#### 1. 获取变更内容

如果用户没有提供 CHANGELOG 内容，先向用户询问变更内容。

#### 2. 优化用户的更新描述

- 将口语化表达改为简洁、专业的书面语
- 保持句式一致，以动词开头（如 **"新增"**、**"优化"**、**"修复"**、**"升级"**）
- 删除冗余词汇，每条控制在 50 字以内
- 技术名词保持准确（如 SDK 名称、API 名称、模块名等）

#### 3. 分类变更内容

| 分类 | 说明 |
|------|------|
| **FEATURES** | 新增功能、新 API、新组件、新模块 |
| **IMPROVEMENTS** | 性能优化、体验优化、代码重构、SDK 升级、非功能性改动 |
| **BUG FIXES** | Bug 修复、崩溃修复、问题修复 |

#### 4. 生成 CHANGELOG 条目

格式如下：

```markdown
## NEW_VERSION (YYYY-MM-DD)

**FEATURES:**

- xxx

**IMPROVEMENTS:**

- xxx

**BUG FIXES:**

- xxx
```

**说明**：
- 日期使用当前日期（`YYYY-MM-DD`）
- 没有内容的分类不显示（例如没有 `BUG FIXES` 就不输出该分类）

---

### 步骤四：更新 CHANGELOG.md（仅 Bump 模式需要）

**Bump 模式**：将新版本内容插入到 `CHANGELOG.md` 文件顶部（在第一个 `##` 之前），文件路径：Packages/combo-sdk-unity/CHANGELOG.md

**Prepare 模式**：跳过此步骤

---

## 执行要求

1. **严格按照步骤顺序执行，不要跳步**
2. **在缺少信息时，必须先向用户提问，不要自行假设**（Prepare 模式下自动计算版本号除外）
3. 输出内容应尽量清晰、结构化
4. **版本号格式**：
   - Bump 模式：`x.x.x`（如：`2.19.0`）
   - Prepare 模式：`x.x.x-dev`（如：`2.19.0-dev`）
5. 每次只执行一个版本号的 Bump/Prepare 操作
6. **区分模式**：Prepare 模式不生成 CHANGELOG，只更新版本号

---

## 示例

### 示例 1：Bump 模式（指定版本号）

#### 用户输入

```
帮我把 iOS 版本号更新到 2.19.0

本次更新：
- 新增微信登录功能
- 优化登录界面 UI
- 修复 iPad 设备崩溃问题
```

#### 期望输出

1. 更新 `Combo/Version.swift` 中 `SDKVersion` 为 `2.19.0`
2. 更新 `project.yml` 中 `MARKETING_VERSION` 为 `2.19.0`
3. 生成 CHANGELOG：

```markdown
## 2.19.0 (2026-03-23)

**FEATURES:**

- 新增微信登录功能

**IMPROVEMENTS:**

- 优化登录界面 UI

**BUG FIXES:**

- 修复 iPad 设备崩溃问题
```

4. 将上述内容插入到 `CHANGELOG.md` 顶部

---

### 示例 2：Prepare 模式（用户未指定版本号）

#### 用户输入

```
Prepare 一个新版本
```

假设当前 `Combo/Version.swift` 中的 `SDKVersion` 是 `2.18.0`

#### 期望输出

1. 读取当前版本号为 `2.18.0`
2. 计算新版本号为 `2.19.0-dev`（minor 版本 +1，添加 `-dev` 后缀）
3. 更新 `Combo/Version.swift` 中 `SDKVersion` 为 `2.19.0-dev`
4. 更新 `project.yml` 中 `MARKETING_VERSION` 为 `2.19.0-dev`
5. **不生成 CHANGELOG，不更新 CHANGELOG.md**

---

### 示例 3：Prepare 模式（用户指定版本号）

#### 用户输入

```
Prepare 新版本，版本号设为 2.20.0
```

#### 期望输出

1. 使用用户指定的版本号 `2.20.0`，自动添加 `-dev` 后缀变为 `2.20.0-dev`
2. 更新 `Combo/Version.swift` 中 `SDKVersion` 为 `2.20.0-dev`
3. 更新 `project.yml` 中 `MARKETING_VERSION` 为 `2.20.0-dev`
4. **不生成 CHANGELOG，不更新 CHANGELOG.md**

---

### 示例 4：Bump 模式（简洁输入）

#### 用户输入

```
更新 iOS 版本到 2.19.0，本次更新：
- 新增微信登录功能
- 优化登录界面 UI
- 修复 iPad 设备崩溃问题
```

#### 期望输出

同示例 1

---
