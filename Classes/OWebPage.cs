/*
' /====================================================\
'| Developed Tony N. Hyde (www.k2host.co.uk)            |
'| Projected Started: 2019-07-04                        | 
'| Use: General                                         |
' \====================================================/
*/

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using Microsoft.CSharp;
using Microsoft.VisualBasic;

using HtmlAgilityPack;
using K2host.WebServer.Delegates;
using K2host.Core;

namespace K2host.WebServer.Classes
{


    /// <summary>
    /// This class is a container for .net pages along side the resource pages.
    /// </summary>
    public class OWebPage : IDisposable
    {

        #region Fields

        /// <summary>
        /// A list of library refernces loaded when the .net script file is compiled.
        /// </summary>
        private readonly List<string> ScriptReferences = new List<string>();

        /// <summary>
        /// This is the html resouce file linked to the script / .net file.
        /// </summary>
        private readonly FileInfo HtmlFileResouce = null;

        /// <summary>
        /// This is the .net script / code file that will get compiled.
        /// </summary>
        private readonly FileInfo CodeFileResouce = null;

        /// <summary>
        /// This is the compiler parameters pre set on the instance using the <see cref="ScriptReferences"/>
        /// </summary>
        private readonly CompilerParameters CompilerParams = null;

        /// <summary>
        /// This si the compiler that will create an assembly from the class in the code.
        /// </summary>
        private readonly CodeDomProvider Complier = null;

        /// <summary>
        /// This the class object in code.
        /// </summary>
        private Type ClassType = null;

        /// <summary>
        /// This is the assembly that was compiled from the <see cref="CodeDomProvider"/>
        /// </summary>
        private Assembly CodeCompiled = null;

        /// <summary>
        /// This is the instance of the <see cref="ClassType"/> from the <see cref="CodeCompiled"/>
        /// </summary>
        private object CodeInstance = null;

        /// <summary>
        /// This is the PageLoad method in the instance of you page code.
        /// </summary>
        private MethodInfo CodeInstanceMethod = null;

        /// <summary>
        /// This is the Page property that holds the html resouce from its counter part.
        /// This will hold a value of type <see cref="HtmlDocument"/>
        /// </summary>
        private PropertyInfo CodeInstanceProperty = null;

        #endregion

        #region Properties

        /// <summary>
        /// This will hold any errors when the code is compiled either at runtime or pre compile.
        /// </summary>
        public List<string> ScriptErrors { get; set; }

        /// <summary>
        /// This is the event that triggers the PageLoad method in the code page file.
        /// This is will triggered by tthe web application.
        /// </summary>
        public OnORequestCompileEvent PageLoad { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates the instance of the container.
        /// </summary>
        public OWebPage()
        {

            ScriptErrors = new List<string>();
            ScriptReferences = new List<string>();
        }

        /// <summary>
        /// Creates the instance of the container.
        /// </summary>
        /// <param name="codeFile">The .net file counter part.</param>
        /// <param name="htmlFile">The html file counter part.</param>
        /// <param name="app">The web application this resource is contained.</param>
        public OWebPage(FileInfo codeFile, FileInfo htmlFile, OWebApplication app)
            : this()
        {


            CodeFileResouce = codeFile;
            HtmlFileResouce = htmlFile;

            switch (codeFile.Extension.ToLower())
            {
                case ".cs":
                    Complier = new CSharpCodeProvider();
                    break;
                case ".vb":
                    Complier = new VBCodeProvider();
                    break;
            }

            if (File.Exists(app.ApplicationRootPath + "\\bin\\refs.cfg"))
                ScriptReferences.AddRange(File.ReadAllLines(app.ApplicationRootPath + "\\bin\\refs.cfg"));

            CompilerParams = new CompilerParameters { GenerateInMemory = true };

            try
            {
                foreach (Assembly CodeAssembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        if (Path.GetExtension(CodeAssembly.Location.ToString().ToLower()) == ".dll")
                            if (!CompilerParams.ReferencedAssemblies.Contains(CodeAssembly.Location.ToString()))
                                CompilerParams.ReferencedAssemblies.Add(CodeAssembly.Location.ToString());
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                ScriptErrors.Add(ex.StackTrace);
                return;
            }

            try { CompilerParams.ReferencedAssemblies.AddRange(ScriptReferences.ToArray()); }
            catch (Exception ex)
            {
                ScriptErrors.Add(ex.StackTrace);
                return;
            }

            if (!app.RunTimeCompile)
                CompileCode();

            PageLoad = new OnORequestCompileEvent(OnPageLoad);

        }

        #endregion

        #region Methods

        /// <summary>
        /// This is used to compile the code resource ready for executing.
        /// </summary>
        /// <returns>If is not sucessfull you can read the <see cref="ScriptErrors"/> property</returns>
        private bool CompileCode()
        {


            ScriptErrors.Clear();

            CodeCompiled = null;
            ClassType = null;
            CodeInstance = null;
            CodeInstanceProperty = null;
            CodeInstanceMethod = null;

            CompilerResults ComplierResults = Complier.CompileAssemblyFromFile(CompilerParams, CodeFileResouce.FullName);

            if (ComplierResults.Errors.HasErrors)
            {
                for (var i = 0; i < ComplierResults.Errors.Count; i++)
                    ScriptErrors.Add(ComplierResults.Errors[i].ErrorText);
                return false;
            }

            CodeCompiled = ComplierResults.CompiledAssembly;
            ClassType = CodeCompiled.GetType(CodeFileResouce.Name.Replace(CodeFileResouce.Extension, string.Empty));
            CodeInstance = CodeCompiled.CreateInstance(ClassType.Name);

            CodeInstanceProperty = CodeInstance.GetType().GetProperty("Page", BindingFlags.Public | BindingFlags.Instance);
            if (CodeInstanceProperty == null)
            {
                ScriptErrors.Add("Error reading property: Page property is missing.");
                return false;
            }

            CodeInstanceMethod = CodeInstance.GetType().GetMethod("PageLoad", BindingFlags.Public | BindingFlags.Instance);
            if (CodeInstanceProperty == null)
            {
                ScriptErrors.Add("Error reading method: PageLoad method is missing.");
                return false;
            }

            return true;

        }

        /// <summary>
        /// This is the method that executes the method on the code page, that web application will do this.
        /// Code compiled at runtime is slower and really used in a development environment. 
        /// The advantage is that you can change code while the applcation is running.
        /// </summary>
        /// <param name="e">The web http request that came in on the client connection.</param>
        /// <param name="IsRunTimeCompile">defined if the code will be compiled at runtime.</param>
        private void OnPageLoad(OWebRequest e, bool IsRunTimeCompile = false)
        {

            if (IsRunTimeCompile)
                if (!CompileCode())
                {
                    e.Application.Parent.GetDefaultHtmlContent(500, "Server Error", e, ScriptErrors.JoinWith(", "));
                    return;
                }

            HtmlDocument Content = new HtmlDocument();
            Content.LoadHtml(
                Encoding.UTF8.GetString(
                    File.ReadAllBytes(HtmlFileResouce.FullName)
                )
            );

            if (CodeInstanceProperty != null)
                CodeInstanceProperty.SetValue(CodeInstance, Content);

            try
            {
                CodeInstanceMethod.Invoke(
                    CodeInstance,
                    new object[] {
                        e
                    }
                );
            }
            catch (Exception ex)
            {
                e.Application.Parent.GetDefaultHtmlContent(500, "Server Error", e, ex.StackTrace);
                return;
            }

            Content.Save(e.Response.OutputStream);

        }

        #endregion

        #region Destructor

        bool IsDisposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
            {
                PageLoad = null;
                ScriptReferences.Clear();
                ScriptErrors.Clear();
            }

            IsDisposed = true;
        }

        #endregion

    }


}
