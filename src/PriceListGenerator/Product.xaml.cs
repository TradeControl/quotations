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
using System.IO;
using Microsoft.Win32;

namespace TradeControl.PriceList
{
    /// <summary>
    /// Interaction logic for Product.xaml
    /// </summary>
    public partial class Product : Window
    {
        FileInfo fileInfo = null;

        public Product(bool isNew)
        {
            InitializeComponent();
            tbxProductName.IsEnabled = isNew;
        }

        public string ProductName { get { return tbxProductName.Text; } set { tbxProductName.Text = value; } }
        public string ProductDescription { get { return tbxProductDescription.Text; } set { tbxProductDescription.Text = value; } }

        #region logo
        void GetProductImage()
        {
            try
            {
                string fileName = OpenFileDialog();
                if (fileName.Length > 0)
                {
                    fileInfo = new FileInfo(fileName);
                    tbxFileName.Text = fileInfo.Name;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        string OpenFileDialog()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();

                openFileDialog.Filter = "Image Files (*.jpg,*.bmp, *.gif, *.png)|*.jpg;*.bmp;*.gif;*.png";
                openFileDialog.CheckFileExists = true;
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() == true)
                {
                    return openFileDialog.FileName;
                }
                else
                    return string.Empty;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }

        public byte[] ProductImage
        {
            get
            {
                try
                {
                    if (fileInfo != null)
                    {
                        FileStream stream = fileInfo.OpenRead();
                        byte[] bytes = new byte[stream.Length];
                        stream.Read(bytes, 0, (int)stream.Length);
                        return bytes;
                    }
                    else
                        return null;
                }
                catch
                {
                    return null;
                }
            }
        }
        #endregion
        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btnImage_Click(object sender, RoutedEventArgs e)
        {
            GetProductImage();
        }
    }
}
