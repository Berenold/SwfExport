using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SwfDotNet.IO.Tags;
using SwfDotNet.IO.Tags.Types;

namespace SwfExport
{
	class Program
	{
		#region decoders
		static Dictionary<Type, Func<object, Tuple<int, Image>>> Decoders = new Dictionary<Type, Func<object, Tuple<int, Image>>>
 		{	
			{typeof(DefineBitsLossLessTag), o => DecodeLossLess((DefineBitsLossLessTag)o)},
			{typeof(DefineBitsLossLess2Tag), o =>  DecodeLossLess2((DefineBitsLossLess2Tag)o)},
			{typeof(DefineBitsJpeg2Tag), o =>  DecodeJpeg2((DefineBitsJpeg2Tag)o)},
			{typeof(DefineBitsJpeg3Tag), o => DecodeJpeg3((DefineBitsJpeg3Tag)o)},
		};
		static Tuple<int, Image> DecodeLossLess(DefineBitsLossLessTag tag)
		{
			return Tuple.Create((int)tag.CharacterId, tag.DecompileToImage());
		}
		static Tuple<int, Image> DecodeLossLess2(DefineBitsLossLess2Tag tag)
		{
			return Tuple.Create((int)tag.CharacterId, tag.DecompileToImage());
		}
		static Tuple<int, Image> DecodeJpeg2(DefineBitsJpeg2Tag tag)
		{
			return Tuple.Create((int)tag.CharacterId, tag.DecompileToImage());
		}
		static Tuple<int, Image> DecodeJpeg3(DefineBitsJpeg3Tag tag)
		{
			return Tuple.Create((int)tag.CharacterId, tag.DecompileToImage());
		}
		#endregion
		static void Main(string[] args)
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Usage: SwfExport <file.swf>");
				return;
			}
			var file = args[0];
			if (!File.Exists(file))
			{
				Console.WriteLine("File not found");
				return;
			}
			try
			{

				Console.WriteLine("Loading swf");

				var sr = new SwfDotNet.IO.SwfReader(File.OpenRead(file));
				var swf = sr.ReadSwf();
				var tags = swf.Tags.Cast<BaseTag>().ToList();

				Console.WriteLine("Gettings assets");

				var asserts = new List<Assert>();
				asserts.AddRange(GetSymbolClasses(tags));
				asserts.AddRange(GetExportAsserts(tags));

				Console.WriteLine("Loading images");

				var imgs = GetImages(tags);

				Console.WriteLine("Dumping images");

				if (!Directory.Exists("output"))
					Directory.CreateDirectory("output");
				if (!Directory.Exists("output\\unknown"))
					Directory.CreateDirectory("output\\unknown");

				var sw = Stopwatch.StartNew();
				int i = 0;
				foreach (var img in imgs)
				{
					var asset = asserts.Find(a => a.TargetCharacterId == img.Item1);
					var name = (asset != null ? asset.Name : "unknown\\" + img.Item1.ToString(CultureInfo.InvariantCulture)) + ".png";
					img.Item2.Save("output\\" + name, ImageFormat.Png);
					i++;
					if (sw.ElapsedMilliseconds > 2000)
					{
						Console.WriteLine("Progress {0}/{1}", i, imgs.Count);
						sw.Restart();
					}
				}


				Console.WriteLine("Success");

			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex);
			}
		}

		static List<Tuple<int, Image>> GetImages(IEnumerable<BaseTag> tags)
		{
			var imgs = tags.Where(t => Decoders.ContainsKey(t.GetType()));
			return imgs.Select(t => Decoders[t.GetType()](t)).ToList();
		}

		static IEnumerable<Assert> GetSymbolClasses(IEnumerable<BaseTag> tags)
		{
			var syms = tags.Where(t => t is SymbolClassTag).Cast<SymbolClassTag>();
			return syms.Select(s => s.ExportedCharacters).SelectMany(s => s.Cast<Assert>().ToList());
		}
		static IEnumerable<Assert> GetExportAsserts(IEnumerable<BaseTag> tags)
		{
			var syms = tags.Where(t => t is ExportAssetsTag).Cast<ExportAssetsTag>();
			return syms.Select(s => s.ExportedCharacters).SelectMany(s => s.Cast<Assert>().ToList());
		}
	}
}
