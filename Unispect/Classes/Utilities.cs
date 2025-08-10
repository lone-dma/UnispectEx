using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Unispect
{
    public static class Utilities
    {
        private static readonly Dictionary<ulong, string> UnknownClassNameCache = new Dictionary<ulong, string>();

        private static Dictionary<string, int> _prefixIndexer;

        private static Dictionary<string, int> PrefixIndexer
        {
            get
            {
                if (_prefixIndexer != null)
                    return _prefixIndexer;

                _prefixIndexer = new Dictionary<string, int>();
                foreach (var e in Enum.GetNames(typeof(UnknownPrefix)))
                    _prefixIndexer.Add(e, 0);

                return _prefixIndexer;
            }
        }

        public static ushort UTF8ToUTF16(this string val)
        {
            byte[] utf16Bytes = Encoding.Unicode.GetBytes(val);
            if (utf16Bytes.Length < 2)
                throw new ArgumentException("Input string is too short.", nameof(val));

            return BitConverter.ToUInt16(utf16Bytes, 0);
        }

        public static string GetSimpleTypeKeyword(this string text)
        {
            var ret = text.Replace("System.", "");
            switch (ret)
            {
                case "Void": return "void";
                case "Object": return "object";
                case "String": return "string";

                case "Boolean": return "bool";

                case "Single": return "float";
                case "Double": return "double";

                case "Byte": return "byte";

                case "SByte": return "sbyte";

                case "Int16": return "short";
                case "Int32": return "int";
                case "Int64": return "long";

                case "UInt16": return "ushort";
                case "UInt32": return "uint";
                case "UInt64": return "ulong";
            }

            return ret;
        }

        public static IEnumerable<int> Step(int fromInclusive, int toExclusive, int step)
        {
            for (var i = fromInclusive; i < toExclusive; i += step)
            {
                yield return i;
            }
        }

        public static string ReadName(this byte[] buffer)
        {
            if (buffer[0] >= 0xE0)
            {
                var nullIndex = Array.IndexOf(buffer, (byte)0);
                if (nullIndex >= 0) Array.Resize(ref buffer, nullIndex);
                var value = Encoding.UTF8.GetString(buffer);
                return $"\\u{value.UTF8ToUTF16():X4}";
            }
            else
            {
                var nullIndex = Array.IndexOf(buffer, (byte)0);
                if (nullIndex >= 0) Array.Resize(ref buffer, nullIndex);
                return Encoding.UTF8.GetString(buffer);
            }
        }

        public static string ToAsciiString(this byte[] buffer, int start = 0)
        {
            var length = 0;
            for (var i = start; i < buffer.Length; i++)
            {
                if (buffer[i] != 0) continue;

                length = i - start;
                break;
            }

            return Encoding.ASCII.GetString(buffer, start, length);
        }

        public static string LowerChar(this string str, int index = 0)
        {
            if (index < str.Length && index > -1) // instead of casting from uint, just check if it's zero or greater
            {
                if (index == 0)
                    return char.ToLower(str[index]) + str.Substring(index + 1);

                return str.Substring(0, index - 1) + char.ToLower(str[index]) + str.Substring(index + 1);
            }

            return str;
        }

        public static string FormatFieldText(this string text)
        {
            var ret = text.Replace("[]", "Array");
            var lessThanIndex = ret.IndexOf('<');
            if (lessThanIndex > -1)
            {
                // The type name _should_ always end at the following index, so we don't need to splice.
                //var greaterThanIndex = ret.IndexOf('>'); 
                ret = ret.Substring(0, lessThanIndex);
            }

            return ret;
        }

        public static string SanitizeFileName(this string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var pattern = invalidChars.Aggregate("[", (current, c) => current + $"\\{c}") + "]";
            var ret = Regex.Replace(fileName, pattern, "_");

            return ret;
        }

        public static int ToInt32(this byte[] buffer, int start = 0) => BitConverter.ToInt32(buffer, start);

        public static string CurrentVersion
        {
            get
            {
                return Assembly.GetEntryAssembly().GetName().Version.ToString();
            }
        }

        public static string GithubLink => "http://www.github.com/Razchek/Unispect";

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern int TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y,
            int nReserved, IntPtr hWnd, IntPtr prcRect);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out IntRect rect);

        public struct IntRect
        {
            public int Left, Top, Right, Bottom;
        }

        public static void ShowSystemMenu(Window window)
        {
            var hWnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            GetWindowRect(hWnd, out var pos);
            var hMenu = GetSystemMenu(hWnd, false);
            var cmd = TrackPopupMenu(hMenu, 0x100, pos.Left + 20, pos.Top + 20, 0, hWnd, IntPtr.Zero);
            if (cmd > 0) SendMessage(hWnd, 0x112, (IntPtr)cmd, IntPtr.Zero);
        }


        public static async void LaunchUrl(string url)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.AppStarting;
                Process.Start(url);
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;
                var nl = Environment.NewLine;
                await MessageBox(
                    $"Couldn't open: {url}.{nl}{nl}Exception:{nl}{ex.Message}");
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

        }

        public static async Task<MessageDialogResult> MessageBox(string msg, string title = "",
            MessageDialogStyle messageDialogStyle = MessageDialogStyle.Affirmative,
            MetroDialogSettings metroDialogSettings = null)
        {
            if (string.IsNullOrEmpty(title))
                title = Application.Current.MainWindow?.Title;

            var mw = (Application.Current.MainWindow as MetroWindow);
            return await mw.ShowMessageAsync(title, msg, messageDialogStyle, metroDialogSettings);
        }

        public static void FadeFromTo(this UIElement uiElement, double fromOpacity, double toOpacity,
            int durationInMilliseconds, bool showOnStart, bool collapseOnFinish)
        {
            var timeSpan = TimeSpan.FromMilliseconds(durationInMilliseconds);
            var doubleAnimation =
                new DoubleAnimation(fromOpacity, toOpacity,
                    new Duration(timeSpan));

            uiElement.BeginAnimation(UIElement.OpacityProperty, doubleAnimation);
            if (showOnStart)
            {
                uiElement.ApplyAnimationClock(UIElement.VisibilityProperty, null);
                uiElement.Visibility = Visibility.Visible;
            }
            if (collapseOnFinish)
            {
                var keyAnimation = new ObjectAnimationUsingKeyFrames { Duration = new Duration(timeSpan) };
                keyAnimation.KeyFrames.Add(new DiscreteObjectKeyFrame(Visibility.Collapsed, KeyTime.FromTimeSpan(timeSpan)));
                uiElement.BeginAnimation(UIElement.VisibilityProperty, keyAnimation);
            }
        }

        public static void FadeIn(this UIElement uiElement, int durationInMilliseconds = 100)
        {
            uiElement.FadeFromTo(0, 1, durationInMilliseconds, true, false);
        }

        public static void FadeOut(this UIElement uiElement, int durationInMilliseconds = 100)
        {
            uiElement.FadeFromTo(1, 0, durationInMilliseconds, false, true);
        }

        public static void ResizeFromTo(this FrameworkElement uiElement, Size fromSize, Size toSize, int durationInMilliseconds)
        {
            var timeSpan = TimeSpan.FromMilliseconds(durationInMilliseconds);

            //var sizeAnimationWidth = new DoubleAnimation(fromSize.Width, toSize.Width, new Duration(timeSpan));
            var sizeAnimationHeight = new DoubleAnimation(fromSize.Height, toSize.Height, new Duration(timeSpan));

            //uiElement.BeginAnimation(FrameworkElement.WidthProperty, sizeAnimationWidth);
            uiElement.BeginAnimation(FrameworkElement.HeightProperty, sizeAnimationHeight);
        }
    }
}