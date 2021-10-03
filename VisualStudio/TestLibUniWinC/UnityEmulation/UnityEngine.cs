using System;
using System.Runtime.InteropServices;

namespace UnityEngine
{
    public struct Color32
    {
        public byte a;
        public byte r;
        public byte g;
        public byte b;

        public Color32(byte r, byte g, byte b, byte a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2
    {
        public float x;
        public float y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2 zero = new Vector2(0, 0);

        override public string ToString()
        {
            return x + ", " + y;
        }
    }

    public class Debug
    {
        public static void Log(string text)
        {
            Console.WriteLine(text);
        }
    }
}
