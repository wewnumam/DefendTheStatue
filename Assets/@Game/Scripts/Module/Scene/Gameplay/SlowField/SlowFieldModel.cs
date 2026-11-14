using Agate.MVC.Base;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectTA.Module.SlowField
{
    /// <summary>
    /// One circular slow zone. SlowFactor multiplies movement speed: 0.2 for the sword time slash,
    /// 0.4 for a triggered spike trap.
    /// </summary>
    public readonly struct SlowField
    {
        public Vector2 Center { get; }
        public float Radius { get; }
        public float SlowFactor { get; }
        public float ExpiryTime { get; }

        public SlowField(Vector2 center, float radius, float slowFactor, float expiryTime)
        {
            Center = center;
            Radius = radius;
            SlowFactor = slowFactor;
            ExpiryTime = expiryTime;
        }
    }

    /// <summary>
    /// Every active slow zone in one place, so the time slash and the spike traps share one query
    /// instead of each entity tracking its own inTimeSlash/slowmoTimer flags like the reference.
    /// Overlapping fields multiply, matching the reference game applying both.
    /// </summary>
    public class SlowFieldModel : BaseModel
    {
        private readonly List<SlowField> _fields = new List<SlowField>();

        public IReadOnlyList<SlowField> Fields => _fields;

        public void Add(Vector2 center, float radius, float slowFactor, float currentTime, float duration)
        {
            _fields.Add(new SlowField(center, radius, slowFactor, currentTime + duration));
            SetDataAsDirty();
        }

        /// <summary>Returns how many fields were dropped. Call once per frame.</summary>
        public int RemoveExpired(float currentTime)
        {
            int removed = 0;
            for (int i = _fields.Count - 1; i >= 0; i--)
            {
                if (_fields[i].ExpiryTime > currentTime)
                {
                    continue;
                }

                _fields.RemoveAt(i);
                removed++;
            }

            if (removed > 0)
            {
                SetDataAsDirty();
            }

            return removed;
        }

        public void Clear()
        {
            _fields.Clear();
            SetDataAsDirty();
        }

        public bool IsInsideAny(Vector2 position, float currentTime)
        {
            for (int i = 0; i < _fields.Count; i++)
            {
                SlowField field = _fields[i];
                if (field.ExpiryTime <= currentTime)
                {
                    continue;
                }

                if ((position - field.Center).sqrMagnitude <= field.Radius * field.Radius)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>1.0 when unaffected, otherwise the product of every field covering the point.</summary>
        public float GetSpeedMultiplierAt(Vector2 position, float currentTime)
        {
            float multiplier = 1f;

            for (int i = 0; i < _fields.Count; i++)
            {
                SlowField field = _fields[i];
                if (field.ExpiryTime <= currentTime)
                {
                    continue;
                }

                if ((position - field.Center).sqrMagnitude <= field.Radius * field.Radius)
                {
                    multiplier *= field.SlowFactor;
                }
            }

            return multiplier;
        }
    }
}
