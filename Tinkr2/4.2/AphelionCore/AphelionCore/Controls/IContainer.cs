using System;

namespace Skewworks.NETMF.Controls
{
   /// <summary>
   /// Provides an interface to use as a basis for container controls
   /// </summary>
   public interface IContainer : IControl
   {
      /// <summary>
      /// Gets/Sets the control that is currently focused inside the container
      /// </summary>
      /// <remarks>
      /// The active child is the only child that will receive button and keyboard events.
      /// </remarks>
      IControl ActiveChild { get; set; }

      /// <summary>
      /// Gets an array of children
      /// </summary>
      IControl[] Children { get; }

      /// <summary>
      /// Gets the containers ability to be rendered
      /// </summary>
      bool CanRender { get; }

      /// <summary>
      /// Returns the top-most parent
      /// </summary>
      /// <remarks>
      /// If container does not have a parent TopLevelContainer returns reference to current container.
      /// </remarks>
      IContainer TopLevelContainer { get; }

      /// <summary>
      /// Sets the containers <see cref="Control.Suspended"/> property to false without redrawing the container
      /// </summary>
      void QuiteUnsuspend();

      /// <summary>
      /// Adds a child control to the container
      /// </summary>
      /// <param name="child">New child to add</param>
      void AddChild(IControl child);

      /// <summary>
      /// Unsafely renders control.
      /// </summary>
      /// <param name="region">Rectangular region of the control to be rendered.</param>
      /// <param name="flush">When true the updates will be pushed to the screen, otherwise they will sit in the buffer</param>
      /// <remarks>
      /// Rendering a control will not cause other controls in the same space to be rendered, calling this method can break z-index ordering.
      /// If it is certain no other controls will overlap the rendered control calling Render() can result in faster speeds than Invalidate().
      /// </remarks>
      void Render(rect region, bool flush = false);

      /// <summary>
      /// Updates the z-index of the specified control so it is rendered on top.
      /// </summary>
      /// <param name="child">Control to move to the top of the z-index</param>
      void BringControlToFront(IControl child);

      /// <summary>
      /// Removes all children from container.
      /// </summary>
      /// <param name="disposeChildren">When true children are completely disposed of. Otherwise children are simply removed from the container.</param>
      void ClearChildren(bool disposeChildren = true);

      /// <summary>
      /// Returns a child by its index.
      /// </summary>
      /// <param name="index">The index inside of the array of controls to return</param>
      /// <returns>Returns the control or null if the index is out of range.</returns>
      IControl GetChildByIndex(int index);

      /// <summary>
      /// Gets a child control by its name.
      /// </summary>
      /// <param name="name">Name of the control</param>
      /// <returns>Returns the control or null if not found.</returns>
      /// <remarks>
      /// The name is compared case sensitive.
      /// </remarks>
      IControl GetChildByName(string name);

      /// <summary>
      /// Returns the index associated with the child.
      /// </summary>
      /// <param name="child">Child to be looked up</param>
      /// <returns>Returns the index of the child or -1 if not found.</returns>
      int GetChildIndex(IControl child);

      /// <summary>
      /// Moves the focus from the active to the next child.
      /// </summary>
      /// <remarks>
      /// If no child is currently active, the 1st child is gets the active one.
      /// </remarks>
      void NextChild();

      /// <summary>
      /// Moves the focus from the active to the previous child.
      /// </summary>
      /// <remarks>
      /// If no child is currently active, the 1st child is gets the active one.
      /// </remarks>
      void PreviousChild();

      /// <summary>
      /// Removes a child from the container
      /// </summary>
      /// <param name="child">Child to remove</param>
      void RemoveChild(IControl child);

      /// <summary>
      /// Removes child by index
      /// </summary>
      /// <param name="index">Index of child to be removed.</param>
      void RemoveChildAt(int index);
   }
}
