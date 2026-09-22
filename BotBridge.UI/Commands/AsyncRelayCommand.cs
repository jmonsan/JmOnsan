using System.Windows.Input;

namespace BotBridge.UI.Commands;

/// <summary>
/// ICommand for async operations (save/refresh/run). Two things
/// RelayCommand + "_ = SomeAsyncMethod()" doesn't give you:
///  - CanExecute is false while the operation is running, so the
///    bound Button disables itself — a double-click can't fire
///    the same save/run twice.
///  - Any exception thrown inside the operation is caught here
///    and handed to onError, instead of becoming an unobserved
///    Task exception that gets silently swallowed (or can crash
///    the process later via TaskScheduler.UnobservedTaskException).
/// </summary>
public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;
    private readonly Action<Exception>? _onError;

    private bool _isRunning;

    public event EventHandler? CanExecuteChanged;

    public AsyncRelayCommand(
        Func<Task> execute,
        Func<bool>? canExecute = null,
        Action<Exception>? onError = null)
    {
        _execute = execute;
        _canExecute = canExecute;
        _onError = onError;
    }

    public bool CanExecute(object? parameter)
    {
        return !_isRunning &&
               (_canExecute?.Invoke() ?? true);
    }

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
        {
            return;
        }

        _isRunning = true;

        RaiseCanExecuteChanged();

        try
        {
            await _execute();
        }
        catch (Exception ex)
        {
            _onError?.Invoke(ex);
        }
        finally
        {
            _isRunning = false;

            RaiseCanExecuteChanged();
        }
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(
            this,
            EventArgs.Empty);
    }
}
