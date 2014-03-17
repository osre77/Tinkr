using System;

//TODO: check out what this TapAndHold preparation was good for

namespace Skewworks.NETMF.Controls
{
   /// <summary>
   /// Base class for creating container controls
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
      //private readonly bool _visible = true;

      // Touch
      /*
              private bool _mDown;            // Touching when true

              private point _ptTapHold;       // Location where touch down occurred
              private Thread _thHold;         // Tap & Hold thread
              private long _lStop;            // Stop waiting for hold after this tick
              private TapState _eTapHold;     // Current tap state
              private long _lastTap;          // Tick count of last time occurrence
      */
      // Dispose
      //private bool _disposing;
      private IControl _removing;

      #endregion

      #region Properties

      /// <summary>
      /// Gets/Sets the control that is currently focused inside the container
      /// </summary>
      /// <remarks>
      /// The active child is the only child that will receive button and keyboard events.
      /// </remarks>
      public virtual IControl ActiveChild
      {
         get { return _active; }
         set
         {
            if (_active == value)
            {
               return;
            }

            IControl tmp = _active;
            _active = null;

            if (tmp != null)
            {
               tmp.Blur();
            }

            _active = value;
            if (_active != null)
            {
               _active.Activate();
            }
         }
      }

      /// <summary>
      /// Gets the containers ability to be rendered
      /// </summary>
      public virtual bool CanRender
      {
         get
         {
            return !((Core.ActiveContainer != this && Parent == null) || (Parent != null && !Parent.CanRender) || !Visible || InternalSuspended);
         }
      }

      /// <summary>
      /// Gets an array of children
      /// </summary>
      public IControl[] Children
      {
         get { return _children; }
      }

      /// <summary>
      /// Returns the top-most parent
      /// </summary>
      /// <remarks>
      /// If container does not have a parent TopLevelContainer returns reference to current container.
      /// </remarks>
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
      /// Adds a child control to the container
      /// </summary>
      /// <param name="child">New child to add</param>
      public virtual void AddChild(IControl child)
      {
         // Update Array Size
         if (_children == null)
         {
            _children = new IControl[1];
         }
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

      /// <summary>
      /// Updates the z-index of the specified control so it is rendered on top.
      /// </summary>
      /// <param name="child">Control to move to the top of the z-index</param>
      public virtual void BringControlToFront(IControl child)
      {
         int idx = GetChildIndex(child);

         if (idx == -1)
         {
            return;
         }

         var tmp = new IControl[_children.Length];
         int c = 0;

         tmp[c++] = child;

         for (int i = 0; i < tmp.Length; i++)
         {
            if (i != idx)
            {
               tmp[c++] = _children[i];
            }
         }
         //tmp[c] = _children[idx];
         _children = tmp;
      }

      /// <summary>
      /// Removes all children from container.
      /// </summary>
      /// <param name="disposeChildren">When true children are completely disposed of. Otherwise children are simply removed from the container.</param>
      public virtual void ClearChildren(bool disposeChildren = true)
      {
         if (_children == null)
         {
            return;
         }

         for (int i = 0; i < _children.Length; i++)
         {
            if (disposeChildren)
            {
               _children[i].Dispose();
            }
            else
            {
               _children[i].Parent = null;
            }
         }

         _children = null;
         Render(true);
      }

      /// <summary>
      /// Returns a child by its index.
      /// </summary>
      /// <param name="index">The index inside of the array of controls to return</param>
      /// <returns>Returns the control or null if the index is out of range.</returns>
      public virtual IControl GetChildByIndex(int index)
      {
         if (_children == null || index < 0 || index >= _children.Length)
         {
            return null;
         }
         return _children[index];
      }

      /// <summary>
      /// Gets a child control by its name.
      /// </summary>
      /// <param name="name">Name of the control</param>
      /// <returns>Returns the control or null if not found.</returns>
      /// <remarks>
      /// The name is compared case sensitive.
      /// </remarks>
      public virtual IControl GetChildByName(string name)
      {
         if (_children == null)
         {
            return null;
         }

         for (int i = 0; i < _children.Length; i++)
         {
            if (_children[i].Name == name)
            {
               return _children[i];
            }
         }

         return null;
      }

      /// <summary>
      /// Returns the index associated with the child.
      /// </summary>
      /// <param name="child">Child to be looked up</param>
      /// <returns>Returns the index of the child or -1 if not found.</returns>
      public virtual int GetChildIndex(IControl child)
      {
         if (_children == null)
         {
            return -1;
         }

         for (int i = 0; i < _children.Length; i++)
         {
            if (_children[i] == child)
            {
               return i;
            }
         }

         return -1;
      }

      /// <summary>
      /// Moves the focus from the active to the next child.
      /// </summary>
      /// <remarks>
      /// If no child is currently active, the 1st child is gets the active one.
      /// </remarks>
      public virtual void NextChild()
      {
         if (_children == null)
         {
            return;
         }

         if (_active == null)
         {
            FocusFirst();
            return;
         }

         int i;
         int ai = ActiveChildIndex();
         if (ai == -1)
         {
            return;
         }

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

      /// <summary>
      /// Moves the focus from the active to the previous child.
      /// </summary>
      /// <remarks>
      /// If no child is currently active, the 1st child is gets the active one.
      /// </remarks>
      public virtual void PreviousChild()
      {
         if (_children == null)
         {
            return;
         }

         if (_active == null)
         {
            FocusFirst();
            return;
         }

         int i;
         int ai = ActiveChildIndex();
         if (ai == -1)
         {
            return;
         }

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
      /// Removes a child from the container
      /// </summary>
      /// <param name="child">Child to remove</param>
      public virtual void RemoveChild(IControl child)
      {
         if (_children == null || child == _removing)
         {
            return;
         }

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
      /// Removes child by index
      /// </summary>
      /// <param name="index">Index of child to be removed.</param>
      public virtual void RemoveChildAt(int index)
      {
         if (_children == null || index < 0 || index >= _children.Length)
         {
            return;
         }

         if (_children.Length == 1)
         {
            ClearChildren();
            return;
         }

         bool toggleSuspend = true;
         if (InternalSuspended)
         {
            toggleSuspend = false;
         }
         else
         {
            InternalSuspended = true;
         }

         rect bnd = _children[index].ScreenBounds;
         _removing = _children[index];
         _children[index].Parent = null;

         var tmp = new IControl[_children.Length - 1];
         int c = 0;
         for (int i = 0; i < _children.Length; i++)
         {
            if (i != index)
            {
               tmp[c++] = _children[i];
            }
         }

         _children = tmp;
         _removing = null;

         if (toggleSuspend)
         {
            QuiteUnsuspend();
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
            {
               return i;
            }
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

      /* identical to base
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
            {
               return;
            }

            OnButtonPressed(this, buttonId);
            if (buttonId == (int) ButtonIDs.Click)
            {
               SendTouchDown(this, Core.MousePosition);
            }
         }
         else
         {
            ButtonReleasedMessage(buttonId, ref handled);
            if (handled)
            {
               return;
            }

            if (buttonId == (int) ButtonIDs.Click)
            {
               SendTouchUp(this, Core.MousePosition);
            }
            OnButtonReleased(this, buttonId);
         }
      }

      #endregion*/

      #region Touch

      /// <summary>
      /// Override this message to handle key events internally.
      /// </summary>
      /// <param name="key">Integer value of the key affected</param>
      /// <param name="pressed">True if the key is currently being pressed; false if released</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// Override handles moving focus between child on key input by tab key
      /// </remarks>
      protected override void KeyboardKeyMessage(char key, bool pressed, ref bool handled)
      {
         if (!handled)
         {
            if (pressed)
            {
               if (key == 9)
               {
                  if (Core.KeyboardShiftDown)
                  {
                     PreviousChild();
                  }
                  else
                  {
                     NextChild();
                  }
                  return;
               }
            }
         }
         base.KeyboardKeyMessage(key, pressed, ref handled);
      }

      /* identical to base except for tab key handling, noew implemented in KeyboardKeyMessage
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
      }*/

      #endregion

      #region Focus

      /// <summary>
      /// Activates the control
      /// </summary>
      /// <remarks>
      /// Activate is called by a container when a control becomes focused. Calling Activate by itself will only invoke Invalidate().
      /// </remarks>
      public override void Activate()
      {
         // don't call base here
         //TODO: check if this is really wanted
      }

      /// <summary>
      /// Deactivates the control
      /// </summary>
      /// <remarks>
      /// Called by the parent when a control loses focus. If called by itself this will only result in Invalidate() being invoked.
      /// </remarks>
      public override void Blur()
      {
         // don't call base here
         //TODO: check if this is really wanted
      }

      /* identical to base
      public new virtual bool HitTest(point point)
      {
         return ScreenBounds.Contains(point);
      }*/

      #endregion

      #region GUI

      /// <summary>
      /// Sets the containers <see cref="Control.Suspended"/> property to false without redrawing the container
      /// </summary>
      public void QuiteUnsuspend()
      {
         InternalSuspended = false;
      }

      /// <summary>
      /// Unsafely renders control.
      /// </summary>
      /// <param name="region">Rectangular region of the control to be rendered.</param>
      /// <param name="flush">When true the updates will be pushed to the screen, otherwise they will sit in the buffer</param>
      /// <remarks>
      /// Rendering a control will not cause other controls in the same space to be rendered, calling this method can break z-index ordering.
      /// If it is certain no other controls will overlap the rendered control calling Render() can result in faster speeds than Invalidate().
      /// </remarks>
      public void Render(rect region, bool flush = false)
      {
         // Check if we actually need to render
         if (!CanRender)
         {
            return;
         }

         if (region.X > Left + Width || region.Y > Top + Height)
         {
            return;
         }

         // Update Region
         if (region.X < Left)
         {
            region.X = Left;
         }
         if (region.Y < Top)
         {
            region.Y = Top;
         }
         if (region.X + region.Width > Core.ScreenWidth)
         {
            region.Width = Core.ScreenWidth - region.X;
         }
         if (region.Y + region.Height > Core.ScreenHeight)
         {
            region.Height = Core.ScreenHeight - region.Y;
         }

         // Check if we actually need to render
         if ((Core.ActiveContainer != this && Parent == null) || !Visible || InternalSuspended)
         {
            return;
         }

         // Update clip
         Core.Screen.SetClippingRectangle(region.X, region.Y, region.Width, region.Height);

         // Render control
         lock (Core.Screen)
         {
            OnRender(region.X, region.Y, region.Width, region.Height);
         }

         // Reset clip
         //Core.Screen.SetClippingRectangle(0, 0, Core.ScreenWidth, Core.ScreenHeight);

         // Flush as needed
         if (flush)
         {
            Core.SafeFlush(region.X, region.Y, region.Width, region.Height);
         }
      }

      #endregion
   }
}
