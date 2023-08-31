using System;
using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace GitCodeCompiler
{
    /// <summary>
    /// Main program class.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            string repoUrl = "https://github.com/YourUsername/YourRepository.git";
            string destinationPath = "repo_clone";

            // Clone the Git repository
            CloneOptions options = new CloneOptions
            {
                BranchName = "master", // Change to your desired branch
                RecurseSubmodules = true,
                CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials
                    {
                        Username = "YourUsername",
                        Password = "YourPassword" // Or use token, depending on your repository settings
                    }
            };

            Repository.Clone(repoUrl, destinationPath, options);

            // Get all files from the repository
            DirectoryInfo repoDirectory = new DirectoryInfo(destinationPath);
            FileInfo[] allFiles = repoDirectory.GetFiles("*.*", SearchOption.AllDirectories);

            // Detect and compile source code files
            string[] sourceCodeExtensions = { ".cs", ".cpp", ".java", ".py", ".rb", ".js", ".html", ".css" }; // Add more extensions as needed

            using (StreamWriter writer = new StreamWriter("compiled_code.txt"))
            {
                foreach (FileInfo file in allFiles.Where(f => IsSourceCodeFile(f.Extension)))
                {
                    writer.WriteLine($"# File: {file.FullName}");
                    writer.WriteLine(File.ReadAllText(file.FullName));
                    writer.WriteLine("##################################################");
                }
            }

            Console.WriteLine("Compilation and merging completed.");
        }

        // Check if a file extension belongs to a source code file
        static bool IsSourceCodeFile(string fileExtension)
        {
            string[] sourceCodeExtensions = { ".cs", ".cpp", ".java", ".py", ".rb", ".js", ".html", ".css" }; // Add more extensions as needed
            return sourceCodeExtensions.Contains(fileExtension.ToLower());
        }
    }
}
