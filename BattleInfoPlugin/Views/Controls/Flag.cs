using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BattleInfoPlugin.Views.Controls
{
    public class Flag : Control
    {
        static Flag()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Flag), new FrameworkPropertyMetadata(typeof(Flag)));
        }

        #region X DependencyProperty


        public int X
        {
            get { return (int)this.GetValue(XProperty); }
            set { this.SetValue(XProperty, value); }
        }

        // Using a DependencyProperty as the backing store for X.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register(nameof(X), typeof(int), typeof(Flag), new PropertyMetadata(0));


        #endregion

        #region Y DependencyProperty


        public int Y
        {
            get { return (int)this.GetValue(YProperty); }
            set { this.SetValue(YProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Y.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register(nameof(Y), typeof(int), typeof(Flag), new PropertyMetadata(0));


        #endregion

    }
}
