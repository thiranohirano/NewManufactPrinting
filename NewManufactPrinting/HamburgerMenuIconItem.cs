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
    }
}