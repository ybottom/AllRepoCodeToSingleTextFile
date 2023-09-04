namespace GitHubSourceToTextFile {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;

    internal class Program {
        static readonly string _helpMessage = $"Usage:\r\n {Path.GetFileNameWithoutExtension(Assembly.GetAssembly(typeof(Program)).ToString())} <user> <repo> <dest path> <branch(default:main)>\r\n" +
                                               "Example:\r\n {Path.GetFileNameWithoutExtension(Assembly.GetAssembly().ToString())} ybottom GitHubSourceToTextFile %USERPROFILE%\\Documents main\r\n";
        static bool IsSourceCodeFileExtension(string path) {
            return (new string[] {
                ".c", ".cc", ".cpp", ".cxx", ".h", ".hh", ".hpp", ".hxx", // C and C++
                ".java", // Java
                ".cs", // C#
                ".py", ".pyc", ".pyo", ".pyw", ".pyz", // Python
                ".rb", ".rbs", ".rbw", // Ruby
                ".js", ".mjs", // JavaScript
                ".html", ".htm", ".xhtml", ".xml", ".svg", // HTML/XML
                ".css", ".scss", ".sass", // CSS
                ".php", ".php3", ".php4", ".php5", ".php7", ".phtml", // PHP
                ".pl", ".pm", // Perl
                ".swift", // Swift
                ".kt", ".kts", // Kotlin
                ".scala", // Scala
                ".groovy", // Groovy
                ".lua", // Lua
                ".sh", ".bash", ".zsh", ".fish", // Shell scripting
                ".bat", ".cmd", // Batch scripting
                ".ps1", ".psm1", // PowerShell
                ".as", ".mxml", // ActionScript and MXML
                ".coffee", // CoffeeScript
                ".dart", // Dart
                ".elm", // Elm
                ".erl", ".hrl", // Erlang
                ".fs", ".fsi", ".fsx", // F#
                ".go", // Go
                ".hs", ".lhs", // Haskell
                ".lisp", ".cl", ".lsp", // Lisp
                ".ml", ".mli", ".sml", ".sig", ".fun", // ML family (Standard ML, OCaml, F#)
                ".pas", ".pp", ".lpr", // Pascal
                ".perl", ".pl", ".pm", ".al", // ALGOL
                ".prolog", ".pl", // Prolog
                ".rust", // Rust
                ".sq", // Squeak
                ".tcl", // Tcl
                ".vb", ".vbs", // Visual Basic
                ".wasm", // WebAssembly
                ".yaml", ".yml", // YAML
                ".json", // JSON
                ".sql", // SQL
                ".tex", ".cls", ".sty", // TeX
                ".r", ".rmd", // R
                ".m", ".mm", // Objective-C
                ".ino", // Arduino
                ".sc", // SuperCollider
                ".d", // D
                ".ex", ".exs", // Elixir
                ".nim", // Nim
                ".l", ".y", // Lex and Yacc
                ".adb", // Ada
                ".lsl", // Linden Scripting Language
                ".abap", // ABAP
                ".apex", // Apex
                ".bashrc", ".bash_profile", ".bash_aliases", // Bash configuration files
                ".bat", // Windows Batch file
                ".cfg", ".ini", // Configuration files
                ".cmake", ".makefile", // CMake and Makefile
                ".config", // Configuration files
                ".cr", // Crystal
                ".dats", ".d2", // ATS
                ".diff", ".patch", // Diff/Patch files
                ".dockerfile", // Dockerfile
                ".fish", // Fish shell script
                ".hcl", // HashiCorp Configuration Language
                ".iced", // IcedCoffeeScript
                ".properties", ".ini", // Properties files
                ".nix", // Nix
                ".md", ".markdown", ".mkd", // Markdown
                ".proto", // Protocol Buffers
                ".rake", // Rakefile
                ".rest", ".rst", // reStructuredText
                ".scm", // Scheme
                ".shader", // Shader files
                ".sln", // Visual Studio Solution
                ".toml", // TOML
                ".txt", // Plain text
                ".vhdl", // VHDL
                ".v", // Verilog
                ".xaml", // XAML
                ".zshrc", // Zsh configuration
            }).Contains(Path.GetExtension(path).ToLower());
        }
        const string _tempZipFn = "repo_temp.zip";
        static void Main(string[] args) {

            if(args.FirstOrDefault().Trim().ToLower() == "help") {
                Console.WriteLine(_helpMessage);
                return;
            }

            DirectoryInfo destinationDirectory = new DirectoryInfo(System.IO.Path.GetDirectoryName(args[2]));
            if(!destinationDirectory.Exists) {
                string[] directoryChain = destinationDirectory.FullName.Split(new char[] { System.IO.Path.PathSeparator,System.IO.Path.AltDirectorySeparatorChar });
                string currentPath = directoryChain.First();
                for(int i = 1;i < directoryChain.Length;i--) {
                    currentPath += System.IO.Path.PathSeparator + directoryChain[i];
                    if(!Directory.Exists(currentPath)) {
                        Directory.CreateDirectory(currentPath);
                    }
                }
            }

            Console.Write("Downloading repo contents...");

            using(HttpClient client = new HttpClient()) {
                client.DefaultRequestHeaders.Add("User-Agent","GitHubSourceToTextFile"); // GitHub API requires User-Agent header
                client.DefaultRequestHeaders.Add("accept","application/vnd.github+json");
                Uri requestUri = new Uri($"https://api.github.com/repos/{args[0]}/{args[1]}/zipball/{args?[3] ?? "main"}"); // Push the initial URL used to obtain the repo root
                HttpResponseMessage zipResponse = client.GetAsync(requestUri).Result;
                zipResponse.EnsureSuccessStatusCode();
                using(Stream srcStream = zipResponse.Content.ReadAsStreamAsync().Result)
                using(FileStream tgtStream = File.Create(_tempZipFn)) {
                    srcStream.CopyTo(tgtStream);
                }
            }

            Console.Write("inflating...");

            string _tempZipPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "temp_repo_contents");
            Directory.CreateDirectory(_tempZipPath);

            ZipFile.ExtractToDirectory(_tempZipFn,_tempZipPath);

            Console.WriteLine("completed.");

            Stack<string> _directories = new Stack<string>();
            _directories.Push(_tempZipPath);

            using(StreamWriter tgtTextStream = File.CreateText(args[2])) {
                tgtTextStream.WriteLine("/// " + _tempZipPath);
                while(_directories.Any()) {
                    DirectoryInfo currentDir = new DirectoryInfo(_directories.Pop());
                    string relativePath = currentDir.FullName.Replace(_tempZipPath,"");
                    tgtTextStream.WriteLine("\t/// " + relativePath);
                    Console.WriteLine($"Searching [{currentDir.FullName}]...");
                    DirectoryInfo[] subDirs = currentDir.GetDirectories();
                    Console.Write($"\tFound {subDirs.Length} sub-directories...");
                    for(int i = 0;i < subDirs.Length;i++) {
                        _directories.Push(subDirs[i].FullName);
                    }
                    Console.WriteLine("completed.");
                    FileInfo[] files = currentDir.GetFiles();
                    Console.WriteLine($"\tAppending {files.Length} files...");
                    for(int i = 0;i < files.Length;i++) {
                        if(IsSourceCodeFileExtension(files[i].Name)) {
                            Console.Write($"\t\tAppending {files[i].Name}...");
                            using(StreamReader srcTextStream = files[i].OpenText()) {
                                tgtTextStream.WriteLine("\t\t/// " + files[i].Name);
                                tgtTextStream.WriteLine(srcTextStream.ReadToEnd());
                            }
                            Console.WriteLine("completed.");
                        } else {
                            Console.WriteLine("\t\tSkipped {files[i].Name} (not a recognised source file name extension).");
                        }
                    }
                }
            }

            Console.Write("Erasing temporary repo copy...");

            Directory.Delete(_tempZipPath, true);

            Console.WriteLine("completed.");
            Console.WriteLine($"Concatenated file completed: \"{args[2]}\" ({new FileInfo(args[2]).Length:#,##0} bytes)");
        }
    }
}
