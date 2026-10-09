using System;
using System.Windows;
using System.Security.Principal;

namespace Arzhe;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // IMPORTANT : ne pas fermer l'app quand le splash se ferme
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        // 1. Verifier les privileges admin
        if (!IsAdmin())
        {
            MessageBox.Show(
                "Arzhe necessite les privileges administrateur pour optimiser votre systeme.\n\n" +
                "Veuillez relancer l'application en tant qu'administrateur.",
                "Privileges insuffisants",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            Shutdown();
            return;
        }

        // 2. Afficher le splash screen
        var splash = new SplashWindow();
        splash.Show();

        // 3. Ouvrir la fenetre principale apres le splash
        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(2200)
        };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            splash.Close();

            var main = new MainWindow();
            MainWindow = main;
            ShutdownMode = ShutdownMode.OnMainWindowClose;  // reprendre le comportement normal
            main.Show();
            main.Activate();
        };
        timer.Start();

        base.OnStartup(e);
    }

    private static bool IsAdmin()
    {
        using var identity = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }
}