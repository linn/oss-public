using Linn.Gui;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using System;
using System.Windows.Forms;

namespace Linn {
namespace Kinsky {
    
public abstract class ViewNodeList
{
    public ViewNodeList(Node aRoot, string aName)
    {
        Assert.Check(aRoot != null);
        Assert.Check(aName != null);

        iRoot = aRoot;

        iList = (NodeList)iRoot.Search(aName);

        Assert.Check(iList != null);

        iKeyBindings = new KeyBindings();
    }
    
    protected bool ProcessKeyboard(Linn.Gui.Resources.Message aMessage)
    {
        if(iList.Focused && iList.Active && aMessage.Fullname == "")
        {
            MsgKeyDown key = aMessage as MsgKeyDown;

            if(key != null)
            {
                string action = iKeyBindings.Action(key.Key);

                if(action == "PageUp")
                {
                    Messenger.Instance.PresentationMessage(new MsgScroll(iList, (int)-iList.LineCount));
                }
                else if(action == "PageDown")
                {
                    Messenger.Instance.PresentationMessage(new MsgScroll(iList, (int)iList.LineCount));
                }
                else if(action == "Up")
                {
                    Messenger.Instance.PresentationMessage(new MsgScroll(iList, -1));
                }
                else if(action == "Down")
                {
                    Messenger.Instance.PresentationMessage(new MsgScroll(iList, 1));
                }
                else if(action == "Select")
                {
                    int index = iList.HighlightedIndex;

                    if(index > -1 && index < iList.ListEntryProvider.Count)
                    {
                        IListable listable = iList.ListEntryProvider.Entries((uint)index, 1)[0];

                        listable.Select();

                        Messenger.Instance.ApplicationMessage(new MsgSelect(iList, index, listable));
                    }
                }

                return (true);
            }
        }

        return (false);
    }

    protected Node iRoot;
    protected NodeList iList;
    protected KeyBindings iKeyBindings;
}

} // Kinsky
} // Linn
