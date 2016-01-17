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
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Threading;

namespace Converter2000
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string[] keywords = { "isk", "eur", "nok", "usd", "gbp", "kr", "$", "dollar", "euro", "pund", "pound", "dol", "norsk króna", "dalur", "dollari", "dkk", "dk" };
        string keywordsPutTogether = "";
        string currensyConverterPattern;

        double isk = 1;
        double eur = 140;
        double usd = 130;
        double gbp = 190;
        double nok = 15;
        double dkk = 19;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void txtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            string inputText = txtInput.Text.ToLower();


            if (Regex.IsMatch(inputText, currensyConverterPattern))
            {
                Match m = Regex.Match(inputText, currensyConverterPattern);

                string strAmmount = m.Groups[1].Value;
                if (Regex.IsMatch(strAmmount, @","))
                {
                    strAmmount = Regex.Replace(strAmmount, @",", ".");
                }

                double amount = 0;
                double.TryParse(strAmmount, out amount);

                string convertFrom = m.Groups[2].Value;
                string convertTo = m.Groups[4].Value;

                double from = 0;
                double to = 0;

                to = GetIndex(convertTo);
                from = GetIndex(convertFrom);

                double result = (amount * (from / to));
                result = Math.Round(result, 3);
                lblOutPut.Content = result.ToString() + " " + convertTo;

            }
        }
        private double GetIndex(string currency)
        {
            if (currency == keywords[0] || currency == keywords[5]) //í isk í 
            {
                return isk;
            }
            else if (currency == keywords[1] || currency == keywords[8]) //i eur
            {
                return eur;
            }
            else if (currency == keywords[2] || currency == "norsk króna") //i nok
            {
                return nok;
            }
            else if (   currency == keywords[3] ||
                        currency == keywords[6] ||
                        currency == keywords[7] || 
                        currency == keywords[11] ||
                        currency == "dalur" ||
                        currency == "dollari") //i usd
            {
                return usd;
            }
            else if (   currency == keywords[4] ||
                        currency == keywords[9] || 
                        currency == keywords[10]) //í GPB
            {
                return gbp;
            }
            else if(currency == "dkk" || currency == "dk")
            {
                return dkk;
            }
            else
            {
                return 0;
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DependencyObject obj = new DependencyObject();
            FocusManager.SetFocusedElement(obj, txtInput);
            keywordsPutTogether = keywords[0];
            for (int i = 1; i < keywords.Length; i++)
            {
                keywordsPutTogether = keywordsPutTogether + @"|" + keywords[i];
            }
            currensyConverterPattern = @"(\d{1,9}|\d{1,9}.\d{1,9}) (" + keywordsPutTogether + @") (in|í|to) (" + keywordsPutTogether + @")";

            Thread t1 = new Thread(new ThreadStart(Connect));
            t1.Start();
        }

        public void Connect()
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(@"http://apis.is/currency/lb");
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            StreamReader sr = new StreamReader(resp.GetResponseStream());
            string json = sr.ReadToEnd();
            sr.Close();
            resp.Close();

            RootObject r = JsonConvert.DeserializeObject<RootObject>(json);

            foreach (currency cur in r.results)
            {
                if (cur.shortName == "EUR")
                {
                    eur = cur.value;
                }
                else if (cur.shortName == "USD")
                {
                    usd = cur.value;
                }
                else if (cur.shortName == "GBP")
                {
                    gbp = cur.value;
                }
                else if (cur.shortName == "NOK")
                {
                    nok = cur.value;
                }
                else if (cur.shortName == "DKK")
                {
                    dkk = cur.value;
                }
            }
        }
    }
    public class currency
    {
        public string shortName { get; set; }
        public double value { get; set; }

    }
    public class RootObject
    {
        public List<currency> results { get; set; }
    }
}
