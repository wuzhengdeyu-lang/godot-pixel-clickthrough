# Godot 点击穿透透明窗口管理器 (C#)

<p align="center">
  <a href="README.md">English</a> | 
  <a href="README_zh.md">中文</a>
</p>

**专为 Windows 上的桌面宠物、覆盖层或始终置顶的透明 Godot 应用程序提供智能的像素级点击穿透功能。**

这个自动加载单例（`ClickThroughManager.cs`）能让您的 Godot 4.x 游戏/窗口实现：

- **完全透明的背景**（仅绘制出的像素可见）
- 在精灵/宠物/角色的 Alpha 值为**零或非常低**（≤ 可配置的阈值）时实现**点击穿透**
- 仅在**可见/不透明**的部分保持**可点击/可交互**（像素级精准）
- 当鼠标离开窗口时自动恢复可点击性
- 通过鼠标移动检测和状态缓存最小化性能开销

### 核心技术
使用一个微小的隐藏 `SubViewport` + `Camera2D` 作为“光学探针”，每帧采样鼠标光标下的确切像素——包括着色器效果、动画、粒子等。
然后通过 `user32.dll` 动态切换 `WS_EX_TRANSPARENT` 标志，以实现真正的逐像素点击穿透行为。

### 功能特性
- 可调节的 Alpha 阈值（`AlphaThreshold`）
- 可选的性能开关（`EnableCheck`）
- 仅在点击状态实际改变时才调用耗时的 WinAPI
- 非常适用于**桌面宠物**、浮动小部件、屏幕覆盖层、动态壁纸等场景

### 运行要求
- Godot 4.x (Mono / C# 支持)
- **仅限 Windows** (使用 `user32.dll`)
- 项目设置：
  - `Display > Window > Transparent Background` = On (开启)
  - `Display > Window > Per Pixel Transparency` = Enabled (启用，推荐)
  - 最好使用 **Compatibility** (兼容) 渲染器

### 快速开始
1. 将 `ClickThroughManager.cs` 添加为自动加载（AutoLoad），或者在您的自动加载场景中创建一个 Node2D 节点并附加该脚本。
2. 运行 → 享受一个只有可见像素块才会阻挡鼠标事件的窗口。
