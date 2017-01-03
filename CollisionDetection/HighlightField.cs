using Microsoft.Xna.Framework;

namespace CollisionDetection
{
    /// <summary>
    /// Indicates how to highlight a collision field
    /// </summary>
    public struct HighlightField
    {
        public Color Color;
        public CollisionField Field;
        public float Duration;

        /// <summary>
        /// Indicates how to highlight a collision field
        /// </summary>
        /// <param name="field">Collision field</param>
        /// <param name="color">Highlight color</param>
        /// <param name="duration">Highlight duration</param>
        public HighlightField(CollisionField field, Color color, float duration)
        {
            Color = color;
            Field = field;
            Duration = duration;
        }
    }
}
