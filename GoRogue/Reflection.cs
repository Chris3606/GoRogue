using System;
using System.Collections.Generic;

namespace GoRogue
{
	/// <summary>
	/// Contains a series of functions that complement the reflection capabilities of C#, to conveniently allow for things like
	/// iterating over all types that a given type can be casted to.
	/// </summary>
	public static class Reflection
	{
		/// <summary>
		/// Gets the entire inheritanc/interface tree for the actual run-time type of the object passed in.  This will include
		/// a type representing the actual run-time type of the object, as well as a type representing each superclass
		/// of that runtime type, and each interface that runtime type or one of its superclasses implement.
		/// </summary>
		/// <param name="obj">Object to return type tree for.</param>
		/// <returns>The complete inheritance/interface tree for the runtime-type of the object passed in, including the runtime type itself,
		/// and all superclasses of that type, as well as Type objects for each interface that runtime type or its superclasses implement.</returns>
		public static IEnumerable<Type> GetRuntimeTypeTree(object obj) => GetTypeTree(obj.GetType());

		/// <summary>
		/// Gets the entire inheritance/interface tree for type T.  This will include a Type that represents type T, as well
		/// as a Type representing each superclass of T, and each interface that T or its superclasses implement.
		/// </summary>
		/// <remarks>
		/// This function may be somewhat expensive to compute, so if you plan to use it frequently it is recommended to cache
		/// the results.
		/// </remarks>
		/// <typeparam name="T">Type to get inheritance/interface tree for.</typeparam>
		/// <returns>The complete interface/inheritance tree of T, including T, all superclasses, and all interfaces T or its
		/// superclasses implement.</returns>
		public static IEnumerable<Type> GetTypeTree<T>() => GetTypeTree(typeof(T));

		/// <summary>
		/// Gets the entire inheritance/interface tree for the type specified.  This will include the <paramref name="type"/> itself, 
		/// as well as a Type representing each superclass of the type represented by <paramref name="type"/>, and each
		/// interface that <paramref name="type"/> or its superclasses implement.
		/// </summary>
		/// <remarks>
		/// This function may be somewhat expensive to compute, so if you plan to use it frequently it is recommended to cache
		/// the results.
		/// </remarks>
		/// <returns>The complete interface/inheritance for the type represented by <paramref name="type"/>, including <paramref name="type"/>,
		/// all superclasses, and all interfaces T or its superclasses implement.</returns>
		public static IEnumerable<Type> GetTypeTree(Type type)
		{
			while (type != null)
			{
				yield return type;
				type = type.BaseType;
			}

			foreach (Type implementedInterface in type.GetInterfaces())
				yield return implementedInterface;
		}
	}
}
