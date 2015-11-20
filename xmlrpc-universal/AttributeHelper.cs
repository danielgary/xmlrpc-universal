using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace xmlrpc_universal
{
	public static class AttributeHelper
	{
		public static bool IsDefined(MemberInfo mi, Type attr)
		{
			return mi.GetCustomAttribute(attr) != null;
		}

		public static Attribute GetCustomAttribute(Type type, Type attributeType)
		{
			return type.GetTypeInfo().GetCustomAttribute(attributeType);
		}
	}
}
