using System;
using System.Globalization;
using UnityEngine;

namespace UnityModManagerNet
{
    /// <summary>
    /// Analog of Vector2Int for UMM [0.29.0]
    /// </summary>
    public struct Vector2i : IEquatable<Vector2i>, IFormattable
    {
        private int m_X;

        private int m_Y;

        private static readonly Vector2i s_Zero = new Vector2i(0, 0);

        private static readonly Vector2i s_One = new Vector2i(1, 1);

        private static readonly Vector2i s_Up = new Vector2i(0, 1);

        private static readonly Vector2i s_Down = new Vector2i(0, -1);

        private static readonly Vector2i s_Left = new Vector2i(-1, 0);

        private static readonly Vector2i s_Right = new Vector2i(1, 0);

        public int x
        {
            get
            {
                return m_X;
            }
            set
            {
                m_X = value;
            }
        }

        public int y
        {
            get
            {
                return m_Y;
            }
            set
            {
                m_Y = value;
            }
        }

        public int this[int index]
        {
            get
            {
                return index switch
                {
                    0 => x,
                    1 => y,
                    _ => throw new IndexOutOfRangeException($"Invalid Vector2i index addressed: {index}!"),
                };
            }
            set
            {
                switch (index)
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException($"Invalid Vector2i index addressed: {index}!");
                }
            }
        }

        public float magnitude
        {
            get
            {
                return Mathf.Sqrt(x * x + y * y);
            }
        }

        public int sqrMagnitude
        {
            get
            {
                return x * x + y * y;
            }
        }

        public static Vector2i zero
        {
            get
            {
                return s_Zero;
            }
        }

        public static Vector2i one
        {
            get
            {
                return s_One;
            }
        }

        public static Vector2i up
        {
            get
            {
                return s_Up;
            }
        }

        public static Vector2i down
        {
            get
            {
                return s_Down;
            }
        }

        public static Vector2i left
        {
            get
            {
                return s_Left;
            }
        }

        public static Vector2i right
        {
            get
            {
                return s_Right;
            }
        }

        public Vector2i(int x, int y)
        {
            m_X = x;
            m_Y = y;
        }

        public void Set(int x, int y)
        {
            m_X = x;
            m_Y = y;
        }

        public static float Distance(Vector2i a, Vector2i b)
        {
            float num = a.x - b.x;
            float num2 = a.y - b.y;
            return (float)Math.Sqrt(num * num + num2 * num2);
        }

        public static Vector2i Min(Vector2i lhs, Vector2i rhs)
        {
            return new Vector2i(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y));
        }

        public static Vector2i Max(Vector2i lhs, Vector2i rhs)
        {
            return new Vector2i(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y));
        }

        public static Vector2i Scale(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.x * b.x, a.y * b.y);
        }

        public void Scale(Vector2i scale)
        {
            x *= scale.x;
            y *= scale.y;
        }

        public void Clamp(Vector2i min, Vector2i max)
        {
            x = Math.Max(min.x, x);
            x = Math.Min(max.x, x);
            y = Math.Max(min.y, y);
            y = Math.Min(max.y, y);
        }

        public static implicit operator Vector2(Vector2i v)
        {
            return new Vector2(v.x, v.y);
        }

        public static implicit operator Vector3(Vector2i v)
        {
            return new Vector3(v.x, v.y, 0);
        }

        public static explicit operator Vector3i(Vector2i v)
        {
            return new Vector3i(v.x, v.y, 0);
        }

        public static Vector2i FloorToInt(Vector2 v)
        {
            return new Vector2i(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y));
        }

        public static Vector2i CeilToInt(Vector2 v)
        {
            return new Vector2i(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y));
        }

        public static Vector2i RoundToInt(Vector2 v)
        {
            return new Vector2i(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
        }

        public static Vector2i operator -(Vector2i v)
        {
            return new Vector2i(-v.x, -v.y);
        }

        public static Vector2i operator +(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.x + b.x, a.y + b.y);
        }

        public static Vector2i operator -(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.x - b.x, a.y - b.y);
        }

        public static Vector2i operator *(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.x * b.x, a.y * b.y);
        }

        public static Vector2i operator *(int a, Vector2i b)
        {
            return new Vector2i(a * b.x, a * b.y);
        }

        public static Vector2i operator *(Vector2i a, int b)
        {
            return new Vector2i(a.x * b, a.y * b);
        }

        public static Vector2i operator /(Vector2i a, int b)
        {
            return new Vector2i(a.x / b, a.y / b);
        }

        public static bool operator ==(Vector2i lhs, Vector2i rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y;
        }

        public static bool operator !=(Vector2i lhs, Vector2i rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector2i))
            {
                return false;
            }

            return Equals((Vector2i)other);
        }

        public bool Equals(Vector2i other)
        {
            return x == other.x && y == other.y;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2);
        }

        public override string ToString()
        {
            return ToString(null, null);
        }

        public string ToString(string format)
        {
            return ToString(format, null);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (formatProvider == null)
            {
                formatProvider = CultureInfo.InvariantCulture.NumberFormat;
            }

            return string.Format("({0}, {1})", x.ToString(format, formatProvider), y.ToString(format, formatProvider));
        }
    }

    /// <summary>
    /// Analog of Vector3Int for UMM [0.29.0]
    /// </summary>
    public struct Vector3i : IEquatable<Vector3i>, IFormattable
    {
        private int m_X;

        private int m_Y;

        private int m_Z;

        private static readonly Vector3i s_Zero = new Vector3i(0, 0, 0);

        private static readonly Vector3i s_One = new Vector3i(1, 1, 1);

        private static readonly Vector3i s_Up = new Vector3i(0, 1, 0);

        private static readonly Vector3i s_Down = new Vector3i(0, -1, 0);

        private static readonly Vector3i s_Left = new Vector3i(-1, 0, 0);

        private static readonly Vector3i s_Right = new Vector3i(1, 0, 0);

        private static readonly Vector3i s_Forward = new Vector3i(0, 0, 1);

        private static readonly Vector3i s_Back = new Vector3i(0, 0, -1);

        public int x
        {
            get
            {
                return m_X;
            }
            set
            {
                m_X = value;
            }
        }

        public int y
        {
            get
            {
                return m_Y;
            }
            set
            {
                m_Y = value;
            }
        }

        public int z
        {
            get
            {
                return m_Z;
            }
            set
            {
                m_Z = value;
            }
        }

        public int this[int index]
        {
            get
            {
                return index switch
                {
                    0 => x,
                    1 => y,
                    2 => z,
                    _ => throw new IndexOutOfRangeException(string.Format("Invalid Vector3i index addressed: {0}!", index)),
                };
            }
            set
            {
                switch (index)
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    case 2:
                        z = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException(string.Format("Invalid Vector3i index addressed: {0}!", index));
                }
            }
        }

        public float magnitude
        {
            get
            {
                return Mathf.Sqrt(x * x + y * y + z * z);
            }
        }

        public int sqrMagnitude
        {
            get
            {
                return x * x + y * y + z * z;
            }
        }

        public static Vector3i zero
        {
            get
            {
                return s_Zero;
            }
        }

        public static Vector3i one
        {
            get
            {
                return s_One;
            }
        }

        public static Vector3i up
        {
            get
            {
                return s_Up;
            }
        }

        public static Vector3i down
        {
            get
            {
                return s_Down;
            }
        }

        public static Vector3i left
        {
            get
            {
                return s_Left;
            }
        }

        public static Vector3i right
        {
            get
            {
                return s_Right;
            }
        }

        public static Vector3i forward
        {
            get
            {
                return s_Forward;
            }
        }

        public static Vector3i back
        {
            get
            {
                return s_Back;
            }
        }

        public Vector3i(int x, int y)
        {
            m_X = x;
            m_Y = y;
            m_Z = 0;
        }

        public Vector3i(int x, int y, int z)
        {
            m_X = x;
            m_Y = y;
            m_Z = z;
        }

        public void Set(int x, int y, int z)
        {
            m_X = x;
            m_Y = y;
            m_Z = z;
        }

        public static float Distance(Vector3i a, Vector3i b)
        {
            return (a - b).magnitude;
        }

        public static Vector3i Min(Vector3i lhs, Vector3i rhs)
        {
            return new Vector3i(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y), Mathf.Min(lhs.z, rhs.z));
        }

        public static Vector3i Max(Vector3i lhs, Vector3i rhs)
        {
            return new Vector3i(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y), Mathf.Max(lhs.z, rhs.z));
        }

        public static Vector3i Scale(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public void Scale(Vector3i scale)
        {
            x *= scale.x;
            y *= scale.y;
            z *= scale.z;
        }

        public void Clamp(Vector3i min, Vector3i max)
        {
            x = Math.Max(min.x, x);
            x = Math.Min(max.x, x);
            y = Math.Max(min.y, y);
            y = Math.Min(max.y, y);
            z = Math.Max(min.z, z);
            z = Math.Min(max.z, z);
        }

        public static implicit operator Vector2(Vector3i v)
        {
            return new Vector2(v.x, v.y);
        }

        public static implicit operator Vector3(Vector3i v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static explicit operator Vector2i(Vector3i v)
        {
            return new Vector2i(v.x, v.y);
        }

        public static Vector3i FloorToInt(Vector3 v)
        {
            return new Vector3i(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), Mathf.FloorToInt(v.z));
        }

        public static Vector3i CeilToInt(Vector3 v)
        {
            return new Vector3i(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y), Mathf.CeilToInt(v.z));
        }

        public static Vector3i RoundToInt(Vector3 v)
        {
            return new Vector3i(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
        }

        public static Vector3i operator +(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3i operator -(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3i operator *(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static Vector3i operator -(Vector3i a)
        {
            return new Vector3i(-a.x, -a.y, -a.z);
        }

        public static Vector3i operator *(Vector3i a, int b)
        {
            return new Vector3i(a.x * b, a.y * b, a.z * b);
        }

        public static Vector3i operator *(int a, Vector3i b)
        {
            return new Vector3i(a * b.x, a * b.y, a * b.z);
        }

        public static Vector3i operator /(Vector3i a, int b)
        {
            return new Vector3i(a.x / b, a.y / b, a.z / b);
        }

        public static bool operator ==(Vector3i lhs, Vector3i rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
        }

        public static bool operator !=(Vector3i lhs, Vector3i rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector3i))
            {
                return false;
            }

            return Equals((Vector3i)other);
        }

        public bool Equals(Vector3i other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            int hashCode = y.GetHashCode();
            int hashCode2 = z.GetHashCode();
            return x.GetHashCode() ^ (hashCode << 4) ^ (hashCode >> 28) ^ (hashCode2 >> 4) ^ (hashCode2 << 28);
        }

        public override string ToString()
        {
            return ToString(null, null);
        }

        public string ToString(string format)
        {
            return ToString(format, null);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (formatProvider == null)
            {
                formatProvider = CultureInfo.InvariantCulture.NumberFormat;
            }

            return string.Format("({0}, {1}, {2})", x.ToString(format, formatProvider), y.ToString(format, formatProvider), z.ToString(format, formatProvider));
        }
    }
}
