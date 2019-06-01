using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace HoneyLibrary.PackageLists
{
	[Serializable]
	public class PackageFileCanNotBeAccessedException : AggregateException
	{

		public PackageFileCanNotBeAccessedException() : base()
		{
		}

		public PackageFileCanNotBeAccessedException(string message) : base(message)
		{

		}

		public PackageFileCanNotBeAccessedException(IEnumerable<Exception> innerExceptions) : base(innerExceptions)
		{
		}

		public PackageFileCanNotBeAccessedException(string message, IEnumerable<Exception> innerExceptions) : base(message, innerExceptions)
		{
		}

		[SecurityCritical]
		protected PackageFileCanNotBeAccessedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{

		}
	}
}
