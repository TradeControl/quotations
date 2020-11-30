using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Diagnostics;

namespace TradeControl.PriceList
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PriceList priceList = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        #region xml file
        bool OpenFile()
        {
            return OpenFile(string.Empty);
        }

        bool OpenFile(string fileName)
        {
            try
            {
                CheckForFileSave();

                if (fileName.Length == 0)
                    fileName = OpenFileDialog(true);

                if (fileName.Length == 0)
                    return false;

                FileInfo fileInfo = new FileInfo(fileName);

                if (!fileInfo.Exists)
                {
                    MessageBox.Show($"File {fileInfo.Name} does not exist exist", Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                else
                {
                    if (fileName != Properties.Settings.Default.XMLFileName)
                    {
                        Properties.Settings.Default.XMLFileName = fileName;
                        Properties.Settings.Default.Save();
                    }

                    priceList = new PriceList(fileInfo.FullName);
                    sbFileName.Text = fileInfo.Name;

                    GridMainWindow.Visibility = Visibility.Visible;

                    return true;
                }

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

        }

        bool SaveAs()
        {
            try
            {
                CheckForFileSave();

                string fileName = OpenFileDialog(false);

                if (fileName.Length == 0)
                    return false;

                FileInfo fileInfo = new FileInfo(fileName);

                if (fileInfo.Exists)
                {
                    MessageBox.Show($"File {fileInfo.Name} already exists", Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                else
                {
                    priceList.SaveAs(fileInfo.FullName);

                    sbFileName.Text = fileInfo.Name;

                    return true;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        string OpenFileDialog(bool checkFileExists)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (Properties.Settings.Default.WorkingFolder.Length > 0)
                    openFileDialog.InitialDirectory = Properties.Settings.Default.WorkingFolder;

                openFileDialog.Filter = "XML Files (*.xml)|*.xml";
                openFileDialog.CheckFileExists = checkFileExists;
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() == true)
                {
                    FileInfo fileInfo = new FileInfo(openFileDialog.FileName);

                    if (Properties.Settings.Default.WorkingFolder != fileInfo.DirectoryName)
                    {
                        Properties.Settings.Default.WorkingFolder = fileInfo.DirectoryName;
                        Properties.Settings.Default.Save();
                    }

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

        void LoadFile()
        {
            try
            {
                MemoryStream logo = priceList.CompanyLogo;

                if (logo != null)
                {
                    BitmapImage img = new BitmapImage();
                    img.BeginInit();
                    img.StreamSource = logo;
                    img.EndInit();

                    imgCompanyLogo.Source = img;
                }
                else
                    imgCompanyLogo.Source = null;

                QuoteTitle = string.Empty;
                lbxQuotations.ItemsSource = null;
                lbxQuotations.ItemsSource = priceList.Quotations;

                MenuItemSave.IsEnabled = true;
                MenuItemSaveAs.IsEnabled = true;
                sbFileName.Text = priceList.XmlFile.Name;
                btnQuote.IsEnabled = true;

                var details = priceList.tbDetail.Select(d => d).FirstOrDefault();

                txtDisclaimer.Text = details.Disclaimer;

                if (details.CustomerName.Length > 0)
                    Title = details.CustomerName;

                if (!details.IsUriNull() && details.Uri.Length > 0)
                {
                    linkUri.NavigateUri = new Uri(details.Uri);
                    lblUri.Content = details.CompanyName;
                    txtUri.Visibility = Visibility.Visible;
                }
                else
                {
                    txtUri.Visibility = Visibility.Collapsed;
                    txtCompanyName.Text = details.CompanyName;
                }

                txtValidFrom.Text = details.ValidFromOn.ToLongDateString();
                txtValidTo.Text = details.ValidToOn.ToLongDateString();
                txtPublishedOn.Text = details.PublishedOn.ToLongDateString();

                gridDetails.Visibility = Visibility.Visible;

                cbProducts.SelectedIndex = -1;
                cbProducts.ItemsSource = priceList.RegisteredProducts;
                if (cbProducts.Items.Count > 0)
                    cbProducts.SelectedIndex = 0;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        void CloseFile()
        {
            MenuItemSave.IsEnabled = false;
            MenuItemSaveAs.IsEnabled = false;
            sbFileName.Text = string.Empty;
            btnQuote.IsEnabled = false;

            gridDetails.Visibility = Visibility.Collapsed;
            cbProducts.SelectedIndex = -1;
            cbProducts.ItemsSource = null;

        }

        void CheckForFileSave()
        {
            try
            {
                if (priceList?.IsEdited == true)
                {
                    if (MessageBox.Show($"Save {priceList.XmlFile.Name}?", Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        priceList.Save();
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region properties
        string ProductName
        {
            get
            {
                return cbProducts.SelectedIndex >= 0 ? (string)cbProducts.Items[cbProducts.SelectedIndex] : string.Empty;
            }
            set
            {
                cbProducts.SelectedIndex = cbProducts.Items.IndexOf(value);
            }
        }

        int Quantity
        {
            get
            {
                return cbQuantity.SelectedIndex >= 0 ? (int)cbQuantity.Items[cbQuantity.SelectedIndex] : -1;
            }
            set
            {
                cbQuantity.SelectedIndex = cbQuantity.Items.IndexOf(value);
            }
        }

        string ExtraName
        {
            get
            {

                if (lbxExtras.SelectedIndex >= 0)
                {
                    CheckBox extra = (CheckBox)lbxExtras.SelectedItem;
                    return (string)extra.Content;
                }
                else
                    return string.Empty;
            }
        }

        string QuoteTitle
        {
            get
            {
                return txtTitle.Text.Length > 0 ? txtTitle.Text : "title not set";
            }
            set
            {
                txtTitle.Text = value;
            }
        }

        int QuoteId
        {
            get
            {
                string quote = lbxQuotations.SelectedIndex >= 0 ? (string)lbxQuotations.Items[lbxQuotations.SelectedIndex] : string.Empty;

                return quote.Length == 0 ? -1 : Int32.Parse(quote.Substring(0, quote.IndexOf(':')));
            }
        }
        #endregion

        #region product quotation
        void LoadProduct()
        {
            try
            {
                cbQuantity.SelectedIndex = -1;
                lbxExtras.SelectedIndex = -1;

                txtProductDescription.Text = priceList.GetProductDescription(ProductName);
                txtProductDescription.Visibility = txtProductDescription.Text.Length > 0 ? Visibility.Visible : Visibility.Collapsed;

                MemoryStream productImage = priceList.GetProductImage(ProductName);

                if (productImage != null)
                {
                    BitmapImage img = new BitmapImage();
                    img.BeginInit();
                    img.StreamSource = productImage;
                    img.EndInit();

                    imgProductImage.Source = img;
                }
                else
                    imgProductImage.Source = null;

                cbQuantity.ItemsSource = priceList.GetQuantities(ProductName);
                if (cbQuantity.Items.Count > 0)
                    cbQuantity.SelectedIndex = 0;

                LoadExtras();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void LoadExtras()
        {
            try
            {
                var extras = priceList.GetExtras(ProductName);
                var quotedExtras = priceList.LiveQuoteExtras;

                lbxExtras.Items.Clear();

                foreach (string extraName in extras)
                {
                    CheckBox checkBox = new CheckBox()
                    {
                        Content = extraName,
                        IsChecked = quotedExtras.Contains(extraName)
                    };

                    checkBox.Checked += CheckBox_Checked;
                    checkBox.Unchecked += CheckBox_Checked;

                    lbxExtras.Items.Add(checkBox);
                }

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void LoadPrice()
        {
            try 
            { 
                priceList.LiveQuoteProduct(ProductName, Quantity);
                SetPrice(priceList.Price);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void AddExtra(string extraName)
        {
            try
            {
                priceList.LiveQuoteExtraAdd(extraName);
                SetPrice(priceList.Price);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void RemoveExtra(string extraName)
        {
            try
            {
                priceList.LiveQuoteExtraDelete(extraName);
                SetPrice(priceList.Price);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void SetPrice(double value)
        {
            txtPrice.Text = value.ToString();
        }
        #endregion

        #region quotations
        private void SaveQuote()
        {
            try
            {
                int quoteId = priceList.SaveQuote(QuoteTitle);
                priceList.Accept();

                RefreshQuotations();

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshQuotations()
        {
            lbxQuotations.SelectedIndex = -1;
            lbxQuotations.ItemsSource = null;
            lbxQuotations.ItemsSource = priceList.Quotations;            
            if (lbxQuotations.Items.Count > 0)
                lbxQuotations.SelectedIndex = 0;
        }

        private void LoadQuote()
        {
            try
            {
                if (QuoteId > 0)
                {
                    priceList.LoadQuote(QuoteId);

                    int quantity = priceList.Quantity;

                    ProductName = priceList.ProductName;
                    QuoteTitle = priceList.QuoteTitle;
                    txtQuotation.Text = priceList.QuoteDetails(QuoteId).ToString();
                    LoadExtras();
                    Quantity = quantity;
                    SetPrice(priceList.Price);

                }
                else
                    txtQuotation.Text = string.Empty;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


        private void DeleteQuote()
        {
            try
            {
                priceList.DeleteQuote(QuoteId);
                txtQuotation.Text = string.Empty;
                RefreshQuotations();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void CopyQuoteToClipboard()
        {
            try
            {
                Clipboard.Clear();
                Clipboard.SetText(priceList.QuoteDetails(QuoteId).ToString());
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CheckForFileSave();
            if (WindowState == WindowState.Normal)
            {
                Properties.Settings.Default.Top = Top;
                Properties.Settings.Default.Left = Left;
                Properties.Settings.Default.Height = Height;
                Properties.Settings.Default.Width = Width;
                Properties.Settings.Default.Save();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.Height > 0)
            {
                Top = Properties.Settings.Default.Top;
                Left = Properties.Settings.Default.Left;
                Height = Properties.Settings.Default.Height;
                Width = Properties.Settings.Default.Width;
            }

            if (Properties.Settings.Default.XMLFileName.Length > 0)
            {
                if (OpenFile(Properties.Settings.Default.XMLFileName))
                    LoadFile();
                else
                    CloseFile();
            }
        }

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (OpenFile())
                    LoadFile();
                else
                    CloseFile();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            priceList.Save();
        }

        private void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveAs();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        private void cbProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadProduct();
        }

        private void cbQuantity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadPrice();
        }


        private void btnQuote_Click(object sender, RoutedEventArgs e)
        {
            SaveQuote();
        }

        private void lbxQuotations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadQuote();
        }

        private void lbxQuotationsDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteQuote();
        }

        private void lbxQuotationCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            CopyQuoteToClipboard();
        }

        private void lbxQuotations_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            lbxQuotationCopyToClipboard.IsEnabled = QuoteId >= 0;
            lbxQuotationsDelete.IsEnabled = lbxQuotationCopyToClipboard.IsEnabled;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                e.Handled = true;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void MenuItemOnline_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(PriceList.AppWebAddress));                
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox checkBox = (CheckBox)sender;
                if (checkBox.IsChecked == true)
                    AddExtra((string)checkBox.Content);
                else
                    RemoveExtra((string)checkBox.Content);

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
