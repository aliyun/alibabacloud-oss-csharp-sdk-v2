
using AlibabaCloud.OSS.v2.Models;
using System.Text;
using System.Xml.Serialization;

namespace AlibabaCloud.OSS.v2.UnitTests.Models;

public class ModelSharedTest {
    [Fact]
    public void TestSerializerAccessControlPolicy() {
        var ns = new XmlSerializerNamespaces();
        ns.Add(string.Empty, string.Empty);
        var serializer = new XmlSerializer(typeof(AccessControlPolicy));

        // All
        var obj = new AccessControlPolicy() {
            Owner = new Owner() {
                Id = "my-id",
                DisplayName = "Name",
            },
            AccessControlList = new AccessControlList() {
                Grant = "private",
            },
        };

        var writer = new Serializers.EncodingStringWriter(Encoding.UTF8);
        serializer.Serialize(writer, obj, ns);
        writer.Flush();
        var xmlStr = writer.ToString();
        var xmlPat = """
<?xml version="1.0" encoding="utf-8"?>
<AccessControlPolicy>
  <Owner>
    <ID>my-id</ID>
    <DisplayName>Name</DisplayName>
  </Owner>
  <AccessControlList>
    <Grant>private</Grant>
  </AccessControlList>
</AccessControlPolicy>
""";
        Assert.Equal(xmlPat, xmlStr);

        // No Owner
        obj = new AccessControlPolicy() {
            AccessControlList = new AccessControlList() {
                Grant = "private",
            },
        };

        writer = new v2.Serializers.EncodingStringWriter(Encoding.UTF8);
        serializer.Serialize(writer, obj, ns);
        writer.Flush();
        xmlStr = writer.ToString();
        xmlPat = """
<?xml version="1.0" encoding="utf-8"?>
<AccessControlPolicy>
  <AccessControlList>
    <Grant>private</Grant>
  </AccessControlList>
</AccessControlPolicy>
""";

        Assert.Equal(xmlPat, xmlStr);
    }

    [Fact]
    public void TestDeserializeAccessControlPolicy() {
        var serializer = new XmlSerializer(typeof(AccessControlPolicy));
        string xmlStr;
        AccessControlPolicy obj;
        StringReader reader;

        // All
        xmlStr = """
<?xml version="1.0" encoding="utf-8"?>
<AccessControlPolicy>
  <Owner>
    <ID>my-id</ID>
    <DisplayName>Name</DisplayName>
  </Owner>
  <AccessControlList>
    <Grant>private</Grant>
  </AccessControlList>
</AccessControlPolicy>
""";
        reader = new StringReader(xmlStr);
        obj = (AccessControlPolicy)serializer.Deserialize(reader);
        Assert.NotNull(obj);
        Assert.NotNull(obj.Owner);
        Assert.Equal("my-id", obj.Owner.Id);
        Assert.Equal("Name", obj.Owner.DisplayName);

        Assert.NotNull(obj.AccessControlList);
        Assert.Equal("private", obj.AccessControlList.Grant);

        // Owner Empty
        xmlStr = """
<?xml version="1.0" encoding="utf-8"?>
<AccessControlPolicy>
  <Owner>
  </Owner>
  <AccessControlList>
    <Grant>private</Grant>
  </AccessControlList>
</AccessControlPolicy>
""";
        reader = new StringReader(xmlStr);
        obj = (AccessControlPolicy)serializer.Deserialize(reader);
        Assert.NotNull(obj);
        Assert.NotNull(obj.Owner);
        Assert.Null(obj.Owner.Id);
        Assert.Null(obj.Owner.DisplayName);

        Assert.NotNull(obj.AccessControlList);
        Assert.Equal("private", obj.AccessControlList.Grant);

        // No Owner Node
        xmlStr = """
<?xml version="1.0" encoding="utf-8"?>
<AccessControlPolicy>
  <AccessControlList>
    <Grant>private</Grant>
  </AccessControlList>
</AccessControlPolicy>
""";
        reader = new StringReader(xmlStr);
        obj = (AccessControlPolicy)serializer.Deserialize(reader);
        Assert.NotNull(obj);
        Assert.Null(obj.Owner);
        Assert.NotNull(obj.AccessControlList);
        Assert.Equal("private", obj.AccessControlList.Grant);

        // No Owner and AccessControlList Node
        xmlStr = """
<?xml version="1.0" encoding="utf-8"?>
<AccessControlPolicy>
</AccessControlPolicy>
""";
        reader = new StringReader(xmlStr);
        obj = (AccessControlPolicy)serializer.Deserialize(reader);
        Assert.NotNull(obj);
        Assert.Null(obj.Owner);
        Assert.Null(obj.AccessControlList);
    }

    [Fact]
    public void TestDeserializeAccessControlPolicyRootName() {
        var serializer = new XmlSerializer(typeof(AccessControlPolicy));
        AccessControlPolicy obj;
        StringReader reader;

        // All
        var xmlStr = """
<?xml version="1.0" encoding="utf-8"?>
<AccessControlPolicyInvalid>
  <Owner>
    <ID>my-id</ID>
    <DisplayName>Name</DisplayName>
  </Owner>
  <AccessControlList>
    <Grant>private</Grant>
  </AccessControlList>
</AccessControlPolicyInvalid>
""";
        // use root name defined in AccessControlPolicy
        try {
            reader = new StringReader(xmlStr);
            serializer.Deserialize(reader);
            Assert.Fail("should not here");
        }
        catch (Exception ex) {
            Assert.Contains("XML", ex.Message);
            Assert.Contains("(2, 2)", ex.Message);
        }

        var root = new XmlRootAttribute("AccessControlPolicyInvalid");
        serializer = new XmlSerializer(typeof(AccessControlPolicy), root);
        reader = new StringReader(xmlStr);
        obj = (AccessControlPolicy)serializer.Deserialize(reader);
        Assert.NotNull(obj);
        Assert.NotNull(obj.Owner);
        Assert.Equal("my-id", obj.Owner.Id);
        Assert.Equal("Name", obj.Owner.DisplayName);

        Assert.NotNull(obj.AccessControlList);
        Assert.Equal("private", obj.AccessControlList.Grant);
    }
}
