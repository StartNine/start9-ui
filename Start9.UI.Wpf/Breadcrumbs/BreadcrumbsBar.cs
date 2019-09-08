﻿using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Start9.UI.Wpf;

namespace Start9.UI.Wpf.Breadcrumbs
{
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(Breadcrumb))]
    [TemplatePart(Name = PartTextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = PartBreadcrumbsStackPanel, Type = typeof(StackPanel))]
    [TemplatePart(Name = PartBackgroundButton, Type = typeof(Button))]
    public class BreadcrumbsBar : ItemsControl
    {
        public bool ShowsPath
        {
            get => (bool)GetValue(ShowsPathProperty);
            set => SetValue(ShowsPathProperty, value);
        }

        public static DependencyProperty ShowsPathProperty =
        DependencyProperty.Register(nameof(ShowsPath), typeof(bool), typeof(BreadcrumbsBar), new FrameworkPropertyMetadata(false, OnShowsPathPropertyChangedCallback));

        static void OnShowsPathPropertyChangedCallback(Object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is BreadcrumbsBar bar)
            {
                if ((bool)e.NewValue)
                {
                    bar._textBox.Text = bar.ToString();
                    //bar._textBox.Focus();
                    //FocusManager.SetFocusedElement(FocusManager.GetFocusScope(bar), bar._textBox);
                }
                else
                {
                    bar._textBox.Text = string.Empty;
                }
            }
        }

        const String PartTextBox = "PART_TextBox";
        const String PartBreadcrumbsStackPanel = "PART_BreadcrumbsStackPanel";
        const String PartBackgroundButton = "PART_BackgroundButton";

        TextBox _textBox;
        StackPanel _breadcrumbsStackPanel;
        Button _backgroundButton;

        public event EventHandler<PathChangedEventArgs> PathUpdated;
        public event EventHandler<PathItemAddedEventArgs> PathItemAdded;
        //public event EventHandler<EventArgs> NavigationCanceled;

        public BreadcrumbsBar() : base()
        {
            Breadcrumb.SelectionChanged += Breadcrumb_SelectionChanged; // += Breadcrumb_SelectionChanged;
        }

        private void Breadcrumb_SelectionChanged(object sender, SelectedBreadcrumbChangedEventArgs e)
        {
            if (_breadcrumbsStackPanel.Children.Contains(sender as UIElement))
            {
                if (e.NewCrumb is FrameworkElement element)
                {
                    if (ItemsSource != null)
                        PathItemAdded?.Invoke(this, new PathItemAddedEventArgs(null, element.DataContext, _breadcrumbsStackPanel.Children.Count));
                    else
                        PathItemAdded?.Invoke(this, new PathItemAddedEventArgs(element, null, _breadcrumbsStackPanel.Children.Count));
                }
            }
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is Breadcrumb;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            var crumb = new Breadcrumb();
            //crumb.PreviewMouseLeftButtonUp += Breadcrumb_PreviewMouseLeftButtonUp; //.SelectionChanged += Breadcrumb_SelectionChanged;
            return crumb;
        }

        private void Breadcrumb_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem item)
            {
                for (int i = 0; i < item.Items.Count; i++)
                {
                    if ((item.Items[i] is TreeViewItem subItem) && (subItem.IsSelected))
                    {
                        if (ItemsSource != null)
                        {
                            //item.ItemsSource[i]
                            SetItemAt(subItem.DataContext, _breadcrumbsStackPanel.Children.IndexOf(sender as UIElement) + 1);
                        }
                        else
                        {
                            SetItemAt(subItem.Header, _breadcrumbsStackPanel.Children.IndexOf(sender as UIElement) + 1);
                        }
                    }
                }
            }
        }

        /*private void Breadcrumb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetItemAt(e.AddedItems[0], _breadcrumbsStackPanel.Children.IndexOf(sender as UIElement) + 1);
        }*/

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _textBox = GetTemplateChild(PartTextBox) as TextBox;
            /*_textBox.GotFocus += (sneder, args) =>
            {
                _breadcrumbsStackPanel.Visibility = Visibility.Collapsed;
            };*/
            _textBox.KeyDown += (sneder, args) =>
            {
                if (args.Key == System.Windows.Input.Key.Enter)
                {
                    PathUpdated?.Invoke(this, new PathChangedEventArgs(GetPathFromItems(ToString()), GetPathFromItems(_textBox.Text)));
                    ShowsPath = false;
                }

                if (args.Key == System.Windows.Input.Key.Escape)
                {
                    ShowsPath = false;
                    //NavigationCanceled?.Invoke(this, null);
                }

                /*if ((args.Key == System.Windows.Input.Key.Enter) || (args.Key == System.Windows.Input.Key.Escape))
                {
                    _textBox.Text = string.Empty;
                    _breadcrumbsStackPanel.Visibility = Visibility.Visible;
                }*/
            };
            _textBox.IsVisibleChanged += (sneder, args) =>
            {
                if ((bool)args.NewValue)
                    _textBox.Focus();
            };
            _textBox.LostFocus += (sneder, args) => ShowsPath = false;

            _breadcrumbsStackPanel = GetTemplateChild(PartBreadcrumbsStackPanel) as StackPanel;

            _backgroundButton = GetTemplateChild(PartBackgroundButton) as Button;
            _backgroundButton.Click += (sneder, args) => ShowsPath = true;
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
        }

        public void SetItemAt(object item, int index)
        {
            for (int i = Items.Count; i > index; i--)
            {
                Items.RemoveAt(i);
            }
            Items.Add(item);
        }

        private static string GetPathFromItems(IEnumerable collection)
        {
            string path = string.Empty;
            foreach (object item in collection)
                path += item.ToString() + "\\";

            if (path.EndsWith("\\"))
                path.TrimEnd('\\');
            return path;
        }

        public override string ToString()
        {
            return GetPathFromItems(Items);
        }
    }

    public class PathChangedEventArgs : EventArgs
    {
        public string OldPath { get; private set; } = null;
        public string NewPath { get; private set; } = null;

        public PathChangedEventArgs(string oldPath, string newPath) : base()
        {
            OldPath = oldPath;
            NewPath = newPath;
        }
    }

    public class PathItemAddedEventArgs : EventArgs
    {
        public FrameworkElement NewElement { get; private set; } = null;
        public object NewValue { get; private set; } = null;
        public int Index { get; private set; } = -1;

        public PathItemAddedEventArgs(FrameworkElement el, object newVal, int index) : base()
        {
            NewElement = el;
            NewValue = newVal;
            Index = index;
        }
    }
}
