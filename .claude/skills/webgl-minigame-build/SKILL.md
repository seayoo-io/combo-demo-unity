---
name: webgl-minigame-build
description: 装配微信 / 抖音小游戏工程，把 Unity 导出的 WebGL 产物变成可运行的小游戏 Native 工程。当用户说「装配微信」「装配抖音」「跑一下微信小游戏」「使用线上产物装配」「同步 combosdk 产物」，或需要排查小游戏装配失败时使用。
---

# WebGL 小游戏工程装配

所有流程逻辑都在 `${CLAUDE_SKILL_DIR}/scripts/build.py` 里，本 skill 只负责：把用户意图翻译成正确的命令行参数、检查前置条件、以及在失败时解释原因。**不要绕过脚本手工执行 Combo CLI 或手工复制产物**，否则实际做了什么就无从追溯，出问题也难复现。

## 环境缺失时的行为约定

脚本在前置检查阶段就会检出环境缺失并退出（不会白跑装配和编译）。遇到下面两类报错，**动手把它解决掉再重跑，不要只把报错转述给用户让他自己去弄**：

| 报错 | 该做什么 |
| --- | --- |
| `未找到 aws CLI` | 直接帮用户装上。**三个平台装法完全不同，先确认用户是 Windows / macOS / Linux 再动手**，各自的命令见 `references/troubleshooting.md`。装完用 `aws --version` 确认能跑 |
| `尚未创建本机配置文件` / `配置文件缺少必要参数` | 按下面「配置向导」逐项问用户，拿到后生成配置文件并**自动继续**原来的构建 |

安装类操作若需要改动用户的全局环境（如 `brew update`），**先问用户**。详细做法和注意事项都在 `references/troubleshooting.md`。

## 配置向导

脚本在开跑前会把**本次操作需要、但还没配的参数一次性全部列出**（按需：装配抖音不会问 S3 凭据，没传 `--local-sdk` 不会问 SDK 路径），每项都带「用途 / 获取方式 / 形如」。遇到这个报错时按下面走完，不要让用户自己去建文件。

### 步骤

1. **把脚本列出的参数逐个问用户**。用 AskUserQuestion，**一次问一个**，问的时候带上脚本给出的「用途」和「获取方式」，让用户知道这个值是干什么的、去哪儿找。不要一次性甩一堆问题，也不要让他自己拼 JSON。
2. **边问边确认合理性**，明显不对的当场指出来，别等写进文件跑失败了才发现：
   - S3 的 Access Key 是控制台「密钥管理」生成的随机串，**不是登录用户名**。用户给了个像人名/邮箱的值，先跟他确认。
   - 武汉和北京是**两个独立后台，凭据不同**，不能拿同一组填两边。
   - 路径必须是绝对路径；Windows 上要用正斜杠 `C:/Users/me/webgl` 或双反斜杠，JSON 里单反斜杠是转义符。
3. **生成配置文件**。**必须用 python 读出已有 JSON、改完再写回**，不要整个覆盖——用户可能已经配了一部分，覆盖会把它冲掉：

   ```python
   import json, pathlib
   p = pathlib.Path(".claude/skills/webgl-minigame-build/config.local.json")
   cfg = json.loads(p.read_text(encoding="utf-8")) if p.is_file() else {}
   cfg["webglSdkDir"] = "<用户给的路径>"
   eps = cfg.setdefault("s3", {}).setdefault("endpoints", [])
   by_name = {e.get("name"): e for e in eps if isinstance(e, dict)}
   for name, ak, sk in [("wuhan", "<AK>", "<SK>"), ("beijing", "<AK>", "<SK>")]:
       ep = by_name.get(name)
       if ep is None:
           ep = {"name": name}; eps.append(ep)
       ep["accessKeyId"], ep["secretAccessKey"] = ak, sk
   p.write_text(json.dumps(cfg, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
   ```

4. **写完不要回显凭据**，只说「已写入配置文件」。这个文件已在 `.gitignore` 中。
5. **自动重跑原来的构建命令**，把流程继续下去——用户要的是「装配微信」，不是「配好文件」。**不要停在这里等他再说一次。**

### 注意

- 只问脚本列出来的那几项。`endpointUrl`、`s3.target`、`region`、`acl` 都有内置默认值，不要拿去烦用户。
- 用户明确不想配的话有退路：`--skip-step3` 跳过上传、去掉 `--local-sdk` 改用线上产物。但别主动往这上面引导，那是降级方案。
- 用户说「凭据我不方便给你」时，告诉他可以自己设端点专属环境变量 `S3_WUHAN_ACCESS_KEY_ID` / `S3_WUHAN_SECRET_ACCESS_KEY`、`S3_BEIJING_*`，脚本同样认，优先级还更高。

## 文件布局

```
.claude/skills/webgl-minigame-build/
├── SKILL.md                        本文件，高频内容
├── scripts/build.py                全部流程逻辑
├── references/
│   ├── usage-guide.md              给人看的使用指南
│   └── troubleshooting.md          排查手册与新机器环境准备
└── config.local.json               本机配置，已 gitignore，各机器自行创建
```

- **脚本报错、或新机器上缺 aws CLI / 缺 S3 凭据时，先读 `references/troubleshooting.md`**，里面按报错信息逐条列了原因和处置方式，不要自己猜。
- 用户问「这个怎么用」「怎么配」「新同事怎么上手」这类问题时，`references/usage-guide.md` 是写给人看的完整指南，可以直接让他读，或从中摘取需要的部分回答。

脚本可以从任意工作目录调用，它自己用 `git rev-parse --show-toplevel` 定位仓库根。让用户手动执行时给仓库相对路径 `.claude/skills/webgl-minigame-build/scripts/build.py`——`${CLAUDE_SKILL_DIR}` 只在 skill 被调用时才有值，直接贴给用户他敲不了。

## 整体流程

| 步骤 | 内容 | 谁执行 |
| --- | --- | --- |
| 前置 | Unity 导出 WebGL / 小游戏工程到 `outputs/` | 人工在 Unity 中操作，脚本不负责 |
| 1 | `Combo webgl setup --distro <distro>` 装配 Native 工程 | 脚本 |
| 2 | 用本地 webgl SDK 编译产物替换 `combosdk/` 中的产物 | 脚本，仅在传了 `--local-sdk` 时执行 |
| 3 | 发布前处理，按 distro 分派（见下） | 脚本 |

编号与脚本的实际执行顺序一致。Unity 导出不参与编号——它由人工在 Unity 里完成，脚本不负责。

**环境依赖**：Python **3.8+**（脚本本身就是 Python，跨 Windows / macOS / Linux 共用一份代码）、`Combo` CLI（步骤 1）；步骤 2 另需 `npm` 和本地 webgl SDK 仓库，微信的步骤 3 另需 `aws` CLI。

调用时 macOS / Linux 用 `python3`，**Windows 上通常是 `python`**（官方安装包不提供 `python3` 这个名字）。

步骤 3 在两个 distro 下是完全不同的两件事：

- **微信**：把 `outputs/weixinminigame/webgl/` 下的 `*.webgl.wasm.code.unityweb.wasm.br` 和 `*.webgl.data.unityweb.bin.txt` 用 `aws s3 cp` **同时上传到武汉、北京两个 S3**。这两个文件超出小游戏包体限制，必须远程加载，地址就是 `game.js` 里的 `DATA_CDN`。

  | 端点名 | endpoint | 说明 |
  | --- | --- | --- |
  | `wuhan` | `https://s3-wuhan.kingamer.cn` | 同时也是 `DATA_CDN` 的域名 |
  | `beijing` | `https://s3.shiyou.kingsoft.com` | |

  两个是各自独立的后台，**凭据不同，必须分别配置**。任一端点上传失败脚本就中止——只传上一边等于没传。
- **抖音**：清理 `outputs/tt-minigame/tt-minigame/StreamingAssets/`，只保留 `game.js`，其余文件与子目录全部删除。

## 目录布局

| distro | 导出根目录 | 小游戏工程目录（Combo CLI 的工作目录） |
| --- | --- | --- |
| `minigame_weixin` | `outputs/weixinminigame` | `outputs/weixinminigame/minigame` |
| `minigame_douyin` | `outputs/tt-minigame` | `outputs/tt-minigame/tt-minigame` |

`Combo webgl setup` 必须在含 `game.js` 的小游戏工程目录里执行（它在该目录下生成 `combosdk/` 并改写 `game.js`），不是在 `webgl/` 目录。

## 本机配置

本机相关的配置放在 `.claude/skills/webgl-minigame-build/config.local.json`，该文件**不纳入版本管理**（已在 `.gitignore` 中），每台机器各自创建：

```json
{
  "webglSdkDir": "/path/to/webgl",
  "s3": {
    "endpoints": [
      { "name": "wuhan",   "accessKeyId": "...", "secretAccessKey": "..." },
      { "name": "beijing", "accessKeyId": "...", "secretAccessKey": "..." }
    ]
  }
}
```

- `webglSdkDir`：webgl SDK 仓库的本地**绝对路径**，步骤 2 用它找编译产物。**必填**（除非不用 `--local-sdk`）。Windows 上要写成 `"C:/Users/me/webgl"` 或 `"C:\\Users\\me\\webgl"`——JSON 里单反斜杠是转义符，直接粘贴资源管理器的路径会出错。
- `s3.endpoints`：步骤 3 上传用。两个端点的 `endpointUrl` 和 `s3.target`（`s3://apps/demo`）都有内置默认值，通常**只需要填 `accessKeyId` / `secretAccessKey`**。按 `name` 与默认端点合并，所以省略 `endpointUrl` 是正常写法。
- 每个端点还可选 `profile`（改用 `~/.aws/credentials` 里的 profile）、`region`、`acl`（桶不是公共读时才需要）。
- 不想把凭据写进文件时，可以改用端点专属环境变量 `S3_WUHAN_ACCESS_KEY_ID` / `S3_WUHAN_SECRET_ACCESS_KEY`、`S3_BEIJING_*`（优先级高于配置文件）。

SDK 路径优先级：`--sdk-dir` > 配置文件的 `webglSdkDir` > 兜底猜测 `<repo>/../../webgl`。**不要把本机路径或凭据写进脚本、本文档、或任何纳入版本管理的文件**，遇到这类问题一律走配置文件或环境变量。

## 常用调用方式

```bash
# 「装配微信」——默认形态：装配 + 编译本地 SDK + 覆盖产物
python3 ${CLAUDE_SKILL_DIR}/scripts/build.py --distro wx --local-sdk

# 「装配抖音」
python3 ${CLAUDE_SKILL_DIR}/scripts/build.py --distro tt --local-sdk

# 只改了 webgl SDK 代码、想快速验证：重新编译并覆盖产物，不重新装配
python3 ${CLAUDE_SKILL_DIR}/scripts/build.py --distro wx --local-sdk --no-setup

# SDK 已经编译过，只想覆盖产物
python3 ${CLAUDE_SKILL_DIR}/scripts/build.py --distro wx --local-sdk --no-setup --no-build-sdk

# 「装配微信，使用线上产物」
python3 ${CLAUDE_SKILL_DIR}/scripts/build.py --distro wx
```

意图 → 参数的对应关系：

- **默认使用本地产物**：「装配微信」「装配抖音」「跑一下微信小游戏」这类只说了平台的指令，一律带上 `--local-sdk`，即 `--distro wx --local-sdk` / `--distro tt --local-sdk`
- 明确说「使用线上产物」「用线上的 / 用发布版 / 不用我本地的」→ 去掉 `--local-sdk`
- 只改了 SDK、导出工程没动（「我改了 SDK，重新同步一下」）→ 再加 `--no-setup`，省掉一次装配和下载
- 提到「不用重新编译 / dist 是最新的」→ 再加 `--no-build-sdk`

平台没说清楚（既没提微信也没提抖音）时问用户，不要替他猜一个 distro。

## 步骤 2 的同步规则

顺序是：读 `ComboSDK.json` 的 `domains` → 按 `domain_to_js` 映射表算出需要哪些 js → 校验 `combo/dist` 中齐备 → **清掉 `combosdk/` 下原有的产物** → 从 dist 复制。

先校验后清理，是为了避免删完才发现 dist 缺文件、把工程留在损坏状态。`combosdk.import.js` 和 `ComboSDK.json` 由 Combo CLI 生成、dist 中没有，永远保留。清理这一步是必要的：Combo CLI 的 setup 只增不删，切换 distro 后旧平台的产物会残留在目录里被打进包体。

映射表的权威依据是 webgl 仓库 `combo/src/combo/manager/AnalyticsManager.ts` 的 `convertToModule`：只有 `solar_engine` / `tg_minigame` / `gravity_engine` 会触发动态 `import()` 对应 chunk，其余 domain 一律 `Module.None`，不加载独立产物。`move_ts.sh` 可作参考，但它有两处与源码不符，**不要照抄**：

- `minigame_weixin_ads` → `combosdk.tg.mg.js`：`convertToModule` 不认这个 domain。腾讯广告能力已并入引力引擎（见 `GravityEngineWeixinManager` 的 `gravity_engine_minigame_weixin_tg_*` 参数），由 `combosdk.ge.mg.wx.js` 承载，独立的 `tg.mg` chunk 在当前配置下是死代码，故不映射。
- `solar_engine` 抖音 → `combosdk.se.mg.dy.js`：dist 中不存在该文件，CLI 和 `import.js` 用的都是 `combosdk.se.mg.wx.js`，故两个 distro 都映射到 wx 版本。

同步完成后用 `combosdk.import.js` 双向校验：

- **缺失**（import.js 要、没复制）→ 直接失败，否则小游戏运行时加载不到模块
- **多余**（复制了、import.js 不要）→ 告警不中断，文件不会被加载但占包体

正常情况下两个 distro 都不应有告警，复制的文件数应与步骤 1 中 CLI `Copied` 的数量一致。出现「多余」告警说明映射表又和 SDK 源码脱节了，去 `convertToModule` 核对，不要直接把文件加进映射表了事。

修改映射表或同步规则时，同步更新 `scripts/build.py` 里 `domain_to_js` 与 `sync_local_sdk` 的注释。
