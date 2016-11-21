using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

public class XmlUtil
{
	// シリアライズ
	public static T Seialize<T>(string filename, T data)
	{
		using (var stream = new FileStream(filename, FileMode.Create))
		{
			var serializer = new XmlSerializer(typeof(T));
			serializer.Serialize(stream, data);
		}

		return data;
	}

	// デシリアライズ
	public static T Deserialize<T>(string filename)
	{
		using (var stream = new FileStream(filename, FileMode.Open))
		{
			var serializer = new XmlSerializer(typeof(T));
			return (T)serializer.Deserialize(stream);
		}
	}
}
