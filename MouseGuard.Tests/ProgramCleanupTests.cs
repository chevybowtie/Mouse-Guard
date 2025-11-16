using System;
using System.Reflection;
using Xunit;

namespace MouseGuard.Tests
{
    public class ProgramCleanupTests
    {
        [Fact]
        public void CleanupResources_ShouldDisposeAndNullFields()
        {
            // Arrange: create simple objects in Program via reflection if necessary
            var programType = typeof(Program);

            // Create a dummy monitorTimer by setting the private static field
            var monitorTimerField = programType.GetField("monitorTimer", BindingFlags.Static | BindingFlags.NonPublic);
            var monitorCountTimerField = programType.GetField("monitorCountTimer", BindingFlags.Static | BindingFlags.NonPublic);
            var trayIconField = programType.GetField("trayIcon", BindingFlags.Static | BindingFlags.NonPublic);
            var silentNotificationField = programType.GetField("silentNotification", BindingFlags.Static | BindingFlags.NonPublic);
            var loadedIconField = programType.GetField("loadedIcon", BindingFlags.Static | BindingFlags.NonPublic);
            var messageWindowField = programType.GetField("messageWindow", BindingFlags.Static | BindingFlags.NonPublic);

            // Verify reflection returned valid fields
            Assert.NotNull(monitorTimerField);
            Assert.NotNull(monitorCountTimerField);
            Assert.NotNull(trayIconField);
            Assert.NotNull(silentNotificationField);
            Assert.NotNull(loadedIconField);
            Assert.NotNull(messageWindowField);

            // Set to non-null dummy values
            monitorTimerField!.SetValue(null, new System.Windows.Forms.Timer());
            monitorCountTimerField!.SetValue(null, new System.Windows.Forms.Timer());
            trayIconField!.SetValue(null, new System.Windows.Forms.NotifyIcon());
            // Create an instance of the nested private type Program.SilentNotification via reflection
            var silentType = programType.GetNestedType("SilentNotification", BindingFlags.NonPublic);
            Assert.NotNull(silentType);
            var silentInstance = Activator.CreateInstance(silentType!, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new object[] { "title", "msg" }, null);
            silentNotificationField.SetValue(null, silentInstance);

            // Set the loaded icon by cloning a system icon so we can safely dispose it
            Assert.NotNull(loadedIconField);
            loadedIconField!.SetValue(null, (System.Drawing.Icon)System.Drawing.SystemIcons.Application.Clone());

            // Create a hidden message window instance and assign
            var hiddenType = programType.GetNestedType("HiddenMessageWindow", BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(hiddenType);
            var hiddenInstance = Activator.CreateInstance(hiddenType!, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new object[] { "testwin" }, null);
            messageWindowField!.SetValue(null, hiddenInstance);

            // Act
            Program.CleanupResources();

            // Assert: fields are null
            Assert.Null(monitorTimerField.GetValue(null));
            Assert.Null(monitorCountTimerField.GetValue(null));
            Assert.Null(trayIconField.GetValue(null));
            Assert.Null(silentNotificationField.GetValue(null));
            Assert.Null(loadedIconField.GetValue(null));
            Assert.Null(messageWindowField.GetValue(null));
        }
    }
}
