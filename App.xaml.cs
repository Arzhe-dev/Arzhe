using System.Windows;
using System.Security.Principal;

namespace Arzhe;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        if (!IsAdmin())
        {
            MessageBox.Show(
                "Arzhe nécessite les privilèges administrateur pour optimiser votre système.\n\n" +
                "Veuillez relancer l'application en tant qu'administrateur.",
                "Privilèges insuffisants",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            Shutdown();
            return;
        }
        base.OnStartup(e);
    }

    private static bool IsAdmin()
    {
        using var identity = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }
}