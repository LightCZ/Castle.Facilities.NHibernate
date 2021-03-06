﻿// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//	 http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using NHibernate.Cfg;

namespace Castle.Facilities.NHibernate
{
	/// <summary>
	/// Knows how to read/write an NH <see cref="Configuration"/> from
	/// a given filename, and whether that file should be trusted or a new
	/// Configuration should be built.
	/// </summary>
	public class FileConfigurationPersister : IConfigurationPersister
	{
		/// <summary>
		/// Gets the <see cref="Configuration"/> from the file.
		/// </summary>
		/// <param name="filename">The name of the file to read from</param>
		/// <returns>The <see cref="Configuration"/></returns>
		public virtual Configuration ReadConfiguration(string filename)
		{
			var formatter = new BinaryFormatter();
			using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return formatter.Deserialize(fileStream) as Configuration;
			}
		}

		/// <summary>
		/// Writes the <see cref="Configuration"/> to the file
		/// </summary>
		/// <param name="filename">The name of the file to write to</param>
		/// <param name="cfg">The NH Configuration</param>
		public virtual void WriteConfiguration(string filename, Configuration cfg)
		{
			using (var fileStream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(fileStream, cfg);
			}
		}

		/// <summary>
		/// Checks if a new <see cref="Configuration"/> is required or a serialized one should be used.
		/// </summary>
		/// <param name="filename">Name of the file containing the NH configuration</param>
		/// <param name="dependencies">Files that the serialized configuration depends on. </param>
		/// <returns>If the <see cref="Configuration"/> should be created or not.</returns>
		public virtual bool IsNewConfigurationRequired(string filename, IList<string> dependencies)
		{
			if (!File.Exists(filename))
				return true;

			DateTime lastModified = File.GetLastWriteTime(filename);
			bool requiresNew = false;
			for (int i = 0; i < dependencies.Count && !requiresNew; i++)
			{
				DateTime dependencyLastModified = File.GetLastWriteTime(dependencies[i]);
				requiresNew |= dependencyLastModified > lastModified;
			}
			return requiresNew;
		}
	}
}