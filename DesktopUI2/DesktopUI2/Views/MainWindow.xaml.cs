using System.ComponentModel;
using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DesktopUI2.ViewModels;
using ReactiveUI;

namespace DesktopUI2.Views;

public class MainWindow : ReactiveWindow<MainViewModel>
{
  public MainWindow()
  {
    this.WhenActivated(disposables => { });
    AvaloniaXamlLoader.Load(this);

#if DEBUG
    this.AttachDevTools(KeyGesture.Parse("CTRL+R"));
#endif
  }

  protected override void OnClosing(CancelEventArgs e)
  {
    Hide();
    e.Cancel = true;
    base.OnClosing(e);
  }
}
