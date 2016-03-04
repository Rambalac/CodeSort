using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace CodeSort
{
    public class ClassMemberSorter
    {
        public void SortAllFiles(string folder)
        {
            foreach (var path in Directory.GetFiles(folder, "*.cs"))
            {
                if (File.Exists(path)) SortFile(path);
            }
        }

        public void SortFile(string path)
        {
            var code = File.ReadAllText(path);
            var options = new CSharpParseOptions(documentationMode: DocumentationMode.Parse);
            var tree = CSharpSyntaxTree.ParseText(code, options: options);
            var root = tree.GetRoot();

            var newRoot = root.ReplaceNodes(root.DescendantNodes(s => !s.IsKind(SyntaxKind.ClassDeclaration)).OfType<ClassDeclarationSyntax>(), computeNewClass);
            newRoot = newRoot.ReplaceNodes(root.DescendantNodes(s => !s.IsKind(SyntaxKind.InterfaceDeclaration)).OfType<InterfaceDeclarationSyntax>(), computeNewInterface);
            var newPath = path + ".bak";
            if (File.Exists(newPath))
            {
                var index = 1;
                while (File.Exists(newPath + index)) index++;
                newPath += index;
            }

            File.Move(path, newPath);

            File.WriteAllText(path, newRoot.ToFullString());
        }

        private SyntaxNode computeNewClass(ClassDeclarationSyntax cl, ClassDeclarationSyntax newcl)
        {
            var ordered = cl.Members.OrderBy(n => GetOrder(n));
            newcl = newcl.RemoveNodes(newcl.Members, SyntaxRemoveOptions.KeepNoTrivia);
            newcl = newcl.AddMembers(ordered.Cast<MemberDeclarationSyntax>().ToArray());
            return newcl;
        }

        private SyntaxNode computeNewInterface(InterfaceDeclarationSyntax cl, InterfaceDeclarationSyntax newcl)
        {
            var ordered = cl.Members.OrderBy(n => GetOrder(n));
            newcl = newcl.RemoveNodes(newcl.Members, SyntaxRemoveOptions.KeepNoTrivia);
            newcl = newcl.AddMembers(ordered.Cast<MemberDeclarationSyntax>().ToArray());
            return newcl;
        }

        private bool NeededKind(SyntaxNode s)
        {
            return s.IsKind(SyntaxKind.MethodDeclaration) ||
                    s.IsKind(SyntaxKind.PropertyDeclaration) ||
                    s.IsKind(SyntaxKind.ConstructorDeclaration) ||
                    s.IsKind(SyntaxKind.FieldDeclaration);
        }

        private string GetOrder(MemberDeclarationSyntax n)
        {
            var field = n as FieldDeclarationSyntax;
            var ident = "";
            var mods = default(SyntaxTokenList);
            int tp = 0;
            if (field != null)
            {
                mods = field.Modifiers;
                ident = field.Declaration.Variables.First().Identifier.Text;
                tp = 0;
            }

            var contr = n as ConstructorDeclarationSyntax;
            if (contr != null)
            {
                mods = contr.Modifiers;
                tp = 1;
            }

            var prop = n as PropertyDeclarationSyntax;
            if (prop != null)
            {
                mods = prop.Modifiers;
                ident = prop.Identifier.Text;
                tp = 2;
            }

            int access = 2;

            var meth = n as MethodDeclarationSyntax;
            if (meth != null)
            {
                mods = meth.Modifiers;
                ident = meth.Identifier.Text;
                tp = 3;
                if (meth.DescendantNodes().Any(f => f.IsKind(SyntaxKind.ExplicitInterfaceSpecifier))) access = 0;
            }

            if (mods.Any(f => f.IsKind(SyntaxKind.PublicKeyword))) access = 0;
            if (mods.Any(f => f.IsKind(SyntaxKind.InternalKeyword))) access = 1;
            if (mods.Any(f => f.IsKind(SyntaxKind.PrivateKeyword))) access = 2;

            int visib = 2;

            if (mods.Any(f => f.IsKind(SyntaxKind.ConstKeyword))) visib = 0;
            if (mods.Any(f => f.IsKind(SyntaxKind.StaticKeyword))) visib = 1;

            var rdo = 1;
            if (mods.Any(f => f.IsKind(SyntaxKind.ReadOnlyKeyword))) rdo = 0;


            return "" + tp + access + visib + rdo + ident;
        }
    }
}
