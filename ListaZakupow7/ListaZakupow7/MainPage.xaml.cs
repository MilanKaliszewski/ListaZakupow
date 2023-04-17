using ListaZakupow7.ContentViews;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace ListaZakupow7;

public partial class MainPage : ContentPage
{

    public MainPage()
    {
        InitializeComponent();

        MessagingCenter.Subscribe<Category, string>(this, "CategoryRemoved", OnCategoryRemoved);

        XMLRead();
    }
    public void OnCategoryRemoved(Category sender, string category)
    {
        //var btn = this.FindByName<Button>(category);

        foreach (var child in this.predefView.Children)
        {
            if (child is Button button && button.Text.Contains(category))
            {
                button.IsEnabled = true;
            }
        }

        foreach (var child in this.customCategories.Children)
        {
            if (child is Button button && button.Text.Contains(category))
            {
                button.IsEnabled = true;
            }
        }
    }


    private async void newCategory_Clicked(object sender, EventArgs e)
    {
        bool check1 = true;
        bool check2 = true;

        string result = await DisplayPromptAsync("Nowa kategoria", "Jak mamy nazwać Twoją kategorię?");

        foreach (var child in this.predefView.Children)
        {
            if (child is Button button && button.Text.ToLower().Contains(result.ToLower()))
            {
                check1 = false;
            }
        }

        foreach (var child in this.customCategories.Children)
        {
            if (child is Button button && button.Text.ToLower().Contains(result.ToLower()))
            {
                check2 = false;
            }
        }

        if (check1 && check2)
        {
            bool answer = await DisplayAlert("Kategorie użytkownika", "Czy chcesz zapisać \"" + result + "\" jako preset?", "Tak", "Nie");
            if (answer)
            {
                VerticalStackLayout vsl = this.customCategories;
                Button btn = new Button
                {

                    Text = result,
                    WidthRequest = 200
                };
                btn.Clicked += (s, e) => { CategoryButton_Clicked(s, e); };
                vsl.Children.Add(btn);
                this.brakCustom.IsVisible = false;
                vsl.IsVisible = true;
            }
            else
            {
                this.Categories.Children.Add(new Category(result));
            }
        }
        else
        {
            await DisplayAlert("Błąd!", "Nie można utworzyć kategorii o tej samej nazwie co kategorie z presetów!", "OK");
        }

    }

    string isCustom;
    private void CategoryButton_Clicked(object sender, EventArgs e)
    {
        Button btn = (Button)sender;
        Debug.WriteLine(btn.Text);

        foreach (var child in this.predefView.Children)
        {
            if (child is Button button && button.Text.ToLower().Contains(btn.Text.ToLower()))
            {
                button.IsEnabled = false;
                this.isCustom = "false";
            }
        }

        foreach (var child in this.customCategories.Children)
        {
            if (child is Button button && button.Text.ToLower().Contains(btn.Text.ToLower()))
            {
                button.IsEnabled = false;
                this.isCustom = "true";
            }
        }

        this.Categories.Children.Add(new Category(btn.Text, this.isCustom));




        //btn - guzik który został wciśnięty.
    }


    //Funkcja ładująca XML
    private void XMLRead()
    {

        string fName = "SavedProducts.xml";

        string appDataPath = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, fName);

        string path = Path.Combine(appDataPath, fName);

        bool check = true;

        if (File.Exists(path))
        {
            XDocument doc = XDocument.Load(path);

            var categories = (from category in doc.Descendants("category")
                              select new { Name = category.Attribute("name").Value, Color = category.Attribute("color").Value, isCustom = category.Attribute("isCustom").Value }).ToList();


            foreach (var category in categories)
            {
                this.Categories.Children.Add(new Category(category.Name, category.Color, category.isCustom));

                if (category.isCustom == "true")
                {
                    Debug.WriteLine("isCustom: " + category.isCustom);

                    VerticalStackLayout vsl = this.customCategories;
                    Button btn = new Button
                    {

                        Text = category.Name,
                        WidthRequest = 200
                    };
                    btn.Clicked += (s, e) => { CategoryButton_Clicked(s, e); };
                    vsl.Children.Add(btn);
                    this.brakCustom.IsVisible = false;
                    vsl.IsVisible = true;
                    btn.IsEnabled = false;
                }
                else
                {
                    Debug.WriteLine("false custom");
                    foreach (var child in this.predefView.Children)
                    {
                        if (child is Button button && button.Text.ToLower().Contains(category.Name.ToLower()))
                        {
                            button.IsEnabled = false;
                            Debug.WriteLine($"{button.Text} is disabled");
                        }
                    }
                }


            }
        }
    }
}
