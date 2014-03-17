using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Controls
{
   [Serializable]
   internal class Vkb : MarshalByRefObject, IContainer
   {
      #region Enumerations

      private enum BtnType
      {
         Character = 0,
         Left = 1,
         Right = 2,
         Clear = 3,
         Done = 4,
         Shift = 5,
         ShiftLock = 6,
         ShiftUnlock = 7,
         Abc = 8,
         Num = 9,
         Alt = 10,
         Back = 11,
         Return = 12,
      }

      #endregion

      #region Structures

      private struct Btn
      {
         public BtnType Type;
         public bool Pressed;
// ReSharper disable once NotAccessedField.Local
         public bool Down;
         public Bitmap Image;
         public rect Rect;
         public readonly string Text;
         public bool Enabled;

// ReSharper disable once UnusedMember.Local
         public Btn(BtnType type, rect rect)
         {
            Type = type;
            Rect = rect;
            Image = null;
            Text = null;
            Pressed = false;
            Down = false;
            Enabled = true;
         }

         public Btn(BtnType type, rect rect, Bitmap image)
         {
            Type = type;
            Rect = rect;
            Image = image;
            Text = null;
            Pressed = false;
            Down = false;
            Enabled = true;
         }

         public Btn(BtnType type, rect rect, string text)
         {
            Type = type;
            Rect = rect;
            Image = null;
            Text = text;
            Pressed = false;
            Down = false;
            Enabled = true;
         }
      }

      #endregion

      #region Variables

      // SPECIFIC

      private readonly Font _font;
      private string _text;
      private readonly KeyboardLayout _layout;
      private readonly string _title;
      private readonly char _pwd = ' ';                // Password character ' ' for none

      private rect _keyRr;
      private rect _lastKeyRr;
      private bool _fullRendered;

      // Rendering Queue
      private bool _renderRequested;
      private bool _canRender;
      internal bool Continue;
      private bool _rendering;

      // Textbox
      private rect _rTxt;
      private int _caret;
      private int _caretX;           // Caret's screen poistion
// ReSharper disable once NotAccessedField.Local
      private int _caretY;           // Caret's screen poistion
      private bool _txtDown;
// ReSharper disable once NotAccessedField.Local
      private int _iLine;                     // Caret's current line
      //private int _col;                       // Caret's position on current line
      protected internal int ShowChar = -1;  // 
      private int _scrollX;
      private Thread _thShow;                  // Thread for hidding a typed character w/ a pwd

      // Virtual Keys
      private readonly Bitmap _keyBuffer;
      private readonly int _vkX;
      private readonly int _vkY;
      private readonly int _btnH;
      private readonly int _btnW;
      private Btn[] _btns;
      private byte _shft;
      private byte _scrn;
      private char _lastc;
      private int _iSel;
      private int _iRender;

      // CONTROL COMMON

      // Name & Tag
      private string _name;
      private object _tag;

      // Size & Location
      private int _x, _y;
      private int _offsetX, _offsetY;
      private int _w, _h;

      // Owner
      private IContainer _parent;

      // States
      private bool _enabled = true;
      private bool _visible = true;
      private bool _suspended;

      // Touch
      private bool _mDown;            // Touching when true
      private point _ptTapHold;       // Location where touch down occurred
      //private Thread _thHold;         // Tap & Hold thread
      //private long _lStop;            // Stop waiting for hold after this tick
      private long _lastTap;          // Tick count of last time occurrance

      // Dispose
      private bool _disposing;

      // CONTAINER COMMON

      // Children
      private readonly IControl[] _children = new IControl[0];
      private IControl _active;


      #endregion

      #region Constructors

      public Vkb(Font font, string defaultValue, char passwordChar, KeyboardLayout layout, string title)
      {
         _font = font;
         _text = defaultValue;
         _layout = layout;
         _title = title;
         _pwd = passwordChar;

         if (_title == null)
         {
            _title = string.Empty;
         }
         if (_text == null)
         {
            _text = string.Empty;
         }

         _w = Core.ScreenWidth;
         _h = Core.ScreenHeight;

         int y = 4;
         if (_title != string.Empty)
         {
            y += _font.Height + 4;
         }

         // Textbox Definitions
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         _rTxt = new rect(4, y, Width - 8, _font.Height + 7);
         y += _rTxt.Height + 4;
         CurrentCharacter = _text.Length;

         // Virtual Keys
         _vkX = 4;
         _vkY = y;
         _keyBuffer = new Bitmap(Width - 8, Height - y - 4);
         _btnH = (_keyBuffer.Height - 28) / 5;
         _btnW = (_keyBuffer.Width - 44) / 10;
         _iSel = -1;
         SetLayout();
         _keyRr = new rect(0, 0, _keyBuffer.Width, _keyBuffer.Height);
         // ReSharper restore DoNotCallOverridableMethodsInConstructor

         // Render Queue
         Continue = true;
         new Thread(RenderWatch).Start();
      }

      #endregion

      #region Events

      public event OnVirtualKeysDone VirtualKeysDone;
      protected virtual void OnVirtualKeysDone(object sender)
      {
         if (VirtualKeysDone != null)
            VirtualKeysDone(sender);
      }

      public event OnGotFocus GotFocus;

      protected virtual void OnGotFocus(object sender)
      {
         if (GotFocus != null)
            GotFocus(sender);
      }

      public event OnLostFocus LostFocus;

      protected virtual void OnLostFocus(object sender)
      {
         if (LostFocus != null)
            LostFocus(sender);
      }

      public event OnButtonPressed ButtonPressed;

      protected virtual void OnButtonPressed(object sender, int buttonId)
      {
         if (ButtonPressed != null)
            ButtonPressed(sender, buttonId);
      }

      public event OnButtonReleased ButtonReleased;

      protected virtual void OnButtonReleased(object sender, int buttonId)
      {
         if (ButtonReleased != null)
            ButtonReleased(sender, buttonId);
      }

      /// <summary>
      /// Raised on a double-tap
      /// </summary>
      public event OnDoubleTap DoubleTap;

      protected virtual void OnDoubleTap(object sender, point e)
      {
         if (DoubleTap != null)
            DoubleTap(sender, e);
      }

      public event OnKeyboardAltKey KeyboardAltKey;

      protected virtual void OnKeyboardAltKey(object sender, int key, bool pressed)
      {
         if (KeyboardAltKey != null)
            KeyboardAltKey(sender, key, pressed);
      }

      public event OnKeyboardKey KeyboardKey;

      protected virtual void OnKeyboardKey(object sender, char key, bool pressed)
      {
         if (KeyboardKey != null)
            KeyboardKey(sender, key, pressed);
      }

      /// <summary>
      /// Raised on tap
      /// </summary>
      public event OnTap Tap;

      public virtual void OnTap(object sender, point e)
      {
         if (Tap != null)
            Tap(sender, e);
      }

      /// <summary>
      /// Raised on tap and hold
      /// </summary>
      public event OnTapHold TapHold;

      public virtual void OnTapHold(object sender, point e)
      {
         if (TapHold != null)
            TapHold(sender, e);
      }

      /// <summary>
      /// Raised on touch down
      /// </summary>
      public event OnTouchDown TouchDown;

      public virtual void OnTouchDown(object sender, point e)
      {
         if (TouchDown != null)
            TouchDown(sender, e);
      }

      /// <summary>
      /// Raised on touch gesture
      /// </summary>
      public event OnTouchGesture TouchGesture;

      public virtual void OnTouchGesture(object sender, TouchType e, float force)
      {
         if (TouchGesture != null)
            TouchGesture(sender, e, force);
      }

      /// <summary>
      /// Raised on touch move
      /// </summary>
      public event OnTouchMove TouchMove;

      public virtual void OnTouchMove(object sender, point e)
      {
         if (TouchMove != null)
            TouchMove(sender, e);
      }

      /// <summary>
      /// Raised on touch up
      /// </summary>
      public event OnTouchUp TouchUp;

      public virtual void OnTouchUp(object sender, point e)
      {
         if (TouchUp != null)
            TouchUp(sender, e);
      }

      #endregion

      #region Properties

      public bool CanRender
      {
         get { return true; }
      }

      public int CurrentCharacter
      {
         get { return _caret; }
         set
         {
            if (_caret == value)
            {
               return;
            }
            if (value < 0)
            {
               value = 0;
            }
            if (value > _text.Length)
            {
               value = _text.Length;
            }

            _caret = value;
            UpdateCaretPos();
         }
      }

      public string Text
      {
         get { return _text; }
         set
         {
            if (value == null)
            {
               value = string.Empty;
            }
            if (_text == value)
            {
               return;
            }
            _text = value;
            CurrentCharacter = _text.Length;
         }
      }


      public virtual IControl ActiveChild
      {
         get { return _active; }
         set
         {
            if (_active == value)
               return;

            IControl tmp = _active;
            _active = null;

            if (tmp != null)
               tmp.Blur();

            _active = value;
            if (_active != null)
               _active.Activate();
         }
      }

      public IControl[] Children
      {
         get { return _children; }
      }

      public virtual IContainer TopLevelContainer
      {
         get { return Parent.TopLevelContainer; }
      }



      public virtual bool CanFocus
      {
         get { return true; }
      }

      public bool Disposing
      {
         get { return _disposing; }
      }

      public bool Enabled
      {
         get { return _enabled; }
         set
         {
            if (_enabled == value)
               return;
            _enabled = value;
            Render(true);
         }
      }

      public bool Focused
      {
         get
         {
            if (_parent == null)
               return false;
            return _parent.ActiveChild == this;
         }
      }

      public virtual int Height
      {
         get { return _h; }
         set
         {
            if (_h == value)
               return;

            // Create Update Region
            var r = new rect(_x, _y, _w, System.Math.Max(_h, value));

            _h = value;
            if (_parent != null)
               _parent.Render(r, true);
         }
      }

      public point LastTouch
      {
         get { return _ptTapHold; }
         protected set { _ptTapHold = value; }
      }

      public int Left
      {
         get { return _x + _offsetX; }
      }

      public string Name
      {
         get { return _name; }
         protected set { _name = value; }
      }

      public virtual IContainer Parent
      {
         get { return _parent; }
         set
         {
            if (_parent == value)
               return;

            if (_parent != null)
               _parent.RemoveChild(this);

            _parent = value;
            UpdateOffsets();
         }
      }

      public rect ScreenBounds
      {
         get { return new rect(Left, Top, Width, Height); }
      }

      public virtual bool Suspended
      {
         get { return _suspended; }
         set
         {
            if (_suspended == value)
               return;
            _suspended = value;
            if (!_suspended)
               Invalidate();
         }
      }

      public object Tag
      {
         get { return _tag; }
         set { _tag = value; }
      }

      public int Top
      {
         get { return _y + _offsetY; }
      }

      public virtual bool Touching
      {
         get { return _mDown; }
      }

      public virtual bool Visible
      {
         get { return _visible; }
         set
         {
            if (_visible == value)
               return;
            _visible = value;
            if (_parent != null)
               _parent.Render(new rect(Left, Top, Width, Height), true);
         }
      }

      public virtual int Width
      {
         get { return _w; }
         set
         {
            if (_w == value)
               return;

            // Create Update Region
            var r = new rect(_x, _y, System.Math.Max(_w, value), _h);

            _w = value;
            if (_parent != null)
               _parent.Render(r, true);
         }
      }

      public virtual int X
      {
         get { return _x; }
         set
         {
            if (_x == value)
               return;

            // Create update region
            var r = new rect(System.Math.Min(_x, value), _y, System.Math.Abs(_x - value) + Width, Height);

            _x = value;
            if (_parent != null)
               _parent.Render(r, true);
         }
      }

      public virtual int Y
      {
         get { return _y; }
         set
         {
            if (_y == value)
               return;

            // Create update region
            var r = new rect(_x, System.Math.Min(_y, value), Width, System.Math.Abs(_y - value) + Height);

            _y = value;
            if (_parent != null)
               _parent.Render(r, true);
         }
      }

      #endregion

      #region Buttons

      protected virtual void ButtonPressedMessage(int buttonId, ref bool handled) { }

      protected virtual void ButtonReleasedMessage(int buttonId, ref bool handled) { }

      public void SendButtonEvent(int buttonId, bool pressed)
      {
         bool handled = false;
         if (pressed)
         {
            ButtonPressedMessage(buttonId, ref handled);
            if (handled)
               return;

            OnButtonPressed(this, buttonId);
            if (buttonId == (int)ButtonIDs.Click || buttonId == (int)ButtonIDs.Select)
               SendTouchDown(this, Core.MousePosition);
         }
         else
         {
            ButtonReleasedMessage(buttonId, ref handled);
            if (handled)
               return;

            if (buttonId == (int)ButtonIDs.Click || buttonId == (int)ButtonIDs.Select)
               SendTouchUp(this, Core.MousePosition);
            OnButtonReleased(this, buttonId);
         }
      }

      #endregion

      #region Keyboard

      public void SendKeyboardAltKeyEvent(int key, bool pressed)
      {

      }

      public void SendKeyboardKeyEvent(char key, bool pressed)
      {

      }

      #endregion

      #region Touch

      /// <summary>
      /// Called when a touch down occurs and the control is active
      /// </summary>
      /// <param name="sender">Sending object</param>
      /// <param name="point">Location of touch</param>
      public void SendTouchDown(object sender, point point)
      {
         // Exit if needed
         if (!_enabled || !_visible || _suspended)
            return;

         // Allow Override

         if (point.Y >= _vkY)
         {
            point.X -= _vkX;
            point.Y -= _vkY;

            // Check buttons
            for (int i = 0; i < _btns.Length; i++)
            {
               if (_btns[i].Rect.Contains(point))
               {
                  _iSel = i;
                  _iRender = i;
                  _btns[i].Down = true;
                  _btns[i].Pressed = true;
                  RenderButton(_iSel);
                  return;
               }
            }
         }

         if (_rTxt.Contains(point))
            _txtDown = true;

         _iSel = -1;


         // Set down state
         _mDown = true;

         // Raise Event
         OnTouchDown(sender, point);
      }

      /// <summary>
      /// Called when a touch up occurs and the control is active
      /// </summary>
      /// <param name="sender">Sending object</param>
      /// <param name="point">Location of touch</param>
      public void SendTouchUp(object sender, point point)
      {
         if (!_enabled || !_visible || _suspended)
         {
            _mDown = false;
            return;
         }

         // Allow Override
         if (_txtDown || _rTxt.Contains(point))
            TextUp(point);

         if (_iSel != -1)
            ButtonsUp(point);


         // Perform normal tap
         if (_mDown)
         {
            if (new rect(Left, Top, Width, Height).Contains(point))
            {
               if (DateTime.Now.Ticks - _lastTap < (TimeSpan.TicksPerMillisecond * 500))
               {
                  OnDoubleTap(this, new point(point.X - Left, point.Y - Top));
                  _lastTap = 0;
               }
               else
               {
                  OnTap(this, new point(point.X - Left, point.Y - Top));
                  _lastTap = DateTime.Now.Ticks;
               }
            }
            _mDown = false;
            OnTouchUp(this, point);
         }
      }

      /// <summary>
      /// Called when a touch move occurs and the control is active
      /// </summary>
      /// <param name="sender">Sending object</param>
      /// <param name="point">Location of touch</param>
      public void SendTouchMove(object sender, point point)
      {
         if (!_enabled || !_visible || _suspended)
            return;

         // Allow Override


         OnTouchMove(this, point);
      }

      /// <summary>
      /// Called when a gesture occurs and the control is active
      /// </summary>
      /// <param name="sender">Sending object</param>
      /// <param name="type">Type of touch gesture</param>
      /// <param name="force"></param>
      public void SendTouchGesture(object sender, TouchType type, float force)
      {
         if (!_enabled || !_visible || _suspended)
            return;

         // Allow Override


         OnTouchGesture(this, type, force);
      }

      private void ButtonsUp(point e)
      {
         _iSel = -1;
         _btns[_iRender].Down = false;
         _btns[_iRender].Pressed = false;

         e.X -= _vkX;
         e.Y -= _vkY;

         // Check buttons
         if (_btns[_iRender].Rect.Contains(e) && _btns[_iRender].Enabled)
         {
            switch (_btns[_iRender].Type)
            {
               case BtnType.Abc:
                  SetLayout();
                  Invalidate();
                  return;

               case BtnType.Alt:
                  SetLayoutAlt();
                  return;

               case BtnType.Back:
                  _lastc = 'x';
                  Backspace();
                  break;

               case BtnType.Character:
                  InsertStrChr(_shft == 0 ? _btns[_iRender].Text.ToLower() : _btns[_iRender].Text);

                  if (_shft == 1)
                  {
                     Unlock();
                  }
                  break;

               case BtnType.Clear:
                  _lastc = 'x';
                  if (_shft == 0)
                  {
                     Shift();
                  }
                  _text = string.Empty;
                  _caret = -1;
                  _caretX = -1;
                  _caretY = -1;
                  _scrollX = 0;
                  _renderRequested = true;
                  break;

               case BtnType.Done:
                  OnVirtualKeysDone(this);
                  break;

               case BtnType.Left:
                  CurrentCharacter -= 1;
                  _lastc = 'x';
                  break;

               case BtnType.Num:
                  SetLayoutNumeric();
                  return;

               case BtnType.Return:
                  if (_shft == 0)
                  {
                     Shift();
                     return;
                  }
                  SensitiveShift(_btns[_iRender].Text);
                  Return();
                  break;

               case BtnType.Right:
                  CurrentCharacter += 1;
                  _lastc = 'x';
                  break;

               case BtnType.Shift:
                  _btns[_iRender].Image = Resources.GetBitmap(Resources.BitmapResources.shift2);
                  _btns[_iRender].Type = BtnType.ShiftLock;
                  _shft = 1;
                  RenderButton(_iRender);
                  return;

               case BtnType.ShiftLock:
                  _btns[_iRender].Image = Resources.GetBitmap(Resources.BitmapResources.shift3);
                  _btns[_iRender].Type = BtnType.ShiftUnlock;
                  _shft = 2;
                  RenderButton(_iRender);
                  break;

               case BtnType.ShiftUnlock:
                  _btns[_iRender].Image = Resources.GetBitmap(Resources.BitmapResources.shift1);
                  _btns[_iRender].Type = BtnType.Shift;
                  _shft = 0;
                  RenderButton(_iRender);
                  return;
            }
         }

         RenderButton(_iRender);
      }

// ReSharper disable once UnusedParameter.Local
      private void TextUp(point e)
      {
         _txtDown = false;
      }

      #endregion

      #region Disposing

      public void Dispose()
      {
         if (_disposing)
            return;

         _disposing = true;

         // Remove Events
         DoubleTap = null;
         Tap = null;
         TapHold = null;
         TouchDown = null;
         TouchGesture = null;
         TouchMove = null;
         TouchUp = null;
      }

      #endregion

      #region Focus

      public virtual void Activate()
      {
         Invalidate();
      }

      public virtual void Blur()
      {
         Invalidate();
      }

      public virtual bool HitTest(point point)
      {
         return ScreenBounds.Contains(point);
      }

      #endregion

      #region GUI

      public void Invalidate()
      {
         if ((_parent == null && Core.ActiveContainer != this) || (_parent != null && _parent.Suspended) || !_visible || _suspended)
            return;

         if (_parent == null)
            Render(true);
         else
            _parent.Render(ScreenBounds, true);
      }


      public void Invalidate(rect area)
      {
         if ((_parent == null && Core.ActiveContainer != this) || (_parent != null && _parent.Suspended) || !_visible || _suspended)
            return;

         if (_parent == null)
            Render(true);
         else
            _parent.TopLevelContainer.Render(area, true);
      }

      public void QuiteUnsuspend()
      {
         _suspended = false;
      }

      public void Render(bool flush = false)
      {
         if (!_canRender)
         {
            _renderRequested = true;
            return;
         }

         while (_rendering)
         { }

         _rendering = true;
         Core.Screen.DrawRectangle(0, 0, Left, Top, Width, Height, 0, 0, Colors.LightGray, 0, 0, Colors.LightGray, 0, 0, 256);

         // Title
         if (_title != string.Empty)
            Core.Screen.DrawText(_title, _font, 0, 4, 4);

         // Virtual Keys
         if (_lastKeyRr.Width != 0)
         {
            Core.Screen.DrawImage(_vkX + _lastKeyRr.X, _vkY + _lastKeyRr.Y, _keyBuffer, _lastKeyRr.X, _lastKeyRr.Y, _lastKeyRr.Width, _lastKeyRr.Height);
            if (_fullRendered)
               Core.SafeFlush(_vkX + _lastKeyRr.X, _vkY + _lastKeyRr.Y, _lastKeyRr.Width, _lastKeyRr.Height);
         }
         _lastKeyRr = _keyRr;

         if (_keyRr.Width != 0)
         {
            Core.Screen.DrawImage(_vkX + _keyRr.X, _vkY + _keyRr.Y, _keyBuffer, _keyRr.X, _keyRr.Y, _keyRr.Width, _keyRr.Height);
            if (_fullRendered)
               Core.SafeFlush(_vkX + _keyRr.X, _vkY + _keyRr.Y, _keyRr.Width, _keyRr.Height);
         }

         // Textbox
         Core.Screen.DrawRectangle(0, 1, _rTxt.X, _rTxt.Y, _rTxt.Width, _rTxt.Height, 0, 0, Colors.White, 0, 0, Colors.White, 0, 0, 256);
         Core.Screen.SetClippingRectangle(_rTxt.X + 1, _rTxt.Y + 1, _rTxt.Width - 2, _rTxt.Height - 2);
         Core.Screen.DrawTextInRect(PwdString(), _rTxt.X + 3 - _scrollX, _rTxt.Y + 3, _rTxt.Width - 6 + _scrollX, _font.Height, Bitmap.DT_AlignmentLeft, 0, _font);
         Core.Screen.DrawLine(Core.SystemColors.SelectionColor, 1, _rTxt.X + _caretX - _scrollX + 3, _rTxt.Y + 3, _rTxt.X + _caretX - _scrollX + 3, _rTxt.Y + _font.Height);
         if (_fullRendered)
            Core.SafeFlush(_rTxt.X, _rTxt.Y, _rTxt.Width, _rTxt.Height);

         _rendering = false;
         if (!_fullRendered)
         {
            Core.SafeFlush();
            _fullRendered = true;
         }
      }

      public void Render(rect region, bool flush = false)
      {
         Render(flush);
      }

      private void RenderWatch()
      {
         while (Continue)
         {
            if (_renderRequested)
            {
               _renderRequested = false;
               while (_rendering)
               { }
               _canRender = true;
               Render(true);
               _canRender = false;
               while (_rendering)
                  { }
            }
            Thread.Sleep(100);
         }
      }

      public void UpdateOffsets()
      {
         if (_parent != null)
         {
            _offsetX = _parent.Left;
            _offsetY = _parent.Top;
         }
         else
         {
            _offsetX = 0;
            _offsetY = 0;
         }
      }

      public void UpdateOffsets(point pt)
      {
         _offsetX = pt.X;
         _offsetY = pt.Y;
      }

      #endregion

      #region Public Container Methods

      /// <summary>
      /// Adds a AddChild
      /// </summary>
      /// <param name="child">Control to add</param>
      public virtual void AddChild(IControl child)
      {

      }

      public virtual void BringControlToFront(IControl child)
      {

      }

      /// <summary>
      /// Removes all controls
      /// </summary>
      public virtual void ClearChildren(bool disposeChildren = true)
      {

      }

      /// <summary>
      /// Returns a control by index
      /// </summary>
      /// <param name="index">Index of control</param>
      /// <returns>Control at index</returns>
      public virtual IControl GetChildByIndex(int index)
      {
         return null;
      }

      /// <summary>
      /// Returns a control by name
      /// </summary>
      /// <param name="name">Name of control</param>
      /// <returns>Retrieved control</returns>
      public virtual IControl GetChildByName(string name)
      {
         return null;
      }

      public virtual int GetChildIndex(IControl child)
      {
         return -1;
      }

      /// <summary>
      /// Removes a specific control
      /// </summary>
      /// <param name="child">Control to remove</param>
      public virtual void RemoveChild(IControl child)
      {

      }

      /// <summary>
      /// Removes a control by index
      /// </summary>
      /// <param name="index">Index of control</param>
      public virtual void RemoveChildAt(int index)
      {

      }

      public virtual void NextChild()
      {
      }

      public virtual void PreviousChild()
      {
      }

      #endregion

      #region VirtualKeys

      private void AddFirstBtns()
      {
         int x = 0;
         _btns[0] = new Btn(BtnType.Left, new rect(x, 0, _btnW, _btnH), Resources.GetBitmap(Resources.BitmapResources.back))
         {
            Enabled = (_caret > 0)
         };
         x += _btnW + 4;
         _btns[1] = new Btn(BtnType.Right, new rect(x, 0, _btnW, _btnH), Resources.GetBitmap(Resources.BitmapResources.next))
         {
            Enabled = (_caret < _text.Length)
         };
         x += _btnW + 4;
         _btns[2] = new Btn(BtnType.Clear, new rect(x, 0, _btnW, _btnH), Resources.GetBitmap(Resources.BitmapResources.clear))
         {
            Enabled = (_text.Length > 0)
         };

         int w = FontManager.ComputeExtentEx(_font, "Done").Width + 17;
         _btns[3] = new Btn(BtnType.Done, new rect(_keyBuffer.Width - w + 3, 0, w, _btnH), "Done");
      }

      private void CreateButtons(string[] row1, string[] row2, string[] row3)
      {
         int i;
         int b = 4;

         // Row 1
         int y = _btnH + 6;
         int x = (_keyBuffer.Width / 2 - (((row1.Length * _btnW) + (row1.Length * 4)) / 2));
         for (i = 0; i < row1.Length; i++)
         {
            _btns[b++] = new Btn(BtnType.Character, new rect(x, y, _btnW, _btnH), row1[i]);
            x += _btnW + 4;
         }

         // Row 2
         y += _btnH + 6;
         x = (_keyBuffer.Width / 2 - (((row2.Length * _btnW) + (row2.Length * 4)) / 2));
         for (i = 0; i < row2.Length; i++)
         {
            _btns[b++] = new Btn(BtnType.Character, new rect(x, y, _btnW, _btnH), row2[i]);
            x += _btnW + 4;
         }

         // Row 3 (with specials)
         y += _btnH + 6;
         x = (_keyBuffer.Width / 2 - (((row3.Length * _btnW) + (row3.Length * 4)) / 2));
         int w = x - 4;

         switch (_scrn)
         {
            case 0:
               _btns[b++] = new Btn(BtnType.Shift, new rect(0, y, w, _btnH), Resources.GetBitmap(Resources.BitmapResources.shift1));
               break;
            case 1:
               _btns[b++] = new Btn(BtnType.Alt, new rect(0, y, w, _btnH), "#+=");
               break;
            case 2:
               _btns[b++] = new Btn(BtnType.Num, new rect(0, y, w, _btnH), "?123");
               break;
         }

         for (i = 0; i < row3.Length; i++)
         {
            _btns[b++] = new Btn(BtnType.Character, new rect(x, y, _btnW, _btnH), row3[i]);
            x += _btnW + 4;
         }
         _btns[b++] = new Btn(BtnType.Back, new rect(x, y, w, _btnH), Resources.GetBitmap(Resources.BitmapResources.del));

         // Row 4
         y += _btnH + 6;
         i = _keyBuffer.Width - w - w - _btnW - _btnW - 20;
         x = 0;

         switch (_scrn)
         {
            case 0:
               _btns[b++] = new Btn(BtnType.Num, new rect(x, y, w, _btnH), "?123");
               break;
            default:
               _btns[b++] = new Btn(BtnType.Abc, new rect(x, y, w, _btnH), "ABC");
               break;
         }
         x += w + 4;
         _btns[b++] = new Btn(BtnType.Character, new rect(x, y, _btnW, _btnH), "@");
         x += _btnW + 4;
         _btns[b++] = new Btn(BtnType.Character, new rect(x, y, i, _btnH), " ");
         x += i + 4;
         _btns[b++] = new Btn(BtnType.Character, new rect(x, y, _btnW, _btnH), ".");
         x += _btnW + 4;
         _btns[b] = new Btn(BtnType.Character, new rect(x, y, w, _btnH), ".com");
      }

      private void EnableButtons()
      {
         bool b = _caret > 0;
         if (_btns[0].Enabled != b)
         {
            _btns[0].Enabled = b;
            if (Parent != null)
            {
               _iSel = 0;
               _renderRequested = true;
            }
         }

         b = _caret < _text.Length;
         if (_btns[1].Enabled != b)
         {
            _btns[1].Enabled = b;
            if (Parent != null)
            {
               _iSel = 1;
               _renderRequested = true;
            }
         }

         b = _text.Length != 0;
         if (_btns[2].Enabled != b)
         {
            _btns[2].Enabled = b;
            if (Parent != null)
            {
               _iSel = 2;
               _renderRequested = true;
            }
         }
      }

      private void SensitiveShift(string value)
      {
         if ((value == "\n" || (value == " " && (_lastc == '.' || _lastc == '!' || _lastc == '?'))) && _shft == 0 && _scrn == 0)
            Shift();
         else if (_shft == 1 && value != " ")
         {
            _shft = 2;
            Shift();
         }
      }

      private void SetLayout()
      {
         _scrn = 0;
         switch (_layout)
         {
            case KeyboardLayout.AZERTY:
               _btns = new Btn[37];
               CreateButtons(
                  new[] { "A", "Z", "E", "R", "T", "Y", "U", "I", "O", "P" }, 
                  new[] { "Q", "S", "D", "F", "G", "H", "J", "K", "L", "M" }, 
                  new[] { "W", "X", "C", "V", "B", "N" });
               break;

            case KeyboardLayout.Numeric:
               _btns = new Btn[16];

               int i;
               int b = 4;
               string[] row1 = { "7", "8", "9" };
               string[] row2 = { "4", "5", "6" };
               string[] row3 = { "1", "2", "3" };
               string[] row4 = { ",", "0", "." };

               // Row 1
               int y = _btnH + 6;
               int x = (_keyBuffer.Width / 2 - (((row1.Length * _btnW) + (row1.Length * 4)) / 2));
               for (i = 0; i < row1.Length; i++)
               {
                  _btns[b++] = new Btn(BtnType.Character, new rect(x, y, _btnW, _btnH), row1[i]);
                  x += _btnW + 4;
               }

               // Row 2
               y += _btnH + 6;
               x = (_keyBuffer.Width / 2 - (((row2.Length * _btnW) + (row2.Length * 4)) / 2));
               for (i = 0; i < row2.Length; i++)
               {
                  _btns[b++] = new Btn(BtnType.Character, new rect(x, y, _btnW, _btnH), row2[i]);
                  x += _btnW + 4;
               }

               // Row 3
               y += _btnH + 6;
               x = (_keyBuffer.Width / 2 - (((row3.Length * _btnW) + (row3.Length * 4)) / 2));
               for (i = 0; i < row3.Length; i++)
               {
                  _btns[b++] = new Btn(BtnType.Character, new rect(x, y, _btnW, _btnH), row3[i]);
                  x += _btnW + 4;
               }

               // Row 3
               y += _btnH + 6;
               x = (_keyBuffer.Width / 2 - (((row4.Length * _btnW) + (row4.Length * 4)) / 2));
               for (i = 0; i < row4.Length; i++)
               {
                  _btns[b++] = new Btn(BtnType.Character, new rect(x, y, _btnW, _btnH), row4[i]);
                  x += _btnW + 4;
               }
               break;

            case KeyboardLayout.QWERTY:
               _btns = new Btn[37];
               CreateButtons(
                  new[] { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" }, 
                  new[] { "A", "S", "D", "F", "G", "H", "J", "K", "L" }, 
                  new[] { "Z", "X", "C", "V", "B", "N", "M" });
               break;
         }
         AddFirstBtns();
         RenderButtons();
      }

      private void SetLayoutAlt()
      {
         _shft = 0;
         _btns = new Btn[37];
         AddFirstBtns();
         _scrn = 2;
         CreateButtons(
            new[] { "~", "`", "|", "·", "µ", "≠", "{", "}" }, 
            new[] { "√", "$", "Ω", "°", "^", "_", "=", "[", "]" }, 
            new[] { "™", "®", "©", "¶", "\\", "<", ">" });
         RenderButtons();
      }

      private void SetLayoutNumeric()
      {
         _shft = 0;
         _btns = new Btn[37];
         AddFirstBtns();
         _scrn = 1;
         CreateButtons(
            new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" }, 
            new[] { "-", "/", ":", ";", "(", ")", "$", "&", "@" }, 
            new[] { "\"", ",", "?", "!", "'", "#", "%" });
         RenderButtons();
      }

      private void Shift()
      {
         _shft = 1;
         for (int i = 4; i < _btns.Length; i++)
         {
            switch (_btns[i].Type)
            {
               case BtnType.Shift:
               case BtnType.ShiftLock:
               case BtnType.ShiftUnlock:
                  _btns[i].Image = Resources.GetBitmap(Resources.BitmapResources.shift2);
                  _btns[i].Type = BtnType.Shift;
                  RenderButton(i, true);
                  return;
            }
         }
      }

      private void Unlock()
      {
         _shft = 0;
         for (int i = 4; i < _btns.Length; i++)
         {
            switch (_btns[i].Type)
            {
               case BtnType.Shift:
               case BtnType.ShiftLock:
               case BtnType.ShiftUnlock:
                  _btns[i].Image = Resources.GetBitmap(Resources.BitmapResources.shift1);
                  _btns[i].Type = BtnType.Shift;
                  RenderButton(i, true);
                  return;
            }
         }
      }

      private void RenderButtons()
      {
         while (_rendering)
         { }
         lock (_keyBuffer)
         {
            _keyBuffer.DrawRectangle(0, 0, 0, 0, _keyBuffer.Width, _keyBuffer.Height, 0, 0, Colors.LightGray, 0, 0, Colors.LightGray, 0, 0, 256);

            int i;
            for (i = 0; i < _btns.Length; i++)
            {
               Color btm;
               Color top;
               Color fore;
               if (_btns[i].Type != BtnType.Character)
               {
                  btm = ColorUtility.ColorFromRGB(123, 131, 143);
                  top = ColorUtility.ColorFromRGB(156, 162, 175);
                  fore = Colors.White;
               }
               else
               {
                  btm = Colors.LightGray;
                  top = Colors.White;
                  fore = 0;
               }

               if (_btns[i].Rect.Width != 0)
               {
                  // Draw Background
                  if (_btns[i].Pressed)
                     _keyBuffer.DrawRectangle(Colors.DarkGray, 1, _btns[i].Rect.X, _btns[i].Rect.Y, _btns[i].Rect.Width - 1, _btns[i].Rect.Height - 1, 0, 0, btm, 0, 0, btm, 0, 0, 256);
                  else
                     _keyBuffer.DrawRectangle(Colors.DarkGray, 1, _btns[i].Rect.X, _btns[i].Rect.Y, _btns[i].Rect.Width - 1, _btns[i].Rect.Height - 1, 0, 0, top, 0, 0, top, 0, 0, 256);

                  // Draw Image
                  if (_btns[i].Image != null)
                     _keyBuffer.DrawImage(_btns[i].Rect.X + (_btns[i].Rect.Width / 2 - _btns[i].Image.Width / 2), _btns[i].Rect.Y + (_btns[i].Rect.Height / 2 - _btns[i].Image.Height / 2), _btns[i].Image, 0, 0, _btns[i].Image.Width, _btns[i].Image.Height, (_btns[i].Enabled) ? (ushort)256 : (ushort)128);

                  // Vertically Center Text
                  if (_btns[i].Text != null)
                  {
                     int w;
                     int h;
                     _font.ComputeTextInRect(_btns[i].Text, out w, out h, _btns[i].Rect.Width - 6);
                     _keyBuffer.DrawTextInRect(_btns[i].Text, _btns[i].Rect.X + 4, _btns[i].Rect.Y + (_btns[i].Rect.Height / 2 - h / 2) - 1, _btns[i].Rect.Width - 8, _font.Height, Bitmap.DT_AlignmentCenter + Bitmap.DT_TrimmingNone, (_btns[i].Enabled) ? fore : Colors.DarkGray, _font);
                  }
               }
            }
         }
         _keyRr = new rect(0, 0, _keyBuffer.Width, _keyBuffer.Height);
         _renderRequested = true;
      }

      private void RenderButton(int buttonIndex, bool force = false)
      {
         while (_rendering)
         { }

         _rendering = true;
         lock (_keyBuffer)
         {
            Color btm;
            Color fore;
            Color top;
            if (_btns[buttonIndex].Type != BtnType.Character)
            {
               btm = ColorUtility.ColorFromRGB(123, 131, 143);
               top = ColorUtility.ColorFromRGB(156, 162, 175);
               fore = Colors.White;
            }
            else
            {
               btm = Colors.LightGray;
               top = Colors.White;
               fore = 0;
            }

            if (_btns[buttonIndex].Rect.Width != 0)
            {
               // Draw Background
               if (_btns[buttonIndex].Pressed)
                  _keyBuffer.DrawRectangle(Colors.DarkGray, 1, _btns[buttonIndex].Rect.X, _btns[buttonIndex].Rect.Y, _btns[buttonIndex].Rect.Width - 1, _btns[buttonIndex].Rect.Height - 1, 0, 0, btm, 0, 0, btm, 0, 0, 256);
               else
                  _keyBuffer.DrawRectangle(Colors.DarkGray, 1, _btns[buttonIndex].Rect.X, _btns[buttonIndex].Rect.Y, _btns[buttonIndex].Rect.Width - 1, _btns[buttonIndex].Rect.Height - 1, 0, 0, top, 0, 0, top, 0, 0, 256);

               // Draw Image
               if (_btns[buttonIndex].Image != null)
                  _keyBuffer.DrawImage(_btns[buttonIndex].Rect.X + (_btns[buttonIndex].Rect.Width / 2 - _btns[buttonIndex].Image.Width / 2), _btns[buttonIndex].Rect.Y + (_btns[buttonIndex].Rect.Height / 2 - _btns[buttonIndex].Image.Height / 2), _btns[buttonIndex].Image, 0, 0, _btns[buttonIndex].Image.Width, _btns[buttonIndex].Image.Height, (_btns[buttonIndex].Enabled) ? (ushort)256 : (ushort)128);

               // Vertically Center Text
               if (_btns[buttonIndex].Text != null)
               {
                  int w;
                  int h;
                  _font.ComputeTextInRect(_btns[buttonIndex].Text, out w, out h, _btns[buttonIndex].Rect.Width - 6);
                  _keyBuffer.DrawTextInRect(_btns[buttonIndex].Text, _btns[buttonIndex].Rect.X + 4, _btns[buttonIndex].Rect.Y + (_btns[buttonIndex].Rect.Height / 2 - h / 2) - 1, _btns[buttonIndex].Rect.Width - 8, _font.Height, Bitmap.DT_AlignmentCenter + Bitmap.DT_TrimmingNone, (_btns[buttonIndex].Enabled) ? fore : Colors.DarkGray, _font);
               }
            }
         }

         _keyRr = _btns[buttonIndex].Rect;
         _renderRequested = true;
         _rendering = false;

         if (force)
         {
            Core.Screen.DrawImage(_vkX + _keyRr.X, _vkY + _keyRr.Y, _keyBuffer, _keyRr.X, _keyRr.Y, _keyRr.Width, _keyRr.Height);
            Core.SafeFlush(_vkX + _keyRr.X, _vkY + _keyRr.Y, _keyRr.Width, _keyRr.Height);
         }
      }

      #endregion

      #region Textbox

      private void HideCharacters()
      {
         Thread.Sleep(750);
         ShowChar = -1;

         //int w = FontManager.ComputeExtentEx(_font, _text.Substring(_caret - 1, 1)).Width;
         //int sw = FontManager.ComputeExtentEx(_font, new string(new[] { _pwd })).Width;
         //_caretX -= w - sw;

         Invalidate();
      }

      public void InsertStrChr(string chr)
      {
         int w, h;

         _font.ComputeExtent(chr, out w, out h);

         if (_caret == -1)
            CurrentCharacter = 0;

         if (_pwd != ' ')
         {
            _thShow = null;
            ShowChar = _caret;
            _thShow = new Thread(HideCharacters);
            _thShow.Start();
         }

         if (_caret == 0)
            _text = chr + _text;
         else
            _text = _text.Substring(0, _caret) + chr + _text.Substring(_caret);

         _caret += 1;
         _caretX += w;

         if (_caretX > Width)
            _scrollX = _caretX - Width + 20;

         EnableButtons();
         _renderRequested = true;
      }

      private void UpdateCaretPos()
      {
         _caretX = _caret == -1 ? 0 : FontManager.ComputeExtentEx(_font, _text.Substring(0, _caret)).Width;
         _renderRequested = true;
      }

      private string PwdString()
      {
         if (_pwd == ' ')
            return _text;

         string strOut = string.Empty;

         if (ShowChar == -1 || ShowChar >= _text.Length)
         {
            for (int i = 0; i < _text.Length; i++)
               strOut += _pwd;
         }
         else
         {
            for (int i = 0; i < ShowChar; i++)
               strOut += _pwd;

            strOut += _text.Substring(ShowChar, 1);

            for (int i = ShowChar + 1; i < _text.Length; i++)
               strOut += _pwd;

         }
         return strOut;
      }

      protected internal void Return()
      {
         if (_caret == -1)
         {
            SendTouchDown(this, new point(5, 5));
            SendTouchUp(this, new point(5, 5));
         }

         if (_caret == 0)
            _text = "\n" + _text;
         else
            _text = _text.Substring(0, _caret) + "\n" + _text.Substring(_caret);
         _caret += 1;
         _iLine += 1;

         _caretX = Left + 5;
         _caretY += _font.Height;

         _renderRequested = true;
      }

      protected internal void Backspace()
      {
         if (_caret == 0)
            return;

         int w, h;
         string chr = PwdString().Substring(_caret - 1, 1);
         _font.ComputeExtent(chr, out w, out h);

         _text = _text.Substring(0, _caret - 1) + _text.Substring(_caret--);
         _caretX -= w;
         _renderRequested = true;
      }

      #endregion

   }
}
