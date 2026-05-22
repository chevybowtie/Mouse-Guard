using System.Reflection;
using System.Windows.Forms;

namespace MouseGuard.Tests;

public class ProgramHotkeyTests
{
    [Fact]
    public void TryRegisterHotkey_ModifierOnlyHotkey_IsRejectedBeforeNativeCall()
    {
        var programType = typeof(Program);
        var registerField = programType.GetField("registerHotKeyCallback", BindingFlags.Static | BindingFlags.NonPublic);
        var method = programType.GetMethod("TryRegisterHotkey", BindingFlags.Static | BindingFlags.NonPublic);

        Assert.NotNull(registerField);
        Assert.NotNull(method);

        var originalRegister = registerField!.GetValue(null);
        var called = false;

        try
        {
            registerField.SetValue(null, new Func<IntPtr, int, uint, uint, bool>((_, _, _, _) =>
            {
                called = true;
                return true;
            }));

            var args = new object?[] { Keys.Control | Keys.Alt, null };
            var result = (bool)method!.Invoke(null, args)!;

            Assert.False(result);
            Assert.False(called);
            Assert.Contains("does not include a non-modifier key", args[1] as string);
        }
        finally
        {
            registerField.SetValue(null, originalRegister);
        }
    }

    [Fact]
    public void TryRegisterHotkey_LogsWin32FailureMessage()
    {
        var programType = typeof(Program);
        var registerField = programType.GetField("registerHotKeyCallback", BindingFlags.Static | BindingFlags.NonPublic);
        var errorField = programType.GetField("getLastWin32ErrorCallback", BindingFlags.Static | BindingFlags.NonPublic);
        var method = programType.GetMethod("TryRegisterHotkey", BindingFlags.Static | BindingFlags.NonPublic);

        Assert.NotNull(registerField);
        Assert.NotNull(errorField);
        Assert.NotNull(method);

        var originalRegister = registerField!.GetValue(null);
        var originalError = errorField!.GetValue(null);

        try
        {
            registerField.SetValue(null, new Func<IntPtr, int, uint, uint, bool>((_, _, _, _) => false));
            errorField.SetValue(null, new Func<int>(() => 1234));

            var args = new object?[] { Keys.Control | Keys.Alt | Keys.B, null };
            var result = (bool)method!.Invoke(null, args)!;

            Assert.False(result);
            Assert.Contains("Win32 error 1234", args[1] as string);
        }
        finally
        {
            registerField.SetValue(null, originalRegister);
            errorField.SetValue(null, originalError);
        }
    }
}
