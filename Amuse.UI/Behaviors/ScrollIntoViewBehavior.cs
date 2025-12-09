using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Amuse.UI.Behaviors
{
    public class ScrollIntoViewBehavior : Behavior<ListBox>
    {
        private string _trackedProperties;
        private List<string> _trackedPropertiesList;


        /// <summary>
        /// Gets or sets the tracked properties in comma separated format.
        /// </summary>
        /// <value>
        /// The tracked properties.
        /// </value>
        public string TrackedProperties
        {
            get { return _trackedProperties; }
            set
            {
                _trackedProperties = value;
                _trackedPropertiesList?.Clear();
                if (!string.IsNullOrEmpty(_trackedProperties))
                {
                    _trackedPropertiesList = _trackedProperties
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .ToList();
                }
            }
        }


        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        /// <remarks>
        /// Override this to hook up functionality to the AssociatedObject.
        /// </remarks>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }


        /// <summary>
        /// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        /// <remarks>
        /// Override this to unhook functionality from the AssociatedObject.
        /// </remarks>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
        }


        /// <summary>
        /// Handles the SelectionChanged event of the ListBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox listBox)
                return;

            if (e.RemovedItems?.Count > 0 && e.RemovedItems[0] is INotifyPropertyChanged previousItem)
                previousItem.PropertyChanged -= OnSelectedPropertyChanged;

            if (e.AddedItems?.Count > 0 && e.AddedItems[0] is INotifyPropertyChanged currentItem)
                currentItem.PropertyChanged += OnSelectedPropertyChanged;
        }


        /// <summary>
        /// Called when one of the property changes on the ListBox SelectedItem
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnSelectedPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_trackedPropertiesList.Contains(e.PropertyName))
                return;

            Action action = () =>
            {
                AssociatedObject.UpdateLayout();
                if (AssociatedObject.SelectedItem != null)
                    AssociatedObject.ScrollIntoView(AssociatedObject.SelectedItem);
            };

            AssociatedObject.Dispatcher.BeginInvoke(action, DispatcherPriority.ContextIdle);
        }
    }
}
