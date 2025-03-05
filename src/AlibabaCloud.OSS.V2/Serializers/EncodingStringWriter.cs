using System.IO;
using System.Text;

namespace AlibabaCloud.OSS.V2.Serializers
{
    class EncodingStringWriter : StringWriter
    {
        // Need to subclass StringWriter in order to override Encoding
        public EncodingStringWriter(Encoding encoding) => Encoding = encoding;

        public override Encoding Encoding { get; }
    }
}
