using Microsoft.Win32;
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

namespace TradeControl.PriceList
{
    public partial class PriceListProperties : Window
    {
        FileInfo fileInfo = null;

        public PriceListProperties()
        {
            InitializeComponent();
        }

        #region logo
        void GetLogo()
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

        public byte[] CompanyLogo
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

        public string CustomerName { get { return tbxCustomerName.Text; } set { tbxCustomerName.Text = value; } }
        public string CompanyName { get { return tbxCompanyName.Text; } set { tbxCompanyName.Text = value; } }
        public Uri Uri 
        { 
            get 
            {
                try
                {
                    if (tbxUri.Text.Length > 0)
                        return new Uri(tbxUri.Text);
                    else
                        return null;
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

            }
            set 
            { 
                if (value != null)
                    tbxUri.Text = value.AbsoluteUri; 
            } 
        }

        public DateTime ValidFrom 
        { 
            get { return dteValidFrom.DisplayDate; } 
            set { dteValidFrom.DisplayDate = value; dteValidFrom.Text = value.ToString(); } 
        }

        public DateTime ValidTo 
        { 
            get { return dteValidTo.DisplayDate; } 
            set { dteValidTo.DisplayDate = value; dteValidTo.Text = value.ToString(); } 
        }

        public DateTime PublishedOn 
        { 
            get { return dtePublishedOn.DisplayDate; } 
            set { dtePublishedOn.DisplayDate = value; dtePublishedOn.Text = value.ToString(); } 
        }

        public string Disclaimer { get { return tbxDisclaimer.Text; } set { tbxDisclaimer.Text = value; } }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void btnLogo_Click(object sender, RoutedEventArgs e)
        {
            GetLogo();
        }
    }
}
