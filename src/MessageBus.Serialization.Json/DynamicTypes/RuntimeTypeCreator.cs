using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json.Serialization;

namespace MessageBus.Serialization.Json.DynamicTypes
{
    internal class RuntimeTypeCreator : IIntermediateInterfaceImplementer
    {
        private const string _moduleName = "_dynamicTypesModule";

        private readonly ConstructorInfo _jsonIncludeAttributeConstructor = typeof(JsonIncludeAttribute).GetConstructor(Array.Empty<Type>())!;
        private readonly ConstructorInfo _jsonIgnoreAttributeConstructor = typeof(JsonIgnoreAttribute).GetConstructor(Array.Empty<Type>())!;
        private readonly ConstructorInfo _argumentNullExceptionContructor = typeof(ArgumentNullException).GetConstructor(new Type[] { typeof(string) })!;
        private readonly ConstructorInfo _invalidOperationExceptionConstructor = typeof(InvalidOperationException).GetConstructor(new Type[] { typeof(string) })!;
        private readonly NullabilityInfoContext _nullabilityContext = new NullabilityInfoContext();
        private readonly ModuleBuilder _dynamicTypesModule;
        private readonly TypeCreationOptions _options;

        public RuntimeTypeCreator(TypeCreationOptions options)
        {
            AssemblyName name = new AssemblyName($"{typeof(RuntimeTypeCreator).Namespace ?? ""}_DynamicAssembly_{Guid.NewGuid():N}");
            AssemblyBuilder builder = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            _dynamicTypesModule = builder.DefineDynamicModule(_moduleName);
            _options = options;
        }

        public Type CreateType(Type interfaceType)
        {
            if (!interfaceType.IsInterface) throw new ArgumentException($"The type {interfaceType} is not an interface");
            return CreateInterfaceImplementation(interfaceType);
        }

        private Type CreateInterfaceImplementation(Type interfaceType)
        {
            string generatedTypeName = GenerateGeneratedTypeName(interfaceType);

            TypeBuilder builder = _dynamicTypesModule.DefineType(generatedTypeName, TypeAttributes.Public | TypeAttributes.Sealed);            
            foreach (var interfaceToImplement in CollectInterfacesToImplement(interfaceType))
            {
                ImplementInterface(builder, interfaceToImplement);
            }
            return builder.CreateType()!;
        }

        private IReadOnlySet<Type> CollectInterfacesToImplement(Type interfaceType)
        {
            HashSet<Type> result = new HashSet<Type>();
            AnalyzeInterfaceInheritance(interfaceType, result);
            return result;
        }

        private void AnalyzeInterfaceInheritance(Type interfaceType, HashSet<Type> collector)
        {
            if (!collector.Add(interfaceType))
                return;

            foreach (var subInterface in interfaceType.GetInterfaces())
                AnalyzeInterfaceInheritance(subInterface, collector);
        }

        private void ImplementInterface(TypeBuilder builder, Type interfaceType)
        {
            builder.AddInterfaceImplementation(interfaceType);
            GeneratePropertiesToImplement(interfaceType, builder);
        }

        private void GeneratePropertiesToImplement(Type interfaceType, TypeBuilder targetType)
        {
            PropertyInfo[] propertiesToImplement = interfaceType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in propertiesToImplement)
                ImplementProperty(targetType, prop);
        }

        private void ImplementProperty(TypeBuilder targetType, PropertyInfo toImplement)
        {
            PropertyBuilder propertyBuilder = targetType.DefineProperty(toImplement.Name, PropertyAttributes.HasDefault, toImplement.PropertyType, Type.EmptyTypes);
            ImplementPropertyBody(targetType, toImplement, propertyBuilder);
        }

        private void ImplementPropertyBody(TypeBuilder targetType, PropertyInfo toImplement, PropertyBuilder propertyBuilder)
        {
            // interfaces can have default implementations for properties. In that case, the getter method
            // for that property will not be abstract. 
            if (toImplement.GetMethod is not null && !toImplement.GetMethod.IsAbstract)
            {
                AddJsonIgnoreProperty(propertyBuilder);
                return;
            }

            ImplementGetAndSetPropertyBody(targetType, toImplement, propertyBuilder);
            AddJsonIncludeProperty(propertyBuilder);
        }

        private void ImplementGetAndSetPropertyBody(TypeBuilder targetType, PropertyInfo toImplement, PropertyBuilder propertyBuilder)
        {
            string fieldName = $"_`{toImplement.Name}`__storage";
            FieldBuilder fieldBuilder = targetType.DefineField(fieldName, toImplement.PropertyType, FieldAttributes.Private);

            MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual;
            MethodBuilder getterBuilder = CreateGetterMethod(targetType, toImplement, fieldBuilder, methodAttributes);
            MethodBuilder setterBuilder = CreateSetterMethod(targetType, toImplement, fieldBuilder, methodAttributes);

            propertyBuilder.SetGetMethod(getterBuilder);
            propertyBuilder.SetSetMethod(setterBuilder);
        }

        private void AddJsonIgnoreProperty(PropertyBuilder propertyBuilder)
        {
            CustomAttributeBuilder builder = new CustomAttributeBuilder(_jsonIgnoreAttributeConstructor, Array.Empty<object>());
            propertyBuilder.SetCustomAttribute(builder);
        }


        private void AddJsonIncludeProperty(PropertyBuilder propertyBuilder)
        {
            CustomAttributeBuilder builder = new CustomAttributeBuilder(_jsonIncludeAttributeConstructor, Array.Empty<object>());
            propertyBuilder.SetCustomAttribute(builder);
        }

        private static string GenerateGeneratedTypeName(Type interfaceType)
        {
            return $"Impl_{interfaceType.Name}_{Guid.NewGuid():N}";
        }

        private MethodBuilder CreateGetterMethod(TypeBuilder typeBuilder, PropertyInfo property, FieldBuilder sourceField, MethodAttributes attributes)
        {
            MethodBuilder result = typeBuilder.DefineMethod($"get_{property.Name}", attributes, property.PropertyType, Type.EmptyTypes);
            ILGenerator generator = result.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, sourceField);

            if (_options.ThrowExceptionGettingUnsetNotNullProperty && IsNotReadNullableProperty(property))
                GenerateFieldNotSetGet(generator, property);
            generator.Emit(OpCodes.Ret);

            return result;
        }

        private MethodBuilder CreateSetterMethod(TypeBuilder typeBuilder, PropertyInfo property, FieldBuilder targetField, MethodAttributes attributes)
        {
            MethodBuilder result = typeBuilder.DefineMethod($"set_{property.Name}", attributes, null, new Type[] { property.PropertyType });
            ILGenerator generator = result.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);

            if (_options.ThrowExceptionSettingNullToNotNullProperty && IsNotWriteNullableProperty(property))
                GenerateNoNullWriteGate(generator, property);

            generator.Emit(OpCodes.Stfld, targetField);
            generator.Emit(OpCodes.Ret);

            return result;
        }

        private void GenerateFieldNotSetGet(ILGenerator generator, PropertyInfo property)
        {
            generator.Emit(OpCodes.Dup);

            Label isFieldNull = generator.DefineLabel();
            generator.Emit(OpCodes.Brtrue_S, isFieldNull);
            generator.Emit(OpCodes.Pop);
            generator.Emit(OpCodes.Ldstr, $"{property.Name} isn't set but its defined as non-nullable.");

            generator.Emit(OpCodes.Newobj, _invalidOperationExceptionConstructor);
            generator.Emit(OpCodes.Throw);

            generator.MarkLabel(isFieldNull);
        }

        private void GenerateNoNullWriteGate(ILGenerator generator, PropertyInfo property)
        {
            Label isValueNull = generator.DefineLabel();

            generator.Emit(OpCodes.Dup);
            generator.Emit(OpCodes.Brtrue_S, isValueNull);
            generator.Emit(OpCodes.Pop);
            generator.Emit(OpCodes.Ldstr, property.Name);
            generator.Emit(OpCodes.Newobj, _argumentNullExceptionContructor);
            generator.Emit(OpCodes.Throw);

            generator.MarkLabel(isValueNull);
        }

        private bool IsNotReadNullableProperty(PropertyInfo property)
        {
            if (!property.PropertyType.IsClass)
                return false;

            NullabilityInfo nullabilityInfo = _nullabilityContext.Create(property);
            return nullabilityInfo.ReadState is not NullabilityState.Nullable;
        }

        private bool IsNotWriteNullableProperty(PropertyInfo property)
        {
            if (!property.PropertyType.IsClass)
                return false;

            NullabilityInfo nullabilityInfo = _nullabilityContext.Create(property);

            // the interface might be read only, so we check that as well
            if (nullabilityInfo.WriteState is NullabilityState.Unknown)
                return nullabilityInfo.ReadState is not NullabilityState.Nullable;

            return nullabilityInfo.WriteState is not NullabilityState.Nullable;
        }
    }
}
