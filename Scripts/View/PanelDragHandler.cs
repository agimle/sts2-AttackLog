using Godot;

namespace AttackLog.View;

public class PanelDragHandler
{
    private bool _dragging;
    private Vector2 _dragOffset;

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
                    _dragging = false;
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

    public void Reset()
    {
        _dragging = false;
    }
}
