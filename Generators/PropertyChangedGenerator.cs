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
        private Dictionary<string, List<GenerationInfos.PartialClass.Property.Validation>> GetValidValidationCalls(GeneratorExecutionContext context)
        {
            var compilation = context.Compilation;
            var returnList = new Dictionary<string, List<GenerationInfos.PartialClass.Property.Validation>>();

            foreach (var invocation in _invocations)
            {
                var invocationModel = compilation.GetSemanticModel(invocation.SyntaxTree);
                var containingMethod = invocationModel.GetEnclosingSymbol(invocation.Parent.GetLocation().SourceSpan.Start);
                var containingClass = containingMethod.ContainingType.ToString();

                var operation = invocationModel.GetOperation(invocation) as IInvocationOperation;
                var operationName = operation.TargetMethod.Name;
                var operationClass = operation.TargetMethod.ContainingType.ToString();

                if (operationClass != "SaneWpf.Framework.Validations" || operationName != "Add")
                    continue;

                var children = operation.Children.Cast<IArgumentOperation>().ToList();
                var viewModelType = children[0].Parameter.ToString();
                if (viewModelType != containingClass)
                {
                    context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("SW002", "Instance must be of same type as ViewModel", "Instance must be of same type as ViewModel", "Error", DiagnosticSeverity.Error, true), invocation.GetLocation()));
                }

                var propertyExp = (SimpleLambdaExpressionSyntax)((ArgumentSyntax)children[1].Syntax).Expression;
                var memberAccess = (MemberAccessExpressionSyntax)propertyExp.Body;
                var fieldName = memberAccess.Name.ToString();

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

                var validationErrorExpression = (SimpleLambdaExpressionSyntax)((ArgumentSyntax)children[3].Syntax).Expression;
                var result = validationErrorExpression.Body.ToString();
                var resultParameter = validationErrorExpression.Parameter.ToString();
                var validation = new GenerationInfos.PartialClass.Property.Validation
                {
                    MethodBody = methodBody,
                    ParameterName = parameterName,
                    ValidationErrorBody = result,
                    ValidationErrorParameter = resultParameter,
                    ParameterType = validationPropertyType,
                    FieldName = fieldName
                };

                if (returnList.TryGetValue(containingClass, out var list))
                    list.Add(validation);
                else
                    returnList.Add(containingClass, new List<GenerationInfos.PartialClass.Property.Validation> { validation });
            }

            return returnList;
        }

        private Dictionary<string, List<GenerationInfos.PartialClass.Property>> GetValidProperties(
            GeneratorExecutionContext context,
            Dictionary<string, List<GenerationInfos.PartialClass.Property.Validation>> validations)
        {
            var result = new Dictionary<string, List<GenerationInfos.PartialClass.Property>>();
            var compilation = context.Compilation;

            foreach (var fieldDeclaration in _relevantFields)
            {
                var fieldModel = compilation.GetSemanticModel(fieldDeclaration.FieldSyntax.SyntaxTree);
                
                var classSymbol = fieldModel.GetEnclosingSymbol(fieldDeclaration.FieldSyntax.Parent.GetLocation().SourceSpan.Start);
                var className = classSymbol.ToString();

                foreach (var variable in fieldDeclaration.FieldSyntax.Declaration.Variables)
                {
                    var property = new GenerationInfos.PartialClass.Property();
                    if (result.TryGetValue(className, out var list))
                        list.Add(property);
                    else
                        result.Add(className, new List<GenerationInfos.PartialClass.Property> { property });

                    var fieldSymbol = (IFieldSymbol)fieldModel.GetDeclaredSymbol(variable);
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

                    if (validations.TryGetValue(property.ClassName, out var propertyValidations))
                    {
                        foreach (var validation in propertyValidations.Where(x => x.FieldName == property.FieldName))
                        {
                            property.Validations.Add(validation);
                        }
                    }

                    #endregion
                }
            }

            return result;
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var generationInfos = new GenerationInfos();
            var validationCalls = GetValidValidationCalls(context);
            var properties = GetValidProperties(context, validationCalls);

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

                var classToCreate = new GenerationInfos.PartialClass
                {
                    Namespace = ns,
                    Name = classSymbol.Name,
                };

                if (properties.TryGetValue(classSymbol.ToString(), out var v))
                    classToCreate.Properties.AddRange(v);

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
