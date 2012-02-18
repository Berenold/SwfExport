using System;
using System.IO;
using System.Xml;

using SwfDotNet.IO.Utils;

namespace SwfDotNet.IO.Tags
{
	public class MetadataTag : BaseTag
	{

		#region Ctor

		/// <summary>
		/// Creates a new <see cref="JpegTableTag"/> instance.
		/// </summary>
		public MetadataTag()
		{
			this._tagCode = (int)TagCodeEnum.Metadata;
		}
		#endregion

		#region Properties

		/// <summary>
		/// JPEG Data is an array of bytes containing the 
		/// encoding table data.
		/// </summary>
		public string Meta { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// see <see cref="SwfDotNet.IO.Tags.BaseTag">base class</see>
		/// </summary>
		public override void ReadData(byte version, BufferedBinaryReader binaryReader)
		{
			RecordHeader rh = new RecordHeader();
			rh.ReadData(binaryReader);

			int tl = System.Convert.ToInt32(rh.TagLength);
			Meta = binaryReader.ReadString();
		}

		/// <summary>
		/// Gets the size of.
		/// </summary>
		/// <returns>Size of this object.</returns>
		protected int GetSizeOf()
		{
			int res = 0;
			if (Meta != null)
				res += Meta.Length;
			return res;
		}

		/// <summary>
		/// see <see cref="SwfDotNet.IO.Tags.BaseTag">base class</see>
		/// </summary>
		public override void UpdateData(byte version)
		{
			MemoryStream m = new MemoryStream();
			BufferedBinaryWriter w = new BufferedBinaryWriter(m);

			RecordHeader rh = new RecordHeader(TagCode, GetSizeOf());

			rh.WriteTo(w);
			if (Meta != null)
				w.Write(Meta);

			w.Flush();
			// write to data array
			_data = m.ToArray();
		}

		/// <summary>
		/// Serializes the specified writer.
		/// </summary>
		/// <param name="writer">Writer.</param>
		public override void Serialize(XmlWriter writer)
		{
			writer.WriteStartElement("Metadata");
			if (this.Meta != null)
				writer.WriteAttributeString("MetadataLength", this.Meta.Length.ToString());
			writer.WriteEndElement();
		}

		#endregion
	}
}