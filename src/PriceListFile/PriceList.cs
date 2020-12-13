using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TradeControl.PriceList
{
    [Serializable]
    public class PriceList : dsPriceList
    {
        string _fileName;

        public static string AppWebAddress = "http://tradecontrol.github.io/quotation";

        #region xml file
        public PriceList() { }

        public PriceList(string fileName)
        {
            _fileName = fileName;
            Clear();
            ReadXml(_fileName);
        }

        public FileInfo XmlFile
        {
            get
            {
                return new FileInfo(_fileName);
            }
        }

        public void NewFile(string fileName)
        {
            _fileName = fileName;
            Clear();
            Save();
            IsEdited = false;
        }

        public void Save()
        {
            WriteXml(_fileName);
            IsEdited = false;
        }

        public void SaveAs(string fileName)
        {
            _fileName = fileName;
            Save();
            IsEdited = false;
        }

        public void Accept()
        {
            AcceptChanges();
            IsEdited = true;
        }

        public bool IsEdited { get; set; } = false;
        #endregion

        #region details
        public void SetDetails(string customerName, string companyName, Uri uri, DateTime validFromOn, DateTime validToOn, string disclaimer)
        {
            var detailRow = tbDetail.Select(d => d).FirstOrDefault();
            bool isEmpty = (tbDetail.Count == 0);
             
            if (isEmpty)
                detailRow = tbDetail.NewtbDetailRow();

            detailRow.CustomerName = customerName;
            detailRow.CompanyName = companyName;
            detailRow.Uri = uri != null ? uri.AbsoluteUri : string.Empty;
            detailRow.PublishedOn = DateTime.Today;
            detailRow.ValidFromOn = validFromOn;
            detailRow.ValidToOn = validToOn;
            detailRow.Disclaimer = disclaimer;

            if (isEmpty)
                tbDetail.AddtbDetailRow(detailRow);

        }

        public void SetLogo(byte[] companyLogo)
        {
            var detailRow = tbDetail.Select(d => d).FirstOrDefault();
            detailRow.CompanyLogo = companyLogo;
        }

        public MemoryStream CompanyLogo
        {
            get
            {
                MemoryStream stream = null;

                var detailRow = tbDetail.Select(d => d).FirstOrDefault();
                if (detailRow != null)
                    if (!detailRow.IsCompanyLogoNull())
                        stream = new MemoryStream(detailRow.CompanyLogo);
                return stream;
            }
        }

        private string PriceValidityText
        {
            get
            {
                var detailRow = tbDetail.Select(d => d).FirstOrDefault();
                if (detailRow != null)
                    return $"Prices are valid from {detailRow.ValidFromOn.ToString("d")} to {detailRow.ValidToOn.ToString("d")}";
                else
                    return string.Empty;
            }
        }

        private string Disclaimer
        {
            get
            {
                var detailRow = tbDetail.Select(d => d).FirstOrDefault();
                if (detailRow != null)
                {
                    return detailRow.IsDisclaimerNull() ? string.Empty : detailRow.Disclaimer;
                }
                else
                    return string.Empty;
            }
        }

        #endregion

        #region product maintenance
        public void AddProduct(string productName, string productDescription)
        {
            var productRow = tbProduct.FindByProductName(productName);
            if (productRow == null)
            {
                productRow = tbProduct.NewtbProductRow();
                productRow.ProductName = productName;
                productRow.ProductDescription = productDescription;
                tbProduct.AddtbProductRow(productRow);
            }
            else
                productRow.ProductDescription = productDescription;
        }

        public void SetProductImage(string productName, byte[] productImage)
        {
            var productRow = tbProduct.FindByProductName(productName);
            productRow.ProductImage = productImage;
        }

        public void DeleteProduct(string productName)
        {
            var productRow = tbProduct.FindByProductName(productName);
            if (productRow != null)
                tbProduct.RemovetbProductRow(productRow);
        }

        public void AddQuanity(string productName, int quantity, double price)
        {
            var quantityRow = tbQuantity.FindByProductNameQuantity(productName, quantity);
            if (quantityRow == null)
            {
                quantityRow = tbQuantity.NewtbQuantityRow();
                quantityRow.ProductName = productName;
                quantityRow.Quantity = quantity;
                quantityRow.Price = price;
                tbQuantity.AddtbQuantityRow(quantityRow);
            }
            else
                quantityRow.Price = price;
        }

        public void DeleteQuantity(string productName, int quantity)
        {
            var quantityRow = tbQuantity.FindByProductNameQuantity(productName, quantity);
            if (quantityRow == null)
                tbQuantity.RemovetbQuantityRow(quantityRow);
        }

        public void AddExtra(string productName, string extraName)
        {
            int ordering = tbExtra.Where(e => e.ProductName == productName).Count() == 0 ? 10 : tbExtra.Where(e => e.ProductName == productName).Max(e => e.Ordering) + 10;

            var extraRow = tbExtra.FindByProductNameExtraName(productName, extraName);
            if (extraRow == null)
            {
                extraRow = tbExtra.NewtbExtraRow();
                extraRow.ProductName = productName;
                extraRow.ExtraName = extraName;
                extraRow.Ordering = ordering;
                tbExtra.AddtbExtraRow(extraRow);
            }
        }

        public void DeleteExtra(string productName, string extraName)
        {
            var extraRow = tbExtra.FindByProductNameExtraName(productName, extraName);
            if (extraRow != null)
                tbExtra.RemovetbExtraRow(extraRow);
        }

        public void AddExtraPrice(string productName, string extraName, int quantity, double price)
        {
            var quantityRow = tbExtraPrice.FindByProductNameExtraNameQuantity(productName, extraName, quantity);
            if (quantityRow == null)
            {
                quantityRow = tbExtraPrice.NewtbExtraPriceRow();
                quantityRow.ProductName = productName;
                quantityRow.Quantity = quantity;
                quantityRow.ExtraName = extraName;
                quantityRow.Price = price;
                tbExtraPrice.AddtbExtraPriceRow(quantityRow);
            }
            else
                quantityRow.Price = price;
        }

        public void DeleteExtraPrice(string productName, string extraName, int quantity)
        {
            var quantityRow = tbExtraPrice.FindByProductNameExtraNameQuantity(productName, extraName, quantity);
            if (quantityRow == null)
                tbExtraPrice.RemovetbExtraPriceRow(quantityRow);
        }

        public void MoveDownExtra(string productName, string extraName)
        {
            int extraOrder = tbExtra.Where(p => p.ProductName == productName && p.ExtraName == extraName).Select(e => e.Ordering).FirstOrDefault();
            int nextOrder = tbExtra.Where(p => p.ProductName == productName && p.Ordering > extraOrder).Select(e => e.Ordering).Min();
            string nextExtra = tbExtra.Where(p => p.ProductName == productName && p.Ordering == nextOrder).Select(e => e.ExtraName).First();
            var extraRow = tbExtra.FindByProductNameExtraName(productName, extraName);
            extraRow.Ordering = nextOrder;
            extraRow = tbExtra.FindByProductNameExtraName(productName, nextExtra);
            extraRow.Ordering = extraOrder;
        }

        public void MoveUpExtra(string productName, string extraName)
        {
            int extraOrder = tbExtra.Where(p => p.ProductName == productName && p.ExtraName == extraName).Select(e => e.Ordering).FirstOrDefault();
            int nextOrder = tbExtra.Where(p => p.ProductName == productName && p.Ordering < extraOrder).Select(e => e.Ordering).Max();
            string nextExtra = tbExtra.Where(p => p.ProductName == productName && p.Ordering == nextOrder).Select(e => e.ExtraName).First();
            var extraRow = tbExtra.FindByProductNameExtraName(productName, extraName);
            extraRow.Ordering = nextOrder;
            extraRow = tbExtra.FindByProductNameExtraName(productName, nextExtra);
            extraRow.Ordering = extraOrder;
        }
        #endregion

        #region product retrieval
        public List<string> RegisteredProducts
        {
            get { return tbProduct.Select(p => p.ProductName).ToList<string>(); }
        }

        public string GetProductDescription(string productName)
        {
            return tbProduct.Where(p => p.ProductName == productName).Select(p => p.ProductDescription).FirstOrDefault();
        }

        public MemoryStream GetProductImage(string productName)
        {
            try
            {
                var productRow = tbProduct.FindByProductName(productName);
                if (productRow != null)
                {
                    if (!productRow.IsProductImageNull())
                        return new MemoryStream(productRow.ProductImage);
                    else
                        return null;
                }
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

        public List<int> GetQuantities(string productName)
        {
            return tbQuantity.Where(q => q.ProductName == productName).Select(q => q.Quantity).ToList<int>();
        }

        public List<string> GetExtras(string productName)
        {
            return tbExtra.Where(e => e.ProductName == productName)
                .OrderBy(e => e.Ordering)
                .Select(e => e.ExtraName).ToList<string>();
        }


        public double GetProductPrice(string productName, int quantity)
        {
            return tbQuantity.Where(q => q.ProductName == productName && q.Quantity == quantity)
                    .Select(q => q.Price).FirstOrDefault();
        }

        public double GetExtraPrice(string productName, string extraName, int quantity)
        {
            return tbExtraPrice.Where(p => p.ProductName == productName && p.ExtraName == extraName && p.Quantity == quantity)
                    .Select(p => p.Price).FirstOrDefault();
        }

        public bool IsFirstExtra(string productName, string extraName)
        {
            return tbExtra.Where(p => p.ProductName == productName && p.ExtraName == extraName).Select(e => e.Ordering).FirstOrDefault()
                == tbExtra.Where(p => p.ProductName == productName).Select(e => e.Ordering).Min();
        }

        public bool IsLastExtra(string productName, string extraName)
        {
            return tbExtra.Where(p => p.ProductName == productName && p.ExtraName == extraName).Select(e => e.Ordering).FirstOrDefault()
                == tbExtra.Where(p => p.ProductName == productName).Select(e => e.Ordering).Max();
        }
        #endregion

        #region live quote
        public string ProductName { get; private set; } = string.Empty;
        public int Quantity { get; private set; } = 0;
        public double Price { get; private set; } = 0;
        public string QuoteTitle { get; private set; } = string.Empty;

        readonly List<string> Extras = new List<string>();

        public void LiveQuoteProduct(string productName, int quantity)
        {
            if (ProductName != productName)
                Extras.Clear();

            ProductName = productName;
            Quantity = quantity;
            SetQuotePrice();
        }

        void SetQuotePrice()
        {
            Price = GetProductPrice(ProductName, Quantity);

            foreach (string extraName in Extras)
                Price += GetExtraPrice(ProductName, extraName, Quantity);
        }

        public void LiveQuoteExtraAdd(string extraName)
        {
            if (!Extras.Contains(extraName))
            {
                Extras.Add(extraName);
                SetQuotePrice();
            }
        }

        public void LiveQuoteExtraDelete(string extraName)
        {
            if (Extras.Contains(extraName))
            {
                Extras.Remove(extraName);
                SetQuotePrice();
            }
        }

        public List<string> LiveQuoteExtras
        {
            get { return Extras; }
        }

        #endregion

        #region quotations

        public int SaveQuote(string title)
        {
            int quoteId = NewQuote(ProductName, title, Quantity);
            foreach (string extraName in Extras)
                AddExtraToQuote(quoteId, extraName);
            return quoteId;
        }

        public void LoadQuote(int quoteId)
        {
            var quoteRow = tbQuote.Where(q => q.QuoteId == quoteId).Select(p => p).FirstOrDefault();
            if (quoteRow != null)
            {
                ProductName = quoteRow.ProductName;
                Quantity = quoteRow.Quantity;
                QuoteTitle = quoteRow.Title;

                Extras.Clear();
                var extras = tbQuoteExtra.Where(e => e.QuoteId == quoteId).OrderBy(e => e.ExtraName).Select(e => e.ExtraName).ToList<string>();
                foreach (string extraName in extras)
                    Extras.Add(extraName);

                SetQuotePrice();
            }
        }

        private int NewQuote(string productName, string title, int quantity)
        {
            var quoteRow = tbQuote.NewtbQuoteRow();
            
            quoteRow.ProductName = productName;
            quoteRow.Title = title;
            quoteRow.QuotedOn = DateTime.Today;
            quoteRow.Quantity = quantity;
            quoteRow.Price = GetProductPrice(productName, quantity);

            tbQuote.AddtbQuoteRow(quoteRow);
            quoteRow.AcceptChanges();
            
            return quoteRow.QuoteId;
        }

        public void DeleteQuote(int quoteId)
        {
            string productName = tbQuote.Where(q => q.QuoteId == quoteId).Select(p => p.ProductName).FirstOrDefault();
            var quoteRow = tbQuote.FindByProductNameQuoteId(productName, quoteId);
            if (quoteRow != null)
                tbQuote.RemovetbQuoteRow(quoteRow);
        }

        public void AddExtraToQuote(int quoteId, string extraName)
        {
            string productName = tbQuote.Where(q => q.QuoteId == quoteId).Select(p => p.ProductName).FirstOrDefault();
            var extraRow = tbQuoteExtra.FindByQuoteIdProductNameExtraName(quoteId, productName, extraName);
            if (extraRow == null)
            {
                extraRow = tbQuoteExtra.NewtbQuoteExtraRow();
                extraRow.ProductName = productName;
                extraRow.QuoteId = quoteId;
                extraRow.ExtraName = extraName;
                tbQuoteExtra.AddtbQuoteExtraRow(extraRow);
            }

            var quoteRow = tbQuote.FindByProductNameQuoteId(productName, quoteId);
            quoteRow.Price += GetExtraPrice(productName, extraName, quoteRow.Quantity);
        }

        public void DeleteExtraFromQuote(int quoteId, string extraName)
        {
            string productName = tbQuote.Where(q => q.QuoteId == quoteId).Select(p => p.ProductName).FirstOrDefault();
            var extraRow = tbQuoteExtra.FindByQuoteIdProductNameExtraName(quoteId, productName, extraName);
            if (extraRow != null)
            {
                tbQuoteExtra.RemovetbQuoteExtraRow(extraRow);
                var quoteRow = tbQuote.FindByProductNameQuoteId(productName, quoteId);
                quoteRow.Price -= GetExtraPrice(productName, extraName, quoteRow.Quantity);
            }
        }

        public void RecalculateQuote(int quoteId)
        {
            string productName = tbQuote.Where(q => q.QuoteId == quoteId).Select(p => p.ProductName).FirstOrDefault();
            var quoteRow = tbQuote.FindByProductNameQuoteId(productName, quoteId);
            quoteRow.Price = GetProductPrice(quoteRow.ProductName, quoteRow.Quantity);

            var extras = tbQuoteExtra.Where(e => e.ProductName == quoteRow.ProductName && e.QuoteId == quoteRow.QuoteId).Select(e => e.ExtraName);

            foreach (string extraName in extras)
                quoteRow.Price += GetExtraPrice(quoteRow.ProductName, extraName, quoteRow.Quantity);

        }
        #endregion

        #region quote retrieval
        public List<string> Quotations
        {
            get 
            {
                var quotations = from tb in tbQuote
                                 orderby tb.QuoteId descending
                                 select $"{tb.QuoteId}: {tb.ProductName} - {tb.Title} [{tb.Quantity} @ {tb.Price}] - {tb.QuotedOn.ToLongDateString()}";

                return quotations.Select(q => q).ToList<string>();
            }
        }

        public StringBuilder QuoteDetails(int quoteId)
        {
            StringBuilder quote = new StringBuilder();

            string productName = tbQuote.Where(q => q.QuoteId == quoteId).Select(p => p.ProductName).FirstOrDefault();
            var quoteRow = tbQuote.FindByProductNameQuoteId(productName, quoteId);
            quote.AppendLine($" ** {quoteRow.Title} **");
            quote.AppendLine();
            quote.AppendLine($"Product: {quoteRow.ProductName}");
            if (GetProductDescription(quoteRow.ProductName).Length > 0)
                quote.AppendLine($"Description: {GetProductDescription(quoteRow.ProductName)}");
            quote.AppendLine($"Quoted On: {quoteRow.QuotedOn.ToString("d")}");
            quote.AppendLine($"Quantity: {quoteRow.Quantity}");

            quoteRow.Price = GetProductPrice(quoteRow.ProductName, quoteRow.Quantity);
            quote.AppendLine($"Unit Price: {quoteRow.Price.ToString("F2")}");

            var extras = tbQuoteExtra.Where(e => e.ProductName == quoteRow.ProductName && e.QuoteId == quoteRow.QuoteId).Select(e => e.ExtraName);

            if (extras.Count() > 0)
            {
                quote.AppendLine();
                quote.AppendLine("EXTRAS:");
                quote.AppendLine();
            }

            foreach (string extraName in extras)
            {
                double extraPrice = GetExtraPrice(quoteRow.ProductName, extraName, quoteRow.Quantity);
                quote.AppendLine($"{extraName}: {extraPrice.ToString("F2")}");
                quoteRow.Price += extraPrice;
            }

            if (extras.Count() > 0)
            {
                quote.AppendLine();
                quote.AppendLine($"Price with Extras: {quoteRow.Price.ToString("F2")}");
            }

            quote.AppendLine();
            quote.AppendLine(PriceValidityText);

            if (Disclaimer.Length > 0)
            {
                quote.AppendLine();
                quote.AppendLine(Disclaimer);
            }
            return quote;
        }
        #endregion
    }
}
