using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Platform;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace ListaZakupow7.ContentViews;

public partial class Category : ContentView
{

    string isCustom;
	public Category()
	{
		InitializeComponent();
	}

    public Category(string name)
    {
        InitializeComponent();
        this.HeaderListy.Text = name;
        XMLRead();
    }

	public Category(string name, string iscustom)
	{
		InitializeComponent();
		this.HeaderListy.Text = name;
        this.isCustom = iscustom;
        SaveXML();
        XMLRead();
	}

    public Category(string name, string color, string iscustom)
    {
        InitializeComponent();
        this.HeaderListy.Text = name;
        this.rameczka.BackgroundColor = Color.FromArgb(color);
        this.isCustom = iscustom;
        SaveXML();
        XMLRead();
    }

    private async void btnUsunKat_Clicked(object sender, EventArgs e)
    {
		bool answer = await App.Current.MainPage.DisplayAlert("Usuwanie", "Czy chcesz usunąć kategorię i podległe jej produkty?", "Tak", "Nie");
		if (answer)
		{
			if(this.Parent is VerticalStackLayout vsl)
			{

				vsl.Children.Remove(this);
                removeFromXML(this.HeaderListy.Text);
                MessagingCenter.Send<Category, string>(this, "CategoryRemoved", this.HeaderListy.Text);
            }
            
		}
		
    }


    bool state = true;
    private void btnZwin_Clicked(object sender, EventArgs e)
    {
		ImageButton btn = (ImageButton)sender;

		

        _ = state ? btn.Source = "down.png" : btn.Source = "up.png";

		_ = state ? state = false : state = true;
        _ = this.productParent.IsVisible == true ? this.productParent.IsVisible = false : this.productParent.IsVisible = true;
		_ = this.rameczkaDwa.IsVisible == true ? this.rameczkaDwa.IsVisible = false : this.rameczkaDwa.IsVisible = true;

        if(!state)
		{
            
            this.rameczka.StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(10)
            };
		}
		else
		{
			this.rameczka.StrokeShape = new RoundRectangle
			{
				CornerRadius = new CornerRadius(10, 10, 0, 0)
			};
		}


    }

    private void addBtn_Clicked(object sender, EventArgs e)
    {
		this.productParent.Insert(1, new ProductsView(this.HeaderListy.Text));
    }

    private void SaveXML()
    {
        string fileName = "SavedProducts.xml";
        string appDataPath = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, fileName);


        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }


        string filePath = System.IO.Path.Combine(appDataPath, fileName);


        if (!File.Exists(filePath))
        {
            XDocument xdoc = new XDocument(new XElement("root"));
            xdoc.Save(filePath);
        }


        XDocument doc = XDocument.Load(filePath);

        XElement category = doc.Descendants("category")
            .FirstOrDefault(x => x.Attribute("name")?.Value == this.HeaderListy.Text);

        if (category == null)
        {
            category = new XElement("category",
                new XAttribute("name", this.HeaderListy.Text),
                new XAttribute("color", this.rameczka.BackgroundColor.ToHex().ToString()),
                new XAttribute("isCustom", this.isCustom)
                );
            doc.Element("root").Add(category);
        }
        else
        {
            category.Attribute("color").Value = this.rameczka.BackgroundColor.ToHex().ToString();

        }


        doc.Save(filePath);
        Debug.WriteLine(filePath);
    }


    private void removeFromXML(string name)
    {
        string fName = "SavedProducts.xml";

        string appDataPath = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, fName);

        string path = System.IO.Path.Combine(appDataPath, fName);

        XDocument xmlDoc = XDocument.Load(path);

        var categoryElements = from category in xmlDoc.Descendants("category")
                               where category.Attribute("name")?.Value == name
                               select category;

        foreach (var categoryElement in categoryElements.ToList())
        {
            categoryElement.Remove();
        }

        xmlDoc.Save(path);
    }


    private async void btnPaletka_Clicked(object sender, EventArgs e)
    {
        var response = await App.Current.MainPage.DisplayActionSheet("Wybierz kolor", "Anuluj", null, "Fioletowy (domyślny)", "Czerwony", "Niebieski", "Zielony", "Różwy", "Pomarańczowy", "Czarny", "Biały");
        if (response != null)
        {
            switch (response)
            {
                case ("Domyślny"):
                    this.rameczka.BackgroundColor = Colors.Purple; 
                    break;
                case ("Czerwony"):
                    this.rameczka.BackgroundColor = Colors.Red;
                    break;
                case ("Niebieski"):
                    this.rameczka.BackgroundColor = Colors.Blue;
                    break;
                case ("Zielony"):
                    this.rameczka.BackgroundColor = Colors.Green;
                    break;
                case ("Różowy"):
                    this.rameczka.BackgroundColor = Colors.Pink;
                    break;
                case ("Pomarańczowy"):
                    this.rameczka.BackgroundColor = Colors.Orange;
                    break;
                case ("Czarny"):
                    this.rameczka.BackgroundColor = Colors.Black;
                    break;
                case ("Biały"):
                    this.rameczka.BackgroundColor = Colors.White;
                    break;
                default:
                    this.rameczka.BackgroundColor = Colors.Purple;
                    break;
            }
            SaveXML();
        }
    }



    private void XMLRead()
    {
        string fName = "SavedProducts.xml";

        string appDataPath = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, fName);

        string path = System.IO.Path.Combine(appDataPath, fName);

        if (File.Exists(path))
        {
            XDocument doc = XDocument.Load(path);

            var products = from p in doc.Descendants("Product")
                           where p.Parent.Attribute("name").Value == this.HeaderListy.Text
                           select new ProductsView(
                               this.HeaderListy.Text,
                               p.Attribute("name").Value,
                               p.Element("ilosc").Value,
                               p.Element("sztukateria").Value,
                               p.Element("isDone").Value,
                               p.Attribute("id").Value
                           ) ;
            
            foreach (var product in products)
            {
                this.productParent.Insert(1, product);

            }
        }
    }
}