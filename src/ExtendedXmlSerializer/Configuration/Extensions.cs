﻿// MIT License
// 
// Copyright (c) 2016 Wojciech Nagórski
//                    Michael DeMond
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using ExtendedXmlSerialization.ContentModel.Members;
using ExtendedXmlSerialization.Core;
using ExtendedXmlSerialization.Core.Specifications;
using ExtendedXmlSerialization.ExtensionModel;
using ExtendedXmlSerialization.TypeModel;

namespace ExtendedXmlSerialization.Configuration
{
	public static class Extensions
	{
		public static IConfiguration Apply<T>(this IConfiguration @this)
			where T : class, ISerializerExtension => Apply(@this, Support<T>.New);

		public static IConfiguration Apply<T>(this IConfiguration @this, Func<T> create)
			where T : class, ISerializerExtension
		{
			if (@this.Find<T>() == null)
			{
				@this.Add(create);
			}
			return @this;
		}

		public static T With<T>(this IConfiguration @this) where T : class, ISerializerExtension
			=> @this.Find<T>() ?? @this.Add<T>();

		public static T Add<T>(this IConfiguration @this) where T : ISerializerExtension
			=> Add(@this, Support<T>.New);

		public static T Add<T>(this IConfiguration @this, Func<T> create) where T : ISerializerExtension
		{
			var result = create();
			@this.Add(result);
			return result;
		}

		public static IConfiguration EnableReferences(this IConfiguration @this)
			=> @this.Apply<ReferencesExtension>();

		public static TypeConfiguration<T> ConfigureType<T>(this IConfiguration @this)
			=> @this.Type<T>();

		public static TypeConfiguration<T> Type<T>(this IConfiguration @this)
			=> TypeConfigurations<T>.Default.Get(@this);

		public static MemberConfiguration<T, TMember> Member<T, TMember>(
			this TypeConfiguration<T> @this,
			Expression<Func<T, TMember>> member)
			=> Members<T, TMember>.Defaults.Get(@this.Configuration).Get(member.GetMemberInfo());

		public static TypeConfiguration<T> Member<T, TMember>(this TypeConfiguration<T> @this,
		                                                      Expression<Func<T, TMember>> member,
		                                                      Action<MemberConfiguration<T, TMember>>
			                                                      configure)
		{
			configure(@this.Member(member));
			return @this;
		}

		public static TypeConfiguration<T> Owner<T>(this IMemberConfiguration @this)
			=> TypeConfigurations<T>.Default.For(@this.Owner);

		public static string Name<T>(this IConfigurationItem<T> @this) where T : MemberInfo => @this.Name.Get();

		public static IConfigurationItem<T> Name<T>(this IConfigurationItem<T> @this, string name) where T : MemberInfo
		{
			@this.Name.Assign(name);
			return @this;
		}

		public static int Order(this IMemberConfiguration @this) => @this.Order.Get();

		public static IMemberConfiguration Order(this IMemberConfiguration @this, int order)
		{
			@this.Order.Assign(order);
			return @this;
		}

		public static IMemberConfiguration Attribute<T, TMember>(
			this MemberConfiguration<T, TMember> @this, Func<TMember, bool> when)
		{
			@this.Configuration.With<MemberConfigurationExtension>().Runtime[@this.Get()] =
				new RuntimeMemberSpecification(new DelegatedSpecification<TMember>(when).Adapt());
			return @this.Attribute();
		}

		public static IMemberConfiguration Attribute(this IMemberConfiguration @this)
		{
			@this.Configuration.With<AttributesExtension>().Registered.Add(@this.Get());
			return @this;
		}

		public static IMemberConfiguration Content(this IMemberConfiguration @this)
		{
			@this.Configuration.With<AttributesExtension>().Registered.Remove(@this.Get());
			return @this;
		}

		public static IMemberConfiguration Encrypt(this IMemberConfiguration @this)
		{
			@this.Configuration.With<EncryptionExtension>().Registered.Add(@this.Get());
			return @this;
		}

		public static IMemberConfiguration Identity(this IMemberConfiguration @this)
		{
			@this.Attribute();
			@this.Configuration.With<ReferencesExtension>().Assign(@this.Owner.Get(), @this.Get());
			return @this;
		}

		public static ITypeConfiguration GetTypeConfiguration(this IConfiguration @this, Type type)
			=> @this.GetTypeConfiguration(type.GetTypeInfo());

		public static ITypeConfiguration GetTypeConfiguration(this IConfiguration @this, TypeInfo type)
			=> TypeConfigurations.Defaults.Get(@this).Get(type);

		public static IMemberConfiguration Member(this ITypeConfiguration @this, string name)
		{
			var member = @this.Get().GetMember(name).SingleOrDefault();
			var result = member != null ? @this.Member(member) : null;
			return result;
		}

		public static TypeConfiguration<T> CustomSerializer<T>(
			this TypeConfiguration<T> @this,
			Action<XmlWriter, T> serializer,
			Func<XElement, T> deserialize)
			=> @this.CustomSerializer(new ExtendedXmlCustomSerializer<T>(deserialize, serializer));

		public static TypeConfiguration<T> CustomSerializer<T>(this TypeConfiguration<T> @this,
		                                                       IExtendedXmlCustomSerializer<T> serializer)
		{
			@this.Configuration.With<CustomXmlExtension>().Assign(@this.Get(), new Adapter<T>(serializer));
			return @this;
		}

		public static TypeConfiguration<T> AddMigration<T>(this TypeConfiguration<T> @this,
														   ICommand<XElement> migration)
			=> @this.AddMigration(migration.Execute);

		public static TypeConfiguration<T> AddMigration<T>(this TypeConfiguration<T> @this,
		                                                   Action<XElement> migration)
			=> @this.AddMigration(migration.Yield());

		public static TypeConfiguration<T> AddMigration<T>(this TypeConfiguration<T> @this,
		                                                   IEnumerable<Action<XElement>> migrations)
		{
			@this.Configuration.With<MigrationsExtension>().Add(@this.Get(), migrations.Fixed());
			return @this;
		}

		public static TypeConfiguration<T> EnableReferences<T, TMember>(this TypeConfiguration<T> @this,
		                                                                Expression<Func<T, TMember>> member)
		{
			@this.Member(member).Identity();
			return @this;
		}

		public static IConfiguration WithSettings(this IConfiguration @this,
		                                          XmlReaderSettings readerSettings,
		                                          XmlWriterSettings writerSettings)
			=> @this.Extend(new XmlSerializationExtension(readerSettings, writerSettings));

		public static IConfiguration WithSettings(this IConfiguration @this,
		                                          XmlReaderSettings readerSettings)
			=> @this.Extend(new XmlSerializationExtension(readerSettings));

		public static IConfiguration WithSettings(this IConfiguration @this,
		                                          XmlWriterSettings writerSettings)
			=> @this.Extend(new XmlSerializationExtension(writerSettings));

		public static IConfiguration Extend(this IConfiguration @this,
		                                    params ISerializerExtension[] extensions)
			=> new ExtendedConfiguration(@this.With(extensions));

		public static ISerializerExtension[] With(this IEnumerable<ISerializerExtension> @this,
		                                          params ISerializerExtension[] extensions)
			=> @this.TypeZip(extensions).ToArray();

		public static IConfiguration UseEncryptionAlgorithm(this IConfiguration @this)
			=> UseEncryptionAlgorithm(@this, Encryption.Default);

		public static IConfiguration UseEncryptionAlgorithm(this IConfiguration @this, IEncryption encryption)
			=> @this.Extend(new EncryptionExtension(encryption));

		public static IConfiguration UseAutoProperties(this IConfiguration @this)
			=> @this.Extend(AutoAttributesExtension.Default);

		public static IConfiguration UseAutoProperties(this IConfiguration @this, int maxTextLength)
			=> @this.Extend(new AutoAttributesExtension(maxTextLength));

		public static IConfiguration UseOptimizedNamespaces(this IConfiguration @this)
			=> @this.Extend(OptimizedNamespaceExtension.Default);
	}
}