# 排查与环境准备

本文件是 `SKILL.md` 的附属参考，出问题或新机器首次配置时再读。

## 环境缺失时自动补齐

脚本在前置检查阶段就会检出缺失并退出（不会白跑装配和编译）。遇到下面两种情况**直接帮用户处理掉再重跑，不要只是把报错转述给用户让他自己去弄**。

### 缺 aws CLI

报错 `未找到 aws CLI`。**先看用户是什么系统再动手**——三个平台的装法完全不同，照着别的平台的命令敲只会失败。

#### Windows

任选其一，推荐 winget（Win10 1809+ / Win11 自带）：

```powershell
winget install -e --id Amazon.AWSCLI
```

没有 winget 就用官方 MSI（需要管理员权限）：

```powershell
msiexec.exe /i https://awscli.amazonaws.com/AWSCLIV2.msi /qn
```

不想要管理员权限时，MSI 支持装到当前用户：

```powershell
curl.exe -o AWSCLIV2.msi https://awscli.amazonaws.com/AWSCLIV2.msi
msiexec.exe /i AWSCLIV2.msi /qn ALLUSERS=2 MSIINSTALLPERUSER=1
```

⚠️ **装完必须新开一个终端**，否则 PATH 不刷新，`aws` 还是找不到——这一步很容易被当成"装失败了"。

#### macOS

**不要用 `brew install awscli`**：brew 版依赖 brew 的 python，而 brew python 链接系统 `libexpat`，系统 expat 偏旧时会 `dlopen` 失败（`Symbol not found: _XML_SetAllocTrackerActivationThreshold`），装完了也跑不起来，且这个坏法 `brew upgrade` 修不了。用官方安装包，自带完整 runtime，而且**免 sudo**：

```bash
curl -fsSL -o /tmp/AWSCLIV2.pkg https://awscli.amazonaws.com/AWSCLIV2.pkg
cat > /tmp/choices.xml <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0"><array><dict>
  <key>choiceAttribute</key><string>customLocation</string>
  <key>attributeSetting</key><string>$HOME</string>
  <key>choiceIdentifier</key><string>default</string>
</dict></array></plist>
EOF
installer -pkg /tmp/AWSCLIV2.pkg -target CurrentUserHomeDirectory -applyChoiceChangesXML /tmp/choices.xml
mkdir -p ~/.local/bin && ln -sf ~/aws-cli/aws ~/.local/bin/aws
```

若 `~/.local/bin` 不在 PATH 里，提示用户加上。

若用户坚持用 brew 且 brew 报 `undefined method '[]' for nil`（`Utils::Bottles.load_tab`），那是 Homebrew 代码版本落后于它下载的 API 元数据，要 `brew update` 才能修——**动 brew 前先问用户**，很多人是刻意设了 `HOMEBREW_NO_AUTO_UPDATE=1` 的。

#### Linux

```bash
curl -fsSL "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o /tmp/awscliv2.zip
unzip -q /tmp/awscliv2.zip -d /tmp && sudo /tmp/aws/install
```

ARM 机器把 URL 里的 `x86_64` 换成 `aarch64`。需要 sudo，让用户自己执行。

#### 装完一定要验证

```
aws --version
```

**要确认它能跑，而不只是文件存在**——macOS 上 brew 版就出现过 `aws --version` 正常、一执行 `aws s3` 就崩的情况（`--version` 不触发 XML 解析，S3 响应是 XML 才会加载出问题的库）。确认无误后重跑构建命令。

### 缺 S3 凭据 / 缺配置文件

报错 `尚未创建本机配置文件`、`配置文件缺少必要参数` 或 `以下端点没有可用的 S3 凭据`。

**完整流程见 `SKILL.md` 的「配置向导」一节**，这里只补充几条容易做错的：

- 不要写进 `~/.aws/credentials`——那是用户的全局配置，可能已经有别的 profile，不该被这个项目污染。凭据只放本项目的 `config.local.json`。
- 用户给的若是控制台**登录用户名/密码**而不是 Access Key，拿去调 S3 会直接 `InvalidAccessKeyId`。先跟他确认清楚，别写进配置文件白跑一趟。
- 生成配置后**直接把原来的构建命令重跑一遍**，不要停下来等用户再说一次——他要的是「装配微信」，配置文件只是路上的一道坎。

## 排查失败

- **`未找到导出目录 ...`**：前置的 Unity 导出没做。Unity 导出目前只能人工操作，直接告诉用户需要先在 Unity 里导出，不要尝试用命令行代跑。
- **`... 下没有 game.js`**：`outputs/` 下的工程是残缺的或导出到了别的路径，让用户确认 Unity 的导出目标目录。
- **`未找到 Combo CLI`**：需要安装 Combo CLI 并配置 `~/.combo/combo.yaml`。
- **`未找到 webgl SDK 仓库: ...`**：`config.local.json` 里的 `webglSdkDir` 没配或配错。新机器上常见原因是还没创建这个文件。
  - 若报错里同时提示**「路径里含有控制字符」**，那是 Windows 路径的反斜杠没转义。JSON 中 `\t` `\n` `\b` 都是合法转义，所以 `"C:\temp\new\build"` 解析时**不报错**、却被静默改成了乱码。让用户改成正斜杠 `"C:/Users/me/webgl"`（推荐）或双反斜杠 `"C:\\Users\\me\\webgl"`，Windows 两种都认。
- **`配置文件不是合法的 JSON`**：`config.local.json` 语法有误，报错下一行会给出具体的行列位置。
- **步骤 1 报错**：通常是 profile 或网络问题。脚本调用的是 PATH 里的 `Combo`，用默认 profile。`Combo` 本身支持 `-p <profile>`，但**脚本还没有透传这个参数**——需要指定 profile 时给脚本加一个选项，不要绕开脚本手工执行，否则本机与 CI 的行为会分叉。
- **步骤 2 报缺文件或目录不存在**：`combo/dist` 是空的，或步骤 1 没跑过导致 `combosdk/` 不存在。
- **步骤 3 提示「StreamingAssets 路径异常，拒绝删除」**：这是 `rm -rf` 前的安全护栏，说明解析出的路径不在 `outputs/` 下，检查 `--distro` 与目录布局。
- **`未找到 aws CLI`** / **`以下端点没有可用的 S3 凭据`**：见上面「环境缺失时自动补齐」，帮用户装上/配好再重跑。用户明确说现在不想上传时才用 `--skip-step3` 跳过。
- **`端点 xxx 上传后 ETag 与本地 MD5 不一致`**：对象内容和本地对不上，重传。这条只在单次上传（ETag 是纯 MD5）时才会触发；`aws s3 cp` 对超过 8MB 的文件自动走分段上传，此时 ETag 形如 `xxx-3`，是各分片 MD5 再取的 MD5，**不等于文件 MD5，这属于正常现象，不是错误**，脚本会自动退化成比对大小与修改时间。文件名里的 hash 是内容 MD5 的中间 16 位，可用 `md5 -q <文件>` 核对。
- **`上传前后状态完全相同，写入未生效`**：远端对象没有被这次上传更新。文件名是内容哈希、同名对象大小必然相同，所以只比大小发现不了这种情况，才加了这个检查。
- **用户报告"文件没传上去"时，先确认他是怎么查的**：S3 后台的搜索框通常在 bucket 根目录做前缀匹配，而对象 key 是 `demo/<文件名>`，直接搜文件名会搜不到——这个坑实际发生过，白查了好几轮。最可靠的核实方式是匿名请求 `<DATA_CDN>/<文件名>` 看 HTTP 状态码和 `content-length`，一条 `curl -I --noproxy '*'` 就能定论。
- **对象在 S3 上、匿名也能下载，但游戏就是加载不了**：跟一个没动过的旧对象比 `content-type` 和 ETag 形态。注意 `wasmcode/` 里随包带了 wasm code 文件，游戏不从 CDN 取它，所以"wasm 正常、data 不正常"是正常现象，**不能据此判断 wasm 传成功了**——真正依赖 CDN 的只有 data 文件（`data-package/` 里只有一个空 `game.js`）。
- **`InvalidAccessKeyId` / `SignatureDoesNotMatch`**：凭据不对。前者常见于用户把控制台登录用户名当成了 Access Key ID，让他去控制台生成真正的 AccessKey。注意两个端点凭据不同，别把一边的填到另一边。
- **`Read timeout on endpoint URL`**：两个 S3 都在内网（域名解析到 `10.x`），而开发机常配着 `http_proxy` / `https_proxy` 走代理上外网，内网地址走代理必然超时。脚本已自动把端点主机加入 `NO_PROXY`，若仍超时，多半是没连公司内网/VPN 或服务端侧抖动——用 `nc -z <host> 443` 确认：TCP 通但 HTTP 无响应就不是本地配置问题，等一会儿重试。
- **`XAmzContentSHA256Mismatch`**：AWS CLI v2.23+ 默认给请求加校验和（`request_checksum_calculation=when_supported`），分段上传时带的 checksum trailer 这两个自建 S3 不认。脚本已在 `run_aws` 里固定设 `AWS_REQUEST_CHECKSUM_CALCULATION=when_required` 规避，**不要删掉这个设置**，否则大文件必然传不上去（两个文件都超过分段阈值）。
- **`DATA_CDN 的路径（...）与上传目标（...）不一致`**：只是告警不中断，但基本可以确定传错桶了——游戏运行时按 `game.js` 里的 `DATA_CDN` 下载，和 `s3.target` 指向的必须是同一份对象。确认 `s3.target` 或 Unity 导出时的 `DATA_CDN` 设置。
- **装配出的工程行为像旧版 SDK**：确认是否忘了加 `--local-sdk`；`combosdk/ComboSDK.json` 里的 `sdk_webgl_version` 反映的是线上配置的版本号，不会因为覆盖了本地产物而改变，不能用它判断当前跑的是哪个产物。

