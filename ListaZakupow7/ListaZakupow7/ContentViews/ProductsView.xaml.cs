

using System.Data;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Xml.Linq;

namespace ListaZakupow7.ContentViews;

public partial class ProductsView : ContentView
{
    public ProductsView()
    {
        InitializeComponent();
    }

    string catName = "";
    public ProductsView(string CatName)
    {
        InitializeComponent();
        this.catName = CatName;
        this.ID_hidden.Text = GenerateId();
    }

    public ProductsView(string CatName, string ProductName, string iloscProductu, string sztukateria, string isCheckeed, string ID)
    {
        InitializeComponent();
        this.ID_hidden.Text = ID;
        this.catName = CatName;
        this.Entry.Text = ProductName;
        this.iloscLabel.Text = iloscProductu;
        this.sztukEntry.Text = sztukateria;
        this.checkProduct.IsChecked = bool.Parse(isCheckeed);
        
    }


    private void checkProduct_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (this.checkProduct.IsChecked)
        {
            if (this.Parent is VerticalStackLayout vsl)
            {
                vsl.Children.Remove(this);
                vsl.Children.Add(this);
            }
            this.ParentTemp.BackgroundColor = Color.FromArgb("#70B74F");
            this.plus.IsEnabled = false;
            this.minus.IsEnabled = false;
            this.Entry.IsEnabled = false;
            this.sztukEntry.IsEnabled = false;
        }
        else
        {
            this.ParentTemp.BackgroundColor = Colors.AliceBlue;
            this.plus.IsEnabled = true;
            this.minus.IsEnabled = true;
            this.Entry.IsEnabled = true;
            this.sztukEntry.IsEnabled = true;
        }
        

        XMLSave(this.catName, this.Entry.Text, this.iloscLabel.Text, this.sztukEntry.Text, this.checkProduct.IsChecked, this.ID_hidden.Text);

    }

    private void minus_Clicked(object sender, EventArgs e)
    {
        int i = int.Parse(this.iloscLabel.Text);
        i--;
        this.iloscLabel.Text = i.ToString();
        XMLSave(this.catName, this.Entry.Text, this.iloscLabel.Text, this.sztukEntry.Text, this.checkProduct.IsChecked, this.ID_hidden.Text);
    }

    private void plus_Clicked(object sender, EventArgs e)
    {
        int i = int.Parse(this.iloscLabel.Text);
        i++;
        this.iloscLabel.Text = i.ToString();
        XMLSave(this.catName, this.Entry.Text, this.iloscLabel.Text, this.sztukEntry.Text, this.checkProduct.IsChecked, this.ID_hidden.Text);
    }

    private async void delBtn_Clicked(object sender, EventArgs e)
    {
        bool answer = await App.Current.MainPage.DisplayAlert("Usuñ", "Czy chcesz usun¹æ przedmiot \"" + this.Entry.Text + "\" z listy?", "Tak", "Nie");
        if (this.Parent is VerticalStackLayout vsl && answer)
        {
            vsl.Children.Remove(this);
            removeFromXML(this.catName, this.Entry.Text, this.ID_hidden.Text);
        }
    }

    private void Entry_Completed(object sender, EventArgs e)
    {
        XMLSave(this.catName, this.Entry.Text, this.iloscLabel.Text, this.sztukEntry.Text, this.checkProduct.IsChecked, this.ID_hidden.Text);
    }

    private async void sztukEntry_Completed(object sender, EventArgs e)
    {
        var response = await App.Current.MainPage.DisplayActionSheet("Wybierz rodzaj miary", "Anuluj", null, "szt. (domyœlny)", "kg.", "deg.", "g.", "mg.", "ml.", "l.","op.", "inny");
        if (response != null)
        {
            switch (response)
            {
                case ("szt. (domyœlny)"):
                    this.sztukEntry.Text = "szt.";
                    break;
                case ("l."):
                    this.sztukEntry.Text = "l.";
                    break;
                case ("ml."):
                    this.sztukEntry.Text = "ml.";
                    break;
                case ("kg."):
                    this.sztukEntry.Text = "kg.";
                    break;
                case ("deg."):
                    this.sztukEntry.Text = "deg.";
                    break;
                case ("g."):
                    this.sztukEntry.Text = "g.";
                    break;
                case ("mg."):
                    this.sztukEntry.Text = "mg.";
                    break;
                case ("op."):
                    this.sztukEntry.Text = "op.";
                    break;
                case ("inny"):
                    string answer = await App.Current.MainPage.DisplayPromptAsync("Podaj inn¹ jednostkê", "Zdefinuj w³asn¹ jednostkê: ", "Zapisz", "Anuluj");
                    if (answer != null)
                    {
                        this.sztukEntry.Text = answer;
                    }
                    break;
                default:
                    this.sztukEntry.Text = "szt.";
                    break;
            }
        }
        XMLSave(this.catName, this.Entry.Text, this.iloscLabel.Text, this.sztukEntry.Text, this.checkProduct.IsChecked, this.ID_hidden.Text);
    }

    private void XMLSave(string catName, string name, string ilosc, string rodzaj_szt, bool isDone, string ID)
    {
        string fileName = "SavedProducts.xml";
        string appDataPath = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, fileName);

        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }

        string filePath = Path.Combine(appDataPath, fileName);

        XDocument doc;
        if (!File.Exists(filePath))
        {
            doc = new XDocument(new XElement("root"));
        }
        else
        {
            doc = XDocument.Load(filePath);
        }

        XElement category = doc.Descendants("category")
            .FirstOrDefault(x => x.Attribute("name")?.Value == catName);

        if (category == null)
        {
            category = new XElement("category", new XAttribute("name", catName));
            doc.Element("root").Add(category);
        }

        XElement product = category.Descendants("Product")
            .FirstOrDefault(x => x.Attribute("id")?.Value == this.ID_hidden.Text);

        if (product == null)
        {
            product = new XElement("Product",
                new XAttribute("id", ID ?? ""),
                new XAttribute("name", name ?? ""),
                new XElement("ilosc", ilosc),
                new XElement("sztukateria", rodzaj_szt),
                new XElement("isDone", isDone.ToString()));
            category.Add(product);
        }
        else
        {
            product.SetAttributeValue("name", name);
            product.SetElementValue("ilosc", ilosc);
            product.SetElementValue("sztukateria", rodzaj_szt);
            product.SetElementValue("isDone", isDone.ToString());
        }

        var sortedProducts = category
            .Elements("Product")
            .OrderBy(x => (!(bool)x.Element("isDone") ? 1 : 0)
            );

        category.ReplaceNodes(sortedProducts);

        doc.Save(filePath);
        Debug.WriteLine(filePath);
    }





    private void removeFromXML(string categoryName, string productName, string iD )
    {
        string fName = "SavedProducts.xml";

        string appDataPath = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, fName);

        string path = System.IO.Path.Combine(appDataPath, fName);

        XDocument doc = XDocument.Load(path);

        var categoryElements = from category in doc.Descendants("category")
                               where (string)category.Attribute("name") == categoryName
                               select category;


        // dla ka¿dego znalezionego elementu kategorii, znajdŸ element produktu z atrybutem name równym productName
        foreach (var categoryElement in categoryElements)
        {

            var productElement = from product in categoryElement.Descendants("Product")
                                 where (string)product.Attribute("id") == iD
                                 select product;


            // jeœli znaleziono szukany produkt, usuñ go z elementów kategorii
            if (productElement.Count() > 0)
            {

                productElement.Remove();
                break; // przerwij pêtlê, bo usunêliœmy ju¿ szukany produkt
            }
        }

        // zapisz zmiany do pliku XML
        doc.Save(path);
    }

    public static string GenerateId()
    {
        Guid guid = Guid.NewGuid();
        return guid.ToString();
    }


}