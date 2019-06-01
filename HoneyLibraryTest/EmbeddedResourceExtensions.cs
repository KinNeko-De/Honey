namespace HoneyLibraryTest
{
	using System;
	using System.IO;
	using System.IO.Compression;
	using System.Reflection;

	public class EmbeddedResourceExtensions
	{
		public static void ExtractEmbeddedZipToDirectory(string namespaceWithPoints, string embeddedFileName, string targetPath)
		{
			var embeddedResourceName = namespaceWithPoints + "." + embeddedFileName;
			try
			{
				Assembly testingAssembling = Assembly.GetExecutingAssembly();
				using (Stream resFilestream = testingAssembling.GetManifestResourceStream(embeddedResourceName))
				{
					if (resFilestream == null)
					{
						var exception = CreateExceptionWithDebugInformation(embeddedResourceName, testingAssembling);
						throw exception;
					}

					var zipArchive = new ZipArchive(resFilestream);
					zipArchive.ExtractToDirectory(targetPath);
				}
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"extracting of embedded zip archiv resource '{embeddedResourceName}' to '{targetPath}' failed.", e);
			}
		}

		public static void CopyEmbeddedResource(string namespaceWithPoints, string embeddedFileName, string targetPath)
		{
			var embeddedResourceName = namespaceWithPoints + "." + embeddedFileName;
			var targetFilePathAndName = Path.Combine(targetPath, embeddedFileName);
			try
			{
				Assembly testingAssembly = Assembly.GetExecutingAssembly();
				
				using (Stream resFilestream = testingAssembly.GetManifestResourceStream(embeddedResourceName))
				{
					if (resFilestream == null)
					{
						var exception = CreateExceptionWithDebugInformation(embeddedResourceName, testingAssembly);
						throw exception;
					}

					using (FileStream writer = File.OpenWrite(targetFilePathAndName))
					{
						resFilestream.CopyTo(writer);
					}
				}
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"extracting of embedded resource '{embeddedResourceName}' to '{targetFilePathAndName}' failed.", e);
			}
		}

		private static InvalidOperationException CreateExceptionWithDebugInformation(string embeddedResourceName, Assembly assembly)
		{
			var exception = new InvalidOperationException($"Embedded resource '{embeddedResourceName}' could not be found.");
			exception.Data["AllEmbeddedResources"] = assembly.GetManifestResourceNames();
			return exception;
		}
	}
}