using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
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
                var model = compilation.GetSemanticModel(fieldDeclaration.ClassSyntax.SyntaxTree);
                var fieldClass = model.GetDeclaredSymbol(fieldDeclaration.ClassSyntax);
                if (fieldClass.ContainingNamespace.ToString() != ns || fieldClass.Name.ToString() != className)
                    continue;

                foreach (var variable in fieldDeclaration.FieldSyntax.Declaration.Variables)
                {
                    var property = new GenerationInfos.PartialClass.Property();
                    partialClass.Properties.Add(property);

                    var fieldSymbol = (IFieldSymbol)model.GetDeclaredSymbol(variable);
                    property.FieldName = fieldSymbol.Name;
                    property.ClassName = className;
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

                    #region property validations

                    foreach (var invocation in _invocations)
                    {
                        var invocationModel = compilation.GetSemanticModel(invocation.SyntaxTree);
                        var operation = invocationModel.GetOperation(invocation) as IInvocationOperation;
                        var operationName = operation.TargetMethod.Name;
                        var operationClass = operation.TargetMethod.ContainingType.ToString();

                        if (operationClass != "SaneWpf.Framework.Validations" || operationName != "Add")
                            continue;

                        var children = operation.Children.Cast<IArgumentOperation>().ToList();
                        var viewModelType = children[0].Parameter.ToString();
                        if (viewModelType != ns + "." + className)
                            continue;

                        var propertyExp = (SimpleLambdaExpressionSyntax)((ArgumentSyntax)children[1].Syntax).Expression;
                        var memberAccess = (MemberAccessExpressionSyntax) propertyExp.Body;
                        var fieldName = memberAccess.Name.ToString();
                        if (fieldName != property.FieldName)
                            continue;

                        var validationPropertyType = operation.TargetMethod.TypeArguments[1].ToString();

                        var func = ((ArgumentSyntax)children[2].Syntax).Expression;
                        string parameterName;
                        string methodBody;
                        if (func is IdentifierNameSyntax id)
                        {
                            methodBody = id.ToString() + "(param_0);";
                            parameterName = "param_0";
                        }
                        else
                        {
                            var lambdaExpression = (SimpleLambdaExpressionSyntax)func;
                            methodBody = lambdaExpression.Body.ToString();
                            parameterName = lambdaExpression.Parameter.ToString();
                        }

                        var validationErrorExpression = (SimpleLambdaExpressionSyntax) ((ArgumentSyntax)children[3].Syntax).Expression;
                        var result = validationErrorExpression.Body.ToString();
                        var resultParameter = validationErrorExpression.Parameter.ToString();
                        var validation = new GenerationInfos.PartialClass.Property.Validation
                        {
                            MethodBody = methodBody,
                            ParameterName = parameterName,
                            ValidationErrorBody = result,
                            ValidationErrorParameter = resultParameter,
                            ParameterType = validationPropertyType
                        };
                        property.Validations.Add(validation);
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
        private readonly List<(ClassDeclarationSyntax ClassSyntax, FieldDeclarationSyntax FieldSyntax)> _relevantFields = new List<(ClassDeclarationSyntax, FieldDeclarationSyntax)>();
        private readonly List<InvocationExpressionSyntax> _invocations = new List<InvocationExpressionSyntax>();

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
            else if (syntaxNode is InvocationExpressionSyntax invocation)
            {
                if (invocation.ChildNodes().Any(x => x is MemberAccessExpressionSyntax exp && x.ToString() == "Validations.Add"))
                {
                    _invocations.Add(invocation);
                }
            }
        }
    }
}
