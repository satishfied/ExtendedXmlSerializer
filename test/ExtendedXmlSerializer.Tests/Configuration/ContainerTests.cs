﻿using ExtendedXmlSerializer.Configuration;
using ExtendedXmlSerializer.ContentModel;
using ExtendedXmlSerializer.ContentModel.Format;
using ExtendedXmlSerializer.ExtensionModel.Xml;
using ExtendedXmlSerializer.ReflectionModel;
using ExtendedXmlSerializer.Tests.Support;
using System.Collections.Generic;
using Xunit;
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace ExtendedXmlSerializer.Tests.Configuration
{
	public sealed class ContainerTests
	{
		[Fact]
		public void Verify()
		{
			new ConfigurationContainer().ToSupport().Cycle(6776);
		}

		[Fact]
		public void Member()
		{
			new ConfigurationContainer().Type<Subject>()
			                            .Member(x => x.Message)
			                            .Register(A<Serializer>.Default)
			                            .Create();
		}

		sealed class Serializer : IContentSerializer<string>
		{
			public string Get(IFormatReader parameter)
			{
				throw new System.NotImplementedException();
			}

			public void Execute(ExtendedXmlSerializer.ContentModel.Writing<string> parameter)
			{
				throw new System.NotImplementedException();
			}
		}


		[Fact]
		public void VerifyClass()
		{
			new ConfigurationContainer().ToSupport()
			                            .Cycle(new Subject {Message = "Hello World!"});
		}

		[Fact]
		public void VerifyArray()
		{
			new ConfigurationContainer().ToSupport()
			                            .Cycle(new[]{1, 2, 3});
		}

		[Fact]
		public void VerifyCollection()
		{
			new ConfigurationContainer().ToSupport()
			                            .Cycle(new List<string>{ "Hello", "World"});
		}

		[Fact]
		public void VerifyDictionary()
		{
			new ConfigurationContainer().ToSupport()
			                            .Cycle(new Dictionary<string, int>{ {"Hello", 1}, {"World!", 2}});
		}


		[Fact]
		public void VerifyNullable()
		{
			var support = new ConfigurationContainer().ToSupport();
			support.Cycle(new int?(6776));
			support.Cycle((int?)null);
		}



		sealed class Subject
		{
			public string Message { get; set; }
		}
	}
}
