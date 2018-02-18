
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Game_Jam
{
    public struct Rect
    {
        public Vector2 min;
        public Vector2 max;
       
        public static Rect Zero { get => new Rect(Vector2.Zero, Vector2.Zero); }
        public Vector2 Center { get => (min + max) / 2; }
        public Vector2 Dim { get => (max - min); }
        public Vector2 TopLeft { get => new Vector2(min.X, max.Y); }
        public Vector2 BottomLeft { get => new Vector2(min.X, min.Y); }
        public Vector2 TopRight { get => new Vector2(max.X, max.Y); }
        public Vector2 BottomRight { get => new Vector2(max.X, min.Y); }
        public Rect(float x, float y, float w, float h)
        {
            min = new Vector2(x, y);
            max = new Vector2(x + w, y + h);
        }

        public Rect(Vector2 Min, Vector2 Dim)
        {
            min = Min;
            max = Min + Dim;
        }
        public void Inflate(float i)
        {
            min = min - Vector2.One * i;
            max = max + Vector2.One * i;
        }
        public void Inflate(float i, float j)
        {
            min = min - new Vector2(i, j);
            max = max + new Vector2(i, j);
        }
        public Rect GetInflatedRect(float i)
        {
            Rect Res = this;
            Res.min = min - Vector2.One * i;
            Res.max = max + Vector2.One * i;
            return Res;
        }

        public bool Contains(Vector2 pos)
        {
            return (pos.X >= min.X) && (pos.Y >= min.Y) && (pos.X <= max.X) && (pos.Y <= max.Y);
        }

        public bool Contains(Rect rect)
        {
            return (rect.min.X >= min.X) && (rect.min.Y >= min.Y) && (rect.max.X <= max.X) && (rect.max.Y <= max.Y);
        }

        public bool Intersect(Rect rect)
        {
            // Exit with no intersection if separated along an axis
            if (rect.max.X < min.X || rect.min.X > max.X) return false;
            if (rect.max.Y < min.Y || rect.min.Y > max.Y) return false;
            // Overlapping on all axes means AABBs are intersecting
            return true;
        }
        public Rectangle ToRectangle()
        {
            var dim = max - min;
            return new Rectangle((int)min.X, (int)min.Y, (int)dim.X, (int)dim.Y);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Rect))
            {
                return false;
            }

            var rect = (Rect)obj;
            return min.Equals(rect.min) &&
                   max.Equals(rect.max) &&
                   Center.Equals(rect.Center) &&
                   Dim.Equals(rect.Dim) &&
                   TopLeft.Equals(rect.TopLeft) &&
                   BottomLeft.Equals(rect.BottomLeft) &&
                   TopRight.Equals(rect.TopRight) &&
                   BottomRight.Equals(rect.BottomRight);
        }

        public override int GetHashCode()
        {
            var hashCode = -1610053745;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(min);
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(max);
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(Center);
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(Dim);
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(TopLeft);
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(BottomLeft);
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(TopRight);
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(BottomRight);
            return hashCode;
        }

        public static Rect operator *(Rect Rect, Vector2 scale)
        {
            Rect Result = Rect;
            Result.min *= scale;
            Result.max *= scale;
            return Result;
        }

        public static bool operator ==(Rect A,Rect B)
        {
            bool Result = false;
            if (A.min == B.min && A.max == B.max)
            {
                Result = true;
            }
            return Result;
        }
        public override string ToString()
        {
            return min.ToString()+max.ToString();
        }
        public static bool operator !=(Rect A, Rect B)
        {
            return !(A==B);
        }
    }


    public enum FacingDirection
    {
        Front = 0,
        Behind = 3,
        Left = 2,
        Right = 1
    }

    public static class Utils
    {
        public static FacingDirection GetFacingDirection(Vector2 dPos)
        {
            int i1 = dPos.Y > dPos.X ? 1 : 0;
            int i2 = dPos.X > -dPos.Y ? 1 : 0;
            return (FacingDirection)(2 * i1 + i2);
        }

        public static Color BrightenColor(Color col, float brightenfactor)
        {

            RgbToHls(col.R, col.G, col.B, out double h, out double l, out double s);
            l *= brightenfactor;
            HlsToRgb(h, l, s, out int r, out int g, out int b);
            return new Color(r, g, b, col.A);
        }

        // Convert an RGB value into an HLS value.
        private static void RgbToHls(int r, int g, int b,
            out double h, out double l, out double s)
        {
            // Convert RGB to a 0.0 to 1.0 range.
            double double_r = r / 255.0;
            double double_g = g / 255.0;
            double double_b = b / 255.0;

            // Get the maximum and minimum RGB components.
            double max = double_r;
            if (max < double_g) max = double_g;
            if (max < double_b) max = double_b;

            double min = double_r;
            if (min > double_g) min = double_g;
            if (min > double_b) min = double_b;

            double diff = max - min;
            l = (max + min) / 2;
            if (Math.Abs(diff) < 0.00001)
            {
                s = 0;
                h = 0;  // H is really undefined.
            }
            else
            {
                if (l <= 0.5) s = diff / (max + min);
                else s = diff / (2 - max - min);

                double r_dist = (max - double_r) / diff;
                double g_dist = (max - double_g) / diff;
                double b_dist = (max - double_b) / diff;

                if (double_r == max) h = b_dist - g_dist;
                else if (double_g == max) h = 2 + r_dist - b_dist;
                else h = 4 + g_dist - r_dist;

                h = h * 60;
                if (h < 0) h += 360;
            }
        }

        // Convert an HLS value into an RGB value.
        private static void HlsToRgb(double h, double l, double s,
            out int r, out int g, out int b)
        {
            double p2;
            if (l <= 0.5) p2 = l * (1 + s);
            else p2 = l + s - l * s;

            double p1 = 2 * l - p2;
            double double_r, double_g, double_b;
            if (s == 0)
            {
                double_r = l;
                double_g = l;
                double_b = l;
            }
            else
            {
                double_r = QqhToRgb(p1, p2, h + 120);
                double_g = QqhToRgb(p1, p2, h);
                double_b = QqhToRgb(p1, p2, h - 120);
            }

            // Convert RGB to the 0 to 255 range.
            r = (int)(double_r * 255.0);
            g = (int)(double_g * 255.0);
            b = (int)(double_b * 255.0);
        }

        private static double QqhToRgb(double q1, double q2, double hue)
        {
            if (hue > 360) hue -= 360;
            else if (hue < 0) hue += 360;

            if (hue < 60) return q1 + (q2 - q1) * hue / 60;
            if (hue < 180) return q2;
            if (hue < 240) return q1 + (q2 - q1) * (240 - hue) / 60;
            return q1;
        }
    }

    public enum DrawCommandType
    {
        DrawRect,
        DrawString,
        DrawTexture
    }

    public struct DrawCommand
    {
        public TextureID tex_id;
        public float Zindex;
        public Rect DestRectangle;
        public DrawCommandType Type;
        public Rect SourceRectangle;
        public Color tint;
        public string str;
        public DrawCommand(TextureID textureID, float zindex, Rect Srect, Rect Drect)
        {
            tex_id = textureID;
            Zindex = zindex;
            SourceRectangle = Srect;
            DestRectangle = Drect;
            Type = DrawCommandType.DrawTexture;
            tint = Color.White;
            str = "";
        }

        public DrawCommand(TextureID textureID, float zindex, Rect Srect, Rect Drect, Color shade)
        {
            tex_id = textureID;
            Zindex = zindex;
            SourceRectangle = Srect;
            DestRectangle = Drect;
            Type = DrawCommandType.DrawTexture;
            tint = shade;
            str = "";
        }
        public DrawCommand(TextureID textureID, float zindex, Rect Drect, Color shade)
        {
            tex_id = textureID;
            Zindex = zindex;
            SourceRectangle = Rect.Zero;
            DestRectangle = Drect;
            Type = DrawCommandType.DrawTexture;
            tint = shade;
            str = "";
        }
        public DrawCommand(string String, float zindex, Rect Srect, Rect Drect, Color shade)
        {
            tex_id = 0;
            Zindex = zindex;
            SourceRectangle = Srect;
            DestRectangle = Drect;
            Type = DrawCommandType.DrawString;
            tint = shade;
            str = String;
        }

        public DrawCommand(string String, float zindex, Rect Drect, Color shade)
        {
            tex_id = TextureID.font;
            Zindex = zindex;
            SourceRectangle = Rect.Zero;
            DestRectangle = Drect;
            Type = DrawCommandType.DrawString;
            tint = shade;
            str = String;
        }
    }

}