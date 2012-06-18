// Based on TemplateMaschine:

/* ---------------------------------------------------------------
    TemplateMaschine - an open source template engine for C#

    written by Stefan Sarstedt (http://www.stefansarstedt.com/)
    Released under GNU Lesser General Public License (LGPL),
    see file 'copying' for details about the license

    History:
        - initial release (version 0.5) on Oct 28th, 2004
        - minor bugfixes (version 0.6) on March 29th, 2005
        - updated to support referencing assemblies that are 
          installed in the GAC (version 0.7) on January 12th, 2007
          Thanks to William.Manning@ips-sendero.com
        - Added support for generic arguments. 
          Ability to pass dictionary of arguments relaxing ordered 
          object[] requirement. Version 0.8 on March 18th, 2007
          Thanks to vijay.santhanam@gmail.com
   --------------------------------------------------------------- */

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace Kodegen
{
    /// <summary>
    /// Template Compiler Exception
    /// </summary>
    public class TemplateException : Exception
    {
        /// <summary>
        /// Template Compiler Exception 
        /// </summary>
        /// <param name="aMessage">Error message</param>
        public TemplateException(string aMessage)
            : base(aMessage)
        {
        }
    }

    /// <summary>
    /// Base class for Generator.
    /// </summary>
    public class GeneratorBase
    {
        protected string newline = "\r\n";

        private StringBuilder result;

        protected string[] args;

        protected GeneratorBase(string[] args)
        {
            this.args = args;
            result = new StringBuilder();
        }

        protected void Write(string text)
        {
            result.Append(text);
        }

        protected string Result()
        {
            return (result.ToString());
        }
    }

    /// <summary>
    /// Template class, used to load and execute a template
    /// </summary>
    public class Template
    {
        private int line = 1;
        private int column = 1;
        private string newline = "\r\n";
        private Assembly assembly;
        private StringBuilder body1 = new StringBuilder(50000);
        private StringBuilder body2 = new StringBuilder(50000);
        private StringBuilder source = new StringBuilder(50000);

        private Dictionary<string, StringBuilder> sections = new Dictionary<string, StringBuilder>();

        /// <summary>
        /// Load a template file
        /// </summary>
        /// <param name="stream">Template stream</param>
        public Template(string uri)
        {
            Read(new StreamReader(uri));
            Process();
        }

        private enum Token
        {
            RTag,
            LTag,
            LAssignment,
            String,
            Backslash,
            Newline,
            QuotationMark,
            Eof
        }

        private struct TokenInfo
        {
            public Token token;

            public string value;

            public TokenInfo(Token token, string value)
            {
                this.token = token;
                this.value = value;
            }
        }

        private int IndexOfStopToken()
        {
            int i = 0;

            while (i < body1.Length)
            {
                if ((body1[i] == '<') || (body1[i] == '\\') || (body1[i] == '%') || (body1[i] == '\r') || (body1[i] == '"'))
                {
                    break;
                }
                i++;
            }

            if (i == 0)
            {
                return (1);
            }

            return i;
        }

        private TokenInfo NextToken()
        {
            Token token = Token.Eof;

            string tokenVal = null;

            int pos = 0;

            if (body1.Length == 0)
            {
                return (new TokenInfo(Token.Eof, null));
            }

            switch (body1[0])
            {
                case '<':
                    // LTag or LAssignment?
                    if (body1[1] == '%')
                    {
                        // LAssignment
                        if (body1[2] == '=')
                        {
                            pos += 3;
                            token = Token.LAssignment;
                            break;
                        }
                        // LTag
                        pos += 2;
                        token = Token.LTag;
                        break;
                    }
                    goto default;
                case '%':
                    // RTag?
                    if (body1[1] == '>')
                    {
                        pos += 2;
                        token = Token.RTag;
                        break;
                    }
                    goto default;
                case '"':
                    token = Token.QuotationMark;
                    tokenVal = "\"";
                    pos++;
                    break;
                case '\\':
                    token = Token.Backslash;
                    tokenVal = "\\";
                    pos++;
                    break;
                case '\r':
                    // Newline?
                    if (body1[1] == '\n')
                    {
                        pos += 2;
                        line++;
                        token = Token.Newline;
                        break;
                    }
                    goto default;
                default:
                    token = Token.String;
                    int stPos = IndexOfStopToken();
                    tokenVal = body1.ToString(pos, stPos);
                    pos += stPos;
                    break;
            }

            if (tokenVal == null)
            {
                tokenVal = body1.ToString(0, pos);
            }

            if (token == Token.Newline)
            {
                column = 1;
            }
            else
            {
                column += pos;
            }

            body1.Remove(0, pos);

            return (new TokenInfo(token, tokenVal));
        }

        private void ParseAssignment()
        {
            TokenInfo tokenInfo;

            while ((tokenInfo = NextToken()).token != Token.RTag)
            {
                if ((tokenInfo.token != Token.String) && (tokenInfo.token != Token.QuotationMark))
                {
                    throw (new TemplateException("Invalid template syntax"));
                }
                body2.Append(tokenInfo.value);
            }
        }

        private void ParseTagBlock()
        {
            TokenInfo tokenInfo;

            while ((tokenInfo = NextToken()).token != Token.RTag)
            {
                switch (tokenInfo.token)
                {
                    case Token.LAssignment:
                        body2.Append("Write(");
                        ParseAssignment();
                        body2.Append(");\r\n");
                        break;
                    case Token.String:
                    case Token.QuotationMark:
                    case Token.Backslash:
                        body2.Append(tokenInfo.value);
                        break;
                    case Token.Newline:
                        body2.Append("\r\n");
                        break;
                    default:
                        throw (new TemplateException("Invalid template syntax"));
                }
            }
        }

        private void Parse()
        {
            bool isBlockOpen = false;

            TokenInfo tokenInfo, lastTokenInfo = new TokenInfo(Token.Eof, null);

            while ((tokenInfo = NextToken()).token != Token.Eof)
            {
                switch (tokenInfo.token)
                {
                    case Token.LAssignment:
                        if (isBlockOpen)
                        {
                            body2.Append("\"+");
                            ParseAssignment();
                            body2.Append("+\"");
                        }
                        else
                        {
                            body2.Append("Write(");
                            ParseAssignment();
                            body2.Append(");\r\n");
                        }
                        break;
                    case Token.LTag:
                        if (isBlockOpen)
                        {
                            int i;
                            for (i = body2.Length - 1; i >= 0; i--)
                            {
                                if ((body2[i] != ' ') && (body2[i] != '\t'))
                                    break;
                            }
                            if ((body2[i] == '"') && (body2[i - 1] == '('))
                            {
                                body2.Remove(i - 7, body2.Length - i + 7);
                            }
                            else
                            {
                                body2.Append("\");\r\n");
                            }
                            isBlockOpen = false;
                        }
                        ParseTagBlock();
                        break;
                    case Token.String:
                        if (!isBlockOpen)
                        {
                            body2.Append("Write(\"");
                        }
                        isBlockOpen = true;
                        body2.Append(tokenInfo.value);
                        break;
                    case Token.QuotationMark:
                        if (!isBlockOpen)
                        {
                            body2.Append("Write(\"");
                        }
                        isBlockOpen = true;
                        body2.Append("\\\"");
                        break;
                    case Token.Backslash:
                        body2.Append("\\\\");
                        break;
                    case Token.Newline:
                        if (isBlockOpen)
                        {
                            body2.Append("\"); Write(newline);\r\n");
                        }
                        else
                        {
                            if ((lastTokenInfo.token == Token.Newline) || (lastTokenInfo.token == Token.String))
                                body2.Append("Write(newline);\r\n");
                        }
                        isBlockOpen = false;
                        break;
                    default:
                        break;
                }
                lastTokenInfo = tokenInfo;
            }
        }

        private string GetLine(StringBuilder text, int line)
        {
            StringReader reader = new StringReader(text.ToString());
            while (--line > 0)
            {
                reader.ReadLine();
            }
            return (reader.ReadLine());
        }

        private void Read(StreamReader stream)
        {
            // process <* section *> directives

            string s;

            while ((s = stream.ReadLine()) != null)
            {
                if (s.Length > 4 && s.StartsWith("<*") && s.EndsWith("*>"))
                {
                    string section = s.Substring(2, s.Length - 4).Trim();

                    StringBuilder value = new StringBuilder();

                    while (true)
                    {
                        s = stream.ReadLine();

                        if (s == null)
                        {
                            throw new ApplicationException("End of file reached within <* " + section + " *> section");
                        }

                        if (s.Length > 4 && s.StartsWith("<*") && s.EndsWith("*>"))
                        {
                            if (s.Substring(2, s.Length - 4).Trim() != section)
                            {
                                throw new ApplicationException("Mismatched end of <* " + section + " *> section");
                            }
                            if (sections.ContainsKey(section))
                            {
                                sections[section].Append(value);
                            }
                            else
                            {
                                sections.Add(section, value);
                            }
                            break;
                        }
                        value.Append(s);
                        value.Append(newline);
                    }
                }
            }
        }

        private void Process()
        {
            body1.Append(sections["body"].ToString());

            Parse();

            sections["body"] = body2;

            string master = "using System;" + newline +
                            "using System.Text;" + newline +
                            "using System.IO;" + newline +
                            "using System.Xml;" + newline +
                            "using System.Xml.XPath;" + newline +
                            "using System.Collections.Generic;" + newline +
                            "<* import *>" + newline +
                            "namespace Kodegen {" + newline +
                            "    public class Generator : GeneratorBase {" + newline +
                            "        public Generator(string[] args) : base(args)" + newline +
                            "        {" + newline +
                            "        }" + newline +
                            "<* function *>" + newline +
                            "        public string Generate()" + newline +
                            "        {" + newline +
                            "<* body *>" + newline +
                            "            return (Result());" + newline +
                            "        }" + newline +
                            "    }" + newline +
                            "}" + newline;

            StringReader stream = new StringReader(master);

            string s;

            while ((s = stream.ReadLine()) != null)
            {
                if (s.Length > 4 && s.StartsWith("<*") && s.EndsWith("*>"))
                {
                    string section = s.Substring(2, s.Length - 4).Trim();
                    if (sections.ContainsKey(section))
                    {
                        source.Append(sections[section]);
                    }
                }
                else
                {
                    source.Append(s);
                    source.Append(newline);
                }
            }
            #if DEBUG
            //StreamWriter debugFile = new StreamWriter("Generator.cs");
            //debugFile.Write(source.ToString());
            //debugFile.Close();
            #endif

            // compile assembly in memory with a yet unknown name

            CSharpCodeProvider codeProvider = new CSharpCodeProvider();

            System.CodeDom.Compiler.CompilerParameters parameters = new CompilerParameters();

            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = true;
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.dll");
            parameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);


            if (sections.ContainsKey("reference"))
            {
                StringReader references = new StringReader(sections["reference"].ToString());
                String reference;
                while ((reference = references.ReadLine()) != null)
                {
                    parameters.ReferencedAssemblies.Add(reference);
                }
            }

            // ensure the template is able to reference assemblies
            // installed into the same directory as Kodegen.

            string dircurrent = System.Environment.CurrentDirectory;
            string dirkodegen = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            System.Environment.CurrentDirectory = dirkodegen;

            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, source.ToString());

            System.Environment.CurrentDirectory = dircurrent;

            if (results.Errors.Count > 0)
            {
                string error = results.Errors[0].ErrorText + " in '";
                error += GetLine(source, results.Errors[0].Line) + "'";
                throw new TemplateException(error);
            }
            else
            {
                assembly = results.CompiledAssembly;
            }
        }

        /// <summary>
        /// Generate
        /// </summary>
        /// <param name="args">Arguments to the template generator</param>
        public string Generate(string[] args)
        {
            try
            {
                object obj = assembly.CreateInstance("Kodegen.Generator", false, System.Reflection.BindingFlags.CreateInstance, null, new object[] { args }, null, null);
                return (string)(obj.GetType().InvokeMember("Generate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, null, obj, null));
            }
            catch (Exception e)
            {
                if (e is TargetInvocationException)
                {
                    throw e.InnerException;
                }
                throw e;
            }
        }
    }
}
