# 🗃️ All Repo Code to Single Text File

## 🛠️ Command Line Application
Built with **C# / .NET**

---

## 📄 Description
This tool extracts and aggregates the contents of all code files from a specified GitHub repository into a single `.txt` file. It (was in the early days of LLMs) useful for ingesting codebases into language models and training.

---

## 🚀 Features
- Supports multiple file extensions (e.g., `.cs`, `.js`, `.py`, `.cpp`, etc.)
- Recursively traverses all folders in the repo
- Outputs a single consolidated `.txt` file
- Optionally specify a branch (defaults to `main`)
- Lightweight and fast

---

## 📦 Prerequisites
- .NET 6.0 SDK or later
- Internet access (to fetch GitHub repo)
- GitHub repo must be public

---

## 🧪 Usage

```bat
AllRepoCodeToSingleTextFile.exe <user> <repo> <dest> <branch>
```
## Example

```bat
AllRepoCodeToSingleTextFile.exe ybottom GitHubSourceToTextFile "%USERPROFILE%\\Documents" main
```

This will create a file like:

```plaintext
%USERPROFILE%\Documents\GitHubSourceToTextFile_AllCode.txt
```

📂 Supported Extensions
The tool scans for the following file types by default:
```plaintext
.cs, .js, .ts, .py, .cpp, .h, .java, .html, .css, .json, .xml, .md, .txt
```
You can modify the list in the source code if needed.

🧰 Build Instructions
```bat
dotnet build
```
The compiled .exe will be located in the bin/Debug/net6.0/ directory.

🤝 Contributing
Feel free to fork the repo and submit pull requests. Bug reports and feature suggestions are welcome via Issues.

📜 License
See LICENSE file for details.
