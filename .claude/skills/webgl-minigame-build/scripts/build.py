#!/usr/bin/env python3
"""WebGL 小游戏工程一键装配脚本。

前置：Unity 导出 WebGL / 小游戏工程到 outputs/，本脚本不负责，目前只能人工在 Unity 中操作。
本脚本按下列顺序执行，步骤编号与实际执行顺序一致：
  步骤 1  在小游戏工程目录执行 Combo CLI，装配 Native 工程
  步骤 2  用本地 webgl SDK 仓库的编译产物替换 combosdk 目录中的产物（可选，仅本机调试使用）
  步骤 3  按 distro 做发布前处理：
            微信 - 用 aws s3 将 webgl 目录下的 wasm/data 大文件上传到 CDN
            抖音 - 清理 StreamingAssets 目录，只保留 game.js

本脚本面向本机开发调试。步骤 2 是可选的：不传 --local-sdk 就跳过它，
直接使用 Combo CLI 下载的线上发布产物。

用 Python 而不是 shell 实现，是为了 Windows / macOS / Linux 共用同一份代码：
两份实现必然会随时间分叉，而这里的每一处校验背后都对应一次实际踩过的坑。
"""

# 延迟注解求值：让 `str | None`、`list[dict]` 这类新式写法在 Python 3.8/3.9 上也能用。
# 目标是 Windows / macOS / Linux 通用，不能假设对方装的是最新版 Python。
from __future__ import annotations

import argparse
import hashlib
import json
import os
import re
import shutil
import subprocess
import sys
from pathlib import Path

# ---------------------------------------------------------------- 常量

# 步骤 3 上传 CDN 的默认参数。这是 Demo 工程自己的存储桶，不随机器变化，
# 因此作为默认值写在脚本里；换桶或换游戏时在配置文件里用 s3.target / s3.endpoints 覆盖。
DEFAULT_S3_TARGET = "s3://apps/demo"

# 大文件要同时上传到武汉和北京两个 S3，两边是各自独立的后台，凭据也不一样。
# 名字用于日志、凭据环境变量名以及与配置文件的合并。
# 武汉这个域名同时也是游戏运行时下载大文件的地址（见 game.js 里的 DATA_CDN）。
DEFAULT_S3_ENDPOINTS = [
    ("wuhan", "https://s3-wuhan.kingamer.cn"),
    ("beijing", "https://s3.shiyou.kingsoft.com"),
]

# 需要上传到 CDN 的大文件，按后缀匹配
CDN_FILE_SUFFIXES = (".webgl.wasm.code.unityweb.wasm.br", ".webgl.data.unityweb.bin.txt")

# distro -> (导出根目录名, 小游戏工程子目录名)
# 微信的导出目录与 Unity 导出面板的默认路径一致：Assets/Editor/Builder.cs 里按
# $"outputs/{selectedPlatform}".ToLower() 推导，微信平台名为 WeixinMiniGame。
DISTRO_LAYOUT = {
    "minigame_weixin": ("weixinminigame", "minigame"),
    "minigame_douyin": ("tt-minigame", "tt-minigame"),
}

DISTRO_ALIASES = {
    "wx": "minigame_weixin",
    "weixin": "minigame_weixin",
    "minigame_weixin": "minigame_weixin",
    "tt": "minigame_douyin",
    "douyin": "minigame_douyin",
    "minigame_douyin": "minigame_douyin",
}

# ---------------------------------------------------------------- 日志


def _supports_color() -> bool:
    """只在输出到终端时上色：重定向到文件时转义符会变成乱码。

    NO_COLOR 强制关闭、FORCE_COLOR 强制开启（https://no-color.org 的通行约定）。
    Windows 10 起的控制台支持 ANSI，但要先显式打开 VT 模式，否则只会打印出转义符本身。
    """
    if os.environ.get("NO_COLOR"):
        return False
    forced = bool(os.environ.get("FORCE_COLOR"))
    if not forced and not sys.stdout.isatty():
        return False
    if os.name == "nt":
        try:
            import ctypes

            kernel32 = ctypes.windll.kernel32
            # -11 = STD_OUTPUT_HANDLE, 0x0004 = ENABLE_VIRTUAL_TERMINAL_PROCESSING
            handle = kernel32.GetStdHandle(-11)
            mode = ctypes.c_uint32()
            if not kernel32.GetConsoleMode(handle, ctypes.byref(mode)):
                return forced
            kernel32.SetConsoleMode(handle, mode.value | 0x0004)
        except Exception:
            return forced
    return True


_COLOR = _supports_color()
_C_INFO = "\033[0;32m" if _COLOR else ""
_C_WARN = "\033[0;33m" if _COLOR else ""
_C_ERROR = "\033[0;31m" if _COLOR else ""
_C_OFF = "\033[0m" if _COLOR else ""


def log_info(msg: str) -> None:
    print(f"{_C_INFO}[INFO]{_C_OFF}  {msg}", flush=True)


def log_warn(msg: str) -> None:
    print(f"{_C_WARN}[WARN]{_C_OFF}  {msg}", flush=True)


def log_error(msg: str) -> None:
    print(f"{_C_ERROR}[ERROR]{_C_OFF} {msg}", file=sys.stderr, flush=True)


class BuildError(Exception):
    """流程中止。在 main 里统一捕获并打印，避免到处 sys.exit 打断调用栈。

    消息为空串表示详情已经用 log_error 逐行打印过了，main 不再重复输出一遍。
    """


def die(msg: str) -> "BuildError":
    return BuildError(msg)


# ---------------------------------------------------------------- 外部命令


def describe_bad_path(raw: str) -> str:
    """检查路径里是否混入了控制字符，是的话返回提示文案，否则返回空串。

    Windows 用户容易在 JSON 里直接写 "C:\\Users\\me\\webgl" 的单反斜杠形式。
    JSON 把反斜杠当转义符，其中 \t \n \b \f \r 都是合法转义，于是
    "C:\temp\new\build" 会被静默解析成 "C:<TAB>emp<LF>ew<BS>uild"——
    解析不报错，只是路径变成了乱码，最后表现为莫名其妙的「未找到 webgl SDK 仓库」。
    这里识别出这种情况并直接告诉用户怎么改，省得他对着路径看半天。
    """
    bad = {c for c in raw if ord(c) < 32}
    if not bad:
        return ""
    names = {"\t": "\\t", "\n": "\\n", "\r": "\\r", "\b": "\\b", "\f": "\\f"}
    shown = "、".join(sorted(names.get(c, repr(c)) for c in bad))
    return (
        f"路径里含有控制字符（{shown}），几乎可以确定是 JSON 中的反斜杠没有转义。\n"
        "  JSON 把反斜杠当转义符，Windows 路径要写成下面两种形式之一:\n"
        '      "webglSdkDir": "C:/Users/me/webgl"          （正斜杠，推荐）\n'
        '      "webglSdkDir": "C:\\\\Users\\\\me\\\\webgl"      （双反斜杠）'
    )


def is_within(path: Path, parent: Path) -> bool:
    """path 是否落在 parent 目录内。Path.is_relative_to 要 Python 3.9+，这里自己实现以兼容 3.8。"""
    try:
        path.relative_to(parent)
        return True
    except ValueError:
        return False


def find_executable(name: str) -> str | None:
    """查找可执行文件。Windows 上会自动匹配 .exe / .cmd 等后缀。"""
    return shutil.which(name)


def run(cmd, cwd=None, env=None, capture=False, check=True):
    """执行外部命令。

    统一走列表形式而不是字符串 + shell=True：路径里有空格或中文时不必操心引号，
    Windows 与 POSIX 的引号规则也不同，交给 subprocess 处理最稳妥。
    """
    merged = dict(os.environ)
    if env:
        merged.update(env)

    try:
        proc = subprocess.run(
            cmd,
            cwd=str(cwd) if cwd else None,
            env=merged,
            capture_output=capture,
            text=True,
            encoding="utf-8",
            errors="replace",
        )
    except FileNotFoundError as err:
        raise die(f"找不到可执行文件: {cmd[0]}（{err}）")

    if check and proc.returncode != 0:
        if capture and proc.stderr:
            sys.stderr.write(proc.stderr)
        raise die(f"命令执行失败（退出码 {proc.returncode}）: {' '.join(str(c) for c in cmd)}")
    return proc


# ---------------------------------------------------------------- 配置


class Config:
    """config.local.json 的读取封装。文件不存在时所有取值都回退到默认值。"""

    def __init__(self, path: Path):
        self.path = path
        self.data = {}
        if path.is_file():
            try:
                with path.open(encoding="utf-8") as f:
                    self.data = json.load(f)
            except ValueError as err:
                raise die(f"配置文件不是合法的 JSON: {path}\n  {err}")
            if not isinstance(self.data, dict):
                raise die(f"配置文件的顶层必须是对象: {path}")

    def get(self, dotted: str, default=None):
        """读取 a.b 形式的嵌套字段，缺失或为 null 时返回 default。"""
        value = self.data
        for part in dotted.split("."):
            if not isinstance(value, dict):
                return default
            value = value.get(part)
        return default if value is None else value

    def endpoints(self) -> list[dict]:
        """解析上传端点。

        配置文件里没有 s3.endpoints 时用脚本内置的默认端点；配了的话按 name 与默认端点合并，
        因此只想补凭据的话，配置里给出 name 和凭据即可，不必重复 endpointUrl。
        """
        default_urls = dict(DEFAULT_S3_ENDPOINTS)
        configured = self.get("s3.endpoints") or []
        if not isinstance(configured, list):
            raise die("s3.endpoints 必须是数组")

        if not configured:
            return [{"name": name, "endpointUrl": url} for name, url in DEFAULT_S3_ENDPOINTS]

        result = []
        for item in configured:
            if not isinstance(item, dict):
                raise die("s3.endpoints 的每一项都必须是对象")
            name = item.get("name") or ""
            if not name:
                raise die("s3.endpoints 的每一项都必须有 name")
            url = item.get("endpointUrl") or default_urls.get(name) or ""
            if not url:
                raise die(f"端点 {name} 缺少 endpointUrl，且不在内置默认端点中")
            merged = dict(item)
            merged["endpointUrl"] = url
            result.append(merged)
        return result

    def s3_target(self) -> str:
        target = str(self.get("s3.target") or DEFAULT_S3_TARGET).rstrip("/")
        if not target.startswith("s3://"):
            raise die(f"s3.target 必须是 s3:// 开头的路径，当前为: {target}")
        return target


# ---------------------------------------------------------------- 工具函数


def file_md5(path: Path) -> str:
    """分块读取，避免把十几 MB 的文件整个读进内存。"""
    h = hashlib.md5()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def content_type_of(name: str) -> str:
    """上传时显式指定 content-type，不依赖 aws 按扩展名的猜测，避免换 CLI 版本后行为漂移。

    取值与历史上传的对象保持一致（已实际比对过线上旧对象的响应头）。
    """
    if name.endswith(".bin.txt"):
        # data 文件虽是二进制，但历史对象一直用 text/plain，保持一致
        return "text/plain"
    # .wasm.br 是 brotli 压缩后的二进制，解压由微信的 Unity 插件在运行时完成，
    # 因此按普通二进制下发。注意绝不能带 Content-Encoding: br，
    # 否则会被 CDN/客户端提前解压，插件拿到的就不是它预期的压缩数据了。
    return "application/octet-stream"


def split_s3_target(target: str) -> tuple[str, str]:
    """s3://apps/demo -> ("apps", "demo")，没有前缀时第二项为空串。"""
    path = target[len("s3://"):]
    bucket, _, prefix = path.partition("/")
    return bucket, prefix


def credential_env_var(endpoint_name: str, suffix: str) -> str:
    """端点专属凭据的环境变量名，如 wuhan -> S3_WUHAN_ACCESS_KEY_ID。

    两个端点凭据不同，所以环境变量名里带端点名，不能用 aws 通用的 AWS_ACCESS_KEY_ID。
    """
    upper = re.sub(r"[^A-Z0-9]", "_", endpoint_name.upper())
    return f"S3_{upper}_{suffix}"


def resolve_credentials(endpoint: dict) -> tuple[str, str, str]:
    """解析某个端点的凭据，返回 (access_key, secret_key, 来源描述)。

    优先级：配置文件里的 accessKeyId/secretAccessKey > 端点专属环境变量 > 配置的 profile
    > aws 默认凭据链。只返回来源描述用于打印，绝不打印凭据本身。
    """
    name = endpoint["name"]
    ak = endpoint.get("accessKeyId") or ""
    sk = endpoint.get("secretAccessKey") or ""
    if ak and sk:
        return ak, sk, "配置文件"

    ak_var = credential_env_var(name, "ACCESS_KEY_ID")
    sk_var = credential_env_var(name, "SECRET_ACCESS_KEY")
    env_ak = os.environ.get(ak_var, "")
    env_sk = os.environ.get(sk_var, "")
    if env_ak and env_sk:
        return env_ak, env_sk, f"环境变量 {ak_var}"

    if endpoint.get("profile"):
        return "", "", f"profile {endpoint['profile']}"
    return "", "", "aws 默认凭据链"


# ---------------------------------------------------------------- 构建器


class Builder:
    def __init__(self, args, skill_dir: Path, repo_root: Path):
        self.args = args
        self.skill_dir = skill_dir
        self.repo_root = repo_root
        self.config = Config(skill_dir / "config.local.json")

        self.distro = DISTRO_ALIASES[args.distro]
        export_name, native_name = DISTRO_LAYOUT[self.distro]
        # EXPORT_DIR  Unity 导出根目录，下面包含 webgl/ 和小游戏工程目录
        # NATIVE_DIR  小游戏 Native 工程目录（含 game.js），Combo CLI 在此目录执行
        self.export_dir = repo_root / "outputs" / export_name
        self.native_dir = self.export_dir / native_name
        self.webgl_dir = self.export_dir / "webgl"
        self.combosdk_dir = self.native_dir / "combosdk"

        # webgl SDK 仓库路径，优先级：--sdk-dir > 配置文件的 webglSdkDir > 同级布局兜底
        # 原始字符串留一份，路径不存在时用它诊断（比如 Windows 上反斜杠没转义）
        self.sdk_dir: Path | None = None
        self.sdk_dir_raw = ""
        if args.local_sdk:
            raw = args.sdk_dir or self.config.get("webglSdkDir") or ""
            self.sdk_dir_raw = str(raw)
            if not raw:
                sibling = repo_root.parent.parent / "webgl"
                raw = str(sibling)
            self.sdk_dir = Path(str(raw).strip())

        # 当前端点的凭据，由 upload_to_endpoint 设置、_aws 使用。
        # 两个端点的凭据不同，因此每次切换端点都要重新解析，不能复用上一个端点的值。
        self._ep_ak = ""
        self._ep_sk = ""

    # ------------------------------------------------------------ 前置检查

    def check_prerequisites(self) -> None:
        if not self.export_dir.is_dir():
            raise die(f"未找到导出目录 {self.export_dir}，请先在 Unity 中导出 WebGL 工程（前置步骤）")
        if not self.native_dir.is_dir():
            raise die(f"未找到小游戏工程目录 {self.native_dir}，请先在 Unity 中导出 WebGL 工程（前置步骤）")
        # Combo CLI 依赖当前目录下的 game.js 来判定小游戏工程，缺失会导致装配出的工程无法运行
        if not (self.native_dir / "game.js").is_file():
            raise die(f"{self.native_dir} 下没有 game.js，该目录不是有效的小游戏工程")

        if self.args.setup and not find_executable("Combo"):
            raise die("未找到 Combo CLI，请先安装并配置 ~/.combo/combo.yaml")

        if self.args.local_sdk:
            if not self.sdk_dir or not self.sdk_dir.is_dir():
                log_error(f"未找到 webgl SDK 仓库: {self.sdk_dir}")
                hint = describe_bad_path(self.sdk_dir_raw)
                if hint:
                    for line in hint.split("\n"):
                        log_error("  " + line if not line.startswith(" ") else line)
                else:
                    log_error(f"请在 {self.config.path} 中配置 webglSdkDir，或使用 --sdk-dir 指定。内容形如:")
                    log_error('    {')
                    log_error('      "webglSdkDir": "/path/to/webgl"       # macOS / Linux')
                    log_error('      "webglSdkDir": "C:/Users/me/webgl"    # Windows，用正斜杠或双反斜杠')
                    log_error('    }')
                raise BuildError("")  # 详情已在上面逐行打印
            if not (self.sdk_dir / "package.json").is_file():
                raise die(f"{self.sdk_dir} 不是 webgl SDK 仓库（缺少 package.json）")
            if self.args.build_sdk and not find_executable("npm"):
                raise die("未找到 npm，无法编译 webgl SDK（可加 --no-build-sdk 跳过）")

        # 微信的步骤 3 依赖 aws CLI 与凭据，提前失败，避免装配和编译都跑完了才发现缺环境
        if self.args.step3 and self.distro == "minigame_weixin":
            if not find_executable("aws"):
                rel = self.skill_dir.relative_to(self.repo_root) if is_within(self.skill_dir, self.repo_root) else self.skill_dir
                raise die(
                    f"未找到 aws CLI，步骤 3 需要它上传 CDN。"
                    f"安装方法见 {rel}/references/troubleshooting.md"
                    f"（不想上传可加 --skip-step3 跳过该步骤）"
                )
            self.check_aws_credentials()

    def check_aws_credentials(self) -> None:
        """逐个端点确认凭据可解析，有端点凑不齐就直接失败并给出配置方式。

        好过跑到上传时才被 aws 拒绝——那时前两步已经白跑了。
        """
        missing, fallback = [], []
        aws_home = Path.home() / ".aws"
        has_aws_home = (aws_home / "credentials").is_file() or (aws_home / "config").is_file()

        for ep in self.config.endpoints():
            ak, sk, source = resolve_credentials(ep)
            if ak and sk:
                continue
            if source.startswith("profile"):
                continue
            # 没有端点专属配置，但机器上有 ~/.aws。放行交给 aws 的默认凭据链去试，
            # 但要提醒：两个端点是不同后台、凭据不同，同一份默认凭据不可能两边都对。
            if has_aws_home:
                fallback.append(ep["name"])
                continue
            missing.append(ep["name"])

        if fallback:
            log_warn(f"以下端点没有专属凭据配置，将回退到 aws 默认凭据链: {' '.join(fallback)}")
            log_warn("  两个端点凭据不同，默认凭据大概率只对其中一个有效，建议按端点分别配置")

        if not missing:
            return

        log_error(f"以下端点没有可用的 S3 凭据，无法执行步骤 3 的 CDN 上传: {' '.join(missing)}")
        log_error("两个端点是各自独立的后台，凭据不同，需要分别配置。任选一种方式:")
        log_error(f"  1. 在 {self.config.path} 的 s3.endpoints 中按端点配置 accessKeyId / secretAccessKey")
        log_error("  2. 导出端点专属环境变量，如 S3_WUHAN_ACCESS_KEY_ID / S3_WUHAN_SECRET_ACCESS_KEY")
        log_error("  3. 在 s3.endpoints 中按端点配置 profile，凭据写在 ~/.aws/credentials")
        log_error("  临时不想上传可加 --skip-step3 跳过步骤 3")
        raise BuildError("")  # 详情已在上面逐行打印

    # ------------------------------------------------------------ 步骤 1

    def run_setup(self) -> None:
        """在小游戏工程目录执行 Combo CLI，生成 combosdk 目录并改写 game.js。

        CLI 会按 distro 对应的 domains 自动下载并放置线上发布的 SDK 产物。
        """
        log_info(f"步骤 1：装配 Native 工程（distro={self.distro}）")
        log_info(f"  工作目录: {self.native_dir}")
        run(["Combo", "webgl", "setup", "--distro", self.distro], cwd=self.native_dir)
        log_info("步骤 1 完成")

    # ------------------------------------------------------------ 步骤 2

    @staticmethod
    def domain_to_js(domain: str) -> str:
        """domain -> 产物文件名的映射，映射关系参考 webgl 仓库的 combo/dist/move_ts.sh。

        返回空串表示该 domain 没有对应的独立产物（如 thinking_data、minigame_weixin_ads），跳过即可。

        权威依据是 webgl 仓库 combo/src/combo/manager/AnalyticsManager.ts 的 convertToModule：
        只有 solar_engine / tg_minigame / gravity_engine 会触发动态 import 对应的 chunk，
        其余 domain 一律走 Module.None，不加载任何独立产物。下面两处与 move_ts.sh 不一致，均以源码为准：

          minigame_weixin_ads  move_ts.sh 映射到 combosdk.tg.mg.js，但 convertToModule 不认这个 domain。
                               腾讯广告能力现已并入引力引擎（见 GravityEngineWeixinManager 中的
                               gravity_engine_minigame_weixin_tg_* 参数），由 combosdk.ge.mg.wx.js 承载，
                               独立的 tg.mg chunk 在当前配置下是死代码，因此这里不做映射。
          solar_engine         move_ts.sh 中抖音映射到 combosdk.se.mg.dy.js，但 dist 中并不存在该文件，
                               CLI 与 combosdk.import.js 实际使用的都是 combosdk.se.mg.wx.js，
                               故两个 distro 都映射到 wx 版本。将来 dist 真正产出 dy 版本时再改。
        """
        return {
            "minigame_weixin": "combosdk.weixin.js",
            "minigame_douyin": "combosdk.douyin.js",
            "tg_minigame": "combosdk.tg.mg.js",
            "gravity_engine": "combosdk.ge.mg.wx.js",
            "sentry": "combosdk.sentry.js",
            "solar_engine": "combosdk.se.mg.wx.js",
        }.get(domain, "")

    def sync_local_sdk(self) -> None:
        """用本地编译产物替换 combosdk 目录中的 SDK 产物。

        流程：读 ComboSDK.json 的 domains -> 按映射表算出需要哪些 js -> 校验 dist 中齐备
              -> 清掉 combosdk 下原有的产物（线上下载的那批）-> 从 dist 复制。
        先校验后清理，避免删完才发现 dist 缺文件、把工程留在损坏状态。
        combosdk.import.js 与 ComboSDK.json 由 Combo CLI 生成，dist 中没有，必须保留。
        """
        log_info("步骤 2：同步本地 SDK 产物")
        assert self.sdk_dir is not None
        dist_dir = self.sdk_dir / "combo" / "dist"

        if self.args.build_sdk:
            log_info(f"  编译 webgl SDK: {self.sdk_dir}")
            npm = find_executable("npm")
            run([npm, "run", "build"], cwd=self.sdk_dir)
        else:
            log_info("  已跳过编译，直接使用现有产物")

        if not dist_dir.is_dir():
            raise die(f"未找到编译产物目录 {dist_dir}，请先在 {self.sdk_dir} 执行 npm run build")
        if not self.combosdk_dir.is_dir():
            raise die(f"未找到 {self.combosdk_dir}，请先执行步骤 1（不要同时使用 --no-setup）")

        combosdk_json = self.combosdk_dir / "ComboSDK.json"
        if not combosdk_json.is_file():
            raise die(f"未找到 {combosdk_json}，请先执行步骤 1（不要同时使用 --no-setup）")

        version = "unknown"
        try:
            with (self.sdk_dir / "package.json").open(encoding="utf-8") as f:
                version = json.load(f).get("version", "unknown")
        except (OSError, ValueError):
            pass
        log_info(f"  本地 SDK 版本: {version}")
        log_info(f"  源目录: {dist_dir}")
        log_info(f"  目标目录: {self.combosdk_dir}")

        try:
            with combosdk_json.open(encoding="utf-8") as f:
                meta = json.load(f)
        except ValueError as err:
            raise die(f"解析失败: {combosdk_json}\n  {err}")

        json_distro = meta.get("distro", "")
        domains = meta.get("domains", []) or []
        log_info(f"  ComboSDK.json: distro={json_distro}, domains={' '.join(domains)}")

        # ComboSDK.json 的 distro 与本次 --distro 不一致，说明装配的工程和目标平台对不上
        if json_distro != self.distro:
            raise die(
                f"ComboSDK.json 中的 distro 是 {json_distro}，与本次的 {self.distro} 不符，"
                "请先执行步骤 1 重新装配"
            )

        # 按 domains 算出需要复制的产物，combosdk.js 是主入口，始终需要
        wanted = {"combosdk.js"}
        for domain in domains:
            js = self.domain_to_js(domain)
            if js:
                wanted.add(js)
            else:
                log_info(f"    domain {domain} 无对应产物，跳过")
        wanted_list = sorted(wanted)

        # 先校验 dist 中齐备，缺任何一个都不动目标目录
        missing = [js for js in wanted_list if not (dist_dir / js).is_file()]
        if missing:
            log_error("dist 中缺少以下产物，已中止（combosdk 目录未做任何改动）:")
            for js in missing:
                log_error(f"    - {js}")
            raise die(f"请确认 {dist_dir} 是完整的构建产物，或检查 domain -> js 映射表是否需要更新")

        # 清掉原有产物（线上下载的那批），保留 CLI 生成的 combosdk.import.js 与 ComboSDK.json
        removed = 0
        for old in self.combosdk_dir.glob("*.js"):
            if old.name == "combosdk.import.js":
                continue
            old.unlink()
            removed += 1
        log_info(f"    已清理原有产物 {removed} 个")

        for js in wanted_list:
            shutil.copyfile(dist_dir / js, self.combosdk_dir / js)
            log_info(f"    已复制: {js}")

        self.verify_imports(wanted_list)
        log_info(f"步骤 2 完成，共复制 {len(wanted_list)} 个文件")

    def verify_imports(self, copied: list[str]) -> None:
        """用 combosdk.import.js 校验本次同步的结果。

        combosdk.import.js 由 Combo CLI 生成，是「这个工程实际会加载哪些模块」的权威依据，
        拿它和映射表算出来的结果两头对：
          缺失（import.js 要、我们没复制）  -> 直接失败，否则小游戏运行时加载不到模块
          多余（我们复制了、import.js 不要）-> 只告警，文件不会被加载，但会占包体
        """
        import_js = self.combosdk_dir / "combosdk.import.js"
        if not import_js.is_file():
            return

        text = import_js.read_text(encoding="utf-8", errors="replace")
        required = [f"{m}.js" for m in re.findall(r"^require\('\./([^']*)'\)", text, re.MULTILINE)]

        missing = [js for js in required if not (self.combosdk_dir / js).is_file()]
        if missing:
            log_error("combosdk.import.js 引用了以下文件，但同步后它们并不存在:")
            for js in missing:
                log_error(f"    - {js}")
            raise die("domain -> js 映射表与 Combo CLI 的实际行为不一致，需要修正 domain_to_js")

        # combosdk.js 是主入口，由 game.js 引入而不出现在 import.js 中，不算多余
        unused = [js for js in copied if js != "combosdk.js" and js not in required]
        if unused:
            log_warn("  以下产物 combosdk.import.js 并未引用，不会被加载，但会占用包体:")
            for js in unused:
                log_warn(f"    - {js}")
            log_warn("  说明 domain -> js 映射表比 Combo CLI 多算了文件，可考虑修正 domain_to_js")

    # ------------------------------------------------------------ 步骤 3

    def run_step3(self) -> None:
        """步骤 3 在两个 distro 下是完全不同的处理，按 distro 分派。"""
        if self.distro == "minigame_weixin":
            self.upload_cdn()
        else:
            self.clean_streaming_assets()

    def upload_cdn(self) -> None:
        """微信小游戏的 wasm code 与 data 两个大文件超出小游戏包体限制，无法随包发布，
        必须托管在 CDN 上由游戏运行时远程加载，加载地址即 game.js 中的 DATA_CDN。
        两个 S3 后台各存一份，任一份缺失都可能导致部分玩家下载不到，因此必须全部上传成功。
        """
        log_info("步骤 3：上传大文件到 CDN")

        if not self.webgl_dir.is_dir():
            log_info(f"  未找到 {self.webgl_dir}，跳过")
            return

        files = sorted(
            p for p in self.webgl_dir.iterdir()
            if p.is_file() and p.name.endswith(CDN_FILE_SUFFIXES)
        )
        if not files:
            log_warn(f"  {self.webgl_dir} 下没有找到 wasm/data 大文件，跳过上传，请确认 Unity 导出是否完整")
            return

        target = self.config.s3_target()
        log_info(f"  目标路径: {target}")
        self.check_data_cdn(target)

        endpoints = self.config.endpoints()
        if not endpoints:
            raise die(f"没有可用的上传端点，请检查 {self.config.path} 中的 s3.endpoints")

        self.bypass_proxy(endpoints)

        for ep in endpoints:
            self.upload_to_endpoint(ep, target, files)

        log_info(f"步骤 3 完成，{len(files)} 个文件已上传到 {len(endpoints)} 个端点")

    @staticmethod
    def bypass_proxy(endpoints: list[dict]) -> None:
        """把所有端点的主机加入 NO_PROXY。

        两个 S3 的域名都解析到内网地址，而开发机上通常配了 http_proxy / https_proxy 走代理上外网，
        内网地址走代理连不通，表现为 aws 报 Read timeout——看起来像端点挂了，实际是代理吃掉了请求。
        只影响本进程内的 aws 调用，不改动用户的 shell 环境。
        """
        hosts = []
        for ep in endpoints:
            host = ep["endpointUrl"].split("://", 1)[-1].split("/", 1)[0]
            if host:
                hosts.append(host)
        if not hosts:
            return

        existing = os.environ.get("NO_PROXY", "")
        merged = ",".join(hosts + ([existing] if existing else []))
        os.environ["NO_PROXY"] = merged
        os.environ["no_proxy"] = merged
        log_info(f"  已为端点主机绕过代理: {','.join(hosts)}")

    def upload_to_endpoint(self, endpoint: dict, target: str, files: list[Path]) -> None:
        name = endpoint["name"]
        log_info(f"  端点 {name}: {endpoint['endpointUrl']}")

        ak, sk, source = resolve_credentials(endpoint)
        self._ep_ak, self._ep_sk = ak, sk
        log_info(f"    凭据来源: {source}")

        # aws 的公共参数：endpoint 必传（不是 AWS 官方服务），profile / region 按需配置
        common = ["--endpoint-url", endpoint["endpointUrl"]]
        if endpoint.get("profile"):
            common += ["--profile", str(endpoint["profile"])]
        if endpoint.get("region"):
            common += ["--region", str(endpoint["region"])]

        for path in files:
            ctype = content_type_of(path.name)
            size = path.stat().st_size
            log_info(f"    上传 {path.name}（{size} 字节，content-type={ctype}）")

            # 先记下远端此刻的状态，上传后再取一次做对比，用来确认这次写入确实落盘了
            before = self.head_object(target, path.name, common)

            cp_args = common + ["--content-type", ctype, "--only-show-errors"]
            # 桶本身若不是公共读，需要按对象授权，配置 s3.endpoints[].acl 即可（如 public-read）
            if endpoint.get("acl"):
                cp_args += ["--acl", str(endpoint["acl"])]

            proc = self._aws(["s3", "cp", str(path), f"{target}/{path.name}"] + cp_args, check=False)
            if proc.returncode != 0:
                raise die(f"上传到端点 {name} 失败: {path.name}")

            self.verify_uploaded(name, target, path, before, common)

    def _aws(self, args: list[str], capture=False, check=True):
        """执行 aws 命令，带上当前端点的凭据。

        AWS CLI v2.23 起默认对请求计算额外的校验和（request_checksum_calculation=when_supported），
        分段上传时会在请求里带上 checksum trailer，自建的 S3 实现不认，
        报 XAmzContentSHA256Mismatch 而失败（两个大文件都超过分段阈值，必然走分段上传）。
        这里退回到「仅在协议要求时计算」，两个 S3 才能正常接收。
        """
        env = {
            "AWS_REQUEST_CHECKSUM_CALCULATION": "when_required",
            "AWS_RESPONSE_CHECKSUM_VALIDATION": "when_required",
        }
        if self._ep_ak and self._ep_sk:
            env["AWS_ACCESS_KEY_ID"] = self._ep_ak
            env["AWS_SECRET_ACCESS_KEY"] = self._ep_sk
        return run(["aws"] + args, env=env, capture=capture, check=check)

    def head_object(self, target: str, name: str, common: list[str]) -> dict | None:
        """查询远端对象，返回 head-object 的 JSON；对象不存在或查询失败时返回 None。

        用 head-object 而不是 s3 ls：后者是前缀匹配，且拿不到修改时间与 ETag。
        """
        bucket, prefix = split_s3_target(target)
        key = f"{prefix}/{name}" if prefix else name
        proc = self._aws(
            ["s3api", "head-object", "--bucket", bucket, "--key", key] + common + ["--output", "json"],
            capture=True,
            check=False,
        )
        if proc.returncode != 0:
            return None
        try:
            return json.loads(proc.stdout)
        except ValueError:
            return None

    def verify_uploaded(self, endpoint_name: str, target: str, path: Path,
                        before: dict | None, common: list[str]) -> None:
        """上传后回读校验。

        只比大小是不够的：文件名本身就是内容哈希，同名对象的大小必然相同，
        远端若本来就有同名对象，写入即使没生效，大小也照样对得上。因此还要比对上传前后的对象状态。

        校验强度随 ETag 形态自适应：
          ETag 是纯 MD5（单次上传）时，直接与本地文件 MD5 比对，连内容损坏都能发现；
          ETag 形如 xxx-N 时说明走了分段上传（aws s3 cp 对超过 8MB 的文件会自动分段），
          此时 ETag 是各分片 MD5 拼接后再取的 MD5，无法与本地 MD5 直接比对，
          只能退化为比大小与状态变化。
        """
        after = self.head_object(target, path.name, common)
        if after is None:
            raise die(f"端点 {endpoint_name} 上传后查不到对象，写入未生效: {target}/{path.name}")

        remote_size = after.get("ContentLength")
        local_size = path.stat().st_size
        if remote_size != local_size:
            raise die(
                f"端点 {endpoint_name} 上传后大小不一致，"
                f"本地 {local_size} 字节，远端 {remote_size} 字节: {path.name}"
            )

        if before is not None and before == after:
            raise die(
                f"端点 {endpoint_name} 的 {path.name} 上传前后状态完全相同"
                f"（修改时间 {after.get('LastModified')}），写入未生效"
            )

        etag = str(after.get("ETag", "")).strip('"')
        if "-" in etag:
            log_info(f"      已上传并校验通过（分段上传，已比对大小与修改时间 {after.get('LastModified')}）")
        else:
            local_md5 = file_md5(path)
            if etag != local_md5:
                raise die(
                    f"端点 {endpoint_name} 上传后 ETag 与本地 MD5 不一致"
                    f"（ETag={etag}，本地 MD5={local_md5}）: {path.name}"
                )
            log_info("      已上传并校验通过（ETag 与本地 MD5 一致）")

    def check_data_cdn(self, target: str) -> None:
        """交叉校验：game.js 中的 DATA_CDN 是游戏运行时下载大文件的地址，它的路径部分应与上传目标一致。

        只告警不中断：域名映射规则由 CDN 侧决定，脚本无法确认，误判时不应挡住发布。
        """
        game_js = self.native_dir / "game.js"
        if not game_js.is_file():
            return

        text = game_js.read_text(encoding="utf-8", errors="replace")
        match = re.search(r"^\s*DATA_CDN:\s*'([^']*)'", text, re.MULTILINE)
        if not match:
            log_warn("  未能从 game.js 中解析出 DATA_CDN，跳过一致性校验")
            return

        data_cdn = match.group(1)
        log_info(f"  DATA_CDN: {data_cdn}")

        # https://host/apps/demo -> apps/demo，s3://apps/demo -> apps/demo
        cdn_path = data_cdn.split("://", 1)[-1].partition("/")[2].rstrip("/")
        target_path = target[len("s3://"):]
        if cdn_path != target_path:
            log_warn(f"  DATA_CDN 的路径（{cdn_path}）与上传目标（{target_path}）不一致，")
            log_warn("  游戏运行时可能下载不到刚上传的文件，请确认 s3.target 或 game.js 中的 DATA_CDN")

    def clean_streaming_assets(self) -> None:
        """抖音小游戏工程的 StreamingAssets 目录只保留 game.js，其余文件与子目录全部删除。"""
        log_info("步骤 3：清理 StreamingAssets")

        assets_dir = self.native_dir / "StreamingAssets"
        if not assets_dir.is_dir():
            log_info(f"  未找到 {assets_dir}，跳过")
            return

        # 下面会递归删除，删除前再确认一次目标路径落在预期的导出目录内
        outputs_dir = self.repo_root / "outputs"
        if not (is_within(assets_dir, outputs_dir) and assets_dir.name == "StreamingAssets"):
            raise die(f"StreamingAssets 路径异常，拒绝删除: {assets_dir}")

        if not (assets_dir / "game.js").is_file():
            log_warn(f"  {assets_dir} 下没有 game.js，请确认导出是否正常")

        removed = 0
        for entry in sorted(assets_dir.iterdir()):
            if entry.name == "game.js":
                continue
            if entry.is_dir() and not entry.is_symlink():
                shutil.rmtree(entry)
            else:
                entry.unlink()
            log_info(f"    已删除: {entry.name}")
            removed += 1

        log_info(f"步骤 3 完成，共删除 {removed} 项")

    # ------------------------------------------------------------ 主流程

    def run(self) -> None:
        log_info(f"distro={self.distro}, 导出目录={self.export_dir}")
        if self.config.path.is_file():
            log_info(f"已加载配置文件: {self.config.path}")
        self.check_prerequisites()

        if self.args.setup:
            self.run_setup()
        else:
            log_info("步骤 1：已跳过（--no-setup）")

        # 步骤 2 必须在步骤 1 之后：Combo CLI 装配时会用线上产物覆盖 combosdk 目录
        if self.args.local_sdk:
            self.sync_local_sdk()
        else:
            log_info("步骤 2：已跳过（未指定 --local-sdk，使用 Combo CLI 下载的线上产物）")

        if self.args.step3:
            self.run_step3()
        else:
            log_info("步骤 3：已跳过（--skip-step3）")

        log_info(f"全部完成，小游戏工程目录: {self.native_dir}")


# ---------------------------------------------------------------- 入口


def find_repo_root(script_dir: Path) -> Path:
    """仓库根目录：优先问 git，拿不到再按脚本位置回退推导
    （当前位置是 <repo>/.claude/skills/webgl-minigame-build/scripts/，上溯四层）。
    走 git 是为了让脚本换目录时不必跟着改层级。
    """
    git = shutil.which("git")
    if git:
        try:
            proc = subprocess.run(
                [git, "-C", str(script_dir), "rev-parse", "--show-toplevel"],
                capture_output=True, text=True, encoding="utf-8",
            )
            if proc.returncode == 0 and proc.stdout.strip():
                return Path(proc.stdout.strip())
        except OSError:
            pass
    return script_dir.parent.parent.parent.parent


def parse_args(argv=None):
    parser = argparse.ArgumentParser(
        prog="build.py",
        description="WebGL 小游戏工程一键装配（微信 / 抖音）",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""\
配置文件:
  与本脚本同级的上一层目录下的 config.local.json，即
  .claude/skills/webgl-minigame-build/config.local.json
  本机配置，不纳入版本管理，各机器自行创建:

      {
        "webglSdkDir": "/path/to/webgl",
        "s3": {
          "target": "s3://apps/demo",
          "endpoints": [
            { "name": "wuhan",   "accessKeyId": "...", "secretAccessKey": "..." },
            { "name": "beijing", "accessKeyId": "...", "secretAccessKey": "..." }
          ]
        }
      }

  步骤 3 会把大文件同时上传到武汉、北京两个 S3，两边凭据不同，需要分别配置。
  target 与两个端点的 endpointUrl 都有内置默认值，通常只需要填凭据。
  不想把凭据写进文件时，也可以改用端点专属环境变量 S3_WUHAN_* / S3_BEIJING_* 。

示例:
  # 本机调试：装配微信小游戏工程，并用本地编译的 SDK 产物覆盖
  python3 build.py --distro wx --local-sdk

  # 本机快速迭代：只重新编译并同步 SDK 产物，不重新装配
  python3 build.py --distro wx --local-sdk --no-setup

  # 只装配，使用线上发布的 SDK 产物（不覆盖成本地编译的）
  python3 build.py --distro minigame_weixin
""",
    )
    parser.add_argument(
        "--distro", required=True, choices=sorted(DISTRO_ALIASES),
        metavar="<distro>",
        help="目标发行版: minigame_weixin (别名 weixin / wx) / minigame_douyin (别名 douyin / tt)",
    )
    parser.add_argument(
        "--local-sdk", action="store_true",
        help="执行步骤 2：用本地 webgl SDK 产物覆盖 combosdk 目录。不传则使用 Combo CLI 下载的线上产物",
    )
    parser.add_argument(
        "--sdk-dir", metavar="<path>",
        help="webgl SDK 仓库路径，隐含 --local-sdk。不传时取配置文件里的 webglSdkDir",
    )
    parser.add_argument(
        "--no-build-sdk", dest="build_sdk", action="store_false",
        help="步骤 2 中跳过 npm run build，直接使用现有的 combo/dist 产物",
    )
    parser.add_argument(
        "--no-setup", dest="setup", action="store_false",
        help="跳过步骤 1（Combo CLI 装配）。用于只重新同步本地 SDK 产物的场景",
    )
    parser.add_argument(
        "--skip-step3", dest="step3", action="store_false",
        help="跳过步骤 3（微信为 CDN 上传，抖音为清理 StreamingAssets）",
    )

    args = parser.parse_args(argv)
    if args.sdk_dir:
        args.local_sdk = True
    return args


def main(argv=None) -> int:
    args = parse_args(argv)
    script_dir = Path(__file__).resolve().parent
    skill_dir = script_dir.parent
    repo_root = find_repo_root(script_dir)

    try:
        Builder(args, skill_dir, repo_root).run()
    except BuildError as err:
        msg = str(err)
        if msg:
            log_error(msg)
        return 1
    except KeyboardInterrupt:
        log_error("已中断")
        return 130
    return 0


if __name__ == "__main__":
    if sys.version_info < (3, 8):
        sys.exit("需要 Python 3.8 或更高版本，当前为 %d.%d" % sys.version_info[:2])
    sys.exit(main())
