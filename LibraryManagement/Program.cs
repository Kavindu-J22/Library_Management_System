using LibraryManagement.UI;

namespace LibraryManagement
{
    /// <summary>
    /// Main program entry point for the Library Management System
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main method - entry point of the application
        /// </summary>
        /// <param name="args">Command line arguments</param>
        static async Task Main(string[] args)
        {
            try
            {
                Console.Title = "Library Management System";

                // Check if test mode is requested
                if (args.Length > 0 && args[0].ToLower() == "--test")
                {
                    Console.WriteLine("Running in TEST mode...\n");
                    var testRunner = new TestRunner();
                    await testRunner.RunTestsAsync();
                    await testRunner.DemonstrateSearchAsync();
                    await testRunner.DemonstrateValidationAsync();

                    Console.WriteLine("\nPress any key to exit...");
                    Console.ReadKey();
                    return;
                }

                // Create and run the console UI
                var consoleUI = new ConsoleUI();
                await consoleUI.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
