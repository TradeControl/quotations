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
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;
using Microsoft.Win32;
using System.Diagnostics;

namespace TradeControl.PriceList
{
    public partial class MainWindow : Window
    {
        PriceList priceList = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Xml File
        bool NewFile()
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
                    priceList = new PriceList();
                    priceList.NewFile(fileInfo.FullName);

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
                    priceList = new PriceList(fileInfo.FullName);
                    sbFileName.Text = fileInfo.Name;

                    if (Properties.Settings.Default.XMLFileName != fileName)
                    {
                        Properties.Settings.Default.XMLFileName = fileName;
                        Properties.Settings.Default.Save();
                    }

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
            MenuItemSave.IsEnabled = true;
            MenuItemSaveAs.IsEnabled = true;
            MenuItemProperties.IsEnabled = true;
            sbFileName.Text = priceList.XmlFile.Name;
            btnAddProduct.IsEnabled = true;
            btnDeleteProduct.IsEnabled = true;
            btnEditProduct.IsEnabled = true;
            cbProducts.SelectedIndex = -1;
            cbProducts.ItemsSource = priceList.RegisteredProducts;            
        }

        void CloseFile()
        {
            MenuItemSave.IsEnabled = false;
            MenuItemSaveAs.IsEnabled = false;
            MenuItemProperties.IsEnabled = false;
            sbFileName.Text = string.Empty;
            btnAddProduct.IsEnabled = false;
            btnDeleteProduct.IsEnabled = false;
            btnEditProduct.IsEnabled = false;
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

        #region price list properties
        void EditProperties()
        {
            try
            {
                PriceListProperties prop = new PriceListProperties();

                var detail = priceList.tbDetail.Select(d => d).FirstOrDefault();

                if (detail != null)
                {
                    prop.CustomerName = detail.CustomerName;
                    prop.CompanyName = detail.CompanyName;
                    prop.Uri = detail.Uri.Length > 0 ? new Uri(detail.Uri) : null;
                    prop.Disclaimer = detail.Disclaimer;
                    prop.ValidFrom = detail.ValidFromOn;
                    prop.ValidTo = detail.ValidToOn;
                    prop.PublishedOn = detail.PublishedOn;
                }
                else
                {
                    prop.ValidFrom = DateTime.Today;
                    prop.ValidTo = DateTime.Today;
                    prop.PublishedOn = DateTime.Today;
                }

                if (prop.ShowDialog() == true)
                {
                    priceList.SetDetails(prop.CustomerName, prop.CompanyName, prop.Uri, 
                                prop.ValidFrom, prop.ValidTo, prop.Disclaimer);

                    byte[] companyLogo = prop.CompanyLogo;
                    if (companyLogo != null)
                        priceList.SetLogo(companyLogo);

                    priceList.Accept();
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
        }

        int Quantity
        {
            get
            {
                return lbxQuantity.SelectedIndex >= 0 ? (int)lbxQuantity.Items[lbxQuantity.SelectedIndex] : -1;
            }
        }

        string ExtraName
        {
            get
            {
                return lbxExtras.SelectedIndex >= 0 ? (string)lbxExtras.Items[lbxExtras.SelectedIndex] : string.Empty;
            }
            set
            {
                lbxExtras.SelectedItem = value;
            }

        }


        double ProductPrice
        {
            get
            {
                try
                {
                    return double.Parse(txtProductPrice.Text);
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    return 0;
                }
            }
            set
            {
                try
                {
                    txtProductPrice.Text = value.ToString();
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }

        double ExtraPrice
        {
            get
            {
                try
                {
                    return double.Parse(txtExtraPrice.Text);
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    return 0;
                }
            }
            set
            {
                try
                {
                    txtExtraPrice.Text = value.ToString();
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }
        #endregion

        #region products
        void NewProduct()
        {
            try
            {
                Product product = new Product(true);

                if (product.ShowDialog() == true)
                {
                    priceList.AddProduct(product.ProductName, product.ProductDescription);

                    byte[] productImage = product.ProductImage;
                    if (productImage != null)
                        priceList.SetProductImage(product.ProductName, productImage);

                    priceList.Accept();
                    cbProducts.ItemsSource = priceList.RegisteredProducts;
                    cbProducts.Text = product.ProductName;
                }

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        void EditProduct()
        {
            try
            {
                if (ProductName.Length == 0)
                    return;

                Product product = new Product(false)
                {
                    ProductName = this.ProductName,
                    ProductDescription = priceList.GetProductDescription(this.ProductName)
                };

                if (product.ShowDialog() == true)
                {
                    priceList.AddProduct(product.ProductName, product.ProductDescription);

                    byte[] productImage = product.ProductImage;
                    if (productImage != null)
                        priceList.SetProductImage(product.ProductName, productImage);

                    priceList.Accept();
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


        void DeleteProduct()
        {
            try
            {
                if (ProductName.Length == 0)
                    return;

       
                if (MessageBox.Show($"Okay to delete product {ProductName}?", Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    priceList.DeleteProduct(ProductName);
                    priceList.Accept();
                    cbProducts.Text = string.Empty;
                    cbProducts.ItemsSource = priceList.RegisteredProducts;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        void LoadProduct()
        {
            try
            {
                lbxQuantity.SelectedIndex = -1;
                lbxExtras.SelectedIndex = -1;
                
                lbxExtras.ItemsSource = priceList.GetExtras(ProductName);
                btnAddExtra.IsEnabled = true;
                btnDeleteExtra.IsEnabled = false;

                lbxQuantity.ItemsSource = priceList.GetQuantities(ProductName);
                btnAddQuantity.IsEnabled = true;
                btnDeleteQuantity.IsEnabled = false;

                ProductPrice = 0;
                ExtraPrice = 0;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        void UnloadProduct()
        {
            try
            {
                lbxExtras.SelectedIndex = -1;
                lbxQuantity.SelectedIndex = -1;

                lbxExtras.ItemsSource = null;
                lbxQuantity.ItemsSource = null;

                btnAddExtra.IsEnabled = false;
                btnDeleteExtra.IsEnabled = false;
                btnAddQuantity.IsEnabled = false;
                btnDeleteQuantity.IsEnabled = false;

                ProductPrice = 0;
                ExtraPrice = 0;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        void AddQuantity()
        {
            try
            {
                Quantities quantity = new Quantities();
                if (quantity.ShowDialog() == true)
                {
                    foreach (string qty in quantity.AddQuantities)
                        priceList.AddQuanity(ProductName, Int32.Parse(qty), 0);

                    priceList.Accept();

                    lbxQuantity.SelectedIndex = -1;
                    lbxQuantity.ItemsSource = priceList.GetQuantities(cbProducts.Text);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void LoadProductPrice()
        {
            try
            {

                if (Quantity > 0 && ProductName.Length > 0)
                    ProductPrice = priceList.GetProductPrice(ProductName, Quantity);
                else
                    ProductPrice = 0;

                LoadExtraPrice();

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        void UpdateProductPrice()
        {
            try
            {
                if (Quantity > 0 && ProductName.Length > 0)
                {
                    priceList.AddQuanity(ProductName, Quantity, ProductPrice);
                    priceList.Accept();
                }

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        #endregion

        #region extras
        void AddExtras()
        {
            try
            {
                Extras extras = new Extras();
                if (extras.ShowDialog() == true)
                {
                    foreach (string extraName in extras.ExtraEntries)
                        priceList.AddExtra(ProductName, extraName);
                    
                    priceList.Accept();

                    lbxExtras.SelectedIndex = -1;
                    lbxExtras.ItemsSource = priceList.GetExtras(ProductName);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void DeleteExtra()
        {
            try
            {
                if (ExtraName.Length > 0)
                {
                    priceList.DeleteExtra(ProductName, ExtraName);
                    lbxExtras.SelectedIndex = -1;
                    lbxExtras.ItemsSource = priceList.GetExtras(ProductName);
                    SetExtraActions();
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
    }
        void LoadExtraPrice()
        {
            try
            {
                if (Quantity > 0 && ProductName.Length > 0 && ExtraName.Length > 0)
                    ExtraPrice = priceList.GetExtraPrice(ProductName, ExtraName, Quantity);
                else
                    ExtraPrice = 0;

                txtExtraPrice.Focus();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void SetExtraActions()
        {
            try
            {
                if (lbxExtras.SelectedIndex > -1)
                {
                    btnDeleteExtra.IsEnabled = true;
                    btnMoveDown.IsEnabled = priceList.IsLastExtra(ProductName, ExtraName) ? false : true;
                    btnMoveUp.IsEnabled = priceList.IsFirstExtra(ProductName, ExtraName) ? false : true; ;
                }
                else
                {
                    btnDeleteExtra.IsEnabled = false;
                    btnMoveDown.IsEnabled = false;
                    btnMoveUp.IsEnabled = false;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        void UpdateExtraPrice()
        {
            try
            {
                if (Quantity > 0 && ProductName.Length > 0 && ExtraName.Length > 0)
                {
                    priceList.AddExtraPrice(ProductName, ExtraName, Quantity, ExtraPrice);
                    priceList.Accept();
                }

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void MoveUpExtra()
        {
            try
            {
                string extraName = ExtraName;
                if (extraName.Length > 0)
                {
                    priceList.MoveUpExtra(ProductName, ExtraName);
                    lbxExtras.ItemsSource = null;
                    lbxExtras.ItemsSource = priceList.GetExtras(ProductName);
                    ExtraName = extraName;
                    priceList.Accept();
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void MoveDownExtra()
        {
            try
            {
                string extraName = ExtraName;
                if (extraName.Length > 0)
                {
                    priceList.MoveDownExtra(ProductName, ExtraName);
                    lbxExtras.ItemsSource = null;
                    lbxExtras.ItemsSource = priceList.GetExtras(ProductName);
                    ExtraName = extraName;
                    priceList.Accept();
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        private void MenuItemNewFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NewFile())
                {
                    LoadFile();
                    EditProperties();
                }
                else
                    CloseFile();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);                
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            priceList.Save();
        }

        private void MenuItemSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveAs();
        }

        private void MenuItemProperties_Click(object sender, RoutedEventArgs e)
        {
            EditProperties();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CheckForFileSave();
        }

        private void btnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            NewProduct();
        }

        private void btnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            EditProduct();
        }

        private void btnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            DeleteProduct();
        }

        private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        private void cbProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbProducts.SelectedIndex >= 0)
                LoadProduct();
            else
                UnloadProduct();
        }

        private void btnAddExtra_Click(object sender, RoutedEventArgs e)
        {
            AddExtras();
        }

        private void btnAddQuantity_Click(object sender, RoutedEventArgs e)
        {
            AddQuantity();
        }


        private void txtProductPrice_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateProductPrice();
        }

        private void lbxQuantity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadProductPrice();
        }

        private void lbxExtras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadExtraPrice();
            SetExtraActions();
        }

        private void txtExtraPrice_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateExtraPrice();
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.XMLFileName.Length > 0)
            {
                if (OpenFile(Properties.Settings.Default.XMLFileName))
                    LoadFile();
                else
                    CloseFile();
            }
        }

        private void btnDeleteExtra_Click(object sender, RoutedEventArgs e)
        {
            DeleteExtra();
        }

        private void btnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            MoveUpExtra();
        }

        private void btnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            MoveDownExtra();
        }
    }
}
