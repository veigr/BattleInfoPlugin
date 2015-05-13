using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BattleInfoPlugin.Views.Controls
{
    public class Cell : Control
    {
        static Cell()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Cell), new FrameworkPropertyMetadata(typeof(Cell)));
        }

        #region Text DependencyProperty

        public string Text
        {
            get { return (string) this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Cell), new PropertyMetadata(""));

        #endregion

        #region X DependencyProperty


        public int X
        {
            get { return (int) this.GetValue(XProperty); }
            set { this.SetValue(XProperty, value); }
        }

        // Using a DependencyProperty as the backing store for X.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(int), typeof(Cell), new PropertyMetadata(0));


        #endregion

        #region Y DependencyProperty


        public int Y
        {
            get { return (int) this.GetValue(YProperty); }
            set { this.SetValue(YProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Y.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(int), typeof(Cell), new PropertyMetadata(0));


        #endregion

        //セル表示は後まわし。Enum化した方がいい？
        #region ColorNumber DependencyProperty


        public int ColorNumber
        {
            get { return (int) this.GetValue(ColorNumberProperty); }
            set { this.SetValue(ColorNumberProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColorNumber.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorNumberProperty =
            DependencyProperty.Register("ColorNumber", typeof(int), typeof(Cell), new PropertyMetadata(0));

        
        #endregion
    }
}
