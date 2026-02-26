using Godot;
using System;
using System.Runtime.InteropServices;

namespace Project.AutoLoads;
public partial class ClickThroughManager : Node2D
{
    public static ClickThroughManager Instance { get; private set; }

    #region Windows API
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNeoLong);

    private const int GWL_EXSTYLE = -20;
    private const uint WS_EX_LAYERED = 0x00080000;
    private const uint WS_EX_TRANSPARENT = 0x00000020;
    #endregion

    [Export] public float AlphaThreshold { get; set; } = 0.05f; // 阈值

    // 性能优化控制
    [Export] public bool EnableCheck { get; set; } = true;

    // 核心组件
    private SubViewport _probeViewport;
    private Camera2D _probeCamera;
    private IntPtr _hWnd;
    private bool _isClickable = true;

    // 缓存上一帧的鼠标位置，减少不必要的处理
    private Vector2 _lastMousePos;
    private bool _mouseMoved = false;

    public override void _EnterTree()
    {
        if (Instance != null && this != Instance) { QueueFree(); return; }
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public override void _Ready()
    {
        _hWnd = GetActiveWindow();

        // 1. 初始化 Windows 窗口（分层 + 透明背景）
        SetWindowLong(_hWnd, GWL_EXSTYLE, WS_EX_LAYERED);
        GetTree().Root.TransparentBg = true;

        // 2. 构建“光学探针” (SubViewport)
        SetupProbeSystem();
    }

    private void SetupProbeSystem()
    {
        // 创建一个不可见的 SubViewport
        _probeViewport = new SubViewport();
        _probeViewport.Name = "PixelProbe";
        _probeViewport.Size = new Vector2I(64, 64);// 理论上越小越好，但太小会失效。64比较稳定。 
        _probeViewport.TransparentBg = true;
        _probeViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Always; // 持续更新

        // 关键：共享主视口的 World2D，这样它能看到主场景里的所有物体（包括Shader效果）
        _probeViewport.World2D = GetTree().Root.World2D;

        // 禁用音频，避免重复播放
        _probeViewport.AudioListenerEnable2D = false;

        // 创建探针摄像机
        _probeCamera = new Camera2D();
        _probeCamera.Zoom = Vector2.One; // 1:1 比例

        // 将探针添加到场景树（不需要要在视觉上显示，挂载在当前节点下即可）
        AddChild(_probeViewport);
        _probeViewport.AddChild(_probeCamera);
    }

    public override void _Process(double delta)
    {
        if (!EnableCheck) return;

        if (!IsMouseInsideWindow())
        {
            // 如果鼠标移出窗口，强制恢复“穿透”状态，防止阻碍用户操作其他程序
            ApplyClickState(false);
            return;
        }

        // 获取全局鼠标位置
        Vector2 globalMousePos = GetGlobalMousePosition();

        // 只有鼠标移动了，或者画面一直在变动（动画），才需要重新检测
        // 考虑到桌宠一直在动，我们每一帧都移动摄像机
        _probeCamera.GlobalPosition = globalMousePos;

        // 延迟一帧读取（因为Godot渲染是延迟的，移动摄像机后，下一帧画面才更新）
        CheckPixelAlpha();
    }

    private void CheckPixelAlpha()
    {
        var tex = _probeViewport.GetTexture();
        var img = tex.GetImage();

        if (img == null || img.GetSize().X <= 0 || img.GetSize().Y <= 0)
            return;

        // 计算中心像素坐标（整数，向下取整）
        Vector2I size = img.GetSize();
        Vector2I center = size / 2;  // 对于奇数宽高正好是正中心，偶数则是偏左上一点的中心

        Color pixelColor = img.GetPixel(center.X, center.Y);

        bool isOpaque = pixelColor.A > AlphaThreshold;
        ApplyClickState(isOpaque);
    }

    private void ApplyClickState(bool clickable)
    {
        // 状态去重，防止频繁调用昂贵的 Windows API
        if (_isClickable == clickable) return;

        _isClickable = clickable;

        if (clickable)
        {
            // 可点击：移除穿透标志
            SetWindowLong(_hWnd, GWL_EXSTYLE, WS_EX_LAYERED);
        }
        else
        {
            // 不可点击：添加穿透标志
            SetWindowLong(_hWnd, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT);
        }
    }

    /// <summary>
    /// 判断鼠标是否在当前 Godot 窗口内
    /// </summary>
    private bool IsMouseInsideWindow()
    {
        // 获取窗口在屏幕上的矩形区域 (Screen Rect)
        Rect2I windowRect = new Rect2I(DisplayServer.WindowGetPosition(), DisplayServer.WindowGetSize());

        // 获取鼠标在屏幕上的全局绝对坐标
        Vector2I mouseScreenPos = (Vector2I)DisplayServer.MouseGetPosition();

        return windowRect.HasPoint(mouseScreenPos);
    }
}
