using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SaneWpf.Attributes;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Generators
{
    [Generator]
    public class PropertyChangedGenerator : ISourceGenerator, ISyntaxReceiver
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var generationInfos = new GenerationInfos();

            foreach (var classDeclaration in _relevantClasses)
            {
                var classModel = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                var classSymbol = classModel.GetDeclaredSymbol(classDeclaration);

                if (!classSymbol.GetAttributes().Any(x => x.AttributeClass.Name == nameof(ViewModelAttribute)))
                    continue;

                if (!classDeclaration.Modifiers.Any(modifierList => modifierList.ToString() == "partial"))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "SW001",
                            "Class is not partial", "The following class is not partial: {0}", "Error",
                            DiagnosticSeverity.Error, true),
                        classDeclaration.GetLocation(), classSymbol.Name));

                    continue;
                }
                var ns = classSymbol.ContainingNamespace.ToString();
                
                var classToCreate = CreatePartialClass(context.Compilation, ns, classSymbol.Name);
                generationInfos.ClassesToCreate.Add(classToCreate);
            }

            context.AddSource("generated.cs", generationInfos.ToString());
        }

        private static string GetPropertyName(string fieldName)
        {
            var offset = fieldName[0] == '_' ? 1 : 0;

            var newStr = new char[fieldName.Length - offset];

            newStr[0] = char.ToUpper(fieldName[offset]);
            for (int i = 1; i < newStr.Length; i++)
            {
                newStr[i] = fieldName[i + offset];
            }

            return new string(newStr);
        }

        private GenerationInfos.PartialClass CreatePartialClass(Compilation compilation, string ns, string className)
        {
            var partialClass = new GenerationInfos.PartialClass();
            partialClass.Namespace = ns;
            partialClass.Name = className;

            foreach (var fieldDeclaration in _relevantFields)
            {
                var model = compilation.GetSemanticModel(fieldDeclaration.Item1.SyntaxTree);
                var fieldClass = model.GetDeclaredSymbol(fieldDeclaration.Item1);
                if (fieldClass.ContainingNamespace.ToString() != ns || fieldClass.Name.ToString() != className)
                    continue;

                foreach (var variable in fieldDeclaration.Item2.Declaration.Variables)
                {
                    var property = new GenerationInfos.PartialClass.Property();
                    partialClass.Properties.Add(property);

                    var fieldSymbol = (IFieldSymbol)model.GetDeclaredSymbol(variable);
                    property.FieldName = fieldSymbol.Name;
                    property.Name = GetPropertyName(fieldSymbol.Name);
                    property.Type = fieldSymbol.Type.ToString();
                    property.TypeNamespace = fieldSymbol.Type.ContainingNamespace.Name;
                    property.PropertiesToNotify.Add(property.Name);

                    #region Properties to notify
                    foreach (var attribute in fieldSymbol.GetAttributes().Where(x => x.AttributeClass.ToString() == typeof(AutoNotifyAttribute).ToString()))
                    {
                        foreach (var constructorArgument in attribute.ConstructorArguments)
                        {
                            foreach (var v in constructorArgument.Values)
                            {
                                if (v.Value == null)
                                {
                                    // TODO Error, hier kann nur mit konstanten gearbeitet werden!
                                }
                                property.PropertiesToNotify.Add(GetPropertyName(v.Value.ToString()));
                            }
                        }
                    }
                    #endregion

                    #region other attributes

                    foreach (var attribute in fieldSymbol.GetAttributes().Where(x => x.AttributeClass.ToString() != typeof(AutoNotifyAttribute).ToString()))
                    {
                        var syntaxReference = attribute.ApplicationSyntaxReference;
                        var attributeSyntax = $"[{syntaxReference.GetSyntax().ToFullString()}]";
                        property.AttributeNamespaces.Add(attribute.AttributeClass.ContainingNamespace.ToString());
                        property.Attributes.Add(attributeSyntax);
                    }

                    #endregion
                }
            }

            return partialClass;
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //Debugger.Launch();
            context.RegisterForSyntaxNotifications(new SyntaxReceiverCreator(() => this));
        }

        private readonly List<ClassDeclarationSyntax> _relevantClasses = new List<ClassDeclarationSyntax>();
        private readonly List<(ClassDeclarationSyntax, FieldDeclarationSyntax)> _relevantFields = new List<(ClassDeclarationSyntax, FieldDeclarationSyntax)>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classSyntax)
            {
                if (classSyntax.AttributeLists
                    .SelectMany(x => x.ChildNodes())
                    .Cast<AttributeSyntax>()
                    .Any(attr => attr.Name.ToString() == "ViewModel" || attr.Name.ToString() == "SaneWpf.ViewModel"))
                {
                    _relevantClasses.Add(classSyntax);
                }
            } 
            else if (syntaxNode is FieldDeclarationSyntax fieldSyntax)
            {
                if (fieldSyntax.AttributeLists
                    .SelectMany(x => x.ChildNodes())
                    .Cast<AttributeSyntax>()
                    .Any(attr => attr.Name.ToString() == "AutoNotify" || attr.Name.ToString() == "SaneWpf.AutoNotify"))
                {
                    var parent = (ClassDeclarationSyntax) fieldSyntax.Parent;
                    _relevantFields.Add((parent, fieldSyntax));
                }
            }
        }
    }
}
