using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// Built off of ReaLTaiizor's Observable List Utility to work with .NET 4.7.2
// https://github.com/Taiizor/ReaLTaiizor

namespace BedrockCosmos.App.UI
{
    public class ObservableListModified<T> : EventArgs
    {
        public List<T> Items { get; private set; }

        public ObservableListModified(List<T> items)
        {
            Items = items;
        }

        public ObservableListModified(T item)
        {
            Items = new List<T> { item };
        }
    }

    public class ObservableList<T> : Collection<T>, IDisposable
    {
        public event EventHandler<ObservableListModified<T>> ItemsAdded;
        public event EventHandler<ObservableListModified<T>> ItemsRemoved;

        private bool _disposed;

        public ObservableList()
            : base()
        {
        }

        public ObservableList(IList<T> list)
            : base(list)
        {
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            OnItemsAdded(new List<T> { item });
        }

        protected override void RemoveItem(int index)
        {
            T item = this[index];

            base.RemoveItem(index);

            OnItemsRemoved(new List<T> { item });
        }

        protected override void SetItem(int index, T item)
        {
            T oldItem = this[index];

            base.SetItem(index, item);

            OnItemsRemoved(new List<T> { oldItem });
            OnItemsAdded(new List<T> { item });
        }

        protected override void ClearItems()
        {
            List<T> items = new List<T>(Items);

            base.ClearItems();

            if (items.Count > 0)
            {
                OnItemsRemoved(items);
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            List<T> added = new List<T>(items);

            foreach (T item in added)
            {
                Add(item);
            }
        }

        // Reorders the list in place without raising ItemsAdded / ItemsRemoved,
        // since a sort is a reorder, not a structural add/remove.
        public void Sort(IComparer<T> comparer)
        {
            if (comparer == null || Count <= 1)
            {
                return;
            }

            List<T> sorted = new List<T>(Items);
            sorted.Sort(comparer);

            // Manipulate the protected Items list directly so InsertItem /
            // RemoveItem (and therefore the events) are not triggered.
            Items.Clear();

            foreach (T item in sorted)
            {
                Items.Add(item);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            ItemsAdded = null;
            ItemsRemoved = null;

            _disposed = true;
        }

        private void OnItemsAdded(List<T> items)
        {
            ItemsAdded?.Invoke(this, new ObservableListModified<T>(items));
        }

        private void OnItemsRemoved(List<T> items)
        {
            ItemsRemoved?.Invoke(this, new ObservableListModified<T>(items));
        }
    }
}