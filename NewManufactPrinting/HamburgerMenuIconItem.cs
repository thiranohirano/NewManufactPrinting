using System.Windows;
using MahApps.Metro.Controls;

namespace NewManufactPrinting
{
    public class HamburgerMenuIconItem : HamburgerMenuItem
    {
        public static readonly DependencyProperty IconProperty
            = DependencyProperty.Register(
                nameof(Icon),
                typeof(object),
                typeof(HamburgerMenuIconItem),
                new PropertyMetadata(default(object)));

        public object Icon
        {
            get { return (object)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty BadgeProperty
            = DependencyProperty.Register(
                nameof(Badge),
                typeof(object),
                typeof(HamburgerMenuIconItem),
                new PropertyMetadata(default(object)));

        public object Badge
        {
            get { return (object)GetValue(BadgeProperty); }
            set { SetValue(BadgeProperty, value); }
        }
    }
}