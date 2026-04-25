using Godot;

namespace AttackLog.View;

/// <summary>
/// 面板拖拽处理器，管理面板的鼠标拖拽移动和屏幕边界限制
/// </summary>
public class PanelDragHandler
{
    /// <summary>是否正在拖拽</summary>
    private bool _dragging;

    /// <summary>拖拽偏移量</summary>
    private Vector2 _dragOffset;

    /// <summary>拖拽结束回调</summary>
    public event Action? DragEnded;

    /// <summary>
    /// 处理输入事件，实现面板拖拽和屏幕边界限制
    /// </summary>
    /// <param name="event">输入事件</param>
    /// <param name="panel">要拖拽的面板控件</param>
    public void HandleInput(InputEvent @event, Control panel)
    {
        if (@event is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == MouseButton.Left)
            {
                if (mb.Pressed)
                {
                    _dragging = true;
                    _dragOffset = mb.Position;
                }
                else
                {
                    if (_dragging)
                    {
                        _dragging = false;
                        DragEnded?.Invoke();
                    }
                }
            }
        }
        else if (@event is InputEventMouseMotion mm && _dragging)
        {
            var viewportSize = panel.GetViewport().GetVisibleRect().Size;
            var newPos = panel.Position + mm.Relative;

            newPos.X = Math.Clamp(newPos.X, 0, viewportSize.X - panel.Size.X);
            newPos.Y = Math.Clamp(newPos.Y, 0, viewportSize.Y - panel.Size.Y);

            panel.Position = newPos;
        }
    }

    /// <summary>
    /// 重置拖拽状态
    /// </summary>
    public void Reset()
    {
        _dragging = false;
    }
}
