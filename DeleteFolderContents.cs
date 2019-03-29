using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using Task = System.Threading.Tasks.Task;

namespace CustomClean
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class DeleteFolderContents
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("17ee8c4d-54ea-4fd8-a09d-1e050a5a6613");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteFolderContents"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private DeleteFolderContents(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static DeleteFolderContents Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in DeleteFolderContents's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new DeleteFolderContents(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            
            ArrayList directories;
            ArrayList extensionExceptions;
            ArrayList nameExceptions;
            int numDeletedFiles = 0;
            ThreadHelper.ThrowIfNotOnUIThread();
            string projectPath = GetProjectBaseDirectory() + "\\";
            LoadXMLSettings(projectPath, out directories, out extensionExceptions, out nameExceptions);
            numDeletedFiles = CleanDirectories(directories, extensionExceptions, nameExceptions);
            displayEndMessage(numDeletedFiles, directories, extensionExceptions, nameExceptions);
        }

        private void LoadXMLSettings(string projectPath, out ArrayList directory_list, out ArrayList extensionExceptions, out ArrayList nameExceptions)
        {
            directory_list = new ArrayList();
            extensionExceptions = new ArrayList();
            nameExceptions = new ArrayList();
            try
            {
                XElement root = XElement.Load(Path.Combine(projectPath + "CustomClean.xml"));
                IEnumerable<XElement> dirs =
                    from element in root.Elements("DIRECTORIES")
                    select element;
                foreach (XElement path in dirs)
                {
                    directory_list.Add(Path.Combine(projectPath, path.Value));
                }

                IEnumerable<XElement> nameExceptionElements =
                    from element in root.Elements("IGNORE").Elements("NAMEEXCEPTIONS")
                    select element;
                foreach (XElement name in nameExceptionElements)
                {
                    nameExceptions.Add(name.Value);
                }

                IEnumerable<XElement> extExceptionElements =
                    from element in root.Elements("IGNORE").Elements("FILETYPEEXCEPTIONS")
                    select element;
                foreach (XElement ext in extExceptionElements)
                {
                    extensionExceptions.Add(ext.Value);
                }
            }
            catch (Exception e)
            {
                displayMessage("CustomClean.xml could not be found. Ensure that there is a properly formatted XML in the project root directory (See user doc)");
            }
        }

        private int CleanDirectories(ArrayList directories, ArrayList extensionExceptions, ArrayList nameExceptions)
        {
            int numDeletedFiles = 0;
            foreach (string dir in directories)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(dir);
                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    string filename = file.Name.Substring(0, file.Name.LastIndexOf("."));
                    if (!(extensionExceptions.Contains(file.Extension) || nameExceptions.Contains(filename)))
                    {
                        file.Delete();
                        numDeletedFiles += 1;
                    }
                }
            }
            return numDeletedFiles;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        private string GetProjectBaseDirectory()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            EnvDTE.DTE dte = (DTE)ServiceProvider.GetService(typeof(DTE));
            string solutionDir = System.IO.Path.GetDirectoryName(dte.Solution.FullName);
            return solutionDir;
        }

        private void displayMessage(string message)
        {
            MessageBox.Show(message);
        }

        private void displayEndMessage(int numDeletedFiles, ArrayList directories, ArrayList extensionExceptions, ArrayList nameExceptions)
        {
            string endString = "Clean Complete: " + numDeletedFiles + " Files deleted" + Environment.NewLine;
            endString += "Directories cleaned:        ";
            foreach (string dir in directories) { endString += dir.Substring(dir.LastIndexOf("\\"), dir.Length - dir.LastIndexOf("\\")) + " "; };
            endString += Environment.NewLine + "Ignored files named:      ";
            foreach (string name in nameExceptions) { endString += name + " "; };
            endString += Environment.NewLine + "Ignored file extenions:   ";
            foreach (string ext in extensionExceptions) { endString += ext + " "; };
            displayMessage(endString);
        }
    }
}