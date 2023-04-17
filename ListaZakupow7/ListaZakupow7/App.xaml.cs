
namespace ListaZakupow7;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}

    protected override Window CreateWindow(IActivationState activationState)
    {
		var windows = base.CreateWindow(activationState);

		windows.X = 170;
		windows.Y = 50;

		windows.Height = 800;
		windows.Width = 1200;

        return windows;
    }
}
