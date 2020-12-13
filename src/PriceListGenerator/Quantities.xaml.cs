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
    /// Interaction logic for Quantities.xaml
    /// </summary>
    public partial class Quantities : Window
    {
        public Quantities()
        {
            InitializeComponent();
        }

        public List<string> AddQuantities
        {
            get
            {
                return Utilities.Lines(txtQuantities.Text);
            }
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
