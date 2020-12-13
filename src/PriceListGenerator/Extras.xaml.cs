using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TradeControl.PriceList
{
    /// <summary>
    /// Interaction logic for Extras.xaml
    /// </summary>
    public partial class Extras : Window
    {
        public Extras()
        {
            InitializeComponent();
        }

        public List<string> ExtraEntries
        {
            get
            {
                return Utilities.Lines(txtExtras.Text);
            }
        }


        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
