# 使用指南

面向第一次用这套流程的人。读完能独立跑通微信 / 抖音小游戏工程的装配。

## 这套东西解决什么问题

Unity 导出的 WebGL 产物**不能直接跑**，要变成可运行的小游戏工程，中间有三步固定操作，手工做既繁琐又容易漏。这个 skill 把它们合并成一条指令。

| 步骤 | 做什么 | 漏了会怎样 |
| --- | --- | --- |
| 1 | 执行 `Combo webgl setup` 装配 Native 工程 | 工程里没有 combosdk，SDK 完全不可用 |
| 2 | 用本地编译的 webgl SDK 产物覆盖线上产物（**可选**） | 调试的是线上版本，本地改的代码不生效 |
| 3 | 微信：把两个大文件传到 CDN<br>抖音：清理 StreamingAssets | 微信：游戏加载不到资源包<br>抖音：包体超限 |

Unity 导出本身**不在**这套流程里，需要你先在 Unity 里做完。

## 一次性准备

### 1. 装好依赖

| 工具 | 用途 | 缺了会怎样 |
| --- | --- | --- |
| Python **3.8+** | 脚本本身就是 Python 写的 | 完全跑不了 |
| `Combo` CLI | 步骤 1 装配 | 步骤 1 失败 |
| `npm` + Node | 步骤 2 编译 webgl SDK | 只在用 `--local-sdk` 时需要 |
| `aws` CLI | 步骤 3 上传 CDN | 只在装配微信时需要 |

`aws` CLI 三个平台的装法完全不同（Windows 用 winget 或 MSI，macOS 用官方 pkg，Linux 用官方 zip），各自的命令见 [`troubleshooting.md`](./troubleshooting.md) 的「缺 aws CLI」一节。

两个容易踩的点：**macOS 上不要用 `brew install awscli`**（brew 版会因系统库版本不匹配而装完跑不起来）；**Windows 上装完必须新开终端**，否则 PATH 不刷新，会让人误以为没装上。

装完务必用 `aws --version` 确认**能跑**，而不只是文件存在。

### 2. 建配置文件

**最省事的做法：什么都不用建，直接对 Claude 说「装配微信」。** 脚本会列出缺哪些参数，Claude 会逐个问你、并帮你把配置文件生成好，然后自动继续装配。

想自己建也可以，在 skill 目录下新建 `config.local.json`（这个文件已被 `.gitignore`，不会提交，每台机器各建各的）：

```json
{
  "webglSdkDir": "/你的路径/webgl",
  "s3": {
    "endpoints": [
      { "name": "wuhan",   "accessKeyId": "...", "secretAccessKey": "..." },
      { "name": "beijing", "accessKeyId": "...", "secretAccessKey": "..." }
    ]
  }
}
```

- **`webglSdkDir`**：本地 webgl SDK 仓库的**绝对路径**。只有用 `--local-sdk`（也就是想调试本地 SDK 改动）时才需要。
  - ⚠️ **Windows 上不能直接粘贴资源管理器里的路径**。JSON 把反斜杠当转义符，`"C:\Users\me\webgl"` 会解析失败；更糟的是 `"C:\temp\new\build"` 这种**不会报错**，但 `\t` `\n` `\b` 会被当成制表符、换行、退格，路径被静默改成乱码。两种正确写法：
    ```json
    "webglSdkDir": "C:/Users/me/webgl"        // 正斜杠，推荐
    "webglSdkDir": "C:\\Users\\me\\webgl"     // 双反斜杠
    ```
    Windows 本身两种都认，脚本会照常工作。真写错了脚本也会直接告诉你哪里错了。
- **`s3.endpoints`**：只有装配**微信**时才需要。两个端点的地址和存储桶路径脚本里都有默认值，通常**只需要填这两组 Key**。
- ⚠️ **武汉和北京是两个独立后台，凭据不一样**，要分别去各自的控制台生成，不能填同一组。
- ⚠️ 要的是 **Access Key ID / Secret Access Key**（控制台「密钥管理」里生成的），**不是登录用的用户名密码**。填错会直接报 `InvalidAccessKeyId`。

不想把凭据写进文件的话，也可以改用环境变量（优先级更高）：

```bash
export S3_WUHAN_ACCESS_KEY_ID=... S3_WUHAN_SECRET_ACCESS_KEY=...
export S3_BEIJING_ACCESS_KEY_ID=... S3_BEIJING_SECRET_ACCESS_KEY=...
```

## 日常使用

### 第一步：在 Unity 里导出

**微信**：菜单 `ComboSDK` → `Build Demo`，平台选 `WeixinMiniGame`，确认 ExportPath 是 `outputs/weixinminigame`（选平台后会自动填），点 `Start Build`。

**抖音**：走抖音官方的 Unity 转换工具，不在上面这个面板里，导出目录是 `outputs/tt-minigame`。具体操作请问项目里熟悉抖音接入的同事——本文不臆测。

导出目录必须是这两个，脚本按它们查找产物：

| 平台 | 导出根目录 | 小游戏工程目录 |
| --- | --- | --- |
| 微信 | `outputs/weixinminigame` | `outputs/weixinminigame/minigame` |
| 抖音 | `outputs/tt-minigame` | `outputs/tt-minigame/tt-minigame` |

### 第二步：跑装配

**推荐方式**——直接对 Claude 说：

> 装配微信

或者：

> 装配抖音

默认会用**你本地编译的 SDK 产物**（适合调试 SDK 改动）。想用线上发布的版本，就说「装配微信，使用线上产物」。

**手动执行**也可以，脚本在任意工作目录下都能跑（它自己会定位仓库根）：

```bash
# macOS / Linux
python3 .claude/skills/webgl-minigame-build/scripts/build.py --distro wx --local-sdk

# Windows（PowerShell 或 cmd）
python .claude\skills\webgl-minigame-build\scripts\build.py --distro wx --local-sdk
```

脚本用 Python 而不是 shell 写的，就是为了三个平台共用一份代码——Windows 上不需要装 Git Bash 或 WSL。

### 第三步：导入小游戏工具

用微信开发者工具打开 `outputs/weixinminigame/minigame`，或用抖音开发者工具打开 `outputs/tt-minigame/tt-minigame`。

## 场景速查

| 你想做什么 | 跟 Claude 说 | 对应命令参数 |
| --- | --- | --- |
| 完整装配微信，用本地 SDK | 装配微信 | `--distro wx --local-sdk` |
| 完整装配抖音，用本地 SDK | 装配抖音 | `--distro tt --local-sdk` |
| 用线上发布的 SDK 装配 | 装配微信，使用线上产物 | `--distro wx` |
| 只改了 SDK 代码，重新同步 | 我改了 SDK，重新同步一下 | `--distro wx --local-sdk --no-setup` |
| SDK 已编译过，只覆盖产物 | 不用重新编译，只同步产物 | 再加 `--no-build-sdk` |
| 暂时不想传 CDN | 跳过上传 | 再加 `--skip-step3` |

`--no-setup` 会跳过步骤 1，省掉一次装配和下载，改 SDK 时反复验证很省时间。

## 出问题怎么办

**先看报错信息，再对照 [`troubleshooting.md`](./troubleshooting.md)**，那里按报错文本逐条列了原因和处置方式。几个高频的：

- `未找到导出目录 ...` → Unity 导出没做，或导到了别的路径
- `未找到 aws CLI` / `以下端点没有可用的 S3 凭据` → 环境或配置没准备好，见上文「一次性准备」
- `Read timeout on endpoint URL` → 两个 S3 都在**内网**，要连公司网络/VPN；脚本已自动绕过 http_proxy，所以不是代理配置问题
- `InvalidAccessKeyId` → 凭据填错了，多半是把登录用户名当成了 Access Key ID

脚本的设计是**能提前失败就提前失败**：缺环境、缺凭据这类问题会在装配和编译**之前**就报出来，不会让你白等几分钟。所以看到报错先别急着重跑，多半重跑还是同样的错。

## 几条注意事项

- **`config.local.json` 绝不能提交**。它已经在 `.gitignore` 里，但别用 `git add -f` 强加，也别把内容贴到聊天记录、issue、文档里——里面有 S3 凭据。
- **别绕过脚本手工执行 `Combo webgl setup` 或手工复制产物**。脚本里有一堆校验（产物齐备性、`combosdk.import.js` 双向对账、上传后回读校验），手工做等于把这些保护全绕过去了，出问题也难复现。
- **步骤 2 会清空 `combosdk/` 下的旧产物再复制**。这是必要的：`Combo webgl setup` 只增不删，切换平台后旧平台的产物会残留并被打进包体。
- **微信的两个大文件必须两个 S3 都传成功**，只传上一边等于没传，所以脚本在任一端点失败时会直接中止。

## 想改脚本行为

流程逻辑全在 `scripts/build.py`，改之前建议先读 `SKILL.md` 的「步骤 2 的同步规则」——那里说明了 domain 到产物的映射依据，改错会导致小游戏运行时加载不到模块。
