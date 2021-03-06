﻿
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using System.Reactive.Linq;

using VVVV.Utils;
using VVVV.Utils.VMath;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Core.Logging;
using VVVV.Utils.IO;

namespace VVVV.Nodes.VObjects
{
    // "stolen" from vvvv group
    public class TypeWriter : IDisposable
    {
        public bool Enabled = true;
        private Keyboard FKeyboard;
        private IDisposable FKeyboardSubscription;
        public List<string> WhiteList = new List<string>();
        public List<string> BlackList = new List<string>();
        public Keyboard Keyboard
        {
            set
            {
                if (value != FKeyboard)
                {
                    if (FKeyboardSubscription != null)
                    {
                        FKeyboardSubscription.Dispose();
                        FKeyboardSubscription = null;
                    }
                    FKeyboard = value;
                    if (FKeyboard != null)
                    {
                        FKeyboardSubscription = FKeyboard.KeyNotifications.Subscribe(n =>
                            {
                                if ((FKeyboard != null) && (Enabled))
                                {
                                    switch (n.Kind)
                                    {
                                        case KeyNotificationKind.KeyDown:
                                            FControlKeyPressed = (FKeyboard.Modifiers & Keys.Control) > 0;
                                            FAltKeyPressed = (FKeyboard.Modifiers & Keys.Alt) > 0;
                                            FShiftKeyPressed = (FKeyboard.Modifiers & Keys.Shift) > 0;
                                            var keyDown = n as KeyDownNotification;
                                            var keyCode = keyDown.KeyCode;
                                            if ((int)keyCode < 48)
                                            {
                                                if (!FAltKeyPressed)
                                                    RunCommand(keyCode);
                                            }
                                            break;
                                        case KeyNotificationKind.KeyPress:
                                            var keyPress = n as KeyPressNotification;
                                            var chr = keyPress.KeyChar;
                                            if (!char.IsControl(chr))
                                                AddNewChar(chr.ToString());
                                            break;
                                    }
                                }
                            }
                        );
                    }
                }
            }
        }

        public int CursorPosition
        {
            get { return FCursorCharPos; }
            set
            {
                FCursorCharPos = Math.Min(FText.Length, Math.Max(0, value));
            }
        }
        public int TabSize
        {
            get { return FTabSize; }
            set
            {
                FTabSize = Math.Max(1, value);
            }
        }
        public int SelectStart
        {
            get { return FSelectStart; }
            set
            {
                if (value < FSelectEnd)
                    FSelectStart = Math.Min(FText.Length, Math.Max(-1, value));
                else
                    FSelectEnd = Math.Min(FText.Length, Math.Max(-1, value));
            }
        }
        public int SelectEnd
        {
            get { return FSelectEnd; }
            set
            {
                if (value > FSelectStart)
                    FSelectEnd = Math.Min(FText.Length, Math.Max(-1, value));
                else
                    FSelectStart = Math.Min(FText.Length, Math.Max(-1, value));
            }
        }

        public int MaxLength
        {
            get { return FMaxLength; }
            set
            {
                FMaxLength = value;

                // I have to trim the FText if the new MaxLength is smaller the the FText.Length.
                // Can I do it in the setter or should I define an extra method?
                if (value >= 0 && value < FText.Length)
                {
                    FText = FText.Substring(0, value);
                    CursorToTextEnd();
                }
            }
        }

        public bool IgnoreNavigationKeys { get; set; }
        public bool IgnoreNewLine { get; set; }

        public string Output { get { return FText; } }

        private int FSelectStart = -1;
        private int FSelectEnd = -1;
        private int FTabSize = 4;
        private bool FControlKeyPressed;
        private bool FShiftKeyPressed;
        private bool FAltKeyPressed;
        private string FLastCapitalKey;
        private string FText = "";
        private int FCursorCharPos = 0;
        private int FMaxLength = -1;
        //defined as string but must be only one character:
        private string FNewlineSymbol = Environment.NewLine;
        private Dictionary<uint, double> FBufferedCommands = new Dictionary<uint, double>();
        private Dictionary<string, double> FBufferedKeys = new Dictionary<string, double>();

        #region Text navigation helper
        //todo:
        //text splitting is done here every call,
        //should be done globally
        int GetLineCount()
        {
            var lines = FText.Split(new string[] { FNewlineSymbol }, StringSplitOptions.None);
            return lines.Length;
        }

        int GetLineOfCursor()
        {
            var lineStart = 0;
            var lineEnd = 0;
            var lineIndex = 0;
            var lines = FText.Split(new string[] { FNewlineSymbol }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                lineEnd += line.Length + FNewlineSymbol.Length;

                if (FCursorCharPos >= lineStart && FCursorCharPos < lineEnd)
                    break;

                lineStart += line.Length + FNewlineSymbol.Length;
                lineIndex++;
            }

            return lineIndex;
        }

        int GetStartOfLine(int lineIndex)
        {
            var lineStart = 0;
            var lines = FText.Split(new string[] { FNewlineSymbol }, StringSplitOptions.None);

            for (int i = 0; i < lineIndex; i++)
                lineStart += lines[i].Length + FNewlineSymbol.Length;

            return lineStart;
        }

        int GetLengthOfLine(int lineIndex)
        {
            var lines = FText.Split(new string[] { FNewlineSymbol }, StringSplitOptions.None);
            return lines[lineIndex].Length;
        }
        #endregion Text navigation helper

        #region Cursor
        void CursorToTextStart()
        {
            FCursorCharPos = 0;
        }

        void CursorToTextEnd()
        {
            FCursorCharPos = FText.Length;
        }

        void CursorOneLineUp()
        {
            int line = GetLineOfCursor();
            if (line > 0)
            {
                int StartCurPos = FCursorCharPos;

                var startOfLine = GetStartOfLine(line);
                var posInLine = FCursorCharPos - startOfLine;
                var newLine = line - 1;

                var newPos = GetStartOfLine(newLine) + Math.Min(posInLine, GetLengthOfLine(newLine));

                FCursorCharPos = EnsureCorrectCursorPlacement(newPos);

                if (FShiftKeyPressed)
                {
                    FSelectEnd = StartCurPos;
                    FSelectStart = FCursorCharPos;
                }
            }
        }

        void CursorOneLineDown()
        {
            int line = GetLineOfCursor();
            if (line < GetLineCount() - 1)
            {
                int StartCurPos = FCursorCharPos;

                var startOfLine = GetStartOfLine(line);
                var posInLine = FCursorCharPos - startOfLine;
                var newLine = line + 1;

                var newPos = GetStartOfLine(newLine) + Math.Min(posInLine, GetLengthOfLine(newLine));

                FCursorCharPos = EnsureCorrectCursorPlacement(newPos);

                if (FShiftKeyPressed)
                {
                    FSelectStart = StartCurPos;
                    FSelectEnd = FCursorCharPos;
                }
            }
        }

        int EnsureCorrectCursorPlacement(int newPos)
        {
            try
            {
                if (FText[newPos] == '\n' && FText[newPos - 1] == '\r')
                    newPos -= 1;
            }
            catch
            {
                //FText[newPos] may access out of range char..nevermind
            }

            return Math.Max(0, Math.Min(FText.Length, newPos));
        }

        void CursorToLineStart()
        {
            int StartCurPos = FCursorCharPos;
            int line = GetLineOfCursor();
            FCursorCharPos = GetStartOfLine(line);
            if(FShiftKeyPressed)
            {
                FSelectEnd = StartCurPos;
                FSelectStart = FCursorCharPos;
            }
        }

        void CursorToLineEnd()
        {
            int StartCurPos = FCursorCharPos;
            int line = GetLineOfCursor();
            FCursorCharPos = GetStartOfLine(line) + GetLengthOfLine(line);
            if (FShiftKeyPressed)
            {
                FSelectStart = StartCurPos;
                FSelectEnd = FCursorCharPos;
            }
        }

        void CursorStepsRight(int steps = 1)
        {
            int StartCurPos = FCursorCharPos;
            var newPos = Math.Min(FText.Length, FCursorCharPos + steps);
            //if cursor lands on an \r and the next symbol is an \n make sure to step one more
            if (newPos >= 0 && newPos < FText.Length)
            {
                if (FText[newPos] == '\n' && FText[newPos - 1] == '\r')
                    newPos += 1;
            }

            FCursorCharPos = Math.Min(FText.Length, newPos);
            if (FShiftKeyPressed)
            {
                if (StartCurPos == FSelectStart)
                    FSelectStart = FCursorCharPos;
                else if (StartCurPos == FSelectEnd)
                    FSelectEnd = FCursorCharPos;
                else
                {
                    FSelectStart = StartCurPos;
                    FSelectEnd = FCursorCharPos;
                }
            }
        }

        void CursorStepsLeft()
        {
            int StartCurPos = FCursorCharPos;
            var newPos = Math.Max(0, FCursorCharPos - 1);
            //if cursor lands on an \n and the previouse symbol is an \r make sure to step one more
            if (newPos >= 0 && newPos < FText.Length)
            {
                if (FText[newPos] == '\n' && FText[newPos - 1] == '\r')
                    newPos -= 1;
            }

            FCursorCharPos = Math.Max(0, newPos);
            if (FShiftKeyPressed)
            {
                if (StartCurPos == FSelectStart)
                    FSelectStart = FCursorCharPos;
                else if (StartCurPos == FSelectEnd)
                    FSelectEnd = FCursorCharPos;
                else
                {
                    FSelectEnd = StartCurPos;
                    FSelectStart = FCursorCharPos;
                }
            }
        }

        void CursorStepsWordRight()
        {
            int StartCurPos = FCursorCharPos;
            var nextSpace = FText.IndexOfAny(new char[] { ' ', FNewlineSymbol[0] }, FCursorCharPos) + 1;
            FCursorCharPos = Math.Min(FText.Length, nextSpace);
            if (FShiftKeyPressed)
            {
                if (StartCurPos == FSelectStart)
                    FSelectStart = FCursorCharPos;
                else if (StartCurPos == FSelectEnd)
                    FSelectEnd = FCursorCharPos;
                else
                {
                    FSelectStart = StartCurPos;
                    FSelectEnd = FCursorCharPos;
                }
            }
        }

        void CursorStepsWordLeft()
        {
            int StartCurPos = FCursorCharPos;
            if (FCursorCharPos > 2)
            {
                var lastSpace = FText.LastIndexOfAny(new char[] { ' ', FNewlineSymbol[0] }, FCursorCharPos - 2) + 1;
                FCursorCharPos = Math.Max(0, lastSpace);
            }
            if (FShiftKeyPressed)
            {
                if (StartCurPos == FSelectStart)
                    FSelectStart = FCursorCharPos;
                else if (StartCurPos == FSelectEnd)
                    FSelectEnd = FCursorCharPos;
                else
                {
                    FSelectEnd = StartCurPos;
                    FSelectStart = FCursorCharPos;
                }
            }
        }

        #endregion

        #region Commands

        private void AddNewChar(string str)
        {
            if (((FSelectStart > -1) && (FSelectEnd > -1)) && (FSelectStart != FSelectEnd))
            {
                DeleteRightChar();
            }

            if (!this.CheckLists(str)) return;
            if (MaxLength != -1)
            {
                int freeSpace = MaxLength - FText.Length;

                if (freeSpace <= 0) return;

                if (str.Length > freeSpace)
                {
                    str = str.Substring(0, freeSpace);
                }
            }

            FText = FText.Insert(FCursorCharPos, str);

            if (str.ToUpper() == str)
                FLastCapitalKey = str;

            int newPos = Math.Min(FText.Length, FCursorCharPos + 1);
            //if cursor lands on an \r and the next symbol is an \n make sure to step one more
            if (newPos >= 0 && newPos < FText.Length)
            {
                if (FText[newPos] == '\n' && FText[newPos - 1] == '\r')
                    newPos += 1;
            }

            FCursorCharPos = Math.Min(FText.Length, newPos);

        }

        private void DeleteLeftChar()
        {
            if (FCursorCharPos > 0)
            {
                CursorStepsLeft();
                DeleteRightChar();
            }
        }

        private void DeleteRightChar()
        {
            if (((FSelectStart > -1) && (FSelectEnd > -1)) && (FSelectStart != FSelectEnd))
            {
                var charCount = FSelectEnd - FSelectStart;

                FText = FText.Remove(FSelectStart, charCount);
                FCursorCharPos = FSelectStart;
                FSelectStart = -1;
                FSelectEnd = -1;
            }
            else
            {
                var charCount = 1;
                if (FText[FCursorCharPos] == '\r' && FNewlineSymbol.Length > 1)
                    charCount = FNewlineSymbol.Length;

                FText = FText.Remove(FCursorCharPos, charCount);
            }
        }

        private bool RunCommand(Keys keyCode)
        {
            try
            {
                switch (keyCode)
                {
                    case Keys.None:
                        return true;
                    case Keys.Return:
                        if(!this.IgnoreNewLine)
                            AddNewChar(FNewlineSymbol);
                        return true;
                    case Keys.Back:
                        DeleteLeftChar();
                        return true;
                    case Keys.Delete:
                        DeleteRightChar();
                        return true;
                    case Keys.Tab:
                        for (int i = 0; i < FTabSize; i++ )
                            AddNewChar(" ");
                        return true;
                    case Keys.Left:
                        if (!IgnoreNavigationKeys)
                            if (FControlKeyPressed)
                                CursorStepsWordLeft();
                            else
                                CursorStepsLeft();
                        return true;
                    case Keys.Right:
                        if (!IgnoreNavigationKeys)
                            if (FControlKeyPressed)
                                CursorStepsWordRight();
                            else
                                CursorStepsRight();
                        return true;
                    case Keys.Up:
                        if (!IgnoreNavigationKeys)
                            CursorOneLineUp();
                        return true;
                    case Keys.Down:
                        if (!IgnoreNavigationKeys)
                            CursorOneLineDown();
                        return true;
                    case Keys.Home:
                        if (!IgnoreNavigationKeys)
                            CursorToLineStart();
                        return true;
                    case Keys.End:
                        if (!IgnoreNavigationKeys)
                            CursorToLineEnd();
                        return true;
                    case Keys.Prior:
                        if (!IgnoreNavigationKeys)
                            CursorToTextStart();
                        return true;
                    case Keys.Next:
                        if (!IgnoreNavigationKeys)
                            CursorToTextEnd();
                        return true;
                }
            }
            catch { };
            return false;
        }
        #endregion

        public void Initialize(string text)
        {
            FText = "";
            CursorToTextStart();
            AddNewChar(text);
        }

        public void InsertText(string text)
        {
            AddNewChar(text);
        }

        public bool CheckLists(string key)
        {
            bool valid = false;
            if (WhiteList.Count == 0)
            {
                valid = true;
                foreach (char c in key.ToCharArray())
                    if (this.BlackList.Contains(c.ToString())) valid = false;
                return valid;
            }
            else
            {
                valid = false;
                foreach (char c in key.ToCharArray())
                    if (this.WhiteList.Contains(c.ToString())) valid = true;
                return valid;
            }
        }

        public void Dispose()
        {
            if (FKeyboardSubscription != null)
            {
                FKeyboardSubscription.Dispose();
                FKeyboardSubscription = null;
            }
        }
    }
}
