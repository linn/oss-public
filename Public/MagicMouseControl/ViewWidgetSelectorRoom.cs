using System;

using Linn.Kinsky;

namespace MagicMouseControl
{
    public class ViewWidgetSelectorRoom : IViewWidgetSelector<Room>
    {
        public ViewWidgetSelectorRoom()
        {
        }

        public void Open()
        {
        }

        public void Close()
        {
        }

        public void InsertItem(int aIndex, Room aItem)
        {
            Console.WriteLine(aItem);
        }

        public void RemoveItem(Room aItem)
        {
        }

        public void ItemChanged(Room aItem)
        {
        }

        public void SetSelected(Room aItem)
        {
            iRoom = aItem;
        }

        public event EventHandler<EventArgsSelection<Room>> EventSelectionChanged;

        public Room Room
        {
            get
            {
                return iRoom;
            }
        }

        private Room iRoom;
    }
}

