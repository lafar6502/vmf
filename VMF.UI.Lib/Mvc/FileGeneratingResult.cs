using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace VMF.UI.Lib.Mvc
{
    /// <summary>
    /// MVC action result that generates the file content using a delegate that writes the content directly to the output stream.
    /// </summary>
    public class FileGeneratingResult : FileResult
    {
        /// <summary>
        /// The delegate that will generate the file content.
        /// </summary>
        private readonly Action<System.IO.Stream> content;

        private readonly bool bufferOutput;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileGeneratingResult" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="content">Delegate with Stream parameter. This is the stream to which content should be written.</param>
        /// <param name="bufferOutput">use output buffering. Set to false for large files to prevent OutOfMemoryException.</param>
        public FileGeneratingResult(string fileName, string contentType, Action<System.IO.Stream> content, bool bufferOutput = false)
            : base(contentType)
        {
            if (content == null)
                throw new ArgumentNullException("content");

            this.content = content;
            this.bufferOutput = bufferOutput;
            this.FileDownloadName = fileName;
        }

        /// <summary>
        /// Writes the file to the response.
        /// </summary>
        /// <param name="response">The response object.</param>
        protected override void WriteFile(System.Web.HttpResponseBase response)
        {
            response.BufferOutput = bufferOutput;
            content(response.OutputStream);
        }
    }
}
