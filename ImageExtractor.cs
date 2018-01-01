using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;

namespace PdfImageExtractor
{
    // credits
    // https://psycodedeveloper.wordpress.com/2013/01/10/how-to-extract-images-from-pdf-files-using-c-and-itextsharp/

    /// <summary>Helper class to extract images from a PDF file. Works with the most 
    /// common image types embedded in PDF files, as far as I can tell.</summary>
    /// <example>
    /// Usage example:
    /// <code>
    /// foreach (var filename in Directory.GetFiles(searchPath, "*.pdf", SearchOption.TopDirectoryOnly))
    /// {
    ///     var images = ImageExtractor.ExtractImages(filename);
    ///     var directory = Path.GetDirectoryName(filename);
    /// 
    ///     foreach (var name in images.Keys)
    ///     {
    ///         images[name].Save(Path.Combine(directory, name));
    ///     }
    /// }
    /// </code></example>
    public class ImageExtractor
    {
        #region Properties

        private const string DEFAULT_FILENAME_FORMAT = "{filename}_Page_{page}_Image_{image}";

        public string FilenameFormat { get; set; }
        public bool Verbose { get; set; }

        #endregion

        #region Ctor

        public ImageExtractor()
        {
            FilenameFormat = DEFAULT_FILENAME_FORMAT;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks whether a specified page of a PDF file contains images.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="pageNumber"></param>
        /// <returns>True if the page contains at least one image; false otherwise.</returns>
        public bool PageContainsImages(string filename, int pageNumber)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");

            if (System.IO.File.Exists(filename) == false)
                throw new Exception("File does not exists");

            using (var reader = new PdfReader(filename))
            {
                var parser = new PdfReaderContentParser(reader);
                ImageRenderListener listener = null;
                parser.ProcessContent(pageNumber, (listener = new ImageRenderListener()));
                return listener.Images.Count > 0;
            }
        }

        /// <summary>
        /// Extracts all images (of types that iTextSharp knows how to decode) from a PDF file.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public Dictionary<string, System.Drawing.Image> ExtractImages(string filename, int pageNumber = 0)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");

            if (System.IO.File.Exists(filename) == false)
                throw new Exception("File does not exists");

            var images = new Dictionary<string, System.Drawing.Image>();

            using (var reader = new PdfReader(filename))
            {
                if (pageNumber < 0 || pageNumber > reader.NumberOfPages)
                    return images;

                int startPage = (pageNumber > 0 ? pageNumber : 1);
                int endPage = (pageNumber > 0 ? pageNumber : reader.NumberOfPages);

                var parser = new PdfReaderContentParser(reader);
                ImageRenderListener listener = null;

                for (var i = startPage; i <= endPage; i++)
                {
                    parser.ProcessContent(i, (listener = new ImageRenderListener()));
                    var index = 1;

                    if (listener.Images.Count > 0)
                    {
                        if (Verbose)
                        {
                            Console.WriteLine("Found {0} images on page {1}.", listener.Images.Count, i);
                        }

                        foreach (var pair in listener.Images)
                        {
                            var imageName = FilenameFormat
                                .Replace("{filename}", System.IO.Path.GetFileNameWithoutExtension(filename))
                                .Replace("{page}", i.ToString())
                                .Replace("{image}", index.ToString()) + pair.Value;

                            images.Add(imageName, pair.Key);
                            index++;
                        }
                    }
                }

                return images;
            }
        }

        #endregion Methods
    }

    internal class ImageRenderListener : IRenderListener
    {
        #region Fields

        Dictionary<System.Drawing.Image, string> images = new Dictionary<System.Drawing.Image, string>();

        #endregion Fields

        #region Properties

        public Dictionary<System.Drawing.Image, string> Images
        {
            get { return images; }
        }

        #endregion Properties

        #region Methods

        public void BeginTextBlock() { }

        public void EndTextBlock() { }

        public void RenderImage(ImageRenderInfo renderInfo)
        {
            PdfImageObject image = renderInfo.GetImage();
            PdfName filter = (PdfName)image.Get(PdfName.FILTER);

            //int width = Convert.ToInt32(image.Get(PdfName.WIDTH).ToString());
            //int bitsPerComponent = Convert.ToInt32(image.Get(PdfName.BITSPERCOMPONENT).ToString());
            //string subtype = image.Get(PdfName.SUBTYPE).ToString();
            //int height = Convert.ToInt32(image.Get(PdfName.HEIGHT).ToString());
            //int length = Convert.ToInt32(image.Get(PdfName.LENGTH).ToString());
            //string colorSpace = image.Get(PdfName.COLORSPACE).ToString();

            /* It appears to be safe to assume that when filter == null, PdfImageObject 
             * does not know how to decode the image to a System.Drawing.Image.
             * 
             * Uncomment the code above to verify, but when I've seen this happen, 
             * width, height and bits per component all equal zero as well. */
            if (filter != null)
            {
                System.Drawing.Image drawingImage = image.GetDrawingImage();

                string extension = ".";

                if (filter == PdfName.DCTDECODE)
                {
                    extension += PdfImageObject.ImageBytesType.JPG.FileExtension;
                }
                else if (filter == PdfName.JPXDECODE)
                {
                    extension += PdfImageObject.ImageBytesType.JP2.FileExtension;
                }
                else if (filter == PdfName.FLATEDECODE)
                {
                    extension += PdfImageObject.ImageBytesType.PNG.FileExtension;
                }
                else if (filter == PdfName.LZWDECODE)
                {
                    extension += PdfImageObject.ImageBytesType.CCITT.FileExtension;
                }

                /* Rather than struggle with the image stream and try to figure out how to handle 
                 * BitMapData scan lines in various formats (like virtually every sample I've found 
                 * online), use the PdfImageObject.GetDrawingImage() method, which does the work for us. */
                this.Images.Add(drawingImage, extension);
            }
        }

        public void RenderText(TextRenderInfo renderInfo) { }

        #endregion Methods
    }
}
