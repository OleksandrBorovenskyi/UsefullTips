/// <summary>
	/// Creates type to represent objects gotten from DB.
	/// <see cref="https://stackoverflow.com/questions/3862226/how-to-dynamically-create-a-class"/>
	/// </summary>
	static class CustomTypeBuilder
	{
		//public static void CreateNewInstance()
		//{
		//	Type type = CompileResultType();
		//	var myObject = Activator.CreateInstance(type);
		//}

		public static Type CompileResultType(string typeName, Dictionary<string, Type> propertyInfos)
		{
			TypeBuilder typeBuilder = GetTypeBuilder(typeName);
			ConstructorBuilder constructor = typeBuilder.DefineDefaultConstructor(
				MethodAttributes.Public |
				MethodAttributes.SpecialName |
				MethodAttributes.RTSpecialName);

			foreach (var prop in propertyInfos)
			{
				CreateProperty(typeBuilder, prop.Key, prop.Value);
			}

			Type objectType = typeBuilder.CreateType();
			return objectType;
		}

		private class PropInfo
		{
			public string PropName { get; set; }
			public Type PropType { get; set; }

			private static byte _counter = 0;

			public static IEnumerable<PropInfo> Gen()
			{
				for (int i = 0; i < 3; i++)
				{
					yield return new PropInfo
					{
						PropName = "Prop" + _counter++,
						PropType = typeof(double)
					};
				}
			}
		}

		private static TypeBuilder GetTypeBuilder(string typeName)
		{
			string typeSignature = typeName;
			var assmeblyName = new AssemblyName(typeSignature);
			AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assmeblyName, AssemblyBuilderAccess.Run);
			ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
			TypeBuilder typeBuilder = moduleBuilder.DefineType(typeSignature,
				TypeAttributes.Public |
				TypeAttributes.Class |
				TypeAttributes.AutoClass |
				TypeAttributes.AnsiClass |
				TypeAttributes.BeforeFieldInit |
				TypeAttributes.AutoLayout,
				null);

			return typeBuilder;
		}

		private static void CreateProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
		{
			FieldBuilder fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

			PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

			MethodBuilder getterBuilder = typeBuilder.DefineMethod("get_" + propertyName,
				MethodAttributes.Public |
				MethodAttributes.HideBySig |
				MethodAttributes.SpecialName,
				propertyType, Type.EmptyTypes);
			ILGenerator getterIlGenerator = getterBuilder.GetILGenerator();

			getterIlGenerator.Emit(OpCodes.Ldarg_0);
			getterIlGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
			getterIlGenerator.Emit(OpCodes.Ret);

			MethodBuilder setterBuilder = typeBuilder.DefineMethod("set_" + propertyName,
				// todo: Check whether it`s gone well when private (specified as Public in the source)
				MethodAttributes.Public |
				MethodAttributes.SpecialName |
				MethodAttributes.HideBySig,
				null, new[] { propertyType });

			ILGenerator setterIlGenerator = setterBuilder.GetILGenerator();
			Label modifyProperty = setterIlGenerator.DefineLabel();
			Label exitSet = setterIlGenerator.DefineLabel();

			setterIlGenerator.MarkLabel(modifyProperty);
			setterIlGenerator.Emit(OpCodes.Ldarg_0);
			setterIlGenerator.Emit(OpCodes.Ldarg_1);
			setterIlGenerator.Emit(OpCodes.Stfld, fieldBuilder);

			setterIlGenerator.Emit(OpCodes.Nop);
			setterIlGenerator.MarkLabel(exitSet);
			setterIlGenerator.Emit(OpCodes.Ret);

			propertyBuilder.SetGetMethod(getterBuilder);
			propertyBuilder.SetSetMethod(setterBuilder);
		}
	}