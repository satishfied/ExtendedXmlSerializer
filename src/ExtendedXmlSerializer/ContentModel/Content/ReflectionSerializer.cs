// MIT License
//
// Copyright (c) 2016-2018 Wojciech Nag�rski
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

using ExtendedXmlSerializer.ContentModel.Format;
using ExtendedXmlSerializer.Core;
using System.Reflection;

namespace ExtendedXmlSerializer.ContentModel.Content
{
	sealed class ReflectionSerializer : ISerializer<MemberInfo>, IContentSerializer<MemberInfo>
	{
		public static ReflectionSerializer Default { get; } = new ReflectionSerializer();
		ReflectionSerializer() {}

		public MemberInfo Get(IFormatReader parameter) => parameter.Get(parameter.Content());

		public void Write(IFormatWriter writer, MemberInfo instance) => writer.Content(writer.Get(instance));

		public void Execute(Writing<MemberInfo> parameter)
		{
			parameter.Writer.Content(parameter.Writer.Get(parameter.Instance));
		}
	}

	sealed class ReflectionSerializer<T> : IContentSerializer<T> where T : MemberInfo
	{
		public static ReflectionSerializer<T> Default { get; } = new ReflectionSerializer<T>();
		ReflectionSerializer() {}

		public T Get(IFormatReader parameter) => parameter.Get(parameter.Content()).To<T>();

		public void Execute(Writing<T> parameter)
		{
			parameter.Writer.Content(parameter.Writer.Get(parameter.Instance));
		}
	}

}