using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace ReflectionExtensions
{
    public static class TypeMixer
    {
        public const BindingFlags VisibilityFlags = BindingFlags.Public | BindingFlags.Instance;
        public const MethodAttributes DefaultPropertyAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual;

        #region Private
        private static readonly Lazy<ModuleBuilder> _module = new Lazy<ModuleBuilder>(CreateModule);
        private static readonly ConcurrentDictionary<string, Type> _types = new ConcurrentDictionary<string, Type>();

        private static ModuleBuilder CreateModule()
        {
            var assemblyName = new Guid().ToString();

            var assembly = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName(assemblyName),
                AssemblyBuilderAccess.Run);
            return assembly.DefineDynamicModule("Module");
        }

        private static T CreateExtendedObject<T>(object source) where T : class
        {
            var parentType = source.GetType();
            var newTypeName = parentType.Name + "_" + typeof(T).Name;
            var newType = GetOrCreateType<T>(_module.Value, parentType, newTypeName);
            return (T)Activator.CreateInstance(newType);
        }

        private static Type GetOrCreateType<T>(ModuleBuilder module, Type parentType, string newTypeName) where T : class
            => _types.GetOrAdd(newTypeName,
                (_newTypeName) => CreateType<T>(module, parentType, _newTypeName));

        private static Type CreateType<T>(ModuleBuilder module, Type parentType, string newTypeName) where T : class
        {
            var type = module.DefineType(newTypeName, TypeAttributes.Public, parentType, new[] { typeof(T) });

            foreach (var property in typeof(T).GetProperties())
            {
                CreateDefaultGetSetProperty(type, property);
            }
            return type.CreateTypeInfo();
        }

        private static void CreateDefaultGetSetProperty(TypeBuilder type, PropertyInfo v)
        {
            var field = type.DefineField("_" + v.Name.ToLower(), v.PropertyType, FieldAttributes.Private);
            var property = type.DefineProperty(v.Name, PropertyAttributes.None, v.PropertyType, new Type[0]);
            var getter = DefineDefaultGetter(type, v, field);
            var setter = DefineDefaultSetter(type, v, field);

            property.SetGetMethod(getter);
            property.SetSetMethod(setter);

            type.DefineMethodOverride(getter, v.GetGetMethod());
            type.DefineMethodOverride(setter, v.GetSetMethod());
        }

        private static MethodBuilder DefineDefaultSetter(TypeBuilder type, PropertyInfo v, FieldBuilder field)
        {
            var setter = type.DefineMethod("set_" + v.Name, DefaultPropertyAttributes, null, new Type[] { v.PropertyType });
            var setGenerator = setter.GetILGenerator();
            setGenerator.Emit(OpCodes.Ldarg_0);
            setGenerator.Emit(OpCodes.Ldarg_1);
            setGenerator.Emit(OpCodes.Stfld, field);
            setGenerator.Emit(OpCodes.Ret);
            return setter;
        }

        private static MethodBuilder DefineDefaultGetter(TypeBuilder type, PropertyInfo v, FieldBuilder field)
        {
            var getter = type.DefineMethod("get_" + v.Name, DefaultPropertyAttributes, v.PropertyType, new Type[0]);
            var getGenerator = getter.GetILGenerator();
            getGenerator.Emit(OpCodes.Ldarg_0);
            getGenerator.Emit(OpCodes.Ldfld, field);
            getGenerator.Emit(OpCodes.Ret);
            return getter;
        }

        private static K CopyValuesTo<T, K>(this T source, K destination)
        {
            var properties = source.GetType().GetProperties(VisibilityFlags);
            foreach (var property in properties)
            {
                var prop = destination.GetType().GetProperty(property.Name, VisibilityFlags);
                if (prop?.CanWrite == true)
                    prop.SetValue(destination, property.GetValue(source), null);
            }

            return destination;
        }
        #endregion Private

        public static T ExtendWith<T>(this object source) where T : class
        {
            var newObject = CreateExtendedObject<T>(source);
            return source?.CopyValuesTo(newObject)
                ?? newObject;
        }
    }
}