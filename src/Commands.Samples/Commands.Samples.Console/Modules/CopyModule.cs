using System.Diagnostics;

namespace Commands.Samples
{
    public class CopyModule : ModuleBase
    {
        [Name("copy")]
        [RequireOperatingSystem(PlatformID.Win32NT)]
        public void Copy([Remainder] string toCopy)
        {
            Process clipboardExecutable = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardInput = true,
                    FileName = "clip",
                }
            };
            clipboardExecutable.Start();

            clipboardExecutable.StandardInput.Write(toCopy);
            clipboardExecutable.StandardInput.Close();

            Console.WriteLine("Succesfully copied the content to your clipboard.");
        }
    }
}
