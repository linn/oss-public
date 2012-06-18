//
// cf-cecil-patcher.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// (C) 2005 Jb Evain
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;

using Mono.Cecil;

class CompactFrameworkPatcher : BaseStructureVisitor {

    static readonly byte [] fwPkToken1 = new byte [] {
        0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89
    };

    static readonly byte [] fwPkToken2 = new byte [] {
        0xb0, 0x3f, 0x5f, 0x7f, 0x11, 0xd5, 0x0a, 0x3a
    };

    static readonly byte [] cfPkToken = new byte [] {
        0x96, 0x9d, 0xb8, 0x05, 0x3d, 0x33, 0x22, 0xac
    };

    static IDictionary mappableAssemblies = new Hashtable ();

    static CompactFrameworkPatcher ()
    {
        mappableAssemblies.Add ("mscorlib", fwPkToken1);
        mappableAssemblies.Add ("System", fwPkToken1);
        mappableAssemblies.Add ("System.Data", fwPkToken1);
        mappableAssemblies.Add ("System.Drawing", fwPkToken2);
        mappableAssemblies.Add ("System.Web.Services", fwPkToken2);
        mappableAssemblies.Add ("System.Windows.Forms", fwPkToken1);
        mappableAssemblies.Add ("System.Xml", fwPkToken1);
        mappableAssemblies.Add ("Microsoft.VisualBasic", fwPkToken2);
    }

    bool CheckPublicKeyToken (AssemblyNameReference asm)
    {
        if (asm.PublicKeyToken == null || asm.PublicKeyToken.Length != 8)
            return false;

        byte [] corresponding = mappableAssemblies [asm.Name] as byte [];

        for (int i = 0; i < 8; i++)
            if (asm.PublicKeyToken [i] != corresponding [i])
                return false;

        return true;
    }

    public override void VisitAssemblyNameReferenceCollection (AssemblyNameReferenceCollection assemblies)
    {
        VisitCollection (assemblies);
    }

    public override void VisitAssemblyNameReference (AssemblyNameReference asm)
    {
        if (!mappableAssemblies.Contains (asm.Name))
            return;

        if (!CheckPublicKeyToken (asm))
            return;

        asm.Flags |= AssemblyFlags.Retargetable;
        asm.PublicKeyToken = cfPkToken;
    }

    static void Main (string [] args)
    {
        if (args.Length != 1) {
            Usage ();
            return;
        }

        string file = args [0];
        AssemblyDefinition asm = AssemblyFactory.GetAssembly (file);
        asm.MainModule.AssemblyReferences.Accept (new CompactFrameworkPatcher ());
        AssemblyFactory.SaveAssembly (asm, file);

        Console.WriteLine ("Assembly {0} patched", file);
    }

    static void Usage ()
    {
        Console.WriteLine ("Mono Assembly To Compact Framework Patcher");
        Console.WriteLine ("usage: cf-patcher.exe assembly");
    }
}
