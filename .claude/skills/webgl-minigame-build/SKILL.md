---
name: webgl-minigame-build
description: 装配微信 / 抖音小游戏工程，把 Unity 导出的 WebGL 产物变成可运行的小游戏 Native 工程。当用户说「装配微信」「装配抖音」「跑一下微信小游戏」「使用线上产物装配」「同步 combosdk 产物」，或需要排查小游戏装配失败时使用。
---

# WebGL 小游戏工程装配

所有流程逻辑都在 `${CLAUDE_SKILL_DIR}/scripts/build.py` 里，本 skill 只负责：把用户意图翻译成正确的命令行参数、检查前置条件、以及在失败时解释原因。**不要绕过脚本手工执行 Combo CLI 或手工复制产物**，否则实际做了什么就无从追溯，出问题也难复现。

## 环境缺失时的行为约定

脚本在前置检查阶段就会检出环境或配置缺失并停下（不会白跑装配和编译）。遇到下面两类情况，**动手帮用户解决掉再重跑，不要只把脚本输出转述给他让他自己去弄**：

| 情况 | 该做什么 |
| --- | --- |
| `[ERROR] 未找到 aws CLI` | 直接帮用户装上。**三个平台装法完全不同，先确认用户是 Windows / macOS / Linux 再动手**，各自的命令见 `references/troubleshooting.md`。装完用 `aws --version` 确认能跑 |
| `[配置]` 开头的青色提示（缺配置文件或参数，退出码 2） | 这不是报错，是首次使用的引导。按下面「配置向导」用表格一次性列出所需参数，让用户一次性发回，据此生成配置文件并**自动继续**原来的构建 |

脚本用青色 `[配置]` 前缀、退出码 2 表示「等你补参数」，与红色 `[ERROR]`、退出码 1 的真正错误区分开——见到 `[配置]` 就走配置向导，不要当成失败。

安装类操作若需要改动用户的全局环境（如 `brew update`），**先问用户**。详细做法和注意事项都在 `references/troubleshooting.md`。

## 配置向导

脚本在开跑前会用青色 `[配置]` 提示列出**本次操作需要、但还没配的参数**（按需：装配抖音不会问 S3 凭据，没传 `--local-sdk` 不会问 SDK 路径），每项都带「用途 / 获取方式 / 形如」。这份清单是给你（Claude）读的信息源——据它**分组逐次地问用户**（`webglSdkDir` 单独一次、每个 S3 端点的两个凭据成对一次），不要把整份清单甩给用户让他自己建文件。

### 步骤

1. **分组问，问哪组说哪组的作用就行**。按下面的分组，一次问一组、用一句话说清作用（作用取脚本输出的「用途」那句）。**不要摆表格，不要连获取方式/示例一起念**——除非用户主动问怎么拿：
   - **`webglSdkDir`** 单独问（就它一项）：

     > 先给我 webgl SDK 仓库在你本机的绝对路径（步骤 2 要从这里取本地编译的 SDK 产物）。

   - **S3 凭据按端点成对问**：同一个端点的 Access Key ID 和 Secret Access Key **一起要**，让用户一条消息把这一对发过来；这个端点收齐了，再问下一个端点。武汉、北京分两次，不要混在一起问，也不要拆成四次。

     > 接下来是武汉 S3 的凭据（步骤 3 上传大文件用），把它的 Access Key ID 和 Secret Access Key 一起发我。

2. **用户答完，什么都别多说，直接问下一组**。不要复述他给的值、不要说「好的/已记录/收到」、不要小结进度，就直接抛出下一组的问题。一组接一组，直到问完。

   唯一的例外是**值明显不对**，这时才出声、让他重给，然后继续：
   - S3 的 Access Key 是控制台「密钥管理」生成的随机串，用户给了个像人名/邮箱/短密码的值 → 提醒「要的是 Access Key 不是登录用户名」。
   - 一对里只发了一个（只给了 AK 没给 SK，或反之）→ 提示还差另一半，别急着往下问。
   - 路径不是绝对路径，或 Windows 上用了单反斜杠（`C:\Users\...`，JSON 里会出错）→ 让他改成正斜杠。
   - 问北京的凭据时如果发现和刚才武汉的一模一样 → 提醒两个是独立后台、凭据不同，是不是拿混了。

3. **全部收齐后，一次性生成配置文件**。**必须用 python 读出已有 JSON、改完再写回**，不要整个覆盖——用户可能已经配了一部分，覆盖会把它冲掉：

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

SDK 路径优先级：`--sdk-dir` > 配置文件的 `webglSdkDir`。两者都没有就视为缺失、进入配置向导要求用户提供——**不做任何基于本机目录布局的兜底猜测**（比如「webgl 和本仓库同级」），那种假设只对个别机器成立，无法通用。同理，S3 凭据也只认显式来源（配置文件 / 环境变量 / 显式 profile），不因机器上碰巧有 `~/.aws` 就当凭据可用。

**不要把本机路径或凭据写进脚本、本文档、或任何纳入版本管理的文件**，遇到这类问题一律走配置文件或环境变量。

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
