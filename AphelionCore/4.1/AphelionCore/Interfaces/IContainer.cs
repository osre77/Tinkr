using System;
using Microsoft.SPOT;

namespace Skewworks.NETMF.Controls
{
    public interface IContainer : IControl
    {

        IControl ActiveChild { get; set; }
        IControl[] Children { get; }
        bool CanRender { get; }
        IContainer TopLevelContainer { get; }
      
        void QuiteUnsuspend();
        void AddChild(IControl child);
        void Render(rect region, bool flush = false);
        void BringControlToFront(IControl child);
        void ClearChildren(bool DisposeChildren = true);
        IControl GetChildByIndex(int index);
        IControl GetChildByName(string name);
        int GetChildIndex(IControl child);
        void NextChild();
        void PreviousChild();
        void RemoveChild(IControl child);
        void RemoveChildAt(int index);
    }
}
