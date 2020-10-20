using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Engine.Serialization
{
	public class Archive
	{
		private delegate void ReadDelegateGeneric<T>(InputArchive archive, ref T value);

		private delegate void WriteDelegateGeneric<T>(OutputArchive archive, T value);

		protected delegate void ReadDelegate(InputArchive archive, ref object value);

		protected delegate void WriteDelegate(OutputArchive archive, object value);

		protected class SerializeData
		{
			public Type Type;

			public bool IsHumanReadableSupported;

			public ReadDelegate Read;

			public WriteDelegate Write;

			public bool UseObjectInfo;

			public bool AutoConstructObject;

			public void MergeOptionsFrom(SerializeData serializeData)
			{
				UseObjectInfo = serializeData.UseObjectInfo;
				AutoConstructObject = serializeData.AutoConstructObject;
			}

			public SerializeData Clone()
			{
				return (SerializeData)MemberwiseClone();
			}
		}

		private static HashSet<Assembly> m_scannedAssemblies = new HashSet<Assembly>();

		private static Dictionary<Type, SerializeData> m_serializeDataByType = new Dictionary<Type, SerializeData>();

		private static Dictionary<Type, SerializeData> m_pendingOptionsByType = new Dictionary<Type, SerializeData>();

		private static Dictionary<Type, TypeInfo> m_genericSerializersByType = new Dictionary<Type, TypeInfo>();

		public int Version
		{
			get;
			set;
		}

		public bool UseObjectInfos
		{
			get;
			set;
		} = true;


		protected Archive(int version)
		{
			Version = version;
		}

		public static bool IsTypeSerializable(Type type)
		{
			return GetSerializeData(type, allowEmptySerializer: true).Read != null;
		}

		public static void SetTypeSerializationOptions(Type type, bool useObjectInfo, bool autoConstructObject)
		{
			lock (m_serializeDataByType)
			{
				SerializeData serializeData = new SerializeData
				{
					UseObjectInfo = useObjectInfo,
					AutoConstructObject = autoConstructObject
				};
				if (m_serializeDataByType.TryGetValue(type, out SerializeData value))
				{
					value.MergeOptionsFrom(serializeData);
				}
				else
				{
					m_pendingOptionsByType[type] = serializeData;
				}
			}
		}

		protected static SerializeData GetSerializeData(Type type, bool allowEmptySerializer)
		{
			lock (m_serializeDataByType)
			{
				if (!m_serializeDataByType.TryGetValue(type, out SerializeData value))
				{
					if (type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(ISerializable)))
					{
						value = CreateSerializeDataForSerializable(type);
						AddSerializeData(value);
					}
					else
					{
						ScanAssembliesForSerializers();
						if (!m_serializeDataByType.TryGetValue(type, out value))
						{
							if (type.IsArray)
							{
								if (m_genericSerializersByType.TryGetValue(typeof(Array), out TypeInfo value2))
								{
									value = CreateSerializeDataForSerializer(value2.MakeGenericType(type.GetElementType()).GetTypeInfo(), type, typeof(Array));
									AddSerializeData(value);
								}
							}
							else if (type.GetTypeInfo().IsGenericType)
							{
								Type genericTypeDefinition = type.GetGenericTypeDefinition();
								if (m_genericSerializersByType.TryGetValue(genericTypeDefinition, out TypeInfo value3))
								{
									value = CreateSerializeDataForSerializer(value3.MakeGenericType(type.GenericTypeArguments).GetTypeInfo(), type, type);
									AddSerializeData(value);
								}
							}
							else if (type.BaseType != null && IsTypeSerializable(type.BaseType))
							{
								value = GetSerializeData(type.BaseType, allowEmptySerializer: true).Clone();
								value.Type = type;
								value.AutoConstructObject = true;
							}
						}
						if (value == null)
						{
							value = CreateEmptySerializeData(type);
							AddSerializeData(value);
						}
					}
				}
				if (!allowEmptySerializer && value.Read == null)
				{
					throw new InvalidOperationException($"ISerializer suitable for type \"{type.FullName}\" not found in any loaded assembly.");
				}
				return value;
			}
		}

		private static void ScanAssembliesForSerializers()
		{
			foreach (Assembly item in TypeCache.LoadedAssemblies.Where((Assembly a) => !TypeCache.IsKnownSystemAssembly(a)))
			{
				if (!m_scannedAssemblies.Contains(item))
				{
					foreach (TypeInfo definedType in item.DefinedTypes)
					{
						foreach (Type implementedInterface in definedType.ImplementedInterfaces)
						{
							if (implementedInterface.IsConstructedGenericType && implementedInterface.GetGenericTypeDefinition() == typeof(ISerializer<>))
							{
								if (!definedType.IsGenericType || !definedType.IsGenericTypeDefinition)
								{
									Type type = implementedInterface.GenericTypeArguments[0];
									if (!m_serializeDataByType.ContainsKey(type))
									{
										SerializeData serializeData = CreateSerializeDataForSerializer(definedType, type, type);
										if (serializeData != null)
										{
											AddSerializeData(serializeData);
										}
									}
								}
								else
								{
									Type type2 = implementedInterface.GenericTypeArguments[0];
									Type key = (type2 == typeof(Array)) ? type2 : type2.GetGenericTypeDefinition();
									m_genericSerializersByType.Add(key, definedType);
								}
							}
						}
					}
					m_scannedAssemblies.Add(item);
				}
			}
		}

		private static SerializeData CreateSerializeDataForSerializable(Type type)
		{
			return (SerializeData)typeof(Archive).GetTypeInfo().GetDeclaredMethod("CreateSerializeDataForSerializableHelper").MakeGenericMethod(type)
				.Invoke(null, new object[0]);
		}

		private static SerializeData CreateSerializeDataForSerializer(TypeInfo serializerType, Type type, Type parameterType)
		{
			MethodInfo methodInfo = serializerType.GetDeclaredMethods("Serialize").FirstOrDefault(delegate(MethodInfo m)
			{
				ParameterInfo[] parameters2 = m.GetParameters();
				return parameters2.Length == 2 && parameters2[0].ParameterType == typeof(InputArchive) && parameters2[1].ParameterType == parameterType.MakeByRefType();
			});
			MethodInfo methodInfo2 = serializerType.GetDeclaredMethods("Serialize").FirstOrDefault(delegate(MethodInfo m)
			{
				ParameterInfo[] parameters = m.GetParameters();
				return parameters.Length == 2 && parameters[0].ParameterType == typeof(OutputArchive) && parameters[1].ParameterType == parameterType;
			});
			if (methodInfo != null && methodInfo2 != null)
			{
				object target = Activator.CreateInstance(serializerType.AsType());
				Type delegateType = typeof(ReadDelegateGeneric<>).MakeGenericType(parameterType);
				Type delegateType2 = typeof(WriteDelegateGeneric<>).MakeGenericType(parameterType);
				Delegate @delegate = methodInfo.CreateDelegate(delegateType, target);
				Delegate delegate2 = methodInfo2.CreateDelegate(delegateType2, target);
				return (SerializeData)typeof(Archive).GetTypeInfo().GetDeclaredMethod("CreateSerializeDataForSerializerHelper").MakeGenericMethod(type, parameterType)
					.Invoke(null, new object[2]
					{
						@delegate,
						delegate2
					});
			}
			return null;
		}

		private static SerializeData CreateSerializeDataForSerializableHelper<T>() where T : ISerializable
		{
			SerializeData serializeData = CreateEmptySerializeData(typeof(T));
			if (typeof(T).GetTypeInfo().IsValueType)
			{
				serializeData.Read = delegate(InputArchive archive, ref object value)
				{
					T val = (T)value;
					val.Serialize(archive);
					value = val;
				};
			}
			else
			{
				serializeData.Read = delegate(InputArchive archive, ref object value)
				{
					((T)value).Serialize(archive);
				};
			}
			serializeData.Write = delegate(OutputArchive archive, object value)
			{
				((T)value).Serialize(archive);
			};
			serializeData.AutoConstructObject = true;
			return serializeData;
		}

		private static SerializeData CreateSerializeDataForSerializerHelper<T, TParam>(Delegate readDelegate, Delegate writeDelegate)
		{
			ReadDelegateGeneric<TParam> readDelegateGeneric = (ReadDelegateGeneric<TParam>)readDelegate;
			WriteDelegateGeneric<TParam> writeDelegateGeneric = (WriteDelegateGeneric<TParam>)writeDelegate;
			SerializeData serializeData = CreateEmptySerializeData(typeof(T));
			serializeData.Read = delegate(InputArchive archive, ref object value)
			{
				TParam value2 = (value != null) ? ((TParam)value) : default(TParam);
				readDelegateGeneric(archive, ref value2);
				value = value2;
			};
			serializeData.Write = delegate(OutputArchive archive, object value)
			{
				writeDelegateGeneric(archive, (TParam)value);
			};
			return serializeData;
		}

		private static SerializeData CreateEmptySerializeData(Type type)
		{
			return new SerializeData
			{
				Type = type,
				UseObjectInfo = (!type.GetTypeInfo().IsValueType && type != typeof(string)),
				IsHumanReadableSupported = HumanReadableConverter.IsTypeSupported(type)
			};
		}

		private static void AddSerializeData(SerializeData serializeData)
		{
			if (m_pendingOptionsByType.TryGetValue(serializeData.Type, out SerializeData value))
			{
				serializeData.MergeOptionsFrom(value);
			}
			m_serializeDataByType.Add(serializeData.Type, serializeData);
		}
	}
}
