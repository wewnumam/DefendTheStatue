using Agate.MVC.Base;

namespace ProjectTA.Module.CameraRig
{
    /// <summary>
    /// Two numbers: the size the camera sits at while running and the size it pulls back to while the
    /// skill upgrade panel is open. Smoothing belongs to the view; this only knows where to aim.
    /// </summary>
    public class CameraRigModel : BaseModel
    {
        public const float ZoomFraction = 0.85f;

        /// <summary>GDD §11: teleport select mode frames the map at 0.45x magnification (a pull-back).</summary>
        public const float SelectZoomFraction = 1f / 0.45f;

        public float DefaultSize { get; private set; }
        public float TargetSize { get; private set; }
        public bool IsZoomed { get; private set; }
        public bool IsSelectMode { get; private set; }

        public void SetSelectMode(bool isSelectMode)
        {
            IsSelectMode = isSelectMode;
            TargetSize = DefaultSize * (isSelectMode ? SelectZoomFraction : IsZoomed ? ZoomFraction : 1f);
            SetDataAsDirty();
        }

        public void SetDefaultSize(float size)
        {
            DefaultSize = size;
            TargetSize = IsZoomed ? size * ZoomFraction : size;
            SetDataAsDirty();
        }

        public void SetZoomed(bool isZoomed)
        {
            IsZoomed = isZoomed;
            TargetSize = DefaultSize * (isZoomed ? ZoomFraction : 1f);
            SetDataAsDirty();
        }
    }
}
