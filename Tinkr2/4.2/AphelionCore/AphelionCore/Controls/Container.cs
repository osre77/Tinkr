using System;

//TODO: check out what this TapAndHold preparation was good for

namespace Skewworks.NETMF.Controls
{
   /// <summary>
   /// 
   /// </summary>
   [Serializable]
   public class Container : Control, IContainer
   {

      #region Variables

      // Children
      private IControl[] _children;
      private IControl _active;

      // States
      //private bool _enabled = true;
      // ReSharper disable once ConvertToConstant.Local
      private readonly bool _visible = true;
      private bool _suspended;

      // Touch
      /*
              private bool _mDown;            // Touching when true

              private point _ptTapHold;       // Location where touch down occurred
              private Thread _thHold;         // Tap & Hold thread
              private long _lStop;            // Stop waiting for hold after this tick
              private TapState _eTapHold;     // Current tap state
              private long _lastTap;          // Tick count of last time occurrance
      */
      // Dispose
      //private bool _disposing;
      private IControl _removing;

      #endregion

      #region Properties

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

      public virtual bool CanRender
      {
         get
         {
            return !((Core.ActiveContainer != this && Parent == null) || (Parent != null && !Parent.CanRender) || !_visible || _suspended);
         }
      }

      public IControl[] Children
      {
         get { return _children; }
      }

      public virtual IContainer TopLevelContainer
      {
         get
         {
            if (Parent == null)
               return this;
            return Parent.TopLevelContainer;
         }
      }

      #endregion

      #region Public Methods

      /// <summary>
      /// Adds a AddChild
      /// </summary>
      /// <param name="child">Control to add</param>
      public virtual void AddChild(IControl child)
      {
         // Update Array Size
         if (_children == null)
            _children = new IControl[1];
         else
         {
            var tmp = new IControl[_children.Length + 1];
            Array.Copy(_children, tmp, _children.Length);
            _children = tmp;
         }

         // Set control properties
         child.Parent = this;
         child.UpdateOffsets();

         // Assign Value
         _children[_children.Length - 1] = child;

         // Render
         if (_active == null && child.CanFocus)
         {
            _active = child;
            _active.Activate();
         }

         Render(child.ScreenBounds, true);
      }

      public virtual void BringControlToFront(IControl child)
      {
         int idx = GetChildIndex(child);

         if (idx == -1)
            return;

         var tmp = new IControl[_children.Length];
         int c = 0;

         tmp[c++] = child;

         for (int i = 0; i < tmp.Length; i++)
         {
            if (i != idx)
               tmp[c++] = _children[i];
         }
         //tmp[c] = _children[idx];
         _children = tmp;
      }

      /// <summary>
      /// Removes all controls
      /// </summary>
      /// <param name="disposeChildren">true if children should be dispose</param>
      public virtual void ClearChildren(bool disposeChildren = true)
      {
         if (_children == null)
            return;

         for (int i = 0; i < _children.Length; i++)
         {
            if (disposeChildren)
               _children[i].Dispose();
            else
               _children[i].Parent = null;
         }

         _children = null;
         Render(true);
      }

      /// <summary>
      /// Returns a control by index
      /// </summary>
      /// <param name="index">Index of control</param>
      /// <returns>Control at index</returns>
      public virtual IControl GetChildByIndex(int index)
      {
         if (_children == null || index < 0 || index >= _children.Length)
            return null;
         return _children[index];
      }

      /// <summary>
      /// Returns a control by name
      /// </summary>
      /// <param name="name">Name of control</param>
      /// <returns>Retrieved control</returns>
      public virtual IControl GetChildByName(string name)
      {
         if (_children == null)
            return null;

         for (int i = 0; i < _children.Length; i++)
            if (_children[i].Name == name)
               return _children[i];

         return null;
      }

      public virtual int GetChildIndex(IControl child)
      {
         if (_children == null)
            return -1;

         for (int i = 0; i < _children.Length; i++)
            if (_children[i] == child)
               return i;

         return -1;
      }

      public virtual void NextChild()
      {
         if (_children == null)
            return;

         if (_active == null)
         {
            FocusFirst();
            return;
         }

         int i;
         int ai = ActiveChildIndex();
         if (ai == -1)
            return;

         for (i = ai + 1; i < _children.Length; i++)
         {
            if (_children[i].CanFocus)
            {
               ActiveChild = _children[i];
               return;
            }
         }

         for (i = ai - 1; i > -1; i--)
         {
            if (_children[i].CanFocus)
            {
               ActiveChild = _children[i];
               return;
            }
         }

      }

      public virtual void PreviousChild()
      {
         if (_children == null)
            return;

         if (_active == null)
         {
            FocusFirst();
            return;
         }

         int i;
         int ai = ActiveChildIndex();
         if (ai == -1)
            return;

         for (i = ai - 1; i > 0; i--)
         {
            if (_children[i].CanFocus)
            {
               ActiveChild = _children[i];
               return;
            }
         }

         for (i = ai + 1; i < _children.Length; i++)
         {
            if (_children[i].CanFocus)
            {
               ActiveChild = _children[i];
               return;
            }
         }
      }

      /// <summary>
      /// Removes a specific control
      /// </summary>
      /// <param name="child">Child to remove</param>
      public virtual void RemoveChild(IControl child)
      {
         if (_children == null || child == _removing)
            return;

         for (int i = 0; i < _children.Length; i++)
         {
            if (_children[i] == child)
            {
               RemoveChildAt(i);
               return;
            }
         }
      }

      /// <summary>
      /// Removes a control by index
      /// </summary>
      /// <param name="index">Index of control</param>
      public virtual void RemoveChildAt(int index)
      {
         if (_children == null || index < 0 || index >= _children.Length)
            return;

         if (_children.Length == 1)
         {
            ClearChildren();
            return;
         }

         bool toggleSuspend = true;
         if (_suspended)
            toggleSuspend = false;
         else
            _suspended = true;

         rect bnd = _children[index].ScreenBounds;
         _removing = _children[index];
         _children[index].Parent = null;

         var tmp = new IControl[_children.Length - 1];
         int c = 0;
         for (int i = 0; i < _children.Length; i++)
         {
            if (i != index)
               tmp[c++] = _children[i];
         }

         _children = tmp;
         _removing = null;

         if (toggleSuspend)
         {
            _suspended = false;
            Render(bnd, true);
         }
      }

      #endregion

      #region Private Methods

      private int ActiveChildIndex()
      {
         for (int i = 0; i < _children.Length; i++)
         {
            if (_active == _children[i])
               return i;
         }
         return -1;
      }

      // ReSharper disable once UnusedMethodReturnValue.Local
      private bool FocusFirst()
      {
         for (int i = 0; i < _children.Length; i++)
         {
            if (_children[i].CanFocus)
            {
               ActiveChild = _children[i];
               return true;
            }
         }
         return false;
      }

      #endregion

      #region Buttons

      protected new virtual void ButtonPressedMessage(int buttonId, ref bool handled) { }

      protected new virtual void ButtonReleasedMessage(int buttonId, ref bool handled) { }

      public new virtual void SendButtonEvent(int buttonId, bool pressed)
      {
         bool handled = false;
         if (pressed)
         {
            ButtonPressedMessage(buttonId, ref handled);
            if (handled)
               return;

            OnButtonPressed(this, buttonId);
            if (buttonId == (int)ButtonIDs.Click)
               SendTouchDown(this, Core.MousePosition);
         }
         else
         {
            ButtonReleasedMessage(buttonId, ref handled);
            if (handled)
               return;

            if (buttonId == (int)ButtonIDs.Click)
               SendTouchUp(this, Core.MousePosition);
            OnButtonReleased(this, buttonId);
         }
      }

      #endregion

      #region Touch

      /// <summary>
      /// Method for detecting Tap & Hold
      /// </summary>
      /*private void TapHoldWaiter()
      {
          while (DateTime.Now.Ticks < _lStop)
          {
              Thread.Sleep(10);
              if (_eTapHold == TapState.Normal)
                  return;
          }
          if (_eTapHold == TapState.Normal || !_enabled || !_visible || _suspended)
              return;

          //_mDown = false;
          _eTapHold = TapState.Normal;

          OnTapHold(this, _ptTapHold);
      }*/

      #endregion

      #region Keyboard

      protected new virtual void KeyboardAltKeyMessage(int key, bool pressed, ref bool handled) { }

      protected new virtual void KeyboardKeyMessage(char key, bool pressed, ref bool handled) { }

      public new virtual void SendKeyboardAltKeyEvent(int key, bool pressed)
      {
         bool handled = false;
         KeyboardAltKeyMessage(key, pressed, ref handled);
         if (!handled)
         {
            OnKeyboardAltKey(this, key, pressed);
         }
      }

      public new virtual void SendKeyboardKeyEvent(char key, bool pressed)
      {
         bool handled = false;
         KeyboardKeyMessage(key, pressed, ref handled);
         if (!handled)
         {
            if (pressed)
            {
               if (key == 9)
               {
                  if (Core.KeyboardShiftDown)
                     PreviousChild();
                  else
                     NextChild();
                  return;
               }
            }
            OnKeyboardKey(this, key, pressed);
         }
      }

      #endregion

      #region Focus

      public new virtual void Activate()
      { }

      public new virtual void Blur()
      { }

      public new virtual bool HitTest(point e)
      {
         return ScreenBounds.Contains(e);
      }

      #endregion

      #region GUI

      public void QuiteUnsuspend()
      {
         _suspended = false;
      }

      public void Render(rect region, bool flush = false)
      {
         // Check if we actually need to render
         if (!CanRender)
            return;

         if (region.X > Left + Width || region.Y > Top + Height)
            return;

         // Update Region
         if (region.X < Left)
            region.X = Left;
         if (region.Y < Top)
            region.Y = Top;
         if (region.X + region.Width > Core.ScreenWidth)
            region.Width = Core.ScreenWidth - region.X;
         if (region.Y + region.Height > Core.ScreenHeight)
            region.Height = Core.ScreenHeight - region.Y;

         // Check if we actually need to render
         if ((Core.ActiveContainer != this && Parent == null) || !_visible || _suspended)
            return;

         // Update clip
         Core.Screen.SetClippingRectangle(region.X, region.Y, region.Width, region.Height);

         // Render control
         lock (Core.Screen)
            OnRender(region.X, region.Y, region.Width, region.Height);

         // Reset clip
         //Core.Screen.SetClippingRectangle(0, 0, Core.ScreenWidth, Core.ScreenHeight);

         // Flush as needed
         if (flush)
            Core.SafeFlush(region.X, region.Y, region.Width, region.Height);
      }

      #endregion

   }
}
